using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    static partial class Main
    {
        private static string configFileName = "StrandedWideMod.config";

        public static int slot0IslandSize = 512;
        public static int slot1IslandSize = 512;
        public static int slot2IslandSize = 512;
        public static int slot3IslandSize = 512;

        public static float slot0IslandSpacing = 1.25f;
        public static float slot1IslandSpacing = 1.25f;
        public static float slot2IslandSpacing = 1.25f;
        public static float slot3IslandSpacing = 1.25f;

        public static int slot0IslandCount = 49;
        public static int slot1IslandCount = 49;
        public static int slot2IslandCount = 49;
        public static int slot3IslandCount = 49;

        private static void ReadConfig()
        {
            Debug.Log(modName + " : config file directory : " + FilePath.SAVE_FOLDER);
            string dataDirectory = FilePath.SAVE_FOLDER;//System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"), @"Beam Team Games\Stranded Deep\Data\");
            if (System.IO.Directory.Exists(dataDirectory))
            {
                string configFilePath = System.IO.Path.Combine(dataDirectory, configFileName);
                if (System.IO.File.Exists(configFilePath))
                {
                    string[] config = System.IO.File.ReadAllLines(configFilePath);
                    foreach (string line in config)
                    {
                        string[] tokens = line.Split(new string[] { "=", ";" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length == 2)
                        {
                            if (tokens[0].Contains("slot0IslandSize"))
                            {
                                slot0IslandSize = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot1IslandSize"))
                            {
                                slot1IslandSize = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot2IslandSize"))
                            {
                                slot2IslandSize = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot3IslandSize"))
                            {
                                slot3IslandSize = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot0IslandSpacing"))
                            {
                                slot0IslandSpacing = float.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot1IslandSpacing"))
                            {
                                slot1IslandSpacing = float.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot2IslandSpacing"))
                            {
                                slot2IslandSpacing = float.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot3IslandSpacing"))
                            {
                                slot3IslandSpacing = float.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot0IslandCount"))
                            {
                                slot0IslandCount = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot1IslandCount"))
                            {
                                slot1IslandCount = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot2IslandCount"))
                            {
                                slot2IslandCount = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("slot3IslandCount"))
                            {
                                slot3IslandCount = int.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("mixProceduralAndCustom"))
                            {
                                mixProceduralAndCustom = bool.Parse(tokens[1]);
                            }
                            else if (tokens[0].Contains("customIslandsRatio"))
                            {
                                customIslandsRatio = float.Parse(tokens[1]);
                            }
                        }
                    }
                }
            }

            slot0IslandSizeBuffer = slot0IslandSize;
            slot1IslandSizeBuffer = slot1IslandSize;
            slot2IslandSizeBuffer = slot2IslandSize;
            slot3IslandSizeBuffer = slot3IslandSize;

            slot0IslandSpacingBuffer = slot0IslandSpacing;
            slot1IslandSpacingBuffer = slot1IslandSpacing;
            slot2IslandSpacingBuffer = slot2IslandSpacing;
            slot3IslandSpacingBuffer = slot3IslandSpacing;

            slot0IslandCountBuffer = slot0IslandCount;
            slot1IslandCountBuffer = slot1IslandCount;
            slot2IslandCountBuffer = slot2IslandCount;
            slot3IslandCountBuffer = slot3IslandCount;
        }

        private static void WriteConfig()
        {
            string dataDirectory = FilePath.SAVE_FOLDER;//System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"), @"Beam Team Games\Stranded Deep\Data\");
            if (System.IO.Directory.Exists(dataDirectory))
            {
                string configFilePath = System.IO.Path.Combine(dataDirectory, configFileName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("slot0IslandSize=" + slot0IslandSize + ";");
                sb.AppendLine("slot1IslandSize=" + slot1IslandSize + ";");
                sb.AppendLine("slot2IslandSize=" + slot2IslandSize + ";");
                sb.AppendLine("slot3IslandSize=" + slot3IslandSize + ";");
                sb.AppendLine("slot0IslandSpacing=" + slot0IslandSpacing + ";");
                sb.AppendLine("slot1IslandSpacing=" + slot1IslandSpacing + ";");
                sb.AppendLine("slot2IslandSpacing=" + slot2IslandSpacing + ";");
                sb.AppendLine("slot3IslandSpacing=" + slot3IslandSpacing + ";");
                sb.AppendLine("slot0IslandCount=" + slot0IslandCount + ";");
                sb.AppendLine("slot1IslandCount=" + slot1IslandCount + ";");
                sb.AppendLine("slot2IslandCount=" + slot2IslandCount + ";");
                sb.AppendLine("slot3IslandCount=" + slot3IslandCount + ";");
                sb.AppendLine("mixProceduralAndCustom=" + mixProceduralAndCustom + ";");
                sb.AppendLine("customIslandsRatio=" + customIslandsRatio + ";");

                System.IO.File.WriteAllText(configFilePath, sb.ToString(), Encoding.UTF8);
            }
        }

        //[HarmonyPatch(typeof(SaveManager), "ChangeCurrentSlot")]
        //class SaveManager_ChangeCurrentSlot_PostFix_Patch
        //{
        //    static void Postfix(int slotID)
        //    {
        //        try
        //        {
        //            //if (!__result)
        //            //{
        //            //    Debug.Log("Stranded Wide (Harmony edition) : save slot changed, reloading options");
        //            //    IslandSizeBuffer = IslandSize;
        //            //    IslandsCountBuffer = IslandsCount;
        //            //    ZoneSpacingBuffer = ZoneSpacing;
        //            //}
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Log("Stranded Wide (Harmony edition) : error while patching SaveManager_ChangeCurrentSlot_PostFix_Patch PostFix : " + e);
        //        }
        //    }
        //}
    }
}
