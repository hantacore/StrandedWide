using Beam;
using HarmonyLib;
using LE_LevelEditor;
using LE_LevelEditor.Core;
using LE_LevelEditor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Funlabs;
using S_SnapTools;
using MyUtility;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        static FieldInfo fi_m_previewInstance = AccessTools.Field(typeof(LE_GUI3dObject), "m_previewInstance");
        static FieldInfo fi_m_cursorHitInfo = AccessTools.Field(typeof(LE_GUI3dObject), "m_cursorHitInfo");
        static FieldInfo fi_m_object = AccessTools.Field(typeof(LE_GUI3dObject), "m_object");
        static FieldInfo fi_m_isObjectPlaceable = AccessTools.Field(typeof(LE_GUI3dObject), "m_isObjectPlaceable");
        static MethodInfo mi_SetDragMessageInUI = AccessTools.Method(typeof(LE_GUI3dObject), "SetDragMessageInUI");
        static MethodInfo mi_IsObjectDraggedInUI = AccessTools.Method(typeof(LE_GUI3dObject), "IsObjectDraggedInUI");
        static MethodInfo mi_OnObjectDrag = AccessTools.Method(typeof(LE_GUI3dObject), "OnObjectDrag");
        static MethodInfo mi_MoveToLayer = AccessTools.Method(typeof(LE_GUI3dObject), "MoveToLayer");
        static MethodInfo mi_AddGridSnapping = AccessTools.Method(typeof(LE_GUI3dObject), "AddGridSnapping");
        static MethodInfo mi_PlaceObject = AccessTools.Method(typeof(LE_GUI3dObject), "PlaceObject");
        static MethodInfo mi_OnNewObjectPlaced = AccessTools.Method(typeof(LE_GUI3dObject), "OnNewObjectPlaced");

        [HarmonyPatch(typeof(LE_GUI3dObject), "IsObjectPlaceable", new Type[] { typeof(LE_Object) })]
        class LE_GUI3dObject_IsObjectPlaceable_Patch
        {
            static bool Prefix(LE_GUI3dObject __instance, LE_Object p_object, ref bool __result)
            {
                try
                {
                    if (p_object.MaxInstancesInLevel != 0)
                    {
                        int num = 0;
                        LE_Object[] array = UnityEngine.Object.FindObjectsOfType<LE_Object>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            // 1.0.38 compatibility
                            LE_Object m_previewInstance = fi_m_previewInstance.GetValue(__instance) as LE_Object;
                            if ((!(m_previewInstance != null) || !(m_previewInstance == array[i])) && array[i].Id == p_object.Id)
                            {
                                num++;
                                if (p_object.MaxInstancesInLevel * Main.IncreaseMaxObjectsRatio <= num)
                                {
                                    __result = false;
                                    return false;
                                }
                            }
                        }
                    }
                    __result = true;
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

        // issue : drops loads of objects on the way

        [HarmonyPatch(typeof(LE_GUI3dObject), "UpdateNewObjectDragAndDrop")]
        class LE_GUI3dObject_UpdateNewObjectDragAndDrop_Patch
        {
            static bool Prefix(LE_GUI3dObject __instance)
            {
                try
                {
                    LE_Object m_object = fi_m_object.GetValue(__instance) as LE_Object;

                    if (m_object != null)
                    {
                        float _objectPlacementBounds = 150f * (Main.IslandSize / (StrandedWorld.ZONE_HEIGHTMAP_SIZE - 1));

                        bool m_isObjectPlaceable = (bool)fi_m_isObjectPlaceable.GetValue(__instance);
                        LE_Object m_previewInstance = fi_m_previewInstance.GetValue(__instance) as LE_Object;
                        RaycastHit m_cursorHitInfo = (RaycastHit)fi_m_cursorHitInfo.GetValue(__instance);

                        //__instance.SetDragMessageInUI();
                        mi_SetDragMessageInUI.Invoke(__instance, new object[] { });
                        if (!__instance.IsInteractable)
                        {
                            if (m_previewInstance != null)
                            {
                                UnityEngine.Object.Destroy(m_previewInstance.gameObject);
                            }
                        }
                        else if (__instance.IsCursorOverSomething && m_cursorHitInfo.point.x < _objectPlacementBounds && m_cursorHitInfo.point.x > -_objectPlacementBounds && m_cursorHitInfo.point.z < _objectPlacementBounds && m_cursorHitInfo.point.z > -_objectPlacementBounds)
                        {
                            //if (__instance.IsObjectDraggedInUI())
                            if ((bool)mi_IsObjectDraggedInUI.Invoke(__instance, new object[] { }))
                            {
                                //if (__instance.OnObjectDrag() && ((m_object.SnapType != LE_Object.ESnapType.SNAP_TO_TERRAIN && m_object.SnapType != LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) || (__instance.m_cursorHitInfo.collider != null && __instance.m_cursorHitInfo.collider.gameObject.layer == __instance.TERRAIN_LAYER)))
                                if ((bool)mi_OnObjectDrag.Invoke(__instance, new object[] { }) && ((m_object.SnapType != LE_Object.ESnapType.SNAP_TO_TERRAIN && m_object.SnapType != LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) 
                                    || (m_cursorHitInfo.collider != null && m_cursorHitInfo.collider.gameObject.layer == __instance.TERRAIN_LAYER)))
                                {
                                    if (m_previewInstance == null)
                                    {
                                        m_previewInstance = UnityEngine.Object.Instantiate<LE_Object>(m_object);
                                        fi_m_previewInstance.SetValue(__instance, m_previewInstance);

                                        m_previewInstance.gameObject.GetComponent<Transform>().parent = Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer;
                                        //__instance.MoveToLayer(m_previewInstance.transform, LayerMask.NameToLayer("Ignore Raycast"));
                                        mi_MoveToLayer.Invoke(__instance, new object[] { m_previewInstance.transform, LayerMask.NameToLayer("Ignore Raycast") });

                                        Rigidbody[] componentsInChildren = m_previewInstance.GetComponentsInChildren<Rigidbody>();
                                        for (int i = 0; i < componentsInChildren.Length; i++)
                                        {
                                            UnityEngine.Object.Destroy(componentsInChildren[i]);
                                        }
                                        if (m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
                                        {
                                            //__instance.AddGridSnapping(m_previewInstance, true);
                                            mi_AddGridSnapping.Invoke(__instance, new object[] { m_previewInstance, true });
                                        }
                                    }
                                    m_previewInstance.transform.position = m_cursorHitInfo.point;
                                    if (m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
                                    {
                                        m_previewInstance.transform.position += m_cursorHitInfo.normal * 0.005f;
                                    }
                                    if (m_previewInstance.IsPlacementRotationByNormal)
                                    {
                                        m_previewInstance.transform.up = m_cursorHitInfo.normal;
                                    }
                                }
                            }
                            else if (m_isObjectPlaceable)
                            {
                                //PlaceObject();
                                mi_PlaceObject.Invoke(__instance, new object[] { });
                            }
                            else if (m_previewInstance != null)
                            {
                                UnityEngine.Object.Destroy(m_previewInstance.gameObject);
                            }
                        }
                        else if (m_previewInstance != null)
                        {
                            UnityEngine.Object.Destroy(m_previewInstance.gameObject);
                        }
                        if (LE_GUIInterface.Instance.delegates.SetDraggableObjectState != null)
                        {
                            //if (!__instance.IsInteractable || !__instance.IsObjectDraggedInUI())
                            if (!__instance.IsInteractable || !(bool)mi_IsObjectDraggedInUI.Invoke(__instance, new object[] { }))
                            {
                                LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.NONE);
                                return false;
                            }
                            if (m_previewInstance != null)
                            {
                                LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.IN_3D_PREVIEW);
                                return false;
                            }
                            LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.NOT_PLACEABLE);
                        }
                    }
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_GUI3dObject.UpdateNewObjectDragAndDrop : " + e);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LE_GUI3dObject), "PlaceObject", new Type[] { typeof(LE_Object), typeof(Transform), typeof(bool), typeof(bool) })]
        class LE_GUI3dObject_PlaceObject_Patch
        {
            static bool Prefix(LE_GUI3dObject __instance, LE_Object p_prefab, Transform p_copyTransform, bool p_isDestroyClonedScripts, ref LE_Object __result, bool clonedOffset = false)
            {
                try
                {
                    Vector3 position = p_copyTransform.position;
                    position.x += 1f;
                    // 1.0.38 compatibility
                    if (position.XZSqr() > (float)Math.Pow(ZoneHalfSize, 2))
                    {
                        return false;
                    }
                    LE_Object le_Object = UnityEngine.Object.Instantiate<LE_Object>(p_prefab);
                    le_Object.gameObject.name = p_prefab.name;
                    le_Object.gameObject.GetComponent<Transform>().parent = Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer;
                    if (clonedOffset)
                    {
                        le_Object.transform.position = p_copyTransform.position + new Vector3(1f, 0f, 0f);
                    }
                    else
                    {
                        le_Object.transform.position = p_copyTransform.position;
                    }
                    le_Object.transform.rotation = p_copyTransform.rotation;
                    le_Object.transform.localScale = p_copyTransform.localScale;
                    if (p_isDestroyClonedScripts)
                    {
                        LE_ObjectEditHandle componentInChildren = le_Object.GetComponentInChildren<LE_ObjectEditHandle>();
                        if (componentInChildren != null)
                        {
                            UnityEngine.Object.Destroy(componentInChildren.gameObject);
                        }
                        S_SnapToWorld component = le_Object.GetComponent<S_SnapToWorld>();
                        if (component != null)
                        {
                            UnityEngine.Object.Destroy(component);
                        }
                        S_SnapToGrid component2 = le_Object.GetComponent<S_SnapToGrid>();
                        if (component2 != null)
                        {
                            UnityEngine.Object.Destroy(component2);
                        }
                        S_SnapToObject[] componentsInChildren = le_Object.GetComponentsInChildren<S_SnapToObject>(true);
                        for (int i = 0; i < componentsInChildren.Length; i++)
                        {
                            LE_ObjectSnapPoint.DestroySnapSystem(componentsInChildren[i]);
                        }
                        UtilityOnDestroyHandler component3 = le_Object.GetComponent<UtilityOnDestroyHandler>();
                        if (component3 != null)
                        {
                            component3.DestroyWithoutHandling();
                        }
                    }
                    mi_OnNewObjectPlaced.Invoke(__instance, new object[] { le_Object });

                    __result = le_Object;
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_GUI3dObject_PlaceObject_Patch : " + e);
                }
                return true;
            }
        }
    }
}
