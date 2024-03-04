using Beam;
using Beam.Terrain;
using Ceto;
using HarmonyLib;
using LE_LevelEditor;
using LE_LevelEditor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        static MethodInfo mi_AddWaveOverlay = AccessTools.Method(typeof(LE_ZoneTileGenerator), "AddWaveOverlay");

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "GenerateShoreMap")]
        class LE_ZoneTileGenerator_GenerateShoreMap_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance)
            {
                try
                {
                    if (Singleton<LE_LevelEditorMain>.Instance.EditMode != LE_EEditMode.TERRAIN)
                    {
                        return false;
                    }
#warning ocean textures
                    Texture2D height = Main.Blur(WorldTools.CreateHeightMapTexture(__instance.terrain.terrainData), 2);
                    Texture2D shore = Main.Blur(WorldTools.CreateShoreMaskTexture(__instance.terrain.terrainData), 2);
                    //__instance.AddWaveOverlay(__instance.terrain.gameObject, height, shore);
                    mi_AddWaveOverlay.Invoke(__instance, new object[] { __instance.terrain.gameObject, height, shore });
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.GenerateShoreMap : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "GenerateRandomIsland")]
        class LE_ZoneTileGenerator_GenerateRandomIsland_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance)
            {
                try
                {
                    Debug.LogError("LE_ZoneTileGenerator::GenerateRandomIsland terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);
                    __instance.terrain.terrainData.heightmapResolution = IslandSize + 1;
                    Debug.LogError("LE_ZoneTileGenerator::GenerateRandomIsland new terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);

                    // skip original method
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.GenerateRandomIsland : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "GenerateAdvancedPerlin")]
        class LE_ZoneTileGenerator_GenerateAdvancedPerlin_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance, int freq, float amplitude, int octaves, float blend)
            {
                try
                {
                    Debug.LogError("LE_ZoneTileGenerator::GenerateAdvancedPerlin terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);
                    __instance.terrain.terrainData.heightmapResolution = IslandSize + 1;
                    Debug.LogError("LE_ZoneTileGenerator::GenerateAdvancedPerlin new terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);

                    //this.terrain.terrainData.SetHeights(0, 0, WorldTools.PerlinGenerator(this.terrain.terrainData.GetHeights(0, 0, _islandSize+1, _islandSize+1), UnityEngine.Random.Range(1, 99999999), freq, amplitude, octaves, blend));
                    __instance.terrain.terrainData.SetHeights(0, 0, Main.StitchTerrainEdges(WorldTools.PerlinGenerator(__instance.terrain.terrainData.GetHeights(0, 0, IslandSize + 1, IslandSize + 1), UnityEngine.Random.Range(1, 99999999), freq, amplitude, octaves, blend)));

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.GenerateAdvancedPerlin : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "GenerateAdvancedNormalise")]
        class LE_ZoneTileGenerator_GenerateAdvancedNormalise_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance, float min, float max, float blend)
            {
                try
                {
                    Debug.LogError("LE_ZoneTileGenerator::GenerateAdvancedNormalise terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);
                    __instance.terrain.terrainData.heightmapResolution = IslandSize + 1;
                    Debug.LogError("LE_ZoneTileGenerator::GenerateAdvancedNormalise new LE_ZoneTileGenerator.terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);

                    __instance.terrain.terrainData.SetHeights(0, 0, WorldTools.NormalizeHeightmap(__instance.terrain.terrainData.GetHeights(0, 0, IslandSize + 1, IslandSize + 1), min, max, blend, Zone.BiomeType.RANDOM));

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.GenerateAdvancedNormalise : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "GenerateSmooth")]
        class LE_ZoneTileGenerator_GenerateSmooth_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance)
            {
                try
                {
                    Debug.LogError("LE_ZoneTileGenerator::GenerateSmooth terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);
                    __instance.terrain.terrainData.heightmapResolution = IslandSize + 1;
                    Debug.LogError("LE_ZoneTileGenerator::GenerateSmooth new terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);

                    __instance.terrain.terrainData.SetHeights(0, 0, WorldTools.SmoothTerrain(__instance.terrain.terrainData.GetHeights(0, 0, IslandSize + 1, IslandSize + 1), _iterations, _blend, Zone.BiomeType.ISLAND));

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.GenerateSmooth : " + e);
                }
                return true;
            }
        }

        static FieldInfo fi_waveOverlay = AccessTools.Field(typeof(LE_ZoneTileGenerator), "_waveOverlay");

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "AddWaveOverlay")]
        class LE_ZoneTileGenerator_AddWaveOverlay_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance, GameObject zone, Texture2D height, Texture2D shore)
            {
                try
                {
                    AddWaveOverlay waveOverlay = fi_waveOverlay.GetValue(__instance) as AddWaveOverlay;

                    if (waveOverlay != null)
                    {
                        UnityEngine.Object.Destroy(waveOverlay.heightTexture.tex);
                        UnityEngine.Object.Destroy(waveOverlay.heightTexture.mask);
                        UnityEngine.Object.Destroy(waveOverlay.normalTexture.tex);
                        UnityEngine.Object.Destroy(waveOverlay.normalTexture.mask);
                        UnityEngine.Object.Destroy(waveOverlay.foamTexture.tex);
                        UnityEngine.Object.Destroy(waveOverlay.foamTexture.mask);
                        UnityEngine.Object.Destroy(waveOverlay.gameObject);
                    }
                    AddWaveOverlay newWaveOverlay = new GameObject("WaveOverlay")
                    {
                        transform =
                        {
                            parent = zone.transform,
                            position = new Vector3(WaveOverlayPosition, 0, WaveOverlayPosition)//new Vector3(0, 0, 0)
                            //position = new Vector3(0, 0, 0)
                        }
                    }.AddComponent<AddWaveOverlay>();
                    newWaveOverlay.width = ZoneTerrainSize;
                    newWaveOverlay.height = ZoneTerrainSize;
                    newWaveOverlay.heightTexture = new OverlayHeightTexture();
                    newWaveOverlay.heightTexture.tex = shore;
                    newWaveOverlay.heightTexture.mask = height;
                    newWaveOverlay.heightTexture.alpha = 0f;
                    newWaveOverlay.heightTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
                    newWaveOverlay.normalTexture = new OverlayNormalTexture();
                    newWaveOverlay.normalTexture.tex = shore;
                    newWaveOverlay.normalTexture.mask = height;
                    newWaveOverlay.normalTexture.alpha = 0f;
                    newWaveOverlay.normalTexture.maskAlpha = 0.4f;
                    newWaveOverlay.normalTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
                    newWaveOverlay.foamTexture = new OverlayFoamTexture();
                    newWaveOverlay.foamTexture.tex = shore;
                    newWaveOverlay.foamTexture.mask = height;
                    newWaveOverlay.foamTexture.alpha = 0.4f;
                    newWaveOverlay.foamTexture.maskMode = OVERLAY_MASK_MODE.WAVES;

                    fi_waveOverlay.SetValue(__instance, newWaveOverlay);
                    // play original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.AddWaveOverlay : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_ZoneTileGenerator), "LoadMap")]
        class LE_ZoneTileGenerator_LoadMap_Patch
        {
            static bool Prefix(LE_ZoneTileGenerator __instance, Map map)
            {
                try
                {
                    Singleton<LE_LevelEditorMain>.Instance.ClearLevel();

                    //Debug.LogError("LE_ZoneTileGenerator::LoadMap terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);
                    __instance.terrain.terrainData.heightmapResolution = IslandSize + 1;
                    //Debug.LogError("LE_ZoneTileGenerator::LoadMap new terrain.terrainData.heightmapResolution : " + __instance.terrain.terrainData.heightmapResolution);
                    __instance.terrain.terrainData.SetHeights(0, 0, map.HeightmapData);
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_ZoneTileGenerator.LoadMap : " + e);
                }
                return true;
            }
        }
    }
}
