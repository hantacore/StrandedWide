using Beam;
using Beam.Utilities;
using Beam.Serialization.Json;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Beam.Terrain;
using System.Reflection;
using Beam.Serialization;
using Funlabs;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        internal static MethodInfo mi_StrandedWorld_SaveZone = typeof(StrandedWorld).GetMethod("SaveZone", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_StrandedWorld_LoadShelterData = typeof(StrandedWorld).GetMethod("LoadShelterData", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo fi_zoneSavedObjectsLookup = typeof(StrandedWorld).GetField("_zoneSavedObjectsLookup", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(StrandedWorld), "Save")]
        class StrandedWorld_Save_Patch
        {
            static bool Prefix(StrandedWorld __instance, ref JObject __result)
            {
                try
                {
                    JObject jobject = new JObject();
                    jobject.AddField("WorldSeed", JSerializer.Serialize(StrandedWorld.WORLD_SEED));
                    JObject jobject2 = new JObject();
                    for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
                    {
                        IPlayer player = PlayerRegistry.AllPlayers[i];
                        JObject jobject3 = new JObject();
                        jobject3.AddField("Position", JSerializer.Serialize(player.transform.position));
                        jobject3.AddField("ZoneId", JSerializer.Serialize(StrandedWorld.GetZone(player.transform.position, true).Id));
                        jobject2.Add(jobject3);
                    }
                    jobject.AddField("PlayersData", jobject2);
                    foreach (Zone zone in __instance.Zones)
                    {
                        if (zone.Loaded)
                        {
                            //__instance.SaveZone(zone, false, true, false);
                            mi_StrandedWorld_SaveZone.Invoke(__instance, new object[] { zone, false, true, false });
                        }
                    }
                    if (__instance.NmlZone.SaveContainer.childCount != 0)
                    {
                        //__instance.SaveZone(__instance.NmlZone, false, true, true);
                        mi_StrandedWorld_SaveZone.Invoke(__instance, new object[] { __instance.NmlZone, false, true, true });
                    }
                    else
                    {
                        __instance.HandleOutOfGameBounds_NoMansLand();
                    }
                    JObject jobject4 = new JObject();
                    for (int k = 0; k < Main.IslandsCount; k++)
                    {
                        jobject4.Add(__instance.GetZoneData(__instance.Zones[k]));
                    }
                    JObject zoneData = __instance.GetZoneData(__instance.NmlZone);
                    jobject4.Add(zoneData);
                    jobject.AddField("Zones", jobject4);
                    jobject.AddField("ShelterCount", JSerializer.Serialize(__instance.ShelterCount));
                    JObject jobject5 = new JObject();
                    int num = 1;
                    int instance = -1;
                    for (int l = 0; l < __instance.Zones.Length; l++)
                    {
                        Zone zone2 = __instance.Zones[l];
                        if (zone2.HasShelter)
                        {
                            jobject5.AddField("Shelter" + num.ToString(), (float)zone2.ShelterType);
                            jobject5.AddField("Shelter" + num.ToString() + "_zone", (float)l);
                            num++;
                        }
                        if (zone2.HasWollie)
                        {
                            instance = l;
                        }
                    }
                    jobject5.AddField("Wollie", (float)instance);
                    jobject.AddField("Shelters", jobject5);
                    __result = jobject;

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_Save_Patch : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(StrandedWorld), "LoadZonePositions")]
        class StrandedWorld_LoadZonePositions_Patch
        {
            static bool Prefix(StrandedWorld __instance)
            {
                try
                {
                    if (World.GenerationZonePositons == null)
                    {
                        World.CreateWorldZonePoints(StrandedWorld.WORLD_SEED);
                        Debug.Log("StrandedWorld::CreateZones:: No Zone Positions Loaded - Creating New World Zone Positions");
                    }
                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        Vector3 zero = Vector3.zero;
                        zero.x = World.GenerationZonePositons[i].x;
                        zero.z = World.GenerationZonePositons[i].y;
                        __instance.Zones[i].transform.position = zero;
                        if (i == 0)
                        {
                            __instance.Zones[i].IsStartingIsland = true;
                        }
                    }

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_LoadZonePositions_Patch : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(StrandedWorld), "LoadObjects")]
        class StrandedWorld_LoadObjects_Patch
        {
            static bool Prefix(StrandedWorld __instance, JObject rootData = null)
            {
                try
                {
                    Dictionary<string, JObject> _zoneSavedObjectsLookup = fi_zoneSavedObjectsLookup.GetValue(__instance) as Dictionary<string, JObject>;

                    _zoneSavedObjectsLookup.Clear();
                    if (Game.State != GameState.LOAD_GAME)
                    {
                        if (Game.State == GameState.NEW_GAME)
                        {
                            for (int i = 0; i < Main.IslandsCount; i++)
                            {
                                Zone zone = __instance.Zones[i];
                                Map map = World.MapList[i];
                                JObject jobject = new JObject();
                                jobject.AddField("Name", map.EditorData.Name);
                                jobject.AddField("Id", map.EditorData.Id);
                                jobject.AddField("Version", (float)map.EditorData.VersionNumber);
                                if (map.IsCustomMap())
                                {
                                    jobject.AddField("Objects", map.ObjectData);
                                    zone.HasObjectsForBillboard = true;
                                }
                                _zoneSavedObjectsLookup.Add(zone.gameObject.name, jobject);
                            }
                        }
                        return false;
                    }
                    rootData = (rootData ?? SaveManager.LoadSaveFile(Game.Mode));
                    JObject persistentData = SaveManager.GetPersistentData(rootData);
                    if (persistentData == null || persistentData.IsNull())
                    {
                        return false;
                    }
                    JObject field = persistentData.GetField("StrandedWorld");
                    if (field == null || field.IsNull())
                    {
                        Debug.LogError("StrandedWorld data is null or empty.");
                        return false;
                    }
                    JObject field2 = field.GetField("Zones");
                    if (field2 == null || field2.IsNull())
                    {
                        Debug.LogError("Zones data is null or empty.");
                        return false;
                    }
                    for (int j = 0; j < Main.IslandsCount; j++)
                    {
                        Zone zone2 = __instance.Zones[j];
                        Map map2 = World.MapList[j];
                        JObject jobject2 = field2.Children[j];
                        string value = jobject2.GetField("Id").GetValue<string>();
                        int value2 = jobject2.GetField("Version").GetValue<int>();
                        if (map2.EditorData.Id != value || map2.EditorData.VersionNumber != value2)
                        {
                            JObject jobject3 = new JObject();
                            jobject3.AddField("Name", map2.EditorData.Name);
                            jobject3.AddField("Id", map2.EditorData.Id);
                            jobject3.AddField("Version", (float)map2.EditorData.VersionNumber);
                            if (map2.IsCustomMap())
                            {
                                jobject3.AddField("Objects", map2.ObjectData);
                            }
                            jobject2 = jobject3;
                        }
                        JObject field3 = jobject2.GetField("Discovered");
                        if (field3 != null)
                        {
                            bool value3 = field3.GetValue<bool>();
                            zone2.HasVisited = value3;
                        }
                        JObject field4 = jobject2.GetField("Attributes");
                        if (field4 != null)
                        {
                            SavingUtilities.Load(zone2.Attributes, field4);
                        }
                        JObject field5 = jobject2.GetField("Objects");
                        if (field5 != null && !field5.IsNull())
                        {
                            zone2.HasObjectsForBillboard = true;
                        }
                        _zoneSavedObjectsLookup.Add(zone2.gameObject.name, jobject2);
                    }
                    //if (field2.Children.Count == 50)
                    if (field2.Children.Count == Main.IslandsCount)
                    {
                        JObject zoneData = field2.GetZoneData(__instance.NmlZone.Id);
                        if (zoneData.IsValid())
                        {
                            _zoneSavedObjectsLookup.Add(__instance.NmlZone.gameObject.name, zoneData);
                        }
                    }
                    //__instance.LoadShelterData(field);
                    mi_StrandedWorld_LoadShelterData.Invoke(__instance, new object[] { field });

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_LoadObjects_Patch : " + e);
                }
                return true;
            }
        }

        internal static FieldInfo fi_zoneLoader = typeof(StrandedWorld).GetField("_zoneLoader", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_InImpostorViewRange = typeof(StrandedWorld).GetMethod("InImpostorViewRange", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_InZoneLoadingBounds = typeof(StrandedWorld).GetMethod("InZoneLoadingBounds", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_InZoneUnLoadingBounds = typeof(StrandedWorld).GetMethod("InZoneUnLoadingBounds", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_PollImposters = typeof(StrandedWorld).GetMethod("PollImposters", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_PollLoad = typeof(StrandedWorld).GetMethod("PollLoad", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_PollUnload = typeof(StrandedWorld).GetMethod("PollUnload", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(StrandedWorld), "PollZones")]
        class StrandedWorld_PollZones_Patch
        {
            static bool Prefix(StrandedWorld __instance)
            {
                try
                {
                    // patch 1.0.35
                    if (PlayerRegistry.AllPeers.Count == 0 || LevelLoader.IsServerJoinInProgress)
                    {
                        return false;
                    }

                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        Zone zone = __instance.Zones[i];
                        //bool active = PlayerRegistry.AllPlayers.Any_NonAlloc(new Func<IPlayer, Zone, bool>(__instance.InImpostorViewRange), zone);
                        bool active = PlayerRegistry.AllPlayers.Any_NonAlloc(player => (bool)mi_InImpostorViewRange.Invoke(__instance, new object[] { player, zone }));
                        zone.Terrain.gameObject.SetActive(active);
                        //__instance.PollImposters(zone);
                        mi_PollImposters.Invoke(__instance, new object[] { zone });
                        //__instance.PollZone(zone);
                        
                        // patch 1.0.35 -> buggy
                        //if (!(bool)mi_PollUnload.Invoke(__instance, new object[] { zone }))
                        //{
                        //    mi_PollLoad.Invoke(__instance, new object[] { zone });
                        //}

                        // raft bug fix
                        mi_PollUnload.Invoke(__instance, new object[] { zone });
                        mi_PollLoad.Invoke(__instance, new object[] { zone });
                    }

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_PollZones_Patch : " + e);
                }
                return true;
            }
        }

#warning for debug
//        [HarmonyPatch(typeof(StrandedWorld), "PollUnload")]
//        class StrandedWorld_PollUnload_Patch
//        {
//            static bool Prefix(StrandedWorld __instance, Zone zone, bool __result)
//            {
//                try
//                {

//                    //if (__result)
//                    //    Debug.Log("Stranded Wide (Harmony edition) : PollUnload " + zone.name + " do unload");
//                    //else
//                    //{
//                    //    Debug.Log("Stranded Wide (Harmony edition) : PollUnload " + zone.name + " do not unload");
//                    //    //Debug.Log("Stranded Wide (Harmony edition) : PollUnload " + zone.name + " do not load");
//                    //    //Debug.Log("Stranded Wide (Harmony edition) : PollUnload " + zone.name + " loading " + zone.Loading);
//                    //    //Debug.Log("Stranded Wide (Harmony edition) : PollUnload " + zone.name + " loaded " + zone.Loaded);

//                    //    ////PlayerRegistry.AllPlayers.Any_NonAlloc(new Func<IPlayer, Zone, bool>(this.InZoneLoadingBounds), zone)
//                    //    //bool playerInZoneLoadingBounds = PlayerRegistry.AllPlayers.Any_NonAlloc(player => (bool)mi_InZoneLoadingBounds.Invoke(__instance, new object[] { player, zone }));
//                    //    //Debug.Log("Stranded Wide (Harmony edition) : PollZones " + zone.name + " playerInZoneLoadingBounds " + playerInZoneLoadingBounds);
//                    //}


//                }
//                catch (Exception e)
//                {
//                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_PollUnload_Patch : " + e);
//                }

//                return true;
//            }
//        }

#warning for debug
        //[HarmonyPatch(typeof(StrandedWorld), "PollLoad")]
        //class StrandedWorld_PollLoad_Patch
        //{
        //    static void Postfix(StrandedWorld __instance, Zone zone, bool __result)
        //    {
        //        try
        //        {
        //            if (__result)
        //                Debug.Log("Stranded Wide (Harmony edition) : PollZones " + zone.name + " do load");
        //            else
        //            {
        //                Debug.Log("Stranded Wide (Harmony edition) : PollZones " + zone.name + " do not load");
        //                Debug.Log("Stranded Wide (Harmony edition) : PollZones " + zone.name + " loading " + zone.Loading);
        //                Debug.Log("Stranded Wide (Harmony edition) : PollZones " + zone.name + " loaded " + zone.Loaded);

        //                //PlayerRegistry.AllPlayers.Any_NonAlloc(new Func<IPlayer, Zone, bool>(this.InZoneLoadingBounds), zone)
        //                bool playerInZoneLoadingBounds = PlayerRegistry.AllPlayers.Any_NonAlloc(player => (bool)mi_InZoneLoadingBounds.Invoke(__instance, new object[] { player, zone }));
        //                Debug.Log("Stranded Wide (Harmony edition) : PollZones " + zone.name + " playerInZoneLoadingBounds " + playerInZoneLoadingBounds);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_PollZones_Patch : " + e);
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(StrandedWorld), "InitializeZones")]
        class StrandedWorld_InitializeZones_Patch
        {
            static bool Prefix(StrandedWorld __instance)
            {
                try
                {
                    //Debug.Log("Stranded Wide (Harmony edition) : ghost zones fix test ");
                    for (int i = Main.IslandsCount; i < __instance.Zones.Length; i++)
                    {
                        Zone zone = __instance.Zones[i];
                        zone.gameObject.SetActive(false);

                        float[,] heights = new float[zone.Terrain.terrainData.heightmapResolution, zone.Terrain.terrainData.heightmapResolution];
                        for (int x = 0; x < zone.Terrain.terrainData.heightmapResolution; x++)
                        {
                            for (int y = 0; y < zone.Terrain.terrainData.heightmapResolution; y++)
                            {
                                heights[x, y] = int.MinValue;
                            }
                        }

                        zone.Terrain.terrainData.SetHeights(0, 0, heights);
                        zone.ZoneName = "Abyss";
                        zone.Id = "Abyss";
                        zone.Version = 0;
                        zone.Biome = Zone.BiomeType.DEEP_SEA;
                        zone.IsMapEditor = false;
                        zone.IsUserMap = false;
                        zone.Seed = 0;
                        zone.WaveOverlay = null;
                    }

                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        Zone zone = __instance.Zones[i];
                        zone.gameObject.SetActive(true);
                        zone.Terrain.heightmapPixelError = 75f;
                        zone.Terrain.basemapDistance = 180f;
                        JObject zoneData = __instance.GetZoneData(zone);
                        if (zoneData != null)
                        {
                            zoneData.IsNull();
                        }
                    }

                    Debug.Log("Stranded Wide (Harmony edition) : InitializeZones StrandedWorld Zones count : " + StrandedWorld.Instance.Zones.Length);
                    Debug.Log("Stranded Wide (Harmony edition) : InitializeZones World.MapList count : " + Beam.Terrain.World.MapList.Length);

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching StrandedWorld_InitializeZones_Patch : " + e);
                }
                return true;
            }
        }
    }
}
