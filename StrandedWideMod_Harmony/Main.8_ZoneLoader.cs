using Beam;
using Beam.Rendering;
using Beam.Serialization;
using Beam.Terrain;
using Beam.Utilities;
using Funlabs;
using HarmonyLib;
using SharpNeatLib.Maths;
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
        internal static FastRandom _scaleRandomizer = new FastRandom();

        [HarmonyPatch(typeof(ZoneLoader), "GenerateObjects")]
        class ZoneLoader_GenerateObjects_Patch
        {
            static bool Prefix(Zone zone, ZoneLoader.GenerationCallback generationCallback)
            {
                try
                {
                    _scaleRandomizer.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.GenerateObjects : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneLoader), "GenerateProceduralObjectsPosition")]
        class ZoneLoader_GenerateProceduralObjectsPosition_Patch
        {
            static bool Prefix(Zone zone, ZoneLoader.GenerationCallback generationCallback)
            {
                try
                {
                    _scaleRandomizer.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.GenerateProceduralObjectsPosition : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneLoader), "GenerateProceduralObjectsProcedural")]
        class ZoneLoader_GenerateProceduralObjectsProcedural_Patch
        {
            static bool Prefix(ZoneLoader __instance, ZoneObjects zoneObjects, Zone zone, ZoneLoader.GenerationCallback generationCallback)
            {
                try
                {
                    if (zoneObjects.Objects == null || zoneObjects.Objects.Length == 0)
                    {
                        UnityEngine.Debug.LogError("ZoneLoader::GenerateProceduralObjectsProcedural:: Objects to generate was null or empty.");
                        return false;
                    }
                    __instance.Random.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);
                    _scaleRandomizer.Reinitialise(StrandedWorld.WORLD_SEED - zone.Seed);

                    //bool hasSnakes = _scaleRandomizer.Next(0, 10) > 5;
                    //bool hasBoar = _scaleRandomizer.Next(0, 10) > 5;
                    bool hasBushes = _scaleRandomizer.Next(0, 10) > 3;
                    //int maxBushes = _scaleRandomizer.Next(40, 120);

                    float num = 0f;
                    if (zoneObjects.Objects.Length != 0)
                    {
                        for (int i = 0; i < zoneObjects.Objects.Length; i++)
                        {
                            ZoneObject zoneObject = zoneObjects.Objects[i];
                            num += zoneObject.Rarity;
                        }
                    }
                    if (zone.IsStartingIsland && !zoneObjects.spawnOnStartingIsland)
                    {
                        return false;
                    }
                    int detailAmountFactor = zoneObjects.detailAmountFactor;
                    int num2 = 0;
                    for (int j = 0; j < IslandSize - detailAmountFactor; j += detailAmountFactor)
                    {
                        for (int k = 0; k < IslandSize - detailAmountFactor; k += detailAmountFactor)
                        {
                            //if (zone.Biome == Zone.BiomeType.ISLAND_SMALL
                            //    && (zoneObjects.name == "GEN_SNAKE"
                            //    || zoneObjects.name == "GEN_HIDINGSPOT_SNAKE"))
                            //{
                            //    if (!hasSnakes)
                            //        return;
                            //}
                            //if (zone.Biome == Zone.BiomeType.ISLAND_SMALL && zoneObjects.name == "GEN_BOAR")
                            //{
                            //    if (!hasBoar)
                            //        return;
                            //}
                            if (zone.Biome == Zone.BiomeType.ISLAND && zoneObjects.name == "GEN_BUSH")
                            {
                                if (!hasBushes)// || num2 >= maxBushes)
                                    return false;
                            }

                            if (num2 >= zoneObjects.maxObjectCount)
                            {
                                return false;
                            }
                            GenerationPoints generationPoints = zone.GenerationPoints[j, k];
                            if ((float)__instance.Random.NextDouble() < (float)zoneObjects.spawnChance / 100f && ((zoneObjects.occupyPoint && !generationPoints.Occupied) || !zoneObjects.occupyPoint) && generationPoints.Position.y >= zoneObjects.minHeight && generationPoints.Position.y < zoneObjects.maxHeight && generationPoints.Soilmap >= zoneObjects.minStrength && generationPoints.Soilmap < zoneObjects.maxStrength && generationPoints.steepNess < zoneObjects.maxSteepness && generationPoints.steepNess > zoneObjects.minSteepness)
                            {
                                int num3 = 0;
                                if (zoneObjects.Objects.Length != 0)
                                {
                                    float num4 = (float)__instance.Random.NextDouble() * num;
                                    for (int l = 0; l < zoneObjects.Objects.Length; l++)
                                    {
                                        if (num4 < zoneObjects.Objects[l].Rarity)
                                        {
                                            num3 = l;
                                            break;
                                        }
                                        num4 -= zoneObjects.Objects[l].Rarity;
                                    }
                                }
                                Vector3 b = new Vector3((float)__instance.Random.Next((int)(zoneObjects.minPositionOffset.x * 100f), (int)(zoneObjects.maxPositionOffset.x * 100f)) / 100f, (float)__instance.Random.Next((int)(zoneObjects.minPositionOffset.y * 100f), (int)(zoneObjects.maxPositionOffset.y * 100f)) / 100f, (float)__instance.Random.Next((int)(zoneObjects.minPositionOffset.z * 100f), (int)(zoneObjects.maxPositionOffset.z * 100f)) / 100f);
                                Vector3 position = generationPoints.Position + zoneObjects.offsetPosition + b;
                                Quaternion quaternion = Quaternion.identity;
                                if (zoneObjects.terrainSnap)
                                {
                                    quaternion = Quaternion.Lerp(Quaternion.identity, generationPoints.Rotation, zoneObjects.TerrainSnapBias);
                                }
                                int num5 = __instance.Random.Next(0, 360);
                                if (zoneObjects.randomRotation)
                                {
                                    quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(Vector3.up * (float)num5)) : Quaternion.Euler(Vector3.up * (float)num5));
                                }
                                else if (zoneObjects.randomFullRotation)
                                {
                                    int num6 = __instance.Random.Next(0, 360);
                                    int num7 = __instance.Random.Next(0, 360);
                                    quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(new Vector3((float)num6, (float)num5, (float)num7))) : Quaternion.Euler(new Vector3((float)num6, (float)num5, (float)num7)));
                                }
                                if (zoneObjects.occupyPoint)
                                {
                                    generationPoints.Occupied = true;
                                }
                                IGenerationObject obj = zoneObjects.Objects[num3].Obj;
                                generationCallback(obj, position, quaternion, zone);
                                if (zoneObjects.maxObjectCount != 0)
                                {
                                    num2++;
                                }
                            }
                        }
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.GenerateProceduralObjectsProcedural : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneLoader), "GenerateShelter")]
        class ZoneLoader_GenerateShelter_Patch
        {
            static bool Prefix(ZoneLoader __instance, ZoneObjects zoneObjects, Zone zone, ZoneLoader.GenerationCallback generationCallback)
            {
                try
                {
                    if (zone.HasShelter)
                    {
                        int shelterType = zone.ShelterType;
                        UnityEngine.Debug.Log(">>Generating shelter type : " + zoneObjects.Objects[shelterType].Obj.gameObject.name + ", on Zone :" + zone.ZoneName);
                        int detailAmountFactor = zoneObjects.detailAmountFactor;
                        int num = 0;
                        int num2 = (shelterType == 7) ? 8 : ((int)Mathf.Sqrt((float)zoneObjects.detailAmountFactor));
                        int num3 = IslandSize + 1;
                        int num4 = 100;
                        int num5 = num3 * num3 % num4;
                        int num6 = 0;
                        int num7 = num3 * num3 / num4;
                        if (num3 * num3 % num4 <= 0)
                        {
                        }
                        while (num6 < num3 * num3 && num < 1)
                        {
                            int num8 = 0;
                            int num9 = num6 % num4;
                            int num10 = num6 / num4;
                            num6++;
                            for (int i = 0; i < num9; i++)
                            {
                                if (i < num5)
                                {
                                    num8 += num3 * num3 / num4 + 1;
                                }
                                else
                                {
                                    num8 += num3 * num3 / detailAmountFactor;
                                }
                            }
                            num8 += num10;
                            if (num8 < num3 * num3)
                            {
                                int num11 = num8 / num3;
                                int num12 = num8 % num3;
                                if (num11 < num3 - 1 && num12 < num3 - 1 && (float)__instance.Random.NextDouble() < (float)zoneObjects.spawnChance / 100f)
                                {
                                    GenerationPoints generationPoints = zone.GenerationPoints[num11, num12];
                                    bool flag = false;
                                    for (int j = num11 - num2; j <= num11 + num2; j++)
                                    {
                                        if (j >= 0 && j < num3 - 1)
                                        {
                                            for (int k = num12 - num2; k <= num12 + num2; k++)
                                            {
                                                if (k >= 0 && k < num3 - 1 && zone.GenerationPoints[j, k].Occupied)
                                                {
                                                    flag = true;
                                                }
                                            }
                                        }
                                    }
                                    if (!flag && ((zoneObjects.occupyPoint && !generationPoints.Occupied) || !zoneObjects.occupyPoint) && generationPoints.Position.y >= zoneObjects.minHeight && generationPoints.Position.y < zoneObjects.maxHeight && generationPoints.Soilmap >= zoneObjects.minStrength && generationPoints.Soilmap < zoneObjects.maxStrength && generationPoints.steepNess < zoneObjects.maxSteepness && generationPoints.steepNess > zoneObjects.minSteepness)
                                    {
                                        Vector3 b = new Vector3((float)__instance.Random.Next((int)(zoneObjects.minPositionOffset.x * 100f), (int)(zoneObjects.maxPositionOffset.x * 100f)) / 100f, (float)__instance.Random.Next((int)(zoneObjects.minPositionOffset.y * 100f), (int)(zoneObjects.maxPositionOffset.y * 100f)) / 100f, (float)__instance.Random.Next((int)(zoneObjects.minPositionOffset.z * 100f), (int)(zoneObjects.maxPositionOffset.z * 100f)) / 100f);
                                        Vector3 position = generationPoints.Position + zoneObjects.offsetPosition + b;
                                        Quaternion quaternion = Quaternion.identity;
                                        if (zoneObjects.terrainSnap)
                                        {
                                            quaternion = Quaternion.Lerp(Quaternion.identity, generationPoints.Rotation, zoneObjects.TerrainSnapBias);
                                        }
                                        int num13 = __instance.Random.Next(0, 360);
                                        if (zoneObjects.randomRotation)
                                        {
                                            quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(Vector3.up * (float)num13)) : Quaternion.Euler(Vector3.up * (float)num13));
                                        }
                                        else if (zoneObjects.randomFullRotation)
                                        {
                                            int num14 = __instance.Random.Next(0, 360);
                                            int num15 = __instance.Random.Next(0, 360);
                                            quaternion = (zoneObjects.terrainSnap ? (quaternion * Quaternion.Euler(new Vector3((float)num14, (float)num13, (float)num15))) : Quaternion.Euler(new Vector3((float)num14, (float)num13, (float)num15)));
                                        }
                                        if (zoneObjects.occupyPoint)
                                        {
                                            generationPoints.Occupied = true;
                                            for (int l = num11 - num2; l <= num11 + num2; l++)
                                            {
                                                if (l >= 0 && l < num3 - 1)
                                                {
                                                    for (int m = num12 - num2; m <= num12 + num2; m++)
                                                    {
                                                        if (m >= 0 && m < num3 - 1)
                                                        {
                                                            zone.GenerationPoints[l, m].Occupied = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        IGenerationObject obj = zoneObjects.Objects[shelterType].Obj;
                                        generationCallback(obj, position, quaternion, zone);
                                        num++;
                                    }
                                }
                            }
                        }
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.GenerateShelter : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneLoader), "GenerateObject")]
        class ZoneLoader_GenerateObject_Patch
        {
            static bool Prefix(ZoneLoader __instance, IGenerationObject generationObject, Vector3 position, Quaternion rotation, Zone zone)
            {
                try
                {
                    //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab " + generationObject.GetType());

                    if (generationObject is SaveablePrefab)
                    {
                        SaveablePrefab sprefab = generationObject as SaveablePrefab;
                        //309 YACHT
                        if (sprefab.PrefabId == 309)
                        {
                            UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab no yachts in here !");
                            return false;
                        }
                        //21 BUOY BOAT
                        if (sprefab.PrefabId == 21)
                        {
                            foreach (GeneratedObject go in zone.GeneratedObjects)
                            {
                                if (go.Prefab.name == "SHIPWRECK_3A")
                                {
                                    //}
                                    //if (_scaleRandomizer.Next(0, 10) > 5)
                                    //{
                                    UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab not too many buoy boats !");
                                    return false;
                                }
                            }
                        }
                    }
                    // working
                    //if (generationObject.gameObject.name == "SHIPWRECK_8A")
                    //{
                    //    UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab no yachts in here !");
                    //    return;
                    //}
                    //if (generationObject.gameObject.name == "SHIPWRECK_3A")
                    //{
                    //    if (_scaleRandomizer.Next(0, 10) > 5)
                    //    {
                    //        UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab not too many buoy boats !");
                    //        return;
                    //    }
                    //}

                    zone.OnObjectGenerated(new GeneratedObject
                    {
                        Prefab = generationObject.gameObject,
                        Position = position,
                        Rotation = rotation,
                        ImpostorParentPrefab = generationObject.gameObject.GetInterface<IImpostorParent>()
                    });

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.GenerateObject : " + e);
                }
                return true;
            }
        }

        //internal static void ClientSetParent(MultiplayerZoneSync.Command command)
        //{
        //    command.Instance.transform.SetParent(command.Zone.SaveContainer);
        //}

        [HarmonyPatch(typeof(ZoneLoader), "CreateGeneratedPrefab")]
        class ZoneLoader_CreateGeneratedPrefab_Patch
        {
            static bool Prefix(ZoneLoader __instance, GeneratedObject objectData, Zone zone)
            {
                try
                {
                    //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World generating prefab for island : " + zone.name);
                    //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World save container name : " + zone.SaveContainer.name);

                    SaveablePrefab component = objectData.Prefab.GetComponent<SaveablePrefab>();
                    GameObject gameObject = null;
                    if (component != null)
                    {
                        uint prefabId = component.PrefabId;

                        // cat's items compatibility patch
                        if (prefabId > 399)
                            return false;

                        //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab Id : " + prefabId);

                        MiniGuid referenceId = MiniGuid.NewFrom(objectData.Position, prefabId, 48879);
                        if (Game.Mode.IsClient() && component.IsMultiplayerEntity)
                        {
                            //MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.ZoneGeneration, referenceId, new Action<MultiplayerZoneSync.Command>(ClientSetParent), null, zone);
                            MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.ZoneGeneration, 
                                referenceId, 
                                AccessTools.MethodDelegate<Action<MultiplayerZoneSync.Command>>(AccessTools.Method(typeof(ZoneLoader), "ClientSetParent"), __instance), 
                                null, 
                                zone);
                            return false;
                        }
                        gameObject = MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, objectData.Position, objectData.Rotation, referenceId, null).gameObject;
                    }
                    else
                    {
                        //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab Name : " + objectData.Prefab.name);
                        if (objectData.Prefab.name != "GENERATOR_ISLAND_SHARKS")
                        {
                            gameObject = UnityEngine.Object.Instantiate<GameObject>(objectData.Prefab, objectData.Position, objectData.Rotation);
                        }
                    }
                    if (gameObject != null)
                    {
                        gameObject.transform.SetParent(zone.SaveContainer, false);
                        gameObject.transform.position = objectData.Position;
                        gameObject.transform.rotation = objectData.Rotation;
                    }

                    if (objectData != null
                        && objectData.Prefab.name == "GENERATOR_ISLAND_SHARKS")
                    {
#warning shark location
                        int radius = 80 * IslandSizeRatio; ;//120;// 170;
                        int offset = (int)Main.WaveOverlayPosition; //110;// (_islandSize / 2);
                        List<Vector3> positions = new List<Vector3>();
                        positions.Add(new Vector3(offset + radius * Mathf.Cos(Mathf.Deg2Rad * 0), 0, offset + radius * Mathf.Sin(Mathf.Deg2Rad * 0)));
                        positions.Add(new Vector3(offset + radius * Mathf.Cos(Mathf.Deg2Rad * 60), 0, offset + radius * Mathf.Sin(Mathf.Deg2Rad * 60)));
                        positions.Add(new Vector3(offset + radius * Mathf.Cos(Mathf.Deg2Rad * 120), 0, offset + radius * Mathf.Sin(Mathf.Deg2Rad * 120)));
                        positions.Add(new Vector3(offset + radius * Mathf.Cos(Mathf.Deg2Rad * 180), 0, offset + radius * Mathf.Sin(Mathf.Deg2Rad * 180)));
                        positions.Add(new Vector3(offset + radius * Mathf.Cos(Mathf.Deg2Rad * 240), 0, offset + radius * Mathf.Sin(Mathf.Deg2Rad * 240)));
                        positions.Add(new Vector3(offset + radius * Mathf.Cos(Mathf.Deg2Rad * 300), 0, offset + radius * Mathf.Sin(Mathf.Deg2Rad * 300)));

                        int sharks_count = _scaleRandomizer.Next(100);
                        if (sharks_count < 5)
                        {
                            sharks_count = 0;
                        }
                        else if (sharks_count < 10)
                        {
                            sharks_count = 1;
                        }
                        else if (sharks_count < 20)
                        {
                            sharks_count = 2;
                        }
                        else if (sharks_count < 30)
                        {
                            sharks_count = 3;
                        }
                        else if (sharks_count < 80)
                        {
                            sharks_count = 4;
                        }
                        else if (sharks_count < 90)
                        {
                            sharks_count = 5;
                        }
                        else
                        {
                            sharks_count = 6;
                        }

                        if (zone.IsStartingIsland)
                            sharks_count = 3;

                        //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World number of sharks = " + sharks_count);

                        for (int sharkindex = 0; sharkindex < sharks_count; sharkindex++)
                        {
                            int positionIndex = _scaleRandomizer.Next(0, positions.Count - 1);
                            Vector3 position = positions[positionIndex];
                            positions.RemoveAt(positionIndex);
                            //Vector3 position = positions[sharkindex];
                            Quaternion rotation = new Quaternion(0, 0, 0, 1);

                            uint sharkType = (uint)_scaleRandomizer.Next(10);
                            if (sharkType < 6)
                                // 218 TIGER
                                sharkType = 218;
                            else if (sharkType < 9)
                                // 334 HAMMER
                                sharkType = 334;
                            else
                                // 335 GOBLIN
                                sharkType = 335;

                            //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World shark type = " + sharkType);

                            //SaveablePrefab sp = null;

                            string text;
                            bool flag = Prefabs.TryGetMultiplayerPrefabName(sharkType, out text);
                            //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World flag = " + flag);
                            MiniGuid referenceId = MiniGuid.NewFrom(position, sharkType, 48879);
                            //UnityEngine.Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World CreateGeneratedPrefab referenceId = " + referenceId);
                            SaveablePrefab instance = MultiplayerMng.Instantiate<SaveablePrefab>(sharkType, referenceId, null);

                            ((PiscusManager)instance).SpawnDistance = 320;

                            LevelLoader.Instance.HashCodes.Add(instance.gameObject.GetHashCode());
                            instance.transform.SetParent(zone.SaveContainer, false);
                            instance.transform.position = position + zone.transform.position;
                            instance.gameObject.transform.SetParent(zone.SaveContainer, true);

                            zone.OnObjectCreated(gameObject);
                        }
                    }
                    else
                    {
                        if (component != null && gameObject != null)
                        {
                            List<uint> plantPrefabs = new List<uint>();
                            //palms
                            plantPrefabs.Add(157);
                            plantPrefabs.Add(158);
                            plantPrefabs.Add(159);
                            plantPrefabs.Add(160);
                            // ficus
                            plantPrefabs.Add(47);
                            plantPrefabs.Add(48);
                            plantPrefabs.Add(49);
                            //ficus
                            plantPrefabs.Add(66);
                            plantPrefabs.Add(67);
                            // shrubs
                            plantPrefabs.Add(50);
                            plantPrefabs.Add(51);
                            plantPrefabs.Add(52);
                            // bush
                            plantPrefabs.Add(205);
                            // pines
                            plantPrefabs.Add(202);
                            plantPrefabs.Add(203);
                            plantPrefabs.Add(204);
                            // pines
                            plantPrefabs.Add(206);
                            plantPrefabs.Add(207);

                            if (plantPrefabs.Contains(component.PrefabId))
                            {
                                int scale = _scaleRandomizer.Next(90, 110);
                                float fscale = (float)scale / (float)100;
                                gameObject.transform.localScale = new Vector3(fscale, fscale, fscale);
                            }
                        }
                        zone.OnObjectCreated(gameObject);
                    }

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.CreateGeneratedPrefab : " + e);
                }
                return true;
            }
        }

        static MethodInfo mi_GetClosestPlayer = AccessTools.Method(typeof(ZoneLoader), "GetClosestPlayer");
        static FieldInfo fi_processes = AccessTools.Field(typeof(ZoneLoader), "_processes");

        [HarmonyPatch(typeof(ZoneLoader), "GetProcessingSteps")]
        class ZoneLoader_GetProcessingSteps_Patch
        {
            static bool Prefix(ZoneLoader __instance, int quantity, Vector3 position, ref int __result)
            {
                try
                {
                    int b = (__instance.ObjectProcessingMode == ZoneLoader.EObjectProcessingMode.Faster) ? 200 : 4;
                    Transform closestPlayer = mi_GetClosestPlayer.Invoke(__instance, new object[] { position }) as Transform;
                    if (closestPlayer.IsNullOrDestroyed())
                    {
                        __result = 1;
                        return false;
                    }
                    float num = (position - closestPlayer.position).magnitude - Main.ZoneTerrainHalfSize;

                    int processes = (int)fi_processes.GetValue(__instance);
                    num /= (float)processes;
                    __result = Mathf.Max(Mathf.CeilToInt((float)quantity / Mathf.Max(1f, num)), b);
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZoneLoader.GetProcessingSteps : " + e);
                }
                return true;
            }
        }
    }
}
