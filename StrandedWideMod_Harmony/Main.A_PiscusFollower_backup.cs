using Beam;
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
        static FieldInfo fi_playerGridGroup = AccessTools.Field(typeof(PiscusFollower), "_playerGridGroup");
        static FieldInfo fi_removedWildlifeSetting = AccessTools.Field(typeof(PiscusFollower), "_removedWildlifeSetting");
        static Type PlayerGridGroupType = AccessTools.Inner(typeof(PiscusFollower), "PlayerGridGroup");
        static FieldInfo fi_player = AccessTools.Field(PlayerGridGroupType, "Player");

        static FieldInfo fi_SpawnGrids = AccessTools.Field(PlayerGridGroupType, "SpawnGrids");
        static Type SpawnGridType = AccessTools.Inner(typeof(PiscusFollower), "SpawnGrid");
        static FieldInfo fi_CanGenerate = AccessTools.Field(SpawnGridType, "CanGenerate");
        static FieldInfo fi_Grid = AccessTools.Field(SpawnGridType, "Grid");

        [HarmonyPatch(typeof(PiscusFollower), "MoveGrid")]
        class PiscusFollower_MoveGrid_Patch
        {
            static bool Prefix(PiscusFollower __instance)
            {
                try
                {
                    if (LevelLoader.IsServerJoinInProgress)
                    {
                        return false;
                    }
                    object[] _playerGridGroup = fi_playerGridGroup.GetValue(__instance) as object[];
                    for (int i = 0; i < _playerGridGroup.Length; i++)
                    {
                        //PiscusFollower.PlayerGridGroup playerGridGroup = __instance._playerGridGroup[i];
                        object playerGridGroup = ((object[])fi_playerGridGroup.GetValue(__instance))[i];
                        Player player = fi_player.GetValue(playerGridGroup) as Player;

                        //if (playerGridGroup.Player != null && playerGridGroup.SpawnGrids[2, 2] == null)
                        if (player != null && )
                        {
                            return false;
                        }
                    }
                    for (int j = 0; j < __instance._playerGridGroup.Length; j++)
                    {
                        PiscusFollower.PlayerGridGroup playerGridGroup2 = __instance._playerGridGroup[j];
                        if (playerGridGroup2.Player != null)
                        {
                            Vector3 position = playerGridGroup2.Player.transform.position;
                            PiscusFollower.SpawnGrid spawnGrid = null;
                            int num = 0;
                            int num2 = 0;
                            for (int k = 0; k < 3; k++)
                            {
                                for (int l = 0; l < 3; l++)
                                {
                                    Vector3 position2 = playerGridGroup2.SpawnGrids[k, l].Grid.position;
                                    if (position.x >= position2.x - _zoneTerrainHalfSize && position.x <= position2.x + _zoneTerrainHalfSize && position.z >= position2.z - _zoneTerrainHalfSize && position.z <= position2.z + _zoneTerrainHalfSize)
                                    {
                                        spawnGrid = playerGridGroup2.SpawnGrids[k, l];
                                        num = 1 - k;
                                        num2 = 1 - l;
                                        break;
                                    }
                                }
                            }
                            if (spawnGrid != playerGridGroup2.SpawnGrids[1, 1])
                            {
                                PiscusFollower.SpawnGrid[,] array = new PiscusFollower.SpawnGrid[3, 3];
                                for (int m = 0; m < 3; m++)
                                {
                                    for (int n = 0; n < 3; n++)
                                    {
                                        int num3 = m + num;
                                        int num4 = n + num2;
                                        if (num3 < 0 || num3 > 2 || num4 < 0 || num4 > 2)
                                        {
                                            if (playerGridGroup2.SpawnGrids[m, n].piscusManager)
                                            {
                                                UnityEngine.Object.Destroy(playerGridGroup2.SpawnGrids[m, n].piscusManager.gameObject);
                                            }
                                            playerGridGroup2.SpawnGrids[m, n].CanGenerate = true;
                                            playerGridGroup2.SpawnGrids[m, n].Generated = false;
                                        }
                                        if (num3 < 0)
                                        {
                                            num3 = 2;
                                        }
                                        else if (num3 > 2)
                                        {
                                            num3 = 0;
                                        }
                                        if (num4 < 0)
                                        {
                                            num4 = 2;
                                        }
                                        else if (num4 > 2)
                                        {
                                            num4 = 0;
                                        }
                                        array[num3, num4] = playerGridGroup2.SpawnGrids[m, n];
                                        playerGridGroup2.SpawnGrids[m, n].Grid.gameObject.name = "Spawn_Grid_" + num3.ToString() + "_" + num4.ToString();
                                    }
                                }
                                playerGridGroup2.SpawnGrids = array;
                                __instance.UpdateGridPositions();
                            }
                        }
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching IslandDebugTexture : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PiscusFollower), "UpdateGridPositions")]
        class PiscusFollower_UpdateGridPositions_Patch
        {
            static bool Prefix(PiscusFollower __instance)
            {
                try
                {
                    object[] _playerGridGroup = fi_playerGridGroup.GetValue(__instance) as object[];
                    for (int i = 0; i < _playerGridGroup.Length; i++)
                    {
                        //PiscusFollower.PlayerGridGroup playerGridGroup = __instance._playerGridGroup[i];
                        object playerGridGroup = _playerGridGroup[i];
                        Player player = fi_player.GetValue(playerGridGroup) as Player;
                        //if (playerGridGroup.Player != null)
                        if (player != null)
                        {
                            object[,] spawnGrids = fi_SpawnGrids.GetValue(playerGridGroup) as object[,];
                            Transform t11 = fi_Grid.GetValue(spawnGrids[1, 1]) as Transform;

                            //playerGridGroup.SpawnGrids[0, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + _zoneTerrainSize);
                            Transform t00 = fi_Grid.GetValue(spawnGrids[0, 0]) as Transform;
                            t00.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[0, 1].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z);
                            Transform t01 = fi_Grid.GetValue(spawnGrids[0, 1]) as Transform;
                            t01.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[0, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - _zoneTerrainSize);
                            Transform t02 = fi_Grid.GetValue(spawnGrids[0, 2]) as Transform;
                            t02.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[1, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + _zoneTerrainSize);
                            Transform t10 = fi_Grid.GetValue(spawnGrids[1, 0]) as Transform;
                            t10.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[1, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - _zoneTerrainSize);
                            Transform t12 = fi_Grid.GetValue(spawnGrids[1, 2]) as Transform;
                            t12.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[2, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + _zoneTerrainSize);
                            Transform t20 = fi_Grid.GetValue(spawnGrids[2, 0]) as Transform;
                            t20.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[2, 1].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z);
                            Transform t21 = fi_Grid.GetValue(spawnGrids[2, 1]) as Transform;
                            t21.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            //playerGridGroup.SpawnGrids[2, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - _zoneTerrainSize);
                            Transform t22 = fi_Grid.GetValue(spawnGrids[2, 2]) as Transform;
                            t22.position = new Vector3(t11.position.x - _zoneTerrainSize, __instance.transform.position.y, t11.position.z + _zoneTerrainSize);

                            if ((bool)fi_removedWildlifeSetting.GetValue(__instance))
                            {
                                return false;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                for (int k = 0; k < 3; k++)
                                {
                                    //PiscusFollower.SpawnGrid spawnGrid = playerGridGroup.SpawnGrids[j, k];
                                    object spawnGrid = ((object[,])fi_SpawnGrids.GetValue(playerGridGroup))[j, k];
                                    //if (spawnGrid.CanGenerate)
                                    if ((bool)fi_CanGenerate.GetValue(spawnGrid))
                                    {
                                        //__instance.CreateGridObjects(spawnGrid);
                                        MethodInfo mi_CreateGridObjects = AccessTools.Method(typeof(PiscusFollower), "CreateGridObjects");
                                        mi_CreateGridObjects.Invoke(__instance, new object[] { });
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
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching IslandDebugTexture : " + e);
                }
                return true;
            }
        }
    }
}
