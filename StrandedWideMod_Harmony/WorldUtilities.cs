using Beam;
using Beam.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace StrandedDeepModsUtils
{
    public static class WorldUtilities
    {
        private static bool worldLoaded = false;
        private static StrandedWorld previousInstance = null;

        private static UnityModManager.ModEntry mewide = null;
        private static PropertyInfo pi_islandSize = null;
        private static PropertyInfo pi_islandSizeRatio = null;
        private static PropertyInfo pi_zoneSize = null;
        private static PropertyInfo pi_zoneSpacing = null;
        private static PropertyInfo pi_islandsCount = null;
        private static PropertyInfo pi_overlayPosition = null;
        private static Type strandedWideMainType = null;

        public static bool IsStrandedWide()
        {
            mewide = UnityModManager.FindMod("StrandedWideMod");
            return (mewide != null && mewide.Active && mewide.Loaded);
        }

        public static int IslandSize
        {
            get
            {
                if (IsStrandedWide())
                {
                    if (pi_islandSize == null)
                    {
                        if (strandedWideMainType == null)
                        {
                            strandedWideMainType = mewide.Assembly.GetType("StrandedWideMod_Harmony.Main");
                        }
                        if (strandedWideMainType != null)
                        {
                            pi_islandSize = strandedWideMainType.GetProperty("IslandSize", BindingFlags.Static | BindingFlags.Public);
                        }
                        else
                        {
                            Debug.Log("Stranded Deep World Utilities : Stranded Wide type null");
                        }
                    }
                    if (pi_islandSize != null)
                    {
                        int swideIslandSize = (int)pi_islandSize.GetValue(null);
                        //Debug.Log("Stranded Deep World Utilities : Stranded Wide island size retrieved : " + swideIslandSize);
                        return swideIslandSize;
                    }
                    else
                    {
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide pi_islandSize null");
                        return 512;
                    }
                }

                return StrandedWorld.ZONE_HEIGHTMAP_SIZE - 1;
            }
        }

        public static float ZoneTerrainSize
        {
            get
            {
                return IslandSize - IslandSizeRatio * 6;//500f//250f;
            }
        }

        public static int IslandSizeRatio
        {
            get
            {
                if (IsStrandedWide())
                {
                    if (pi_islandSizeRatio == null)
                    {
                        if (strandedWideMainType == null)
                        {
                            strandedWideMainType = mewide.Assembly.GetType("StrandedWideMod_Harmony.Main");
                        }
                        if (strandedWideMainType != null)
                        {
                            pi_islandSizeRatio = strandedWideMainType.GetProperty("IslandSizeRatio", BindingFlags.Static | BindingFlags.Public);
                        }
                        else
                        {
                            Debug.Log("Stranded Deep World Utilities : Stranded Wide type null");
                        }
                    }
                    if (pi_islandSizeRatio != null)
                    {
                        int swideIslandSizeRatio = (int)pi_islandSizeRatio.GetValue(null);
                        //Debug.Log("Stranded Deep World Utilities : Stranded Wide island size ratio retrieved : " + swideIslandSizeRatio);
                        return swideIslandSizeRatio;
                    }
                    else
                    {
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide pi_islandSizeRatio null");
                        return 2;
                    }
                }

                return 1;
            }
        }

        public static float ZoneSize
        {
            get
            {
                if (IsStrandedWide())
                {
                    if (pi_zoneSize == null)
                    {
                        if (strandedWideMainType == null)
                        {
                            strandedWideMainType = mewide.Assembly.GetType("StrandedWideMod_Harmony.Main");
                        }
                        if (strandedWideMainType != null)
                        {
                            pi_zoneSize = strandedWideMainType.GetProperty("ZoneSize", BindingFlags.Static | BindingFlags.Public);
                        }
                        else
                        {
                            Debug.Log("Stranded Deep World Utilities : Stranded Wide type null");
                        }
                    }
                    if (pi_zoneSize != null)
                    {
                        float swideZoneSize = (float)pi_zoneSize.GetValue(null);
                        //Debug.Log("Stranded Deep World Utilities : Stranded Wide zone size retrieved : " + swideZoneSize);
                        return swideZoneSize;
                    }
                    else
                    {
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide pi_zoneSize null");
                        return 1000;
                    }
                }

                return StrandedWorld.ZONE_SIZE;
            }
        }

        public static float ZoneSpacing
        {
            get
            {
                if (IsStrandedWide())
                {
                    if (pi_zoneSpacing == null)
                    {
                        if (strandedWideMainType == null)
                        {
                            strandedWideMainType = mewide.Assembly.GetType("StrandedWideMod_Harmony.Main");
                        }
                        if (strandedWideMainType != null)
                        {
                            pi_zoneSpacing = strandedWideMainType.GetProperty("ZoneSpacing", BindingFlags.Static | BindingFlags.Public);
                        }
                        else
                        {
                            Debug.Log("Stranded Deep World Utilities : Stranded Wide type null");
                        }
                    }
                    if (pi_zoneSpacing != null)
                    {
                        float swideZoneSpacing = (float)pi_zoneSpacing.GetValue(null);
                        //Debug.Log("Stranded Deep World Utilities : Stranded Wide zone spacing retrieved : " + swideZoneSpacing);
                        return swideZoneSpacing;
                    }
                    else
                    {
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide pi_zoneSpacing null");
                        return 1.25f;
                    }
                }

                return StrandedWorld.ZONE_SPACING;
            }
        }


        public static int IslandsCount
        {
            get
            {
                if (IsStrandedWide())
                {
                    if (pi_islandsCount == null)
                    {
                        if (strandedWideMainType == null)
                        {
                            strandedWideMainType = mewide.Assembly.GetType("StrandedWideMod_Harmony.Main");
                        }
                        if (strandedWideMainType != null)
                        {
                            pi_islandsCount = strandedWideMainType.GetProperty("IslandsCount", BindingFlags.Static | BindingFlags.Public);
                        }
                        else
                        {
                            Debug.Log("Stranded Deep World Utilities : Stranded Wide type null");
                        }
                    }
                    if (pi_islandsCount != null)
                    {
                        int swideIslandCount = (int)pi_islandsCount.GetValue(null);
                        //Debug.Log("Stranded Deep World Utilities : Stranded Wide island count retrieved : " + swideIslandCount);
                        return swideIslandCount;
                    }
                    else
                    {
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide pi_islandsCount null");
                        return 49;
                    }
                }

                return StrandedWorld.WORLD_ZONES_SQUARED;
            }
        }

        public static float WaveOverlayPosition
        {
            get
            {
                if (IsStrandedWide())
                {
                    if (pi_overlayPosition == null)
                    {
                        if (strandedWideMainType == null)
                        {
                            strandedWideMainType = mewide.Assembly.GetType("StrandedWideMod_Harmony.Main");
                        }
                        if (strandedWideMainType != null)
                        {
                            pi_overlayPosition = strandedWideMainType.GetProperty("WaveOverlayPosition", BindingFlags.Static | BindingFlags.Public);
                        }
                        else
                        {
                            Debug.Log("Stranded Deep World Utilities : Stranded Wide type null");
                        }
                    }
                    if (pi_overlayPosition != null)
                    {
                        float waveOverlayPosition = (float)pi_overlayPosition.GetValue(null);
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide overlay position retrieved : " + waveOverlayPosition);
                        return waveOverlayPosition;
                    }
                    else
                    {
                        Debug.Log("Stranded Deep World Utilities : Stranded Wide pi_islandsCount null");
                        return 110f;
                    }
                }

                return StrandedWorld.WORLD_ZONES_SQUARED;
            }
        }

        public static int MaxPlayers
        {
            get
            {
                return 2;
            }
        }

        public static bool IsWorldLoaded()
        {
            if (Beam.Game.State == GameState.NEW_GAME
                || Beam.Game.State == GameState.LOAD_GAME)
            {
                // anti memory leak
                if (previousInstance != null
                && !System.Object.ReferenceEquals(previousInstance, StrandedWorld.Instance))
                {
                    Debug.Log("Stranded Deep World Utilities : world instance changed, clearing events");
                    previousInstance.WorldGenerated -= Instance_WorldGenerated;
                    previousInstance = null;
                    worldLoaded = false;
                }

                if (StrandedWorld.Instance != null
                    && !System.Object.ReferenceEquals(StrandedWorld.Instance, previousInstance))
                {
                    Debug.Log("Stranded Deep World Utilities : world instance found, registering events");
                    previousInstance = StrandedWorld.Instance;
                    StrandedWorld.Instance.WorldGenerated -= Instance_WorldGenerated;
                    StrandedWorld.Instance.WorldGenerated += Instance_WorldGenerated;
                    worldLoaded = false;
                }
            }
            else
            {
                Reset();
            }

            return worldLoaded;
        }

        private static void Reset()
        {
            worldLoaded = false;
        }

        private static void Instance_WorldGenerated()
        {
            Debug.Log("Stranded Deep World Utilities : World Loaded event");
            worldLoaded = true;
        }
    }
}