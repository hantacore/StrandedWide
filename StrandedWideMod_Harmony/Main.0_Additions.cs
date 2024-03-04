using Beam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        #region additions

        #region buffers

        public static int slot0IslandSizeBuffer = 512;
        public static int slot1IslandSizeBuffer = 512;
        public static int slot2IslandSizeBuffer = 512;
        public static int slot3IslandSizeBuffer = 512;

        public static float slot0IslandSpacingBuffer = 1.25f;
        public static float slot1IslandSpacingBuffer = 1.25f;
        public static float slot2IslandSpacingBuffer = 1.25f;
        public static float slot3IslandSpacingBuffer = 1.25f;

        public static int slot0IslandCountBuffer = 49;
        public static int slot1IslandCountBuffer = 49;
        public static int slot2IslandCountBuffer = 49;
        public static int slot3IslandCountBuffer = 49;

        public static int IslandSizeBuffer
        {
            get
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        return slot0IslandSizeBuffer;//512;//256;
                    case 1:
                        return slot1IslandSizeBuffer;//512;//256;
                    case 2:
                        return slot2IslandSizeBuffer;//512;//256;
                    case 3:
                        return slot3IslandSizeBuffer;//512;//256;
                    default:
                        return 512;
                }
            }
            set
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        slot0IslandSizeBuffer = value;//512;//256;
                        break;
                    case 1:
                        slot1IslandSizeBuffer = value;//512;//256;
                        break;
                    case 2:
                        slot2IslandSizeBuffer = value;//512;//256;
                        break;
                    case 3:
                        slot3IslandSizeBuffer = value;//512;//256;
                        break;
                    default:
                        break;
                }
            }
        }
        public static int IslandsCountBuffer
        {
            get
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        return slot0IslandCountBuffer;
                    case 1:
                        return slot1IslandCountBuffer;
                    case 2:
                        return slot2IslandCountBuffer;
                    case 3:
                        return slot3IslandCountBuffer;
                    default:
                        return 49;
                }
            }
            set
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        slot0IslandCountBuffer = value;
                        break;
                    case 1:
                        slot1IslandCountBuffer = value;
                        break;
                    case 2:
                        slot2IslandCountBuffer = value;
                        break;
                    case 3:
                        slot3IslandCountBuffer = value;
                        break;
                    default:
                        break;
                }
            }
        }
        public static float ZoneSpacingBuffer
        {
            get
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        return slot0IslandSpacingBuffer;
                    case 1:
                        return slot1IslandSpacingBuffer;
                    case 2:
                        return slot2IslandSpacingBuffer;
                    case 3:
                        return slot3IslandSpacingBuffer;
                    default:
                        return 1.25f;
                }
            }
            set
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        slot0IslandSpacingBuffer = value;
                        break;
                    case 1:
                        slot1IslandSpacingBuffer = value;
                        break;
                    case 2:
                        slot2IslandSpacingBuffer = value;
                        break;
                    case 3:
                        slot3IslandSpacingBuffer = value;
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        // image size : must be power of 2 : 256, 512, 1024, 2048...
        //internal static int _islandSize;
        public static int IslandSize
        {
            get
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        return slot0IslandSize;//512;//256;
                    case 1:
                        return slot1IslandSize;//512;//256;
                    case 2:
                        return slot2IslandSize;//512;//256;
                    case 3:
                        return slot3IslandSize;//512;//256;
                    default:
                        return 512;
                }
            }
            set
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        slot0IslandSize = value;//512;//256;
                        break;
                    case 1:
                        slot1IslandSize = value;//512;//256;
                        break;
                    case 2:
                        slot2IslandSize = value;//512;//256;
                        break;
                    case 3:
                        slot3IslandSize = value;//512;//256;
                        break;
                    default:
                         break;
                }
            }
        }

        public static float ZoneSize
        {
            get
            {
                return 500 * IslandSizeRatio;//= 1000;//500f;
            }
        }
        //public static int _islandsCount = 48;
        public static int IslandsCount
        {
            get
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        return slot0IslandCount;
                    case 1:
                        return slot1IslandCount;
                    case 2:
                        return slot2IslandCount;
                    case 3:
                        return slot3IslandCount;
                    default:
                        return 49;
                }
            }
            set
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        slot0IslandCount = value;
                        break;
                    case 1:
                        slot1IslandCount = value;
                        break;
                    case 2:
                        slot2IslandCount = value;
                        break;
                    case 3:
                        slot3IslandCount = value;
                        break;
                    default:
                        break;
                }
            }
        }

        //public static float _zoneSpacing = 1.25f;
        public static float ZoneSpacing
        {
            get
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        return slot0IslandSpacing;
                    case 1:
                        return slot1IslandSpacing;
                    case 2:
                        return slot2IslandSpacing;
                    case 3:
                        return slot3IslandSpacing;
                    default:
                        return 1.25f;
                }
            }
            set
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        slot0IslandSpacing = value;
                        break;
                    case 1:
                        slot1IslandSpacing = value;
                        break;
                    case 2:
                        slot2IslandSpacing = value;
                        break;
                    case 3:
                        slot3IslandSpacing = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public static float ZoneHalfSize
        {
            get
            {
                return ZoneSize / 2;//250f;
            }
        }

        public static int IslandSizeRatio
        {
            get
            {
                int ratio = IslandSize / (StrandedWorld.ZONE_HEIGHTMAP_SIZE - 1);
                //Debug.Log("Stranded Wide (Harmony edition) : computed IslandSize ratio = " + ratio);
                return ratio;
            }
        }

        public static float ZoneTerrainSize
        {
            get
            {
                return IslandSize - IslandSizeRatio * 6;//500f//250f;
            }
        }
        public static float ZoneTerrainHalfSize
        {
            get
            {
                return ZoneTerrainSize / 2;//125f;
            }
        }
        public static float WaveOverlayPosition
        {
            get
            {
                if (IslandSize == 256)
                {
                    return 0;
                }
                else if (IslandSize == 512)
                {
                    //Debug.Log("Stranded Wide (Harmony edition) : computed WaveOverlayPosition 512 = " + (ZoneTerrainSize / 4));
                    return ZoneTerrainSize / 4f; // 128
                }
                else if (IslandSize == 1024)
                {
                    //Debug.Log("Stranded Wide (Harmony edition) : computed WaveOverlayPosition 1024 = " + (ZoneTerrainSize / 2.67f));
                    return ZoneTerrainSize / 2.67f; // 374.5 (375 ?)
                }
                else if (IslandSize == 2048)
                {
                    float result = (ZoneTerrainSize / 2.67f);
                    Debug.Log("Stranded Wide (Harmony edition) : computed WaveOverlayPosition 2048 = " + result);
                    return result;
                }
                return 0;
            }
        }
        public static float _zoneLoadDistance = 490f;//240f;
        public static float _zoneUnloadDistance = 2000;//500;//250f;

        public static float _saveBoundsRadius = 400;//310;//155f;
        public static float _sqrSaveBoundsRadius
        {
            get
            {
                return (float)Math.Pow(_saveBoundsRadius, 2);//96100;//24025f;
            }
        }

        public static int SmallIslandGroundSize
        {
            get
            {
                return (int)((0.8f * IslandSize)/2f);//200//140;
            }
        }
        public static int BigIslandGroundSize
        {
            get
            {
                return (int)((0.95f * IslandSize) / 2f);//250//200;
            }
        }
        public static int _stitchBlurRadius = 50;
        public static int _editorCameraBoxWidth = 300;

        public static int _iterations = 8;//2
        public static int _iterationsFactor = 3;//1
        public static float _blend = 0.5f;//0.25f
        public static float _perlinblend = 0.04f;//0.02f

        public static int IncreaseMaxObjectsRatio
        {
            get
            {
                return IslandSizeRatio;
            }
        }

        //internal static float ZONE_TERRAIN_SIZE_HALF = 125f;
        // Token: 0x04000534 RID: 1332
        //private const float ZONE_LOAD_UNLOAD_BUFFER = 10f;
        // Token: 0x04000535 RID: 1333
        //public const float ZONE_LOAD_DISTANCE = 240f;
        //// Token: 0x04000536 RID: 1334
        //public const float ZONE_UNLOAD_DISTANCE = 250f;

        public static Texture2D CreateTransparentTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);

            Color fillColor = Color.clear;
            Color[] fillPixels = new Color[tex.width * tex.height];

            for (int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = fillColor;
            }

            tex.SetPixels(fillPixels);

            //for (int i = 0; i < 100; i++)
            //{
            //    for (int j = 0; j < 15; j++)
            //    {
            //        tex.SetPixel(i, j, Color.green);
            //    }
            //}

            tex.Apply();
            return tex;
        }

        public static Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static void ExportTexture(Texture2D tex, string name)
        {
            try
            {
                Texture2D t = duplicateTexture(tex) as Texture2D;
                byte[] bytes = t.EncodeToPNG();
                File.WriteAllBytes("e:\\" + name + ".png", bytes);
            }
            catch
            {

            }
        }

        public static Texture2D Blur(Texture2D image, int blurSize)
        {
            Texture2D blurred = new Texture2D(image.width, image.height);

            // look at every pixel in the blur rectangle
            for (int xx = 0; xx < image.width; xx++)
            {
                for (int yy = 0; yy < image.height; yy++)
                {
                    float avgR = 0, avgG = 0, avgB = 0, avgA = 0;
                    int blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.height); y++)
                        {
                            Color pixel = image.GetPixel(x, y);

                            avgR += pixel.r;
                            avgG += pixel.g;
                            avgB += pixel.b;
                            avgA += pixel.a;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;
                    avgA = avgA / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (int x = xx; x < xx + blurSize && x < image.width; x++)
                    {
                        for (int y = yy; y < yy + blurSize && y < image.height; y++)
                        {
                            blurred.SetPixel(x, y, new Color(avgR, avgG, avgB, avgA));
                        }
                    }
                }
            }
            blurred.Apply();
            return blurred;
        }

        public static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            Texture2D nTex = new Texture2D(newWidth, newHeight);
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            nTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }

        public static Texture2D ResizeTexture2(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            return result;
        }

        public static void IncreaseObjectsNumberForProceduralGeneration()
        {
            if (StrandedWorld.Instance == null
                || StrandedWorld.Instance.ProceduralObjectsManager == null
                || StrandedWorld.Instance.ProceduralObjectsManager.BiomeGeneration == null
                || StrandedWorld.Instance.ProceduralObjectsManager.ObjectGeneration == null)
            {
                return;
            }

            SortedList<string, KeyValuePair<int, int>> rarityUpdate = new SortedList<string, KeyValuePair<int, int>>();
            rarityUpdate.Add("GEN_BUSH", new KeyValuePair<int, int>(80, (int)(20 * (float)IslandSizeRatio * 3))); // GEN_BUSH / spawnChance = 80 / maxObjectCount = 20 / detailAmountFactor = 4 
            rarityUpdate.Add("GEN_BIGROCKS_SLOPE", new KeyValuePair<int, int>(70, (int)(75 * (float)IslandSizeRatio * 3))); // GEN_BIGROCKS_SLOPE / spawnChance = 70 / maxObjectCount = 75 / detailAmountFactor = 2 
            rarityUpdate.Add("GEN_PINES_YOUNG", new KeyValuePair<int, int>(45, (int)(40 * (float)IslandSizeRatio * 2))); // GEN_PINES_YOUNG / spawnChance = 45 / maxObjectCount = 40 / detailAmountFactor = 3 

            rarityUpdate.Add("GEN_FICUS", new KeyValuePair<int, int>((int)Math.Ceiling(50 / (float)IslandSizeRatio), (int)(15 * IslandSizeRatio))); // GEN_FICUS / spawnChance = 50 / maxObjectCount = 15 / detailAmountFactor = 4 
            rarityUpdate.Add("GEN_FICUS_TREE", new KeyValuePair<int, int>(90, (int)(7 * IslandSizeRatio * 2))); // GEN_FICUS_TREE / spawnChance = 90 / maxObjectCount = 7 / detailAmountFactor = 7 
            rarityUpdate.Add("GEN_PINES", new KeyValuePair<int, int>((int)Math.Ceiling(60 / (float)IslandSizeRatio), (int)(25 * (float)IslandSizeRatio))); //GEN_PINES / spawnChance = 60 / maxObjectCount = 25 / detailAmountFactor = 6
            rarityUpdate.Add("GEN_CLIFFS", new KeyValuePair<int, int>(100, (int)(20 * (float)IslandSizeRatio))); // GEN_CLIFFS / spawnChance = 100 / maxObjectCount = 20 / detailAmountFactor = 4
            rarityUpdate.Add("GEN_BOAR", new KeyValuePair<int, int>(20, (int)(2 * (float)IslandSizeRatio))); // GEN_BOAR / spawnChance = 20 / maxObjectCount = 2 / detailAmountFactor = 8 
            rarityUpdate.Add("GEN_SNAKE", new KeyValuePair<int, int>((int)Math.Ceiling(15 / (float)IslandSizeRatio), (int)(2 * (float)IslandSizeRatio))); // GEN_SNAKE / spawnChance = 15 / maxObjectCount = 2 / detailAmountFactor = 8 
            rarityUpdate.Add("GEN_HIDINGSPOT_SNAKE", new KeyValuePair<int, int>(15, (int)(45 * (float)IslandSizeRatio))); // GEN_HIDINGSPOT_SNAKE / spawnChance = 15 / maxObjectCount = 45 / detailAmountFactor = 8 
            rarityUpdate.Add("GEN_HOG", new KeyValuePair<int, int>(20, 1)); // GEN_HOG / spawnChance = 20 / maxObjectCount = 1 / detailAmountFactor = 8 

            rarityUpdate.Add("GEN_PALM_LARGE", new KeyValuePair<int, int>(70, (int)(20 * (float)IslandSizeRatio * 4)));// GEN_PALM_LARGE / spawnChance = 70 / maxObjectCount = 20 / detailAmountFactor = 4  
            rarityUpdate.Add("GEN_PALM_SMALL", new KeyValuePair<int, int>(50, (int)(20 * (float)IslandSizeRatio * 4)));// GEN_PALM_SMALL / spawnChance = 50 / maxObjectCount = 20 / detailAmountFactor = 5 
            rarityUpdate.Add("GEN_MEDIUM_PLANTS", new KeyValuePair<int, int>(50, (int)(30 * (float)IslandSizeRatio * 2)));// GEN_MEDIUM_PLANTS / spawnChance = 50 / maxObjectCount = 30 / detailAmountFactor = 6 

            rarityUpdate.Add("GEN_BIGROCKS", new KeyValuePair<int, int>(20, (int)(30 * (float)IslandSizeRatio * 2)));// GEN_BIGROCKS / spawnChance = 20 / maxObjectCount = 30 / detailAmountFactor = 9 
            rarityUpdate.Add("GEN_SMALLROCKS", new KeyValuePair<int, int>(60, (int)(30 * (float)IslandSizeRatio * 3)));// GEN_SMALLROCKS / spawnChance = 60 / maxObjectCount = 30 / detailAmountFactor = 6  
            rarityUpdate.Add("GEN_ROCKS_FOREST_BORDER", new KeyValuePair<int, int>(20, (int)(40 * (float)IslandSizeRatio * 2)));// GEN_ROCKS_FOREST_BORDER / spawnChance = 20 / maxObjectCount = 40 / detailAmountFactor = 4
            rarityUpdate.Add("GEN_ROCKS_SHORELINE", new KeyValuePair<int, int>(50, (int)(80 * (float)IslandSizeRatio * 4))); // GEN_ROCKS_SHORELINE / spawnChance = 50 / maxObjectCount = 80 / detailAmountFactor = 4  

            // JusRob's remark about repartition
            rarityUpdate.Add("GEN_PALM_YOUNG", new KeyValuePair<int, int>((int)Math.Ceiling((float)50 / (float)IslandSizeRatio), 50));// GEN_PALM_YOUNG / spawnChance = 55 / maxObjectCount = 50 / detailAmountFactor = 4
            rarityUpdate.Add("GEN_CRABS", new KeyValuePair<int, int>((int)Math.Ceiling((float)40 / (float)IslandSizeRatio), (int)(10 * (float)IslandSizeRatio)));// GEN_CRABS / spawnChance = 40 / maxObjectCount = 10 / detailAmountFactor = 8 
            rarityUpdate.Add("GEN_ITEM_ROCK", new KeyValuePair<int, int>((int)Math.Ceiling((int)Math.Ceiling((float)50 / (float)IslandSizeRatio) / (float)IslandSizeRatio), 15));// GEN_ITEM_ROCK / spawnChance = 50 / maxObjectCount = 15 / detailAmountFactor = 8

            rarityUpdate.Add("GEN_ITEM_STICK", new KeyValuePair<int, int>((int)Math.Ceiling(20 / (float)IslandSizeRatio), (int)(25 * (float)IslandSizeRatio)));// GEN_ITEM_STICK / spawnChance = 20 / maxObjectCount = 25 / detailAmountFactor = 5 

            rarityUpdate.Add("GEN_DRIFTWOOD", new KeyValuePair<int, int>(15, (int)(8 * (float)IslandSizeRatio)));// GEN_DRIFTWOOD / spawnChance = 15 / maxObjectCount = 8 / detailAmountFactor = 14 
            rarityUpdate.Add("GEN_MINING", new KeyValuePair<int, int>((int)Math.Ceiling(75 / (float)IslandSizeRatio), (int)(3 * (float)IslandSizeRatio)));// GEN_MINING / spawnChance = 75 / maxObjectCount = 3 / detailAmountFactor = 6 
            rarityUpdate.Add("GEN_MINING_CLAY", new KeyValuePair<int, int>((int)Math.Ceiling(75 / (2 * (float)IslandSizeRatio)), (int)(8 * (float)IslandSizeRatio)));// GEN_MINING_CLAY / spawnChance = 75 / maxObjectCount = 8 / detailAmountFactor = 15 

            // cat,,,'s remark about repartition
            rarityUpdate.Add("GEN_ITEM_TARP", new KeyValuePair<int, int>((int)Math.Ceiling((float)50 / (float)IslandSizeRatio), 2));// GEN_ITEM_TARP / spawnChance = 50 / maxObjectCount = 2 / detailAmountFactor = 6 
            rarityUpdate.Add("GEN_ITEM_SCRAP", new KeyValuePair<int, int>((int)Math.Ceiling((float)6 / (float)IslandSizeRatio), 12));// GEN_ITEM_SCRAP / spawnChance = 6 / maxObjectCount = 12 / detailAmountFactor = 8 
            rarityUpdate.Add("GEN_GIANT_CRABS", new KeyValuePair<int, int>((int)Math.Ceiling((float)15 / (float)IslandSizeRatio), 4));// GEN_GIANT_CRABS / spawnChance = 15 / maxObjectCount = 2 / detailAmountFactor = 12 
            rarityUpdate.Add("GEN_YUCCA", new KeyValuePair<int, int>((int)Math.Ceiling((float)40 / (float)IslandSizeRatio), 3));// GEN_YUCCA / spawnChance = 40 / maxObjectCount = 3 / detailAmountFactor = 5 
            rarityUpdate.Add("GEN_TREES_FARMING", new KeyValuePair<int, int>((int)Math.Ceiling((float)20 / (float)IslandSizeRatio), 6));// GEN_TREES_FARMING / spawnChance = 20 / maxObjectCount = 6 / detailAmountFactor = 6 
            rarityUpdate.Add("GEN_MEDICAL_PLANTS", new KeyValuePair<int, int>((int)Math.Ceiling((float)40 / (float)IslandSizeRatio), 4));// GEN_MEDICAL_PLANTS / spawnChance = 40 / maxObjectCount = 4 / detailAmountFactor = 5 
            rarityUpdate.Add("GEN_ALOEVERA", new KeyValuePair<int, int>((int)Math.Ceiling((float)15 / (float)IslandSizeRatio), 2));// GEN_ALOEVERA / spawnChance = 15 / maxObjectCount = 2 / detailAmountFactor = 7 
            rarityUpdate.Add("GEN_POTATO_PLANT", new KeyValuePair<int, int>((int)Math.Ceiling((float)35 / (float)IslandSizeRatio), 3));// GEN_POTATO_PLANT / spawnChance = 35 / maxObjectCount = 3 / detailAmountFactor = 5
            rarityUpdate.Add("GEN_ROWBOAT", new KeyValuePair<int, int>((int)Math.Ceiling((float)7 / (float)IslandSizeRatio), 1));// GEN_ROWBOAT / spawnChance = 7 / maxObjectCount = 1 / detailAmountFactor = 3 
            rarityUpdate.Add("GEN_SHORELINE_SHIPWRECKS", new KeyValuePair<int, int>((int)Math.Ceiling((float)29 / (2 * (float)IslandSizeRatio)), 3));// GEN_SHORELINE_SHIPWRECKS / 29 / 3 
            rarityUpdate.Add("GEN_SHIPPING_CONTAINER", new KeyValuePair<int, int>((int)Math.Ceiling((float)1 / (2 * (float)IslandSizeRatio)), 1));// GEN_SHIPPING_CONTAINER / spawnChance = 1 / maxObjectCount = 1 / detailAmountFactor = 16 
            rarityUpdate.Add("GEN_DEEPSEA_SHIPWRECK", new KeyValuePair<int, int>((int)Math.Ceiling((float)7 / (2 * (float)IslandSizeRatio)), 5));// GEN_SHORELINE_SHIPWRECKS / spawnChance = 7 / maxObjectCount = 3 / detailAmountFactor = 29 
            rarityUpdate.Add("GEN_DETAILED_SHIPWRECKS", new KeyValuePair<int, int>((int)Math.Ceiling((float)68 / (2 * (float)IslandSizeRatio)), 1));// GEN_DEEPSEA_SHIPWRECK / spawnChance = 68 / maxObjectCount = 5 / detailAmountFactor = 34 

            try
            {
                Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World attempting to add more objects");

                if (StrandedWorld.Instance.ProceduralObjectsManager != null
                    && StrandedWorld.Instance.ProceduralObjectsManager.BiomeGeneration != null)
                {
                    ZoneObjects zob_GEN_SNAKE = null;
                    ZoneObjects zob_GEN_HIDINGSPOT_SNAKE = null;
                    ZoneObjects zob_GEN_BUSH = null;
                    ZoneObjects zob_GEN_BOAR = null;

                    for (int i = 0; i < StrandedWorld.Instance.ProceduralObjectsManager.BiomeGeneration.Count; i++)
                    {
                        BiomeCategory biomeCategory = StrandedWorld.Instance.ProceduralObjectsManager.BiomeGeneration[i];
                        for (int j = 0; j < biomeCategory.ProceduralObjects.Count; j++)
                        {
                            ZoneObjects zoneObjects = biomeCategory.ProceduralObjects[j];
                            ZoneGenerationType generationType = zoneObjects.generationType;

                            //Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World BiomeGeneration : " + zoneObjects.name + " / spawnChance = " + zoneObjects.spawnChance + " / maxObjectCount = " + zoneObjects.maxObjectCount + " / detailAmountFactor = " + zoneObjects.detailAmountFactor);
                            
                            if (generationType == ZoneGenerationType.Procedural)
                            {
                                if (zoneObjects.name == "GEN_SNAKE")
                                    zob_GEN_SNAKE = zoneObjects;
                                if (zoneObjects.name == "GEN_HIDINGSPOT_SNAKE")
                                    zob_GEN_HIDINGSPOT_SNAKE = zoneObjects;
                                if (zoneObjects.name == "GEN_BUSH")
                                    zob_GEN_BUSH = zoneObjects;
                                if (zoneObjects.name == "GEN_BOAR")
                                    zob_GEN_BOAR = zoneObjects;

                                //Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World BiomeGeneration : " + zoneObjects.name + " / spawnChance = " + zoneObjects.spawnChance + " / maxObjectCount = " + zoneObjects.maxObjectCount + " / detailAmountFactor = " + zoneObjects.detailAmountFactor);
                                if (rarityUpdate.ContainsKey(zoneObjects.name))
                                {
                                    //zoneObjects.detailAmountFactor = rarityUpdate[zoneObjects.name].Key;
                                    zoneObjects.spawnChance = rarityUpdate[zoneObjects.name].Key;
                                    zoneObjects.maxObjectCount = rarityUpdate[zoneObjects.name].Value;
                                }
                                //Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World BiomeGeneration : " + zoneObjects.name + " / spawnChance = " + zoneObjects.spawnChance + " / maxObjectCount = " + zoneObjects.maxObjectCount + " / detailAmountFactor = " + zoneObjects.detailAmountFactor);
                            }
                        }
                    }

                    for (int i = 0; i < StrandedWorld.Instance.ProceduralObjectsManager.BiomeGeneration.Count; i++)
                    {
                        BiomeCategory biomeCategory = StrandedWorld.Instance.ProceduralObjectsManager.BiomeGeneration[i];
                        // adding random things on small islands
                        if (biomeCategory.Biome == Zone.BiomeType.ISLAND_SMALL)
                        {
                            Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World BiomeGeneration : adding things on small islands");
                            zob_GEN_SNAKE.spawnOnStartingIsland = false;
                            biomeCategory.ProceduralObjects.Add(zob_GEN_SNAKE);
                            zob_GEN_HIDINGSPOT_SNAKE.spawnOnStartingIsland = false;
                            biomeCategory.ProceduralObjects.Add(zob_GEN_HIDINGSPOT_SNAKE);
                            biomeCategory.ProceduralObjects.Add(zob_GEN_BOAR);
                        }
                        // adding random things on big islands
                        if (biomeCategory.Biome == Zone.BiomeType.ISLAND)
                        {
                            Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World BiomeGeneration : adding things on big islands");
                            biomeCategory.ProceduralObjects.Add(zob_GEN_BUSH);
                        }
                    }
                }

                if (StrandedWorld.Instance.ProceduralObjectsManager != null
                    && StrandedWorld.Instance.ProceduralObjectsManager.ObjectGeneration != null)
                {
                    for (int i = 0; i < StrandedWorld.Instance.ProceduralObjectsManager.ObjectGeneration.Count; i++)
                    {
                        ObjectCategory objectCategory = StrandedWorld.Instance.ProceduralObjectsManager.ObjectGeneration[i];
                        for (int j = 0; j < objectCategory.ProceduralObjects.Count; j++)
                        {
                            ZoneObjects zoneObjects = objectCategory.ProceduralObjects[j];
                            ZoneGenerationType generationType = zoneObjects.generationType;
                            //Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World ObjectGeneration : " + zoneObjects.name + " / spawnChance = " + zoneObjects.spawnChance + " / maxObjectCount = " + zoneObjects.maxObjectCount + " / detailAmountFactor = " + zoneObjects.detailAmountFactor);
                            if (generationType == ZoneGenerationType.Procedural)
                            {
                                //Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World ObjectGeneration : " + zoneObjects.name + " / spawnChance = " + zoneObjects.spawnChance + " / maxObjectCount = " + zoneObjects.maxObjectCount + " / detailAmountFactor = " + zoneObjects.detailAmountFactor);
                                if (rarityUpdate.ContainsKey(zoneObjects.name))
                                {
                                    //zoneObjects.detailAmountFactor = rarityUpdate[zoneObjects.name].Key;
                                    zoneObjects.spawnChance = rarityUpdate[zoneObjects.name].Key;
                                    zoneObjects.maxObjectCount = rarityUpdate[zoneObjects.name].Value;
                                }
                                //Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World ObjectGeneration : " + zoneObjects.name + " / spawnChance = " + zoneObjects.spawnChance + " / maxObjectCount = " + zoneObjects.maxObjectCount + " / detailAmountFactor = " + zoneObjects.detailAmountFactor);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World crash while attempting to add more objects " + e);
            }
        }

        private static void RemoveWorldBarrier()
        {
            try
            {
                WorldBarrier[] barriers = Game.FindObjectsOfType<WorldBarrier>();
                foreach (WorldBarrier barrier in barriers)
                {
                    Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World attempting to remove world barrier");
                    barrier.gameObject.SetActive(false);
                    Game.Destroy(barrier.gameObject);
                    Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World world barrier removed");
                }
            }
            catch (Exception e)
            {
                Debug.Log("StrandedWorld::CreateWorld:: Stranded Wide World error while attempting to remove world barrier " + e);
            }
        }

        #endregion
    }
}
