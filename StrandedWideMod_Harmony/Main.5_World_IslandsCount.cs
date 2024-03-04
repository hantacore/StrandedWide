using Beam.Terrain;
using Beam.Utilities;
using HarmonyLib;
using SharpNeatLib.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Funlabs;
using Beam.UI;
using Beam.UI.MapEditor;
using Beam;
using System.Reflection;
using UnityEngine.EventSystems;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        internal static MethodInfo mi_World_CreateProceduralMap = typeof(World).GetMethod("CreateProceduralMap", BindingFlags.Static | BindingFlags.NonPublic);
        internal static MethodInfo mi_World_GetWorldMapFolderName = typeof(World).GetMethod("GetWorldMapFolderName", BindingFlags.Static | BindingFlags.NonPublic);
        internal static MethodInfo mi_World_CreateMissionTiles = typeof(World).GetMethod("CreateMissionTiles", BindingFlags.Static | BindingFlags.NonPublic);
        internal static PropertyInfo pi_mapList = typeof(World).GetProperty("_mapList", BindingFlags.Static | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(World), "ReplaceMapWithProceduralMap")]
        class World_ReplaceMapWithProceduralMap_Patch
        {
            static bool Prefix(Map replaceMap, out Map proceduralMap, ref bool __result)
            {
                proceduralMap = null;
                try
                {
                    int num = 0;
                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        if (World.MapList[i] == replaceMap)
                        {
                            //Map map = World.CreateProceduralMap(i, num);
                            Map map = mi_World_CreateProceduralMap.Invoke(null, new object[] { i, num }) as Map;
                            World.MapList[i] = map;
                            proceduralMap = map;
                            __result = true;
                            return false;
                        }
                        num++;
                    }
                    __result = false;
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_ReplaceMapWithProceduralMap_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World), "ReplaceMap")]
        class World_ReplaceMap_Patch
        {
            static bool Prefix(Map mapToReplace, Map replacingMap)
            {
                try
                {
                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        if (World.MapList[i] == mapToReplace)
                        {
                            World.MapList[i] = replacingMap;
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_ReplaceMap_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World), "CreateMissionTiles")]
        class World_CreateMissionTiles_Patch
        {
            static bool Prefix(ref int[] __result)
            {
                try
                {
                    FastRandom fastRandom = new FastRandom(StrandedWorld.WORLD_SEED);
                    int upperBound = Main.IslandsCount - 1;
                    List<int> list = new List<int>();
                    int[] array = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        int num = fastRandom.Next(1, upperBound);
                        while (list.Contains(num))
                        {
                            num = fastRandom.Next(1, upperBound);
                        }
                        list.Add(num);
                        array[i] = num;
                    }
                    __result = array;
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_CreateMissionTiles_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World), "CreateWorld")]
        class World_CreateWorld_Patch
        {
            static bool Prefix(bool legacy = false)
            {
                try
                {
                    // push values
                    IslandsCount = IslandsCountBuffer;
                    IslandSize = IslandSizeBuffer;
                    ZoneSpacing = ZoneSpacingBuffer;
                    // persist values
                    WriteConfig();

                    Main._zoneLoadDistance = IslandSize - 6;
                    Debug.Log("Stranded Wide (Harmony edition) : CreateWorld _zoneLoadDistance = " + _zoneLoadDistance);
                    Main._zoneUnloadDistance = _zoneLoadDistance - 10;
                    Debug.Log("Stranded Wide (Harmony edition) : CreateWorld _zoneUnloadDistance = " + _zoneUnloadDistance);

#warning create world async ?

                    World.CreateWorldZonePoints(StrandedWorld.WORLD_SEED);
                    //World.MapList = new Map[Main.IslandsCount];
                    Debug.Log("Stranded Wide (Harmony edition) : CreateWorld island count (without NML) : " + (Main.IslandsCount));
                    pi_mapList.SetValue(null, new Map[Main.IslandsCount]);
                    Debug.Log("Stranded Wide (Harmony edition) : CreateWorld world bundle size : " + World.MapList.Length);
                    if (legacy)
                    {
                        for (int i = 0; i < World.GenerationZonePositons.Length; i++)
                        {
                            int seed = i;
                            //Map map = World.CreateProceduralMap(i, seed);
                            Debug.Log("Stranded Wide (Harmony edition) : creating procedural island (legacy loop) n°" + i + " " + DateTime.Now);
                            Map map = mi_World_CreateProceduralMap.Invoke(null, new object[] { i, seed }) as Map;
                            World.MapList[i] = map;
                        }

                        return false;
                    }

#warning world randomizer

                    LoadingResult lr = new LoadingResult();
                    Maps.LoadDefaultMaps(out lr);
                    Maps.LoadLocalMaps(out lr);
                    try
                    {
                        PropertyInfo pi_Instance = typeof(Beam.AccountServices.SteamWorkshop).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        if (pi_Instance == null)
                        {
                            Debug.Log("Stranded Wide (Harmony edition) : Steam Workshop not found, skipping remote maps");
                        }
                        else
                        {
                            Debug.Log("Stranded Wide (Harmony edition) : Steam Workshop found, loading remote maps");
                            //lr = DynamicSteamMapsLoad(lr);
                            MethodInfo mi_DynamicSteamMapsLoad = typeof(Main).GetMethod("DynamicSteamMapsLoad", BindingFlags.Public | BindingFlags.Static);
                            if (mi_DynamicSteamMapsLoad != null)
                            {
                                lr = (LoadingResult)mi_DynamicSteamMapsLoad.Invoke(null, new object[] { lr });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Stranded Wide (Harmony edition) : Steam Workshop not found, skipping remote maps");
                    }

#warning end randomizer

                    //int[] array = World.CreateMissionTiles();
                    int[] array = mi_World_CreateMissionTiles.Invoke(null, new object[] { }) as int[];
                    //int num = 0;
                    num = 0;

#warning world randomizer
                    if (Maps.DefaultMapList.Count == 0)
                    {
                        LoadingResult loadingResult;
                        Maps.LoadDefaultMaps(out loadingResult);
                        bool succeeded = loadingResult.Succeeded;
                    }

                    FastRandom frIslands = new FastRandom(StrandedWorld.WORLD_SEED);
                    for (int i = 0; i < Maps.AllMapList.Count(); i++)
                    {
                        // fuzzy bug with duplicate missions
                        Map map = Maps.AllMapList.ElementAt(i);
                        if (!map.EditorData.Author.Contains("Beam Team") && SaveManager.ValidateVersionNumber(new Version(map.EditorData.GameVersionNumber)))
                        {
                            availableIslands.Add(i);
                        }
                        else
                        {
                            Debug.Log("Stranded Wide (Harmony edition) : world randomizer skipping map " + map.EditorData.Name + " by " + map.EditorData.Author);
                        }
                    }
#warning end randomizer

                    for (int j = 0; j < Main.IslandsCount; j++)
                    {
                        float density = (Main.customIslandsRatio * Main.IslandsCount) / 100f;

                        //Debug.Log("Stranded Wide (Harmony edition) : trying to start create world coroutine");
                        //DialogueBoxViewAdapter db = MainMenuPresenter.FindObjectOfType<DialogueBoxViewAdapter>();

#warning world randomizer
                        int availableIslandIndex = -1;
                        int customIslandIndex = -1;
                        if (availableIslands.Count > 0)
                        {
                            availableIslandIndex = frIslands.Next(0, availableIslands.Count);
                            customIslandIndex = availableIslands[availableIslandIndex];
                            availableIslands.RemoveAt(availableIslandIndex);
                        }
                        bool addCustomIsland = (frIslands.Next(0, 1000) <= density * 1000);
                        CreateIsland(j, array, Maps.AllMapList, mixProceduralAndCustom && Maps.AllMapList.Count() > 0 && customIslandIndex > 0 && addCustomIsland, customIslandIndex);
#warning end randomizer
                        //MainMenuPresenter.Instance.StartCoroutine(CreateIsland(j, array, db));
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_CreateWorld_Patch : " + ex);
                }
                return false;
            }

            public static LoadingResult DynamicSteamMapsLoad(LoadingResult lr)
            {
                if (Beam.AccountServices.SteamWorkshop.Instance != null)
                {
                    Beam.AccountServices.SteamWorkshop.Instance.RequestUserContent();
                    FieldInfo fi_userContent = typeof(Beam.AccountServices.SteamWorkshop).GetField("m_content", BindingFlags.Instance | BindingFlags.NonPublic);
                    List<Beam.AccountServices.LoadedContent> steamContent = fi_userContent.GetValue(Beam.AccountServices.SteamWorkshop.Instance) as List<Beam.AccountServices.LoadedContent>;

                    IEnumerable<string> remoteMapPaths = (from c in steamContent
                                                          where c.Tags.Any_NonAlloc((string tag) => tag.Equals(Beam.AccountServices.UgcTags.MAPS))
                                                          select c).SelectMany((Beam.AccountServices.LoadedContent c) => Directory.GetDirectories(c.Path));
                    Maps.LoadRemoteMaps(remoteMapPaths, out lr);
                }

                return lr;
            }
        }

        static List<int> availableIslands = new List<int>();
        static int num = 0;

        static void CreateIsland(int j, int[] array, IEnumerable<Map> maps = null, bool isCustomIsland = false, int customIslandIndex = 0)
        {
            bool flag = false;
            for (int k = 0; k < 4; k++)
            {
                if (j == array[k])
                {
                    flag = true;
                    break;
                }
            }
            if (flag && num < Maps.DefaultMapList.Count)
            {
                Map map2 = Maps.DefaultMapList[num];
                World.MapList[j] = map2;
                map2.EditorData.SmallPreviewImage = (Resources.Load("Icons/WorldMap_Icon_Mission") as Texture2D);
                num++;
            }
            else
            {
                if (!isCustomIsland)
                {
                    int seed2 = j;
                    //Map map3 = World.CreateProceduralMap(j, seed2);
                    Debug.Log("Stranded Wide (Harmony edition) : creating procedural island (vanilla loop) n°" + j + " " + DateTime.Now);
                    Map map3 = mi_World_CreateProceduralMap.Invoke(null, new object[] { j, seed2 }) as Map;
                    World.MapList[j] = map3;
                }
                else if (maps != null && customIslandIndex < maps.Count())
                {
                    Map customMap = maps.ElementAt(customIslandIndex);
                    Debug.Log("Stranded Wide (Harmony edition) : adding custom island (vanilla loop) n°" + j + " " + DateTime.Now);
                    // upscale island if necessary

                    World.MapList[j] = customMap;
                }
            }
        }

        static System.Collections.IEnumerator CreateIsland(int j, int[] array, DialogueBoxViewAdapter db)
        {
            CreateIsland(j, array);

            db.Title.Text = "Island " + j;

            yield break;
        }

        [HarmonyPatch(typeof(World), "SaveWorldMaps")]
        class World_SaveWorldMaps_Patch
        {
            static bool Prefix(ref LoadingResult __result)
            {
                try
                {
                    LoadingResult result;
                    try
                    {
                        // cleaning the world correctly
                        foreach(string existingDirectory in Directory.GetDirectories(FilePath.WORLD_FOLDER))
                        {
                            if (Directory.Exists(existingDirectory))
                            {
                                Directory.Delete(existingDirectory, true);
                            }
                        }

                        for (int i = 0; i < Main.IslandsCount; i++)
                        {
                            string text = PathTools.Combine(new string[]
                            {
                                FilePath.WORLD_FOLDER,
                                i.ToString()
                            });
                            if (Directory.Exists(text))
                            {
                                Directory.Delete(text, true);
                            }
                            Directory.CreateDirectory(text);
                            Maps.DeleteMapFolder(Directory.GetDirectories(text));
                            Maps.SaveMap(World.MapList[i], text);
                        }
                        result = new LoadingResult
                        {
                            Message = "WORLD_MSG_SUCCESS",
                            Succeeded = true
                        };
                    }
                    catch (Exception ex)
                    {
                        LoadingResult loadingResult = new LoadingResult();
                        loadingResult.Message = "WORLD_MSG_ERROR_SAVING_WORLD " + ex.Message;
                        loadingResult.Succeeded = false;
                        Debug.LogError("World::SaveWorldMaps:: " + ex.Message);
                        result = loadingResult;
                    }
                    __result = result;
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_SaveWorldMaps_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World), "LoadWorldMaps")]
        class World_LoadWorldMaps_Patch
        {
            static bool Prefix(ref LoadingResult __result, bool legacy = false)
            {
                try
                {
                    LoadingResult loadingResult = new LoadingResult
                    {
                        Succeeded = true
                    };
                    LoadingResult loadingResult2;
                    try
                    {
                        //int num = legacy ? 5 : 7;
                        int islandCount = legacy ? 5 * 5 : Main.IslandsCount;
                        //World.MapList = new Map[islandCount + 1];
                        pi_mapList.SetValue(null, new Map[Main.IslandsCount]);
                        for (int i = 0; i < islandCount; i++)
                        {
                            Debug.Log("Stranded Wide (Harmony edition) : LoadWorldMaps loading zone n°" + i + " " + DateTime.Now);

                            //string worldMapFolderName = World.GetWorldMapFolderName(i, legacy);
                            string worldMapFolderName = mi_World_GetWorldMapFolderName.Invoke(null, new object[] { i, legacy }) as string;
                            string text = PathTools.Combine(new string[]
                            {
                                FilePath.WORLD_FOLDER,
                                worldMapFolderName
                            });
                            string[] directories = Directory.GetDirectories(text);
                            for (int j = 0; j < directories.Length; j++)
                            {
                                string fileName = Path.GetFileName(directories[j]);
                                string mapPath = PathTools.Combine(new string[]
                                {
                                    text,
                                    fileName
                                });
                                try
                                {
                                    Map map = Maps.LoadMap(mapPath, fileName, true);
                                    World.MapList[i] = map;
                                }
                                catch (VersionException ex)
                                {
                                    loadingResult.Message = "WORLD_MSG_MAP_VERSION_EXCEPTION";
                                    loadingResult.Succeeded = false;
                                    Debug.LogError("World::LoadWorldMaps:: " + ex.Message);
                                    goto IL_141;
                                }
                                catch (Exception ex2)
                                {
                                    loadingResult2 = loadingResult;
                                    loadingResult2.Message = string.Concat(new string[]
                                    {
                                        loadingResult2.Message,
                                        "WORLD_MSG_ERROR_LOADING_MAP ",
                                        fileName,
                                        "\n\n",
                                        ex2.Message
                                    });
                                    loadingResult.Succeeded = false;
                                    Debug.LogError("World::LoadWorldMaps:: " + ex2.Message);
                                }
                            }
                        }
                    IL_141:
                        if (loadingResult.Succeeded)
                        {
                            loadingResult.Message = "WORLD_MSG_SUCCESS";
                            Debug.Log("World::LoadWorldMaps:: Success.");
                        }
                        loadingResult2 = loadingResult;
                    }
                    catch (Exception ex3)
                    {
                        loadingResult.Message = "WORLD_MSG_ERROR_LOADING_WORLD_MAPS " + ex3.Message;
                        loadingResult.Succeeded = false;
                        Debug.LogError("World::LoadWorldMaps:: " + ex3.Message);
                        loadingResult2 = loadingResult;
                    }
                    __result = loadingResult2;
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_LoadWorldMaps_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World.Bundle), "Clear")]
        class WorldBundle_Clear_Patch
        {
            static bool Prefix(World.Bundle __instance, ref World.Bundle __result)
            {
                try
                {
                    __instance.Maps = new Map[Main.IslandsCount];
                    __instance.GenerationZonePositons = (__instance.GeneratedZonePoints = null);
                    __result = __instance;
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching WorldBundle_Clear_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World.Bundle), "Save")]
        class WorldBundle_Save_Patch
        {
            static bool Prefix(World.Bundle __instance, Stream stream, byte[] header = null)
            {
                try
                {
                    using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        if (header != null && header.Length != 0)
                        {
                            gzipStream.Write(header, 0, header.Length);
                        }
                        using (BinaryWriter binaryWriter = new BinaryWriter(gzipStream))
                        {
                            for (int i = 0; i < Main.IslandsCount; i++)
                            {
                                Map map = __instance.Maps[i];
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    using (MemoryStream memoryStream2 = new MemoryStream())
                                    {
                                        using (MemoryStream memoryStream3 = new MemoryStream())
                                        {
                                            map.Save(memoryStream, memoryStream2, memoryStream3);
                                            byte[] array = memoryStream.ToArray();
                                            byte[] array2 = memoryStream2.ToArray();
                                            byte[] array3 = memoryStream3.ToArray();
                                            binaryWriter.Write(array.Length);
                                            binaryWriter.Write(array2.Length);
                                            binaryWriter.Write(array3.Length);
                                            binaryWriter.Write(array);
                                            binaryWriter.Write(array2);
                                            binaryWriter.Write(array3);
                                        }
                                    }
                                }
                            }
                            for (int j = 0; j < Main.IslandsCount; j++)
                            {
                                binaryWriter.Write(__instance.GenerationZonePositons[j]);
                            }
                            binaryWriter.Write((ushort)__instance.GeneratedZonePoints.Length);
                            for (int k = 0; k < __instance.GeneratedZonePoints.Length; k++)
                            {
                                binaryWriter.Write(__instance.GeneratedZonePoints[k]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching WorldBundle_Save_Patch : " + ex);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World.Bundle), "Load")]
        class WorldBundle_Load_Patch
        {
            static bool Prefix(World.Bundle __instance, Stream stream, bool createTextures, byte[] header = null)
            {
                try
                {
                    using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
                    {
                        if (header != null && header.Length != 0)
                        {
                            gzipStream.Read(header, 0, header.Length);
                        }
                        using (BinaryReader binaryReader = new BinaryReader(gzipStream))
                        {
                            __instance.Maps = new Map[Main.IslandsCount];
                            for (int i = 0; i < Main.IslandsCount; i++)
                            {
                                Map map = new Map();
                                int num = binaryReader.ReadInt32();
                                int num2 = binaryReader.ReadInt32();
                                int num3 = binaryReader.ReadInt32();
                                byte[] buffer = new byte[num];
                                byte[] buffer2 = new byte[num2];
                                byte[] buffer3 = new byte[num3];
                                binaryReader.Read(buffer, 0, num);
                                binaryReader.Read(buffer2, 0, num2);
                                binaryReader.Read(buffer3, 0, num3);
                                using (MemoryStream memoryStream = new MemoryStream(buffer, 0, num))
                                {
                                    using (MemoryStream memoryStream2 = new MemoryStream(buffer2, 0, num2))
                                    {
                                        using (MemoryStream memoryStream3 = new MemoryStream(buffer3, 0, num3))
                                        {
                                            map.Load(memoryStream, memoryStream2, memoryStream3, false, createTextures);
                                        }
                                    }
                                }
                                __instance.Maps[i] = map;
                            }
                            __instance.GenerationZonePositons = new Vector2[Main.IslandsCount];
                            for (int j = 0; j < Main.IslandsCount; j++)
                            {
                                __instance.GenerationZonePositons[j] = binaryReader.ReadVector2();
                            }
                            ushort num4 = binaryReader.ReadUInt16();
                            __instance.GeneratedZonePoints = new Vector2[(int)num4];
                            for (int k = 0; k < (int)num4; k++)
                            {
                                __instance.GeneratedZonePoints[k] = binaryReader.ReadVector2();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching WorldBundle_Load_Patch : " + ex);
                }
                return false;
            }
        }

        internal static MethodInfo mi_GetWorldMapFolderName = typeof(World).GetMethod("GetWorldMapFolderName", BindingFlags.Static | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(World), "ValidateWorldFolders")]
        class World_ValidateWorldFolders_Patch
        {
            static bool Prefix(ref LoadingResult __result, bool legacy = false)
            {
                try
                {
                    if (!File.Exists(PathTools.Combine(new string[]
                    {
                        FilePath.WORLD_FOLDER,
                        "Seed.sdd"
                    })))
                    {
                        __result = new LoadingResult
                        {
                            Message = "WORLD_MSG_SEED_FILE_MISSING",
                            Succeeded = false
                        };
                        return false;
                    }
                    try
                    {
                        if (Directory.GetDirectories(FilePath.WORLD_FOLDER).Length == 0)
                        {
                            __result = new LoadingResult
                            {
                                Message = "WORLD_MSG_EMPTY_WORLD_FOLDER",
                                Succeeded = false
                            };
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        __result = new LoadingResult
                        {
                            Message = ex.Message,
                            Succeeded = false
                        };
                        return false;
                    }
                    try
                    {
                        //int num = legacy ? 5 : 7;
                        int islandCount = legacy ? 5 * 5 : Main.IslandsCount;
                        for (int i = 0; i < islandCount; i++)
                        {
                            //string worldMapFolderName = World.GetWorldMapFolderName(i, legacy);
                            string worldMapFolderName = mi_GetWorldMapFolderName.Invoke(null, new object[] { i, legacy }) as string;
                            string text = PathTools.Combine(new string[]
                            {
                                FilePath.WORLD_FOLDER,
                                worldMapFolderName
                            });
                            if (!Directory.Exists(text))
                            {
                                __result = new LoadingResult
                                {
                                    Message = "WORLD_MSG_MISSING_WORLD_FOLDER",
                                    Succeeded = false
                                };
                                return false;
                            }
                            string[] directories = Directory.GetDirectories(text);
                            for (int j = 0; j < directories.Length; j++)
                            {
                                string fileName = Path.GetFileName(directories[j]);
                                if (!Maps.ValidateMapFolder(PathTools.Combine(new string[]
                                    {
                                        text,
                                        fileName
                                    }), fileName))
                                {
                                    __result = new LoadingResult
                                    {
                                        Message = "WORLD_MSG_MISSING_MAP_FOLDER " + fileName,
                                        Succeeded = false
                                    };
                                    return false;
                                }
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        __result = new LoadingResult
                        {
                            Message = ex2.Message,
                            Succeeded = false
                        };
                        return false;
                    }
                    __result = new LoadingResult
                    {
                        Succeeded = true
                    };

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World_ValidateWorldFolders_Patch : " + e);
                }
                return true;
            }
        }
    }
}
