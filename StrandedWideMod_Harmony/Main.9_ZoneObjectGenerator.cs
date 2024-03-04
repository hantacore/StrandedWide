using Beam.Terrain;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(ZoneObjectGenerator), "GenerateGridData")]
        class ZoneObjectGenerator_GenerateGridData_Patch
        {
            static bool Prefix(Zone zone, int seed, ref GenerationPoints[,] __result)
            {
                try
                {
                    float num = 0.9765625f;
                    GenerationPoints[,] array = new GenerationPoints[IslandSize, IslandSize];
                    UnityEngine.Terrain terrain = zone.Terrain;
                    TerrainData terrainData = zone.Terrain.terrainData;
                    float[,] heights = terrainData.GetHeights(0, 0, IslandSize + 1, IslandSize + 1);
                    float[,] array2 = WorldTools.GENERATE_ISLAND_HEIGHTMAP(StrandedWorld.WORLD_SEED - seed, zone.Biome, true);
                    for (int i = 0; i < IslandSize; i++)
                    {
                        for (int j = 0; j < IslandSize; j++)
                        {
                            float y = 150f * heights[i, j] + -100f;
                            Vector3 vector = new Vector3(terrain.transform.position.x + num * (float)j, y, terrain.transform.position.z + num * (float)i);
                            float x = (vector.x - terrain.transform.position.x) / ZoneHalfSize;
                            float y2 = (vector.z - terrain.transform.position.z) / ZoneHalfSize;
                            Quaternion rot = Quaternion.LookRotation(Vector3.forward, terrainData.GetInterpolatedNormal(x, y2));
                            float steepness = terrainData.GetSteepness(x, y2);
                            array[i, j] = new GenerationPoints(vector, rot, false, steepness, array2[i, j]);
                        }
                    }
                    __result = array;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneObjectGenerator.GenerateGridData : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneObjectGenerator), "DebugSoilMap")]
        class ZoneObjectGenerator_DebugSoilMap_Patch
        {
            static bool Prefix(Zone zone, float[,] soilMap)
            {
                try
                {
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    gameObject.gameObject.name = "Debug_SoilMap";
                    gameObject.transform.parent = zone.transform;
                    gameObject.transform.position = zone.transform.position + Vector3.up * 50f;
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
                    gameObject.transform.localScale = new Vector3(-25f, 1f, 25f);
                    Texture2D texture2D = IslandDebugTexture.RenderSoilMapImageIcon(soilMap, IslandSize + 1);
                    texture2D.wrapMode = TextureWrapMode.Clamp;
                    texture2D.Apply();
                    gameObject.GetComponent<Renderer>().material.mainTexture = texture2D;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneObjectGenerator.DebugSoilMap : " + e);
                }
                return true;
            }
        }
    }
}
