using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TheArchive.Core.Bootstrap;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Core.Interop;

internal static class LocaliaCoreInterop
{
    
    private const string LOCALIA_CORE_GUID = "Localia.LocaliaCore";
    private const string LOCALIA_MODLIST_GUID = "Localia.ModList";

    private static bool _hasExecuted;
    private static Harmony _harmonyInstance;
    private static Assembly _localiaCoreAssembly;
    private static Assembly _localiaModlistAssembly;
    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateArSubLoggerInstance(nameof(LocaliaCoreInterop));
    
    internal static void TryApplyPatch()
    {
        if (_hasExecuted)
            return;
        
        _hasExecuted = true;
        
        try
        {
            _localiaCoreAssembly = IL2CPPChainloader.Instance.Plugins
                .FirstOrDefault(kvp => kvp.Key == LOCALIA_CORE_GUID).Value?.Instance?.GetType().Assembly;

            if (_localiaCoreAssembly == null)
                return;

            _harmonyInstance = new Harmony($"{ArchiveMod.GUID}_{nameof(LocaliaCoreInterop)}");

            var Type__Patch_DoChangeState =
                AccessTools.GetTypesFromAssembly(_localiaCoreAssembly).FirstOrDefault(t => t.Name == "Patch_DoChangeState");

            var targetMethod = Type__Patch_DoChangeState?.GetMethod("Postfix");

            var transpilerMethod =
                typeof(LocaliaCoreInterop).GetMethod(nameof(Transpiler), BindingFlags.Static | BindingFlags.NonPublic);

            _harmonyInstance.Patch(original: targetMethod, transpiler: new HarmonyMethod(transpilerMethod));
            
            
            _localiaModlistAssembly = IL2CPPChainloader.Instance.Plugins
                .FirstOrDefault(kvp => kvp.Key == LOCALIA_MODLIST_GUID).Value?.Instance?.GetType().Assembly;

            if (_localiaModlistAssembly == null)
                return;

            var Type__ModList_Manager = AccessTools.GetTypesFromAssembly(_localiaModlistAssembly).FirstOrDefault(t => t.Name == "ModList_Manager");
            
            //public static Dictionary<string, string> myModDic = new Dictionary<string, string>();
            var myModDic = (Dictionary<string, string>) Type__ModList_Manager?.GetField("myModDic", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

            if (myModDic == null)
                return;
            
            foreach (var (moduleKey, module) in ArchiveModuleChainloader.Instance.Modules)
            {
                var key = moduleKey.Replace(';', ' ');
                myModDic.Add(key, module.Metadata.Name);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"{nameof(LocaliaCoreInterop)} failed!:");
            Logger.Exception(ex);
        }
    }

    private static StringBuilder DoThings(StringBuilder stringBuilder)
    {
        var Type__Network_Manager = _localiaCoreAssembly.GetTypes().FirstOrDefault(t => t.Name == "Network_Manager");
        //public static List<string> myModList = new List<string>();
        var FI__Network_Manager__myModList = Type__Network_Manager?.GetField("myModList", BindingFlags.Public | BindingFlags.Static);
        
        var myModList = (List<string>) FI__Network_Manager__myModList?.GetValue(null);

        if (myModList == null)
            return stringBuilder;
        
        foreach (var (moduleKey, module) in ArchiveModuleChainloader.Instance.Modules)
        {
            var key = moduleKey.Replace(';', ' ');
            myModList.Add(key);
            stringBuilder.Append($"{key};");
        }
        
        return stringBuilder;
    }
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var hasInserted = false;
        foreach (var instruction in instructions)
        {
            if (hasInserted || instruction.opcode != OpCodes.Callvirt)
            {
                yield return instruction;
                continue;
            }

            if (instruction.operand is MethodInfo method && method.Name == "ToString")
            {
                var methodInfo = typeof(LocaliaCoreInterop).GetMethod("DoThings", BindingFlags.Static | BindingFlags.NonPublic);
                yield return new CodeInstruction(OpCodes.Call, methodInfo);
                
                hasInserted = true;
            }
            
            yield return instruction;
        }
    }
}