using Beam.Terrain;
using Beam.Utilities;
using HarmonyLib;
using SharpNeatLib.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        static PropertyInfo pi_generatedZonePoints = AccessTools.Property(typeof(World), "_generatedZonePoints");
        static PropertyInfo pi_generationZonePositons = AccessTools.Property(typeof(World), "_generationZonePositons");

        //static FieldInfo fi_worlds = typeof(World).GetField("_worlds", BindingFlags.NonPublic | BindingFlags.Static);//AccessTools.Field(typeof(World), "_worlds");
        //static FieldInfo fi_currentWorld = typeof(World).GetField("_currentWorld", BindingFlags.NonPublic | BindingFlags.Static);//AccessTools.Field(typeof(World), "_currentWorld");

        [HarmonyPatch(typeof(World), "CreateWorldZonePoints")]
        class World_CreateWorldZonePoints_Patch
        {
            static bool Prefix(int seed)
            {
                try
                {
                    Debug.LogError("Stranded Wide (Harmony edition) : CreateWorldZonePoints " + seed + " / zone size " + ZoneSize + " / spacing " + ZoneSpacing + " / count " + IslandsCount);

                    int num = IslandsCount;
                    float islandZoneSize = ZoneSize;
                    float fullMapSize = islandZoneSize * ZoneSpacing * 7f;
                    int numSamplesBeforeRejection = 30;

                    Vector2[] generatedPoints = ZonePositionGenerator.GeneratePoints(seed, islandZoneSize, new Vector2(fullMapSize, fullMapSize), numSamplesBeforeRejection);
                    //World._generatedZonePoints = generatedPoints;
                    pi_generatedZonePoints.SetValue(null, generatedPoints);
                    //if (!System.Object.ReferenceEquals(generatedPoints, World.GeneratedZonePoints))
                    //{
                    //    Debug.LogError("Stranded Wide (Harmony edition) : _generatedZonePoints instances are different");
                    //}
                    Debug.LogError("Stranded Wide (Harmony edition) : World.GeneratedZonePoints.Length : " + World.GeneratedZonePoints.Length);

                    Vector2[] generatedPositions = new Vector2[num];
                    //World._generationZonePositons = generatedPositions;
                    pi_generationZonePositons.SetValue(null, generatedPositions);
                    //if (!System.Object.ReferenceEquals(generatedPositions, World.GenerationZonePositons))
                    //{
                    //    Debug.LogError("Stranded Wide (Harmony edition) : _generationZonePositons instances are different");
                    //}
                    Debug.LogError("Stranded Wide (Harmony edition) : World.GenerationZonePositons.Length : " + World.GenerationZonePositons.Length);

                    FastRandom fastRandom = new FastRandom(seed);
                    List<int> list = new List<int>();
                    int upperBound = generatedPoints.Length;
                    Debug.LogError("Stranded Wide (Harmony edition) : start picking positions");
                    for (int i = 0; i < num; i++)
                    {
                        int num4 = fastRandom.Next(0, upperBound);
                        while (list.Contains(num4))
                        {
                            num4 = fastRandom.Next(0, upperBound);
                        }
                        if (i == 0)
                        {
                            num4 = 0;
                        }
                        list.Add(num4);
                        //Debug.Log("Stranded Wide (Harmony edition) : adding generated position " + i);
                        generatedPositions[i] = generatedPoints[num4];
                        //World.GenerationZonePositons[i] = World.GeneratedZonePoints[num4];
                    }
                    Debug.LogError("Stranded Wide (Harmony edition) : end picking positions");

                    // should never happen !
                    if (generatedPositions.Length < num - 1)
                    {
                        Debug.LogError("Error: Not enough island positions");
                    }

                    pi_generationZonePositons.SetValue(null, generatedPositions);
                    //if (!System.Object.ReferenceEquals(generatedPositions, World.GenerationZonePositons))
                    //{
                    //    Debug.LogError("Stranded Wide (Harmony edition) : _generationZonePositons instances are different");
                    //}

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World.CreateWorldZonePoints : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(World), "LoadWorldMaps")]
        class World_LoadWorldMaps_PostFix_Patch
        {
            static void Postfix(ref LoadingResult __result, bool legacy)
            {
                try
                {
                    Main.IncreaseObjectsNumberForProceduralGeneration();
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World.LoadWorldMaps PostFix : " + e);
                }
            }
        }

        [HarmonyPatch(typeof(World), "ImportHeightmapFromRawFile")]
        class World_ImportHeightmapFromRawFile_Patch
        {
            static bool Prefix(string path, ref float[,] __result)
            {
                try
                {
                    float[,] array = new float[IslandSize + 1, IslandSize + 1];
                    int num = 66049;
                    num *= 2;
                    byte[] array2 = new byte[num];
                    try
                    {
                        using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            try
                            {
                                fileStream.Read(array2, 0, num);
                            }
                            catch (ArgumentException)
                            {
                                throw new Exception("Error raw heightmap import.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("World::ImportHeightmapFromRawFile::\n\n" + ex.Message);
                        __result = array;
                        return false;
                    }
                    int num2 = 0;
                    for (int i = 0; i < IslandSize + 1; i++)
                    {
                        for (int j = 0; j < IslandSize + 1; j++)
                        {
                            float num3 = (float)array2[num2++] + (float)array2[num2++] * IslandSize;
                            array[IslandSize + 1 - i - 1, j] = num3 / 65535f;
                        }
                    }
                    __result = array;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching World.ImportHeightmapFromRawFile : " + e);
                }
                return true;
            }
        }
    }
}
