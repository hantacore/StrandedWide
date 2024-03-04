using HarmonyLib;
using IslandGen;
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
        static MethodInfo mi_FillTexturePolygon = AccessTools.Method(typeof(IslandDebugTexture), "FillTexturePolygon");

        [HarmonyPatch(typeof(IslandDebugTexture), "AttachTexture")]
        class IslandDebugTexture_AttachTexture_Patch
        {
            static bool Prefix(IslandDebugTexture __instance, Island island)
            {
                try
                {
                    int num = IslandSize;
                    int num2 = IslandSize;

                    int num3 = (int)((ZoneHalfSize - island.Size) * 0.5f);
                    Texture2D texture2D = new Texture2D(num, num2);
                    texture2D.SetPixels(Enumerable.Repeat<Color>(Color.magenta, num * num2).ToArray<Color>());
                    foreach (Center center in island.Centers)
                    {
                        List<Vector2> list = new List<Vector2>();
                        foreach (Center secondary in center.Neighbors)
                        {
                            List<Vector3> list2 = new List<Vector3>();
                            Edge edge = island.LookUpEdgeFromCenter(center, secondary);
                            if (edge != null && edge.D0 != null && edge.D1 != null)
                            {
                                Center d = edge.D0;
                                Center d2 = edge.D1;
                                list.Add(d.Point);
                                list.Add(d2.Point);
                                list.Add(center.Point);
                                if (edge != null && edge.V0 != null && edge.V1 != null)
                                {
                                    list2.Add(new Vector3(center.Point.x, center.Elevation, center.Point.y));
                                    list2.Add(new Vector3(edge.V0.Point.x, edge.V0.Elevation, edge.V0.Point.y));
                                    list2.Add(new Vector3(edge.V1.Point.x, edge.V1.Elevation, edge.V1.Point.y));
                                }
                                foreach (Vector3 vector in list2)
                                {
                                    //this.FillTexturePolygon(texture2D, list2.ToArray(), Island.BiomeProperties.Colors[center.Biome], num3);
                                    mi_FillTexturePolygon.Invoke(__instance, new object[] { texture2D, list2.ToArray(), Island.BiomeProperties.Colors[center.Biome], num3 });
                                }
                            }
                        }
                        texture2D.SetPixel((int)center.Point.x + num3, (int)center.Point.y + num3, Color.red);
                    }
                    texture2D.Apply();
                    GameObject gameObject = GameObject.Find("DebugVoronoi");
                    if (gameObject)
                    {
                        gameObject.GetComponent<Renderer>().material.mainTexture = texture2D;
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching IslandDebugTexture.AttachTexture : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IslandDebugTexture), "RenderSoilMapImageIcon")]
        class IslandDebugTexture_RenderSoilMapImageIcon_Patch
        {
            static bool Prefix(float[,] soilmap, int heightmapRes, ref Texture2D __result)
            {
                try
                {
                    Texture2D texture2D = new Texture2D(IslandSize, IslandSize);
                    for (int i = 0; i < heightmapRes - 1; i++)
                    {
                        for (int j = 0; j < heightmapRes - 1; j++)
                        {
                            float num = soilmap[j, i];
                            Color color = Color.Lerp(Color.yellow, Color.green, num);
                            if (num == 0f)
                            {
                                color = Color.black;
                            }
                            texture2D.SetPixel(j, i, color);
                        }
                    }
                    __result = texture2D;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching IslandDebugTexture.RenderSoilMapImageIcon : " + e);
                }
                return true;
            }
        }
    }
}
