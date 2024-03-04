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
        static MethodInfo mi_IsValid = AccessTools.Method(typeof(ZonePositionGenerator), "IsValid");

        //public static Vector2[] GeneratePoints(int WORLD_SEED, float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
        //{
        //    float num = radius / Mathf.Sqrt(2f);
        //    int[,] array = new int[Mathf.CeilToInt(sampleRegionSize.x / num), Mathf.CeilToInt(sampleRegionSize.y / num)];
        //    List<Vector2> list = new List<Vector2>();
        //    List<Vector2> list2 = new List<Vector2>();
        //    list2.Add(sampleRegionSize / 2f);
        //    FastRandom fastRandom = new FastRandom(WORLD_SEED);
        //    while (list2.Count > 0)
        //    {
        //        int index = fastRandom.Next(0, list2.Count);
        //        Vector2 vector = list2[index];
        //        bool flag = false;
        //        for (int i = 0; i < numSamplesBeforeRejection; i++)
        //        {
        //            float f = (float)fastRandom.NextDouble() * 3.1415927f * 2f;
        //            Vector2 a = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
        //            Vector2 vector2 = vector + a * (float)(fastRandom.Next((int)(radius * 100f), (int)(_zoneSpacing * radius * 100f)) / 100);
        //            if (i == 0)
        //            {
        //                vector2 = vector;
        //            }
        //            bool isValid = (bool)mi_IsValid.Invoke(null, new object[] { vector2, sampleRegionSize, num, radius, list, array });

        //            if (isValid)
        //            {
        //                list.Add(vector2);
        //                list2.Add(vector2);
        //                array[(int)(vector2.x / num), (int)(vector2.y / num)] = list.Count;
        //                flag = true;
        //                break;
        //            }
        //        }
        //        if (!flag)
        //        {
        //            list2.RemoveAt(index);
        //        }
        //    }
        //    Vector2[] array2 = new Vector2[list.Count];
        //    for (int j = 0; j < list.Count; j++)
        //    {
        //        float num2 = sampleRegionSize.x * 0.5f;
        //        array2[j] = new Vector2(list[j].x - num2, list[j].y - num2);
        //    }
        //    return array2;
        //}

        [HarmonyPatch(typeof(ZonePositionGenerator), "GeneratePoints")]
        class ZonePositionGenerator_GeneratePoints_Patch
        {
            static bool Prefix(ref Vector2[] __result, int WORLD_SEED, float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
            {
                try
                {
                    float fullMapSquareToCircle = radius / Mathf.Sqrt(2f);
                    int[,] array = new int[Mathf.CeilToInt(sampleRegionSize.x / fullMapSquareToCircle), Mathf.CeilToInt(sampleRegionSize.y / fullMapSquareToCircle)];
                    List<Vector2> list = new List<Vector2>();
                    List<Vector2> list2 = new List<Vector2>();
                    list2.Add(sampleRegionSize / 2f);
                    FastRandom fastRandom = new FastRandom(WORLD_SEED);
                    while (list2.Count > 0)
                    {
                        int index = fastRandom.Next(0, list2.Count);
                        Vector2 previousPosition = list2[index];
                        bool flag = false;
                        for (int i = 0; i < numSamplesBeforeRejection; i++)
                        {
                            float f = (float)fastRandom.NextDouble() * 3.1415927f * 2f;
                            Vector2 direction = new Vector2(Mathf.Sin(f), Mathf.Cos(f));

                            float minDistanceBewteenIslands = radius;

                            Vector2 nextPosition = previousPosition + direction * (float)(fastRandom.Next((int)(minDistanceBewteenIslands * 100f), (int)(ZoneSpacing * minDistanceBewteenIslands * 100f)) / 100);
                            if (i == 0)
                            {
                                nextPosition = previousPosition;
                            }
                            //if (ZonePositionGenerator.IsValid(vector2, sampleRegionSize, num, radius, list, array))
                            bool isValid = (bool)mi_IsValid.Invoke(null, new object[] { nextPosition, sampleRegionSize, fullMapSquareToCircle, minDistanceBewteenIslands, list, array });

                            if (isValid)
                            {
                                list.Add(nextPosition);
                                list2.Add(nextPosition);
                                array[(int)(nextPosition.x / fullMapSquareToCircle), (int)(nextPosition.y / fullMapSquareToCircle)] = list.Count;
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            list2.RemoveAt(index);
                        }
                    }
                    Vector2[] array2 = new Vector2[list.Count];
                    for (int j = 0; j < list.Count; j++)
                    {
                        float num2 = sampleRegionSize.x * 0.5f;
                        array2[j] = new Vector2(list[j].x - num2, list[j].y - num2);
                    }
                    __result = array2;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ZonePositionGenerator.GeneratePoints : " + e);
                }
                return true;
            }
        }

        //private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
        //{
        //    if (candidate.x >= 0f && candidate.x < sampleRegionSize.x && candidate.y >= 0f && candidate.y < sampleRegionSize.y)
        //    {
        //        int num = (int)(candidate.x / cellSize);
        //        int num2 = (int)(candidate.y / cellSize);
        //        int num3 = Mathf.Max(0, num - 2);
        //        int num4 = Mathf.Min(num + 2, grid.GetLength(0) - 1);
        //        int num5 = Mathf.Max(0, num2 - 2);
        //        int num6 = Mathf.Min(num2 + 2, grid.GetLength(1) - 1);
        //        for (int i = num3; i <= num4; i++)
        //        {
        //            for (int j = num5; j <= num6; j++)
        //            {
        //                int num7 = grid[i, j] - 1;
        //                if (num7 != -1 && (candidate - points[num7]).sqrMagnitude < radius * radius)
        //                {
        //                    return false;
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}
    }
}
