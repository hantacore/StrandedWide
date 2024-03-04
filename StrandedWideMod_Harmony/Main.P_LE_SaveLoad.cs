using HarmonyLib;
using LE_LevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        static string mapsFilePath = "/CustomMaps/Original/";

        [HarmonyPatch(typeof(LE_SaveLoad), "SaveMapHeightMapFile")]
        class LE_SaveLoad_SaveMapHeightMapFile_Patch
        {
            static bool Prefix(float[,] heightmap, string mapName)
            {
                try
                {
                    throw new Exception("Unused, but could cause problems if suddenly used");

                    int mapSize = heightmap.GetLength(0); // maps are supposed to be square
                    Debug.Log("Stranded Wide (Harmony edition) : saving map with dimension " + mapSize + " x " + mapSize);

                    FileStream fileStream = new FileStream(Application.dataPath + mapsFilePath + mapName + "_heightmap.map", FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                    for (int i = 0; i < mapSize; i++)
                    {
                        for (int j = 0; j < mapSize; j++)
                        {
                            if (heightmap[i, j] > 1f)
                            {
                                heightmap[i, j] = 1f;
                            }
                            if (heightmap[i, j] < 0f)
                            {
                                heightmap[i, j] = 0f;
                            }
                            ushort value = (ushort)(heightmap[i, j] * 65535f);
                            binaryWriter.Write(value);
                        }
                    }
                    fileStream.Close();
                    binaryWriter.Close();

                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_SaveLoad_SaveMapHeightMapFile_Patch : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_SaveLoad), "LoadMapHeightMapFile")]
        class LE_SaveLoad_LoadMapHeightMapFile_Patch
        {
            static bool Prefix(string mapName, ref float[,] __result)
            {
                try
                {
                    throw new Exception("Unused, but could cause problems if suddenly used");

                    FileStream fileStream = new FileStream(Application.dataPath + mapsFilePath + mapName + "_heightmap.map", FileMode.Open, FileAccess.ReadWrite);

                    Debug.Log("Stranded Wide (Harmony edition) : LoadMapHeightMapFile filestream length = " + fileStream.Length);

                    int mapSize = 257;
                    Debug.Log("Stranded Wide (Harmony edition) : loading map with dimension " + mapSize + " x " + mapSize);

                    float[,] array = new float[mapSize, mapSize];
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
                    for (int i = 0; i < mapSize; i++)
                    {
                        for (int j = 0; j < mapSize; j++)
                        {
                            array[i, j] = (float)binaryReader.ReadUInt16() / 65535f;
                        }
                    }
                    fileStream.Close();
                    binaryReader.Close();
                    __result = array;

                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_SaveLoad_LoadMapHeightMapFile_Patch : " + e);
                }
                return true;
            }
        }
    }
}
