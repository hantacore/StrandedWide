using Beam;
using Beam.Utilities;
using Beam.Serialization;
using Beam.Serialization.Json;
using Funlabs;
using Beam.Terrain;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(Maps), "Save")]
        class Maps_Save_Patch
        {
            static bool Prefix(Map map, Stream streamEditor, Stream streamObject, Stream streamHeight)
            {
                try
                {
                    if (!MPExtensions.IsCompressStableValuesInitialized())
                    {
                        MPExtensions.InitializeCompressStableValues();
                    }
                    try
                    {
                        new BinaryFormatter().Serialize(streamEditor, map.EditorData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Map::SaveMap:: Error saving map editor data: " + ex.Message);
                        throw;
                    }
                    try
                    {
                        using (StreamWriter streamWriter = new StreamWriter(streamObject))
                        {
                            streamWriter.Write(map.ObjectData.ToString());
                        }
                    }
                    catch (Exception ex2)
                    {
                        Debug.LogError("Map::SaveMap:: Error saving map object data: " + ex2.Message);
                        throw;
                    }
                    try
                    {
                        using (BinaryWriter binaryWriter = new BinaryWriter(streamHeight))
                        {
                            //convert standard map format
                            if (map.HeightmapData.GetLength(0) < IslandSize + 1)
                            {
                                for (int i = 0; i < IslandSize + 1; i++)
                                {
                                    for (int j = 0; j < IslandSize + 1; j++)
                                    {
                                        if (i >= map.HeightmapData.GetLength(0) || j >= map.HeightmapData.GetLength(0))
                                            binaryWriter.Write(map.HeightmapData[0, 0].Compress());
                                        else
                                            binaryWriter.Write(map.HeightmapData[i, j].Compress());
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < IslandSize + 1; i++)
                                {
                                    for (int j = 0; j < IslandSize + 1; j++)
                                    {
                                        binaryWriter.Write(map.HeightmapData[i, j].Compress());
                                    }
                                }
                            }
                            // end convert
                        }
                    }
                    catch (Exception ex3)
                    {
                        Debug.LogError("Map::SaveMap:: Error saving map heightmap data: " + ex3.Message);
                        throw;
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching Maps.Save : " + e);
                }
                return true;
            }
        }

        static MethodInfo mi_PatchObjectsReferenceId = AccessTools.Method(typeof(Maps), "PatchObjectsReferenceId");

        [HarmonyPatch(typeof(Maps), "Load")]
        class Maps_Load_Patch
        {
            static bool Prefix(Map map, Stream editorStream, Stream objectStream, Stream heightStream, bool checkVersion, bool createTextures = true)
            {
                try
                {
                    if (!MPExtensions.IsCompressStableValuesInitialized())
                    {
                        MPExtensions.InitializeCompressStableValues();
                    }
                    try
                    {
                        MapEditorData editorData = (MapEditorData)new BinaryFormatter().Deserialize(editorStream);
                        map.EditorData = editorData;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Map::LoadMap:: Error loading map editor data:\n\n" + ex.Message + "\n" + ex.StackTrace);
                        throw;
                    }
                    if (checkVersion)
                    {
                        try
                        {
                            if (!SaveManager.ValidateVersionNumber(new Version(map.EditorData.GameVersionNumber)))
                            {
                                throw new VersionException("Map [" + map.EditorData.Name + "] is out of date and no longer supported.");
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            throw new VersionException("Map [" + map.EditorData.Name + "] is out of date and no longer supported.");
                        }
                    }
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(objectStream))
                        {
                            try
                            {
                                JObject jobject = new JObject(streamReader.ReadToEnd());

                                mi_PatchObjectsReferenceId.Invoke(null, new object[] { jobject });
                                //Maps.PatchObjectsReferenceId(jobject);
                                map.ObjectData = jobject;
                            }
                            catch (JSerializerException ex2)
                            {
                                Debug.LogError(ex2.Message);
                                throw;
                            }
                        }
                    }
                    catch (Exception ex3)
                    {
                        Debug.LogError(string.Concat(new string[]
                        {
                    "Map::LoadMap:: Error loading map [",
                    map.EditorData.Name,
                    "] object data:\n\n",
                    ex3.Message,
                    "\n",
                    ex3.StackTrace
                        }));
                        throw;
                    }
                    try
                    {
                        using (BinaryReader binaryReader = new BinaryReader(heightStream))
                        {
                            float[,] array = null;

                            // handle the case of smaller islands ?
                            try
                            {
                                // original code
                                binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
                                array = new float[IslandSize + 1, IslandSize + 1];
                                for (int i = 0; i < IslandSize + 1; i++)
                                {
                                    for (int j = 0; j < IslandSize + 1; j++)
                                    {
                                        ushort value = binaryReader.ReadUInt16().ToCompressStable();
                                        array[i, j] = value.Decompress();
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // magic number for original island format
                                binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
                                array = new float[257, 257];
                                for (int i = 0; i < 257; i++)
                                {
                                    for (int j = 0; j < 257; j++)
                                    {
                                        ushort value = binaryReader.ReadUInt16().ToCompressStable();
                                        array[i, j] = value.Decompress();
                                    }
                                }
                            }
                            map.HeightmapData = array;
                        }
                    }
                    catch (Exception ex4)
                    {
                        Debug.LogError("Map::LoadMap:: Error loading map heightmap data:\n\n" + ex4.Message + "\n" + ex4.StackTrace);
                        throw;
                    }
                    if (createTextures)
                    {
                        map.CreateTextures();
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching Maps.Load : " + e);
                }
                return true;
            }
        }
    }
}
