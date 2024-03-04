using HarmonyLib;
using IslandGen;
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
        private static Texture2D _bigIslandStitch = null;

        private static Texture2D GetBiggerIslandStitch()
        {
            if (_bigIslandStitch != null)
            {
                if (_bigIslandStitch.height == IslandSize + 1
                    && _bigIslandStitch.width == IslandSize + 1)
                {
                    return _bigIslandStitch;
                }
            }

            Texture2D tex = (Resources.Load("Terrain/Mask_Island_Stitch") as Texture2D);
            //ExportTexture(tex, "smallstitch");
            //Texture2D bigTex = StrandedWorld.Blur(StrandedWorld.ResizeTexture(tex, _islandSize+1, _islandSize+1), 5);
            //Texture2D bigTex = StrandedWorld.ResizeTexture(tex, _islandSize + 1, _islandSize + 1);
            //_bigIslandStitch = StrandedWorld.Blur(StrandedWorld.ResizeTexture(tex, _islandSize + 1, _islandSize + 1), 5);

            _bigIslandStitch = new Texture2D(IslandSize + 1, IslandSize + 1);

            float ratio = 0.95f;

            Texture2D largertex = Main.ResizeTexture(tex, (int)Math.Round(ratio * IslandSize), (int)Math.Round(ratio * IslandSize));

            for (int i = 0; i < _bigIslandStitch.width; i++)
            {
                for (int j = 0; j < _bigIslandStitch.height; j++)
                {
                    _bigIslandStitch.SetPixel(i, j, tex.GetPixel(0, 0));
                }
            }

            Color[] pixels = largertex.GetPixels();
            for (int i = 0; i < largertex.width; i++)
            {
                for (int j = 0; j < largertex.height; j++)
                {
                    Color c = largertex.GetPixel(i, j);
                    //c = new Color(c.r * 1.5f, c.g * 1.5f, c.b * 1.5f);
                    //_bigIslandStitch.SetPixel(i + ((_islandSize - largertex.width) / 2) + _stitchBlurRadius / 2, j + ((_islandSize - largertex.height) / 2) + _stitchBlurRadius / 2, c);
                    _bigIslandStitch.SetPixel(i + ((IslandSize - largertex.width) / 2), j + ((IslandSize - largertex.height) / 2), c);
                }
            }

            //pixels = tex.GetPixels();
            //for (int i = 0; i < tex.width; i++)
            //{
            //    for (int j = 0; j < tex.height; j++)
            //    {
            //        _bigIslandStitch.SetPixel(i + ((_islandSize - tex.width) / 2), j + ((_islandSize - tex.height) / 2), tex.GetPixel(i, j));
            //    }
            //}

            _bigIslandStitch.Apply();
            _bigIslandStitch = Main.Blur(_bigIslandStitch, 10);

            ExportTexture(_bigIslandStitch, "bigstitch");

            return _bigIslandStitch;
        }

        public static float[,] StitchTerrainEdges(float[,] heightmap)
        {
            FastRandom fr = new FastRandom();
            fr.Reinitialise(StrandedWorld.WORLD_SEED);

            int num = IslandSize + 1;
            //Texture2D tex = (Resources.Load("Terrain/Mask_Island_Stitch") as Texture2D);
            //Texture2D bigTex = StrandedWorld.ResizeTexture(tex, _islandSize + 1, _islandSize + 1);
            Texture2D tex = GetBiggerIslandStitch();

            Color[] pixels = tex.GetPixels();
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    if (heightmap[i, j] <= 0.66f)
                    {
                        float r = pixels[i * num + j].r;
                        //Debug.Log("ISLAND GENERATION : stitch red level r = " + r); // 0 -> 1
                        //Debug.Log("ISLAND GENERATION : 1f * r = " + 1f * r);
                        heightmap[i, j] -= 1f * r;
                    }
                }
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    float relativeX = (IslandSize / 2) - i;
                    float relativeY = (IslandSize / 2) - j;
                    float distanceToCenter = Mathf.Sqrt(relativeX * relativeX + relativeY * relativeY);
                    //Debug.Log("ISLAND GENERATION : distanceToCenter = " + distanceToCenter); // 0 -> 256
                    float depth = heightmap[i, j];
                    //Debug.Log("ISLAND GENERATION : heightmap[i, j] = " + heightmap[i, j]);
                    //Debug.Log("ISLAND GENERATION : depth = " + depth); // 0 -> 1
                    // 0.66f = island base height
                    // basically we take the stitch texture and if the height of the pixel is below 0.66f
                    // we take the red of the pixel and remove it form the current position of the heightmap
                    //if (heightmap[i, j] <= 0.66f)
                    if (depth <= 0.662 && depth >= 0.64)
                    {
                        //Debug.Log("ISLAND GENERATION : depth = " + depth); // 0 -> 1
                        //Debug.Log("ISLAND GENERATION : distanceToCenter = " + distanceToCenter); 
                        //Debug.Log("ISLAND GENERATION : before = " + heightmap[i, j]);
                        //heightmap[i, j] -= Math.Min(-depth * (1f + 50 * (distanceToCenter / 100f)), 1f * r);
                        heightmap[i, j] = depth - depth / 40f * (distanceToCenter / 100f);
                        //Debug.Log("ISLAND GENERATION : after = " + heightmap[i, j]);
                    }
                }
            }
            return heightmap;
        }

        [HarmonyPatch(typeof(WorldTools), "GenerateZoneHeightmap")]
        class WorldTools_GenerateZoneHeightmap_Patch
        {
            static bool Prefix(Zone.BiomeType biomeType, int randomSeed, ref float[,] __result)
            {
                try
                {
                    float[,] array = new float[IslandSize + 1, IslandSize + 1];
                    switch (biomeType)
                    {
                        case Zone.BiomeType.DEEP_SEA:
                            array = WorldTools.PerlinGenerator(array, randomSeed, 4, 0.9f, 4, 1f);
                            array = WorldTools.NormalizeHeightmap(array, 0.1f, 0.2f, 1f, biomeType);
                            break;
                        case Zone.BiomeType.MEDIUM:
                            array = WorldTools.PerlinGenerator(array, randomSeed, 12, 0.5f, 6, 1f);
                            array = WorldTools.NormalizeHeightmap(array, 0.3f, 0.45f, 1f, biomeType);
                            break;
                        case Zone.BiomeType.SHALLOW:
                            array = WorldTools.PerlinGenerator(array, randomSeed, 6, 0.5f, 6, 1f);
                            array = WorldTools.NormalizeHeightmap(array, 0.44f, 0.55f, 1f, biomeType);
                            break;
                        case Zone.BiomeType.ISLAND:
                            array = WorldTools.GENERATE_ISLAND_HEIGHTMAP(randomSeed, biomeType, false);
                            break;
                        case Zone.BiomeType.ISLAND_SMALL:
                            array = WorldTools.GENERATE_ISLAND_HEIGHTMAP(randomSeed, biomeType, false);
                            break;
                        case Zone.BiomeType.ISLAND_ROCK:
                            array = WorldTools.GENERATE_ISLAND_HEIGHTMAP(randomSeed, biomeType, false);
                            break;
                    }
                    __result = array;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching WorldTools.GenerateZoneHeightmap : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(WorldTools), "GENERATE_ISLAND_HEIGHTMAP")]
        class WorldTools_GENERATE_ISLAND_HEIGHTMAP_Patch
        {
            static bool Prefix(ref float[,] __result, int seed, Zone.BiomeType biome, bool soilMapGeneration = false)
            {
                try
                {
                    System.Random random = new System.Random(seed);
                    int smallIslandSize = SmallIslandGroundSize;
                    int bigIslandSize = BigIslandGroundSize;
                    int newIslandGroundSize = bigIslandSize;
                    if (biome == Zone.BiomeType.ISLAND_SMALL)
                    {
                        newIslandGroundSize = random.Next(smallIslandSize, smallIslandSize + 20);
                    }
                    else if (biome == Zone.BiomeType.ISLAND)
                    {
                        newIslandGroundSize = random.Next(bigIslandSize - 20, bigIslandSize);
                    }
                    float num4 = 0.685f;
                    float num5 = 0.705f;
                    float num6 = (float)(newIslandGroundSize - smallIslandSize) / (float)(bigIslandSize - smallIslandSize);
                    float maxHeight = num4 + (num5 - num4) * num6;

                    int numPoints = newIslandGroundSize;
                    Island island = new Island((float)newIslandGroundSize);

                    Island.IslandTypes islandType = Island.IslandTypes.Radial;
                    int islandTypeR = random.Next(0, 90);
                    if (islandTypeR > 40)
                        islandType = Island.IslandTypes.Radial;
                    else if (islandTypeR > 20)
                        islandType = Island.IslandTypes.Blob;
                    else
                        islandType = Island.IslandTypes.Perlin;
                    island.NewIsland(islandType, Island.PointTypes.Random, numPoints, seed, (float)newIslandGroundSize);
                    //island.NewIsland(Island.IslandTypes.Radial, Island.PointTypes.Random, numPoints, seed, (float)newIslandGroundSize);
                    int newHeightMapResolution = IslandSize + 1;
                    float heightmapOffset = (float)((int)((float)(ZoneHalfSize - newIslandGroundSize) * 0.5f));
                    float[,] array = new float[newHeightMapResolution, newHeightMapResolution];

                    // false by default
                    if (!soilMapGeneration)
                    {
                        for (int i = 0; i < newHeightMapResolution; i++)
                        {
                            for (int j = 0; j < newHeightMapResolution; j++)
                            {
                                // 0.66f = island base height
                                array[i, j] = 0.66f;
                            }
                        }
                    }

                    foreach (Center center in island.Centers)
                    {
                        if (center.Biome != Island.Biome.Ocean)
                        {
                            foreach (Center center2 in center.Neighbors)
                            {
                                if (center2.Biome == Island.Biome.Lake)
                                {
                                    Island.Biome biome2 = center.Biome;
                                }
                                List<Vector3> list = new List<Vector3>();
                                Edge edge = island.LookUpEdgeFromCenter(center, center2);
                                if (edge != null && edge.V0 != null && edge.V1 != null)
                                {
                                    Corner v = edge.V0;
                                    Corner v2 = edge.V1;
                                    list.Add(new Vector3(v.Point.x, v.Elevation, v.Point.y));
                                    list.Add(new Vector3(v2.Point.x, v2.Elevation, v2.Point.y));
                                    list.Add(new Vector3(center.Point.x, center.Elevation, center.Point.y));
                                    if (soilMapGeneration)
                                    {
                                        WorldTools.FillCellHeightmap(array, list.ToArray(), (int)heightmapOffset, false, 0f, 1f);
                                    }
                                    else
                                    {
                                        // 0.66f = island base height
                                        WorldTools.FillCellHeightmap(array, list.ToArray(), (int)heightmapOffset, false, 0.66f, maxHeight);
                                    }
                                }
                            }
                        }
                    }
                    if (!soilMapGeneration)
                    {
                        array = Main.StitchTerrainEdges(array);

                        // attempt to make slopes more smooth
                        //array = WorldTools.SmoothTerrain(array, _iterations * _iterationsFactor, _blend, biome);

                        array = WorldTools.PerlinGenerator(array, seed, 24, 1f, 6, _perlinblend);
                        foreach (Edge edge2 in island.Edges)
                        {
                            if (edge2 != null && edge2.V0 != null && edge2.V1 != null)
                            {
                                Corner v3 = edge2.V0;
                                Corner v4 = edge2.V1;
                                if (edge2.River > 0)
                                {
                                    WorldTools.HeightmapRiver(array, (int)v3.Point.x, (int)v3.Point.y, (int)v4.Point.x, (int)v4.Point.y);
                                    WorldTools.HeightmapRiver(array, (int)v3.Point.x + 1, (int)v3.Point.y, (int)v4.Point.x + 1, (int)v4.Point.y);
                                    WorldTools.HeightmapRiver(array, (int)v3.Point.x, (int)v3.Point.y + 1, (int)v4.Point.x, (int)(v4.Point.y + 1f));
                                }
                            }
                        }

                        //array = WorldTools.SmoothTerrain(array, 2, 0.7f, biome);
                        array = WorldTools.SmoothTerrain(array, 6, 0.7f, biome);
                        // attempt to make slopes more smooth
                        //array = WorldTools.SmoothTerrain(array, _iterations, _blend, biome);
                    }
                    island = null;
                    __result = array;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching WorldTools.GENERATE_ISLAND_HEIGHTMAP : " + e);
                }
                return true;
            }
        }

        static MethodInfo mi_GeneratePerlin = AccessTools.Method(typeof(WorldTools), "GeneratePerlin");
        static MethodInfo mi_Normalise = AccessTools.Method(typeof(WorldTools), "Normalise");
        static MethodInfo mi_smooth = AccessTools.Method(typeof(WorldTools), "smooth");
        static FieldInfo fi_normaliseBlend = AccessTools.Field(typeof(WorldTools), "normaliseBlend");
        static FieldInfo fi_generatorType = AccessTools.Field(typeof(WorldTools), "generatorType");

        [HarmonyPatch(typeof(WorldTools), "GenerateTerrain")]
        class WorldTools_GenerateTerrain_Patch
        {
            static bool Prefix(float[,] heightmap, Zone.BiomeType biome, ref float[,] __result)// = Zone.BiomeType.RANDOM)
            {
                try
                {
                    int num = IslandSize + 1;
                    int num2 = num;
                    float[,] array = (float[,])heightmap.Clone();
                    float[,] array2 = (float[,])heightmap.Clone();
                    int generatorType = (int)fi_generatorType.GetValue(null);
                    switch (generatorType)
                    {
                        case 0:// WorldTools.GeneratorType.Perlin:
                            //array2 = WorldTools.GeneratePerlin(array2, new Vector2((float)num, (float)num2));
                            array2 = mi_GeneratePerlin.Invoke(null, new object[] { array2, new Vector2((float)num, (float)num2) }) as float[,];
                            for (int i = 0; i < num2; i++)
                            {
                                for (int j = 0; j < num; j++)
                                {
                                    float num3 = array[j, i];
                                    float num4 = array2[j, i] * Main._perlinblend + num3 * (1f - Main._perlinblend);
                                    array[j, i] = num4;
                                }
                            }
                            __result = array;
                            return false;
                        case 1:// WorldTools.GeneratorType.Normalise:
                            //array2 = WorldTools.Normalise(array2, new Vector2((float)num, (float)num2));
                            array2 = mi_Normalise.Invoke(null, new object[] { array2, new Vector2((float)num, (float)num2) }) as float[,];
                            for (int k = 0; k < num2; k++)
                            {
                                for (int l = 0; l < num; l++)
                                {
                                    float num5 = array[l, k];
                                    float normaliseBlend = (float)fi_normaliseBlend.GetValue(null);
                                    float num6 = array2[l, k] * normaliseBlend + num5 * (1f - normaliseBlend);
                                    array[l, k] = num6;
                                }
                            }
                            __result = array;
                            return false;
                        case 3:// WorldTools.GeneratorType.Smooth:
                            //array2 = WorldTools.smooth(array2, new Vector2((float)num, (float)num2));
                            array2 = mi_smooth.Invoke(null, new object[] { array2, new Vector2((float)num, (float)num2) }) as float[,];
                            for (int m = 0; m < num2; m++)
                            {
                                for (int n = 0; n < num; n++)
                                {
                                    float num7 = array[n, m];
                                    float num8 = array2[n, m] * WorldTools.smoothBlend + num7 * (1f - WorldTools.smoothBlend);
                                    array[n, m] = num8;
                                }
                            }
                            __result = array;
                            return false;
                    }
                    __result = null;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching WorldTools.GenerateTerrain : " + e);
                }
                return true;
            }
        }
    }
}
