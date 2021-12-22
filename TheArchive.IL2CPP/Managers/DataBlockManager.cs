using GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Core;
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

        public static void DumpDataBlocksToDisk()
        {
            if (!ArchiveMod.Settings.DumpDataBlocks) return;

            ArchiveLogger.Msg(ConsoleColor.Green, $"{nameof(DataBlockManager)}: Dumping DataBlocks to disk ...");
            try
            {
                foreach (var type in DataBlockTypes)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkGreen, $"> {type.FullName}");

                    var genericType = typeof(GameDataBlockBase<>).MakeGenericType(type);

                    string fileContents = (string) genericType.GetMethod("GetFileContents").Invoke(null, new object[0]);


                    var path = Path.Combine(LocalFiles.DataBlockDumpPath, type.Name + ".json");

                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"  > Writing to file: {path}");

                    File.WriteAllText(path, fileContents);
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error(ex.Message);
                ArchiveLogger.Error(ex.StackTrace);
            }

        }

    }
}
