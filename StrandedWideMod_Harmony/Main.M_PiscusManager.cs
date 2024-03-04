using Beam.Serialization.Json;
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
        static MethodInfo mi_SaveablePrefabLoad = AccessTools.Method(typeof(SaveablePrefab), nameof(SaveablePrefab.Load));
        static FieldInfo fi_respawningTimer = AccessTools.Field(typeof(PiscusManager), "_respawningTimer");
        static FieldInfo fi_spawnDistance = AccessTools.Field(typeof(PiscusManager), "_spawnDistance");
        static MethodInfo mi_Initialize = AccessTools.Method(typeof(PiscusManager), "Initialize");

        [HarmonyPatch(typeof(PiscusManager), "Load")]
        class PiscusManager_Load_Patch
        {
            static bool Prefix(PiscusManager __instance, JObject data)
            {
                try
                {
                    //base.Load(data);
                    //mi_SaveablePrefabLoad.Invoke(__instance, new object[] { data });
#warning CRASHES HERE
                    //mi_SaveablePrefabLoad.Invoke(__instance, new object[] { });

                    if (data != null && !data.IsNull())
                    {
                        JSerializer.DeserializeInto(data.GetField("Transform"), __instance.transform);
                        __instance.CompleteTransformSerialization();
                        int value = data.GetField("respawnCounter").GetValue<int>();

                        //__instance._respawningTimer = value;

                        fi_respawningTimer.SetValue(__instance, value);

                        int value2 = data.GetField("spawnDistance").GetValue<int>();

                        //__instance._spawnDistance = (float)value2;
                        fi_spawnDistance.SetValue(__instance, value2);

                        float num = __instance.transform.localPosition.x;
                        float num2 = __instance.transform.localPosition.z;
                        float y = __instance.transform.localPosition.y;
                    }
                    //__instance.Initialize();
                    mi_Initialize.Invoke(__instance, new object[] { });

                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching PiscusManager.Load : " + e);
                }
                return true;
            }
        }
    }
}
