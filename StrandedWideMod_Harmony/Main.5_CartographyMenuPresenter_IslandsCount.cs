using Beam.Terrain;
using Beam.Utilities;
using HarmonyLib;
using SharpNeatLib.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Funlabs;
using Beam.UI;
using Beam.UI.MapEditor;
using Beam;
using System.Reflection;
using UnityEngine.EventSystems;

namespace StrandedWideMod_Harmony
{
    static partial class Main
    {
        [HarmonyPatch(typeof(CartographerMenuPresenter), "ValidateDuplicateMap")]
        class CartographerMenuPresenter_ValidateDuplicateMap_Patch
        {
            static bool Prefix(Map newMap, ref bool __result, bool checkSource = true)
            {
                try
                {
                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        Map map = World.MapList[i];
                        if (map.EditorData.Id == newMap.EditorData.Id && (!checkSource || map.EditorData.Source == newMap.EditorData.Source))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching CartographerMenuPresenter_ValidateDuplicateMap_Patch : " + ex);
                }
                return false;
            }
        }

        internal static FieldInfo fi_CartographerMenuPresenter_reservedZoneIds = typeof(CartographerMenuPresenter).GetField("_reservedZoneIds", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo fi_CartographerMenuPresenter_view = typeof(CartographerMenuPresenter).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo fi_CartographerMenuPresenter_currentMapSlot = typeof(CartographerMenuPresenter).GetField("_currentMapSlot", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_CartographerMenuPresenter_WorldMapSlot_Click = typeof(CartographerMenuPresenter).GetMethod("WorldMapSlot_Click", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_CartographerMenuPresenter_WorldMapSlot_Enter = typeof(CartographerMenuPresenter).GetMethod("WorldMapSlot_Enter", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_CartographerMenuPresenter_WorldMapSlot_Leave = typeof(CartographerMenuPresenter).GetMethod("WorldMapSlot_Leave", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo mi_CartographerMenuPresenter_WorldMapSlot_Drop = typeof(CartographerMenuPresenter).GetMethod("WorldMapSlot_Drop", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(CartographerMenuPresenter), "CreateWorldMapSlots")]
        class CartographerMenuPresenter_CreateWorldMapSlots_Patch
        {
            static bool Prefix(CartographerMenuPresenter __instance)
            {
                try
                {
                    CartographerMenuViewAdapterBase _view = fi_CartographerMenuPresenter_view.GetValue(__instance) as CartographerMenuViewAdapterBase;
                    List<string> _reservedZoneIds = fi_CartographerMenuPresenter_reservedZoneIds.GetValue(__instance) as List<string>;

                    BeamTeam.DestroyAllChildren(_view.MapSlotsContainer.transform);
                    ZoomComponent componentInParent = _view.MapSlotsContainer.GetComponentInParent<ZoomComponent>();
                    componentInParent.Reset();
                    for (int i = 0; i < Main.IslandsCount; i++)
                    {
                        Map map = World.MapList[i];
                        UWorldMapSlot uworldMapSlot = UnityEngine.Object.Instantiate<UWorldMapSlot>(_view.MapSlotPrefab);
                        uworldMapSlot.transform.SetParent(_view.MapSlotsContainer.transform, false);
                        uworldMapSlot.SetMap(map);
                        //uworldMapSlot.Click += __instance.WorldMapSlot_Click;
                        uworldMapSlot.Click += delegate (IWorldMapSlot sender, PointerEventData eventData)
                        {
                            mi_CartographerMenuPresenter_WorldMapSlot_Click.Invoke(__instance, new object[] { sender, eventData });
                        };
                        //uworldMapSlot.Enter += __instance.WorldMapSlot_Enter;
                        uworldMapSlot.Enter += delegate (IWorldMapSlot sender, PointerEventData eventData)
                        {
                            mi_CartographerMenuPresenter_WorldMapSlot_Enter.Invoke(__instance, new object[] { sender, eventData });
                        };
                        //uworldMapSlot.Leave += __instance.WorldMapSlot_Leave;
                        uworldMapSlot.Leave += delegate (IWorldMapSlot sender, PointerEventData eventData)
                        {
                            mi_CartographerMenuPresenter_WorldMapSlot_Leave.Invoke(__instance, new object[] { sender, eventData });
                        };
                        //uworldMapSlot.Drop += __instance.WorldMapSlot_Drop;
                        uworldMapSlot.Drop += delegate (IWorldMapSlot sender, PointerEventData eventData)
                        {
                            mi_CartographerMenuPresenter_WorldMapSlot_Drop.Invoke(__instance, new object[] { sender, eventData });
                        };
                        if ((_reservedZoneIds.Count == 0 && i == 0) || _reservedZoneIds.Contains(map.EditorData.Id) || map.EditorData.Id.Contains("INTERNAL_MISSION"))
                        {
                            uworldMapSlot.Map.EditorData.Valid = false;
                            uworldMapSlot.Button.Enabled = false;
                        }
                        else
                        {
                            uworldMapSlot.Map.EditorData.Valid = true;
                            uworldMapSlot.Button.Enabled = true;
                        }
                        Vector2 vector = World.GenerationZonePositons[i];
                        RectTransform rectTransform = (RectTransform)uworldMapSlot.transform;
                        rectTransform.localPosition = new Vector3(vector.x, vector.y, 0f);

#warning something fishy with this magic 257
                        //rectTransform.sizeDelta = new Vector2(257f, 257f);
                        rectTransform.sizeDelta = new Vector2(IslandSize + 1, IslandSize + 1);
                    }
                    componentInParent.ScaleToFitContent();
                    componentInParent.SetScale(0.5f);
                    //__instance._currentMapSlot = null;
                    fi_CartographerMenuPresenter_currentMapSlot.SetValue(__instance, null);
                }
                catch (Exception ex)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching CartographerMenuPresenter_CreateWorldMapSlots_Patch : " + ex);
                }
                return false;
            }
        }

        //[HarmonyPatch(typeof(DeveloperMode), "Teleport")]
        //class DeveloperMode_Teleport_Patch
        //{
        //}
    }
}
