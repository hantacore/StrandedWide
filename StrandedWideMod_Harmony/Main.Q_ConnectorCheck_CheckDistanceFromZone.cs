using Beam;
using Beam.Crafting;
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
        [HarmonyPatch(typeof(ConnectorCheck_CheckDistanceFromZone), "Check")]
        class ConnectorCheck_CheckDistanceFromZone_Check_Patch
        {
            static bool Prefix(ConnectorCheck_CheckDistanceFromZone __instance, Beam.Crafting.Placement placement, Connector connector, ref bool __result)
            {
                try
                {
                    Zone zone = StrandedWorld.GetZone(placement.Position, false);
                    __result = zone && Vector3Tools.SqrDistanceOnAxis(zone.transform.position, placement.Position, Vector3Tools.xz) <= (float)Math.Pow(ZoneHalfSize, 2);
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching ConnectorCheck_CheckDistanceFromZone_Check_Patch : " + e);
                }
                return true;
            }
        }
    }
}
