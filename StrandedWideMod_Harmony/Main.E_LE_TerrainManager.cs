using HarmonyLib;
using LE_LevelEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(LE_TerrainManager), "ClampHeightBounds")]
        class LE_TerrainManager_ClampHeightBounds_Patch
        {
            static bool Prefix(float heightToClamp, int x, int y, ref float __result)
            {
                try
                {
                    float num = 20f;
                    float num2 = Main.IslandSize + 1;
                    if (((float)x <= num || (float)x >= num2 - num || (float)y <= num || (float)y >= num2 - num) && heightToClamp > 0.65f)
                    {
                        heightToClamp = 0.65f;
                    }
                    __result = heightToClamp;

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_TerrainManager.ClampHeightBounds : " + e);
                }
                return true;
            }
        }
    }
}
