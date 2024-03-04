using Beam;
using Beam.Terrain;
using Beam.UI;
using Beam.Utilities;
using Ceto;
using HarmonyLib;
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
        [HarmonyPatch(typeof(StrandedWorld), "LoadWorld", new Type[] { })]
        class StrandedWorld_LoadWorld_Patch
        {
            static bool Prefix()
            {
                try
                {
                    Main._zoneLoadDistance = IslandSize - 6;
                    Debug.Log("Stranded Wide (Harmony edition) : LoadWorld _zoneLoadDistance = " + _zoneLoadDistance);
                    Main._zoneUnloadDistance = _zoneLoadDistance - 10;
                    Debug.Log("Stranded Wide (Harmony edition) : LoadWorld _zoneUnloadDistance = " + _zoneUnloadDistance);

                    Beam.Terrain.LoadingResult loadingResult = World.LoadWorld();
                    if (loadingResult.Succeeded)
                    {
                        StrandedWorld.Instance.ApplyWorldToZones();
                        RemoveWorldBarrier();
                        return false;
                    }
                    Debug.LogError("StrandedWorld:: Error loading world: " + loadingResult.Message);

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld.LoadWorld : " + e);
                }
                return true;
            }
        }

        static MethodInfo mi_LoadZonePositions = AccessTools.Method(typeof(StrandedWorld), "LoadZonePositions");
        static MethodInfo mi_StrandedWorldAddWaveOverlay = AccessTools.Method(typeof(StrandedWorld), "AddWaveOverlay");

        [HarmonyPatch(typeof(StrandedWorld), "ApplyWorldToZones")]
        class StrandedWorld_ApplyWorldToZones_Patch
        {
            static bool Prefix(StrandedWorld __instance)
            {
                try
                {
#warning TODO async loading

                    //StrandedWorld.Instance.LoadZonePositions();
                    mi_LoadZonePositions.Invoke(StrandedWorld.Instance, new object[] { });
                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        Zone zone = StrandedWorld.Instance.Zones[i];
                        try
                        {
                            Map map = World.MapList[i];
                            Debug.LogError("StrandedWorld:: Loading zone (" + i + ") " + DateTime.Now);
                            //try
                            //{
                            //    Beam.UI.TMPTextScreenViewAdapter loadingscreen = Game.FindObjectOfType<Beam.UI.TMPTextScreenViewAdapter>();
                            //    if (loadingscreen != null)
                            //    {
                            //        loadingscreen.SetTooltipText("Loading zone (" + i + "/" + 48 + ")");
                            //    }
                            //}
                            //catch { }

                            //Debug.LogError("StrandedWorld:: zone.Terrain.terrainData.heightmapResolution : " + zone.Terrain.terrainData.heightmapResolution);
                            zone.Terrain.terrainData.heightmapResolution = IslandSize + 1;
                            //Debug.LogError("StrandedWorld:: new zone.Terrain.terrainData.heightmapResolution : " + zone.Terrain.terrainData.heightmapResolution);

                            zone.Terrain.terrainData.SetHeights(0, 0, map.HeightmapData);
                            zone.ZoneName = map.EditorData.Name;
                            zone.Id = map.EditorData.Id;
                            zone.Version = map.EditorData.VersionNumber;
                            zone.Biome = map.EditorData.Biome;
                            zone.IsMapEditor = map.IsCustomMap();
                            zone.IsUserMap = map.IsUserMap();
                            zone.Seed = i;
                            Texture2D height = Main.Blur(WorldTools.CreateHeightMapTexture(zone.Terrain.terrainData), 2);
                            //ExportTexture(height, "height" + i);
                            Texture2D shore = Main.Blur(WorldTools.CreateShoreMaskTexture(zone.Terrain.terrainData), 2);
                            //ExportTexture(shore, "shore" + i);
                            if (zone.Biome == Zone.BiomeType.ISLAND || zone.Biome == Zone.BiomeType.ISLAND_SMALL || zone.Biome == Zone.BiomeType.ISLAND_ROCK || zone.Biome == Zone.BiomeType.CARRIER)
                            {
#warning ocean textures
                                //zone.WaveOverlay = StrandedWorld.Instance.AddWaveOverlay(zone.gameObject, height, shore);
                                zone.WaveOverlay = (Ceto.AddWaveOverlay)mi_StrandedWorldAddWaveOverlay.Invoke(StrandedWorld.Instance, new object[] { zone.gameObject, height, shore });
                                //OLD DO NOT USE zone.WaveOverlay = this.AddWaveOverlay(zone.gameObject, CreateTransparentTexture(_islandSize), CreateTransparentTexture(_islandSize));
                            }
                            zone.SaveContainer.transform.SetAsLastSibling();

                            Debug.LogError("StrandedWorld:: TEST pre-creating zone (" + i + ") " + DateTime.Now);
                            __instance.ZoneLoader.GenerateZone(zone);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("StrandedWorld:: Error in ApplyWorldToZones (" + zone.Biome + ") : " + e.Message);
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogError("StrandedWorld:: Error in ApplyWorldToZones.ApplyWorldToZones : " + ex.Message);
                }

                return true;
            }
        }

#warning for debug
        static bool isPreLoading = false;
        [HarmonyPatch(typeof(StrandedWorld), "ZoneLoader_LoadedZone")]
        class StrandedWorld_ZoneLoader_LoadedZone_Patch
        {

            static void Postfix(Zone zone, StrandedWorld __instance)
            {
                try
                {
                    LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
                    localizedNotification.Priority = NotificationPriority.Immediate;
                    localizedNotification.Duration = 8f;
                    localizedNotification.TitleText.SetTerm("Zone " + (isPreLoading ? "pre" : "") + " loaded");
                    if (zone == StrandedWorld.Instance.NmlZone)
                    {
                        localizedNotification.MessageText.SetTerm("Zone NML loaded");
                    }
                    else
                    {
                        localizedNotification.MessageText.SetTerm("Zone " + zone.ZoneName + (isPreLoading ? "pre" : "") + " loaded");
                    }
                    localizedNotification.Raise();
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld.ZoneLoader_LoadedZone : " + e);
                }
            }
        }



#warning necessary
        [HarmonyPatch(typeof(StrandedWorld), "InZoneUnLoadingBounds")]
        class StrandedWorld_InZoneUnLoadingBounds_Patch
        {
            static bool Prefix(IPlayer player, Zone zone, ref bool __result)
            {
                try
                {
                    __result = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(zone.transform.position.x, zone.transform.position.z)) < _zoneUnloadDistance;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld.InZoneUnLoadingBounds : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(StrandedWorld), "InZoneLoadingBounds")]
        class StrandedWorld_InZoneLoadingBounds_Patch
        {
            static bool Prefix(IPlayer player, Zone zone, ref bool __result)
            {
                try
                {
                    __result = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(zone.transform.position.x, zone.transform.position.z)) < _zoneLoadDistance;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld.InZoneLoadingBounds : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(StrandedWorld), "IsOutOfGameBounds")]
        class StrandedWorld_IsOutOfGameBounds_Patch
        {
            static bool Prefix(Vector3 position, ref bool __result)
            {
                try
                {
                    float zoneSize = Main.ZoneSize;
                    float zoneSpacing = Main.ZoneSpacing;
                    float numberOfIslandsOnDiameter = 8f;
                    float worldRadius = (zoneSize * zoneSpacing * numberOfIslandsOnDiameter) / 2.0f;

                    __result = position.y > 300f || position.y < -25f || position.x > worldRadius || position.x < -worldRadius || position.z > worldRadius || position.z < -worldRadius;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld.IsOutOfGameBounds : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(StrandedWorld), "AddWaveOverlay")]
        class StrandedWorld_AddWaveOverlay_Patch
        {
            static bool Prefix(StrandedWorld __instance, GameObject zone, Texture2D height, Texture2D shore, ref AddWaveOverlay __result)
            {
                try
                {
                    AddWaveOverlay addWaveOverlay = new GameObject("WaveOverlay")
                    {
                        transform =
                        {
                            parent = zone.transform,
                            localPosition = new Vector3(WaveOverlayPosition, 0, WaveOverlayPosition)//Vector3.zero
                        }
                    }.AddComponent<AddWaveOverlay>();
                    addWaveOverlay.width = ZoneTerrainSize;
                    addWaveOverlay.height = ZoneTerrainSize;
                    addWaveOverlay.heightTexture = new OverlayHeightTexture();
                    addWaveOverlay.heightTexture.tex = shore;
                    addWaveOverlay.heightTexture.mask = height;
                    addWaveOverlay.heightTexture.alpha = 0f;
                    addWaveOverlay.heightTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
                    addWaveOverlay.heightTexture.maskAlpha = 0.9f;
                    addWaveOverlay.normalTexture = new OverlayNormalTexture();
                    addWaveOverlay.normalTexture.tex = shore;
                    addWaveOverlay.normalTexture.mask = height;
                    addWaveOverlay.normalTexture.alpha = 0f;
                    addWaveOverlay.normalTexture.maskAlpha = 0.4f;
                    addWaveOverlay.normalTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
                    addWaveOverlay.foamTexture = new OverlayFoamTexture();
                    addWaveOverlay.foamTexture.tex = shore;
                    addWaveOverlay.foamTexture.mask = height;
                    addWaveOverlay.foamTexture.alpha = 0.4f;
                    addWaveOverlay.foamTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
                    addWaveOverlay.clipTexture = new OverlayClipTexture();
                    addWaveOverlay.clipTexture.tex = shore;
                    addWaveOverlay.clipTexture.ignoreQuerys = true;
                    __result = addWaveOverlay;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld.AddWaveOverlay : " + e);
                }
                return true;
            }
        }
    }
}
