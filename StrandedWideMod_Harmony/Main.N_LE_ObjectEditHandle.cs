using HarmonyLib;
using LE_LevelEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Funlabs;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        static MethodInfo mi_GetEditDelta = AccessTools.Method(typeof(LE_ObjectEditHandle), "GetEditDelta");
        static FieldInfo fi_m_dragSkipCounter = AccessTools.Field(typeof(LE_ObjectEditHandle), "m_dragSkipCounter");
        static FieldInfo fi_m_activeEditAxis = AccessTools.Field(typeof(LE_ObjectEditHandle), "m_activeEditAxis");

        [HarmonyPatch(typeof(LE_ObjectEditHandle), "Move")]
        class LE_ObjectEditHandle_Move_Patch
        {
            static bool Prefix(LE_ObjectEditHandle __instance)
            {
                try
                {
                    float editDelta = (float)mi_GetEditDelta.Invoke(__instance, new object[] { });
                    int dragSkipCounter = (int)fi_m_dragSkipCounter.GetValue(__instance);
                    if (dragSkipCounter == 0)
                    {
                        // expand bounds
                        //float num = 15625f; // = 250²
                        float num = (float)Math.Pow(ZoneHalfSize, 2);

                        Vector3 a = __instance.transform.TransformDirection((Vector3)fi_m_activeEditAxis.GetValue(__instance));
                        if ((a * editDelta + __instance.transform.position).XZSqr() < num)
                        {
                            __instance.transform.parent.position += a * editDelta;
                        }
                        Vector3 position = __instance.transform.parent.position;
                        position.y = Mathf.Clamp(__instance.transform.parent.position.y, -25f, 300f);
                        __instance.transform.parent.position = position;
                        if (__instance.m_onTransformed != null)
                        {
                            __instance.m_onTransformed(__instance, EventArgs.Empty);
                            return false;
                        }
                    }
                    else if (Mathf.Abs(editDelta) > 0.0005f)
                    {
                        dragSkipCounter++;
                        fi_m_dragSkipCounter.SetValue(__instance, dragSkipCounter);
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_GUI3dObject.IsObjectPlaceable : " + e);
                }
                return true;
            }
        }
    }
}
