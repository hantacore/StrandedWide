using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        //        [HarmonyPatch(typeof(StrandedWorld), "ApplyWorldToZones")]
        //        class StrandedWorld_ApplyWorldToZones_Patch
        //        {
        //            static bool Prefix()
        //            {
        //                try
        //                {
        //#warning TODO async loading

        //                    //StrandedWorld.Instance.LoadZonePositions();
        //                    mi_LoadZonePositions.Invoke(StrandedWorld.Instance, new object[] { });

        //                    Beam.UI.LoadingScreenPresenter presenter = Game.FindObjectOfType<Beam.UI.LoadingScreenPresenter>();
        //                    Beam.UI.TextScreenViewAdapterBase loadingText = null;
        //                    FieldInfo fi_view = typeof(Beam.UI.LoadingScreenPresenter).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);
        //                    if (presenter != null)
        //                    {
        //                        loadingText = fi_view.GetValue(presenter) as Beam.UI.TextScreenViewAdapterBase;
        //                    }

        //                    for (int i = 0; i < Main.IslandsCount; i++)
        //                    {
        //                        Zone zone = StrandedWorld.Instance.Zones[i];
        //                        //LoadZone(zone, i);
        //                        loadingText.SetHeaderText("Loading island n° " + i);
        //                        StrandedWorld.Instance.StartCoroutine(LoadZone(zone, i));
        //                    }

        //                    return false;
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.LogError("StrandedWorld:: Error in ApplyWorldToZones.ApplyWorldToZones : " + ex.Message);
        //                }

        //                return true;
        //            }
        //        }

        //        static System.Collections.IEnumerator LoadZone(Zone zone, int i)
        //        {
        //            yield return new WaitForSeconds(0.01f);
        //            try
        //            {
        //                Map map = World.MapList[i];
        //                Debug.LogError("StrandedWorld:: Loading zone (" + i + ") " + DateTime.Now);
        //                //try
        //                //{
        //                //    Beam.UI.TMPTextScreenViewAdapter loadingscreen = Game.FindObjectOfType<Beam.UI.TMPTextScreenViewAdapter>();
        //                //    if (loadingscreen != null)
        //                //    {
        //                //        loadingscreen.SetTooltipText("Loading zone (" + i + "/" + 48 + ")");
        //                //    }
        //                //}
        //                //catch { }

        //                //Debug.LogError("StrandedWorld:: zone.Terrain.terrainData.heightmapResolution : " + zone.Terrain.terrainData.heightmapResolution);
        //                zone.Terrain.terrainData.heightmapResolution = IslandSize + 1;
        //                //Debug.LogError("StrandedWorld:: new zone.Terrain.terrainData.heightmapResolution : " + zone.Terrain.terrainData.heightmapResolution);

        //                zone.Terrain.terrainData.SetHeights(0, 0, map.HeightmapData);
        //                zone.ZoneName = map.EditorData.Name;
        //                zone.Id = map.EditorData.Id;
        //                zone.Version = map.EditorData.VersionNumber;
        //                zone.Biome = map.EditorData.Biome;
        //                zone.IsMapEditor = map.IsCustomMap();
        //                zone.IsUserMap = map.IsUserMap();
        //                zone.Seed = i;
        //                Texture2D height = Main.Blur(WorldTools.CreateHeightMapTexture(zone.Terrain.terrainData), 2);
        //                //ExportTexture(height, "height" + i);
        //                Texture2D shore = Main.Blur(WorldTools.CreateShoreMaskTexture(zone.Terrain.terrainData), 2);
        //                //ExportTexture(shore, "shore" + i);
        //                if (zone.Biome == Zone.BiomeType.ISLAND || zone.Biome == Zone.BiomeType.ISLAND_SMALL || zone.Biome == Zone.BiomeType.ISLAND_ROCK || zone.Biome == Zone.BiomeType.CARRIER)
        //                {
        //#warning ocean textures
        //                    //zone.WaveOverlay = StrandedWorld.Instance.AddWaveOverlay(zone.gameObject, height, shore);
        //                    zone.WaveOverlay = (Ceto.AddWaveOverlay)mi_StrandedWorldAddWaveOverlay.Invoke(StrandedWorld.Instance, new object[] { zone.gameObject, height, shore });
        //                    //OLD DO NOT USE zone.WaveOverlay = this.AddWaveOverlay(zone.gameObject, CreateTransparentTexture(_islandSize), CreateTransparentTexture(_islandSize));
        //                }
        //                zone.SaveContainer.transform.SetAsLastSibling();
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.LogError("StrandedWorld:: Error in ApplyWorldToZones (" + zone.Biome + ") : " + e.Message);
        //            }
        //            yield break;
        //        }
    }
}
