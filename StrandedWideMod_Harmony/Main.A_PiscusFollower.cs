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
        static PropertyInfo pi_player = AccessTools.Property(PlayerGridGroupType, "Player");

        static FieldInfo fi_SpawnGrids = AccessTools.Field(PlayerGridGroupType, "SpawnGrids");
        static Type SpawnGridType = AccessTools.Inner(typeof(PiscusFollower), "SpawnGrid");
        static FieldInfo fi_CanGenerate = AccessTools.Field(SpawnGridType, "CanGenerate");
        static FieldInfo fi_Generated = AccessTools.Field(SpawnGridType, "Generated");
        static FieldInfo fi_Grid = AccessTools.Field(SpawnGridType, "Grid");
        static FieldInfo fi_piscusManager = AccessTools.Field(SpawnGridType, "piscusManager");

        static MethodInfo mi_UpdateGridPositions = AccessTools.Method(typeof(PiscusFollower), "UpdateGridPositions");

        //internal class SpawnGrid
        //{
        //    public SpawnGrid(Transform grid)
        //    {
        //        this.Grid = grid;
        //    }

        //    public Transform Grid;

        //    public PiscusManager piscusManager;

        //    public bool CanGenerate = true;

        //    public bool Generated;
        //}

        [HarmonyPatch(typeof(PiscusFollower), "MoveGrid")]
        class PiscusFollower_MoveGrid_Patch
        {
            static bool Prefix(PiscusFollower __instance)
            {
                try
                {
                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 1");
                    if (LevelLoader.IsServerJoinInProgress)
                    {
                        return false;
                    }
                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 2");
                    object[] _playerGridGroup = fi_playerGridGroup.GetValue(__instance) as object[];
                    //for (int i = 0; i < this._playerGridGroup.Length; i++)
                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 3");
                    for (int i = 0; i < _playerGridGroup.Length; i++)
                    {
                        //PiscusFollower.PlayerGridGroup playerGridGroup = __instance._playerGridGroup[i];
                        object playerGridGroup = ((object[])fi_playerGridGroup.GetValue(__instance))[i];
                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 4 " + (playerGridGroup != null ? "not null" : "null"));
                        IPlayer player = pi_player.GetValue(playerGridGroup) as IPlayer;

                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 5");
                        object[,] spawnGrids = fi_SpawnGrids.GetValue(playerGridGroup) as object[,];
                        Transform t22 = fi_Grid.GetValue(spawnGrids[2, 2]) as Transform;

                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 6");
                        //if (playerGridGroup.Player != null && playerGridGroup.SpawnGrids[2, 2] == null)
                        if (player != null && t22 == null)
                        {
                            return false;
                        }
                    }
                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 7");
                    //for (int j = 0; j < this._playerGridGroup.Length; j++)
                    for (int j = 0; j < _playerGridGroup.Length; j++)
                    {
                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 8");
                        //PiscusFollower.PlayerGridGroup playerGridGroup2 = __instance._playerGridGroup[j];
                        object playerGridGroup2 = ((object[])fi_playerGridGroup.GetValue(__instance))[j];
                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 9");
                        Player player = pi_player.GetValue(playerGridGroup2) as Player;
                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 10");
                        object[,] spawnGrids = fi_SpawnGrids.GetValue(playerGridGroup2) as object[,];

                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 11");
                        //if (playerGridGroup2.Player != null)
                        if (player != null)
                        {
                            Vector3 position = player.transform.position;//playerGridGroup2.Player.transform.position;
                            //PiscusFollower.SpawnGrid spawnGrid = null;
                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 12");
                            object spawnGrid = null;
                            int num = 0;
                            int num2 = 0;
                            for (int k = 0; k < 3; k++)
                            {
                                for (int l = 0; l < 3; l++)
                                {
                                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 13");
                                    //Vector3 position2 = playerGridGroup2.SpawnGrids[k, l].Grid.position;
                                    Transform tkl = fi_Grid.GetValue(spawnGrids[k, l]) as Transform;
                                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 14");
                                    Vector3 position2 = tkl.position;
                                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 15");
                                    if (position.x >= position2.x - ZoneTerrainHalfSize && position.x <= position2.x + ZoneTerrainHalfSize && position.z >= position2.z - ZoneTerrainHalfSize && position.z <= position2.z + ZoneTerrainHalfSize)
                                    {
                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 16");
                                        //spawnGrid = playerGridGroup2.SpawnGrids[k, l];
                                        spawnGrid = spawnGrids[k, l];
                                        num = 1 - k;
                                        num2 = 1 - l;
                                        break;
                                    }
                                }
                            }
                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 17");
                            object sg11 = spawnGrids[1, 1];
                            //if (spawnGrid != playerGridGroup2.SpawnGrids[1, 1])
                            if (spawnGrid != sg11)
                            {
                                //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 18");
                                //PiscusFollower.SpawnGrid[,] array = new PiscusFollower.SpawnGrid[3, 3];
                                object[,] array = new object[3,3];
                                for (int m = 0; m < 3; m++)
                                {
                                    for (int n = 0; n < 3; n++)
                                    {
                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 19");
                                        int num3 = m + num;
                                        int num4 = n + num2;
                                        if (num3 < 0 || num3 > 2 || num4 < 0 || num4 > 2)
                                        {
                                            object sgmn = spawnGrids[m, n];
                                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 20");
                                            PiscusManager piscusManager = fi_piscusManager.GetValue(sgmn) as PiscusManager;
                                            //if (playerGridGroup2.SpawnGrids[m, n].piscusManager)
                                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 21");
                                            if (piscusManager != null)
                                            {
                                                //UnityEngine.Object.Destroy(playerGridGroup2.SpawnGrids[m, n].piscusManager.gameObject);
                                                UnityEngine.Object.Destroy(piscusManager.gameObject);
                                                //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 22");
                                            }
                                            //playerGridGroup2.SpawnGrids[m, n].CanGenerate = true;
                                            fi_CanGenerate.SetValue(sgmn, true);
                                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 23");
                                            //playerGridGroup2.SpawnGrids[m, n].Generated = false;
                                            fi_Generated.SetValue(sgmn, false);
                                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 24");
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
                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 25");
                                        array[num3, num4] = spawnGrids[m, n];//playerGridGroup2.SpawnGrids[m, n];

                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 26");
                                        //playerGridGroup2.SpawnGrids[m, n].Grid.gameObject.name = "Spawn_Grid_" + num3.ToString() + "_" + num4.ToString();
                                        Transform grid = fi_Grid.GetValue(spawnGrids[m, n]) as Transform;
                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 27");
                                        grid.gameObject.name = "Spawn_Grid_" + num3.ToString() + "_" + num4.ToString();
                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 28");
                                    }
                                }

                                //playerGridGroup2.SpawnGrids = array;
                                for (int i2 = 0; i2 < 3; i2++)
                                {
                                    for (int j2 = 0; j2 < 3; j2++)
                                    {
                                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 29");
                                        spawnGrids[i2, j2] = array[i2,j2];
                                    }
                                }

                                //__instance.UpdateGridPositions();
                                //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 30");
                                mi_UpdateGridPositions.Invoke(__instance, new object[] { });
                                //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.MoveGrid 31");
                            }
                        }
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching PiscusFollower.MoveGrid : " + e);
                }
                return true;
            }
        }

        static MethodInfo mi_CreateGridObjects = AccessTools.Method(typeof(PiscusFollower), "CreateGridObjects");

        [HarmonyPatch(typeof(PiscusFollower), "UpdateGridPositions")]
        class PiscusFollower_UpdateGridPositions_Patch
        {
            static bool Prefix(PiscusFollower __instance)
            {
                try
                {
                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 1");
                    object[] _playerGridGroup = fi_playerGridGroup.GetValue(__instance) as object[];
                    //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 2");
                    for (int i = 0; i < _playerGridGroup.Length; i++)
                    {
                        //PiscusFollower.PlayerGridGroup playerGridGroup = __instance._playerGridGroup[i];
                        object playerGridGroup = _playerGridGroup[i];
                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 3");
                        IPlayer player = pi_player.GetValue(playerGridGroup) as IPlayer;
                        //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 4");
                        //if (playerGridGroup.Player != null)
                        if (player != null)
                        {
                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 5");
                            object[,] spawnGrids = fi_SpawnGrids.GetValue(playerGridGroup) as object[,];
                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 6");
                            Transform t11 = fi_Grid.GetValue(spawnGrids[1, 1]) as Transform;
                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 7");
                            //playerGridGroup.SpawnGrids[0, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + _zoneTerrainSize);
                            Transform t00 = fi_Grid.GetValue(spawnGrids[0, 0]) as Transform;
                            t00.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[0, 1].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z);
                            Transform t01 = fi_Grid.GetValue(spawnGrids[0, 1]) as Transform;
                            t01.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[0, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - _zoneTerrainSize);
                            Transform t02 = fi_Grid.GetValue(spawnGrids[0, 2]) as Transform;
                            t02.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[1, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + _zoneTerrainSize);
                            Transform t10 = fi_Grid.GetValue(spawnGrids[1, 0]) as Transform;
                            t10.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[1, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - _zoneTerrainSize);
                            Transform t12 = fi_Grid.GetValue(spawnGrids[1, 2]) as Transform;
                            t12.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[2, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + _zoneTerrainSize);
                            Transform t20 = fi_Grid.GetValue(spawnGrids[2, 0]) as Transform;
                            t20.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[2, 1].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z);
                            Transform t21 = fi_Grid.GetValue(spawnGrids[2, 1]) as Transform;
                            t21.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //playerGridGroup.SpawnGrids[2, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + _zoneTerrainSize, __instance.transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - _zoneTerrainSize);
                            Transform t22 = fi_Grid.GetValue(spawnGrids[2, 2]) as Transform;
                            t22.position = new Vector3(t11.position.x - ZoneTerrainSize, __instance.transform.position.y, t11.position.z + ZoneTerrainSize);

                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 8");
                            if ((bool)fi_removedWildlifeSetting.GetValue(__instance))
                            {
                                return false;
                            }

                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 9");
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
                                        mi_CreateGridObjects.Invoke(__instance, new object[] { spawnGrid });
                                    }
                                }
                            }
                            //Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower.UpdateGridPositions 10");
                        }
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching PiscusFollower.UpdateGridPositions : " + e);
                }
                return true;
            }
        }
    }
}
