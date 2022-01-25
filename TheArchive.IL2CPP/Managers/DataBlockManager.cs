using GameData;
using Gear;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive.Utilities;

namespace TheArchive.Managers
{
    public class DataBlockManager
    {

        private static List<Type> _dataBlockTypes;
        public static List<Type> DataBlockTypes
        {
            get
            {
                if (_dataBlockTypes == null)
                    GetAllDataBlockTypes();
                return _dataBlockTypes;
            }
        }

        public static void GetAllDataBlockTypes()
        {
            var AllTypesOfGameDataBlockBase = from x in Assembly.GetAssembly(typeof(EnemyDataBlock)).GetTypes()
                                              let y = x.BaseType
                                              where !x.IsAbstract && !x.IsInterface &&
                                              y != null && y.IsGenericType &&
                                              y.GetGenericTypeDefinition() == typeof(GameDataBlockBase<>)
                                              select x;
            _dataBlockTypes = AllTypesOfGameDataBlockBase.ToList();
        }

        public static List<string> DefaultOfflineGear { get; private set; } = new List<string>();

        public static void DumpDataBlocksToDisk()
        {
            if (!ArchiveMod.Settings.DumpDataBlocks) return;

            ArchiveLogger.Msg(ConsoleColor.Green, $"{nameof(DataBlockManager)}: Dumping DataBlocks to disk ...");
            try
            {
                foreach (var type in DataBlockTypes)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkGreen, $"> {type.FullName}");

                    var path = Path.Combine(LocalFiles.DataBlockDumpPath, type.Name + ".json");

                    var genericType = typeof(GameDataBlockBase<>).MakeGenericType(type);

                    string fileContents = (string) genericType.GetMethod("GetFileContents").Invoke(null, new object[0]);

                    if (ArchiveMod.Settings.AlwaysOverrideDataBlocks || !File.Exists(path))
                    {
                        ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"  > Writing to file: {path}");

                        File.WriteAllText(path, fileContents);
                    }

                    if (type != typeof(PlayerOfflineGearDataBlock))
                    {
                        continue;
                    }

                    if (ArchiveMod.Settings.EnableDisabledGear)
                    {
                        ArchiveLogger.Msg(ConsoleColor.Green, $"Enabling disabled gear");
                    }

                    ArchiveLogger.Msg(ConsoleColor.Green, $"Creating Gear cache ...");

                    var wrapperType = typeof(GameDataBlockWrapper<>).MakeGenericType(type);
                    var wrapper = genericType.GetProperty("Wrapper").GetValue(null);

                    // List<DataBlockType>
                    var blocks = (Il2CppSystem.Collections.Generic.List<PlayerOfflineGearDataBlock>) wrapperType.GetProperty("Blocks").GetValue(wrapper);

                    foreach (var block in blocks)
                    {
                        if(block.internalEnabled)
                        {
                            var gidr = new GearIDRange(block.GearJSON);
                            DefaultOfflineGear.Add(gidr.PublicGearName);
                        }
                        if (block.name == "Mine_Deployer_Glue")
                        {
                            // C-Foam Mine Deployer actually works although the mines have the wrong model and a red laser,
                            // the particle effects do also feel a little unpolished (it is an unfinished and unused tool after all lol)
                            block.GearJSON = "{\"Ver\": 1,\"Name\": \"MineDeployer Glue\",\"Packet\": {\"Comps\": {\"Length\": 9,\"a\": {\"c\": 2,\"v\": 13},\"b\": {\"c\": 3,\"v\": 37},\"c\": {\"c\": 4,\"v\": 14},\"d\": {\"c\": 27,\"v\": 12},\"e\": {\"c\": 30,\"v\": 2},\"f\": {\"c\": 33,\"v\": 2},\"g\": {\"c\": 36,\"v\": 1},\"h\": {\"c\": 37,\"v\": 1},\"i\": {\"c\": 40,\"v\": 1},\"j\": {\"c\": 42,\"v\": 2}},\"MatTrans\": {\"tDecalA\": {\"scale\": 0.1},\"tDecalB\": {\"scale\": 0.1},\"tPattern\": {\"scale\": 0.1}},\"publicName\": {\"data\": \"C-Foam Mine Deployer\"}}}";
                        }

                        if(block.name == "Map_Device")
                        {
                            // Sadly the mapping device doesn't work :c
                            block.GearJSON = "{\"Ver\": 1,\"Name\": \"Mapper\",\"Packet\": {\"Comps\": {\"Length\": 9,\"a\": {\"c\": 2,\"v\": 10},\"b\": {\"c\": 3,\"v\": 74},\"c\": {\"c\": 4,\"v\": 16},\"d\": {\"c\": 27,\"v\": 16},\"e\": {\"c\": 30,\"v\": 6},\"f\": {\"c\": 32,\"v\": 3},\"g\": {\"c\": 33,\"v\": 5},\"h\": {\"c\": 36,\"v\": 1},\"i\":  {\"c\": 42,\"v\": 3}},\"MatTrans\": {\"tDecalA\": {\"scale\": 0.1},\"tDecalB\": {\"scale\": 0.1},\"tPattern\": {\"scale\": 0.1}},\"publicName\": {\"data\": \"Mapper\"}}}";
                        }
                        if(ArchiveMod.Settings.EnableDisabledGear)
                        {
                            block.internalEnabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }

        }

    }
}
