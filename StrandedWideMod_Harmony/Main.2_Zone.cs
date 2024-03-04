using Beam;
using Beam.Utilities;
using HarmonyLib;
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
        [HarmonyPatch(typeof(Zone), "InBounds")]
        class Zone_InBounds_Patch
        {
            static bool Prefix(Zone __instance, Vector3 position, ref bool __result)
            {
                try
                {
                    __result = Vector3Tools.SqrDistanceOnAxis(__instance.transform.position, position, Vector3Tools.xz) < BeamMath.Sqr(Main.ZoneHalfSize);
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching Zone.InBounds : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Zone), "InSaveBoundsRadius")]
        class Zone_InSaveBoundsRadius_Patch
        {
            static bool Prefix(Zone __instance, Vector3 position, ref bool __result)
            {
                try
                {
                    __result = Vector3Tools.SqrDistanceOnAxis(__instance.transform.position, position, Vector3Tools.xz) < Main._sqrSaveBoundsRadius;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching Zone.InSaveBoundsRadius : " + e);
                }
                return true;
            }
        }
    }
}
