using HarmonyLib;
using LE_LevelEditor;
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
        private static void EnlargeBoundaries(LE_LevelEditorMain levelEditor)
        {
            if (levelEditor.Boundary != null)
            {
                for (int i = 0; i < levelEditor.Boundary.transform.childCount; i++)
                {
                    Transform tchild = levelEditor.Boundary.transform.GetChild(i);

                    if (tchild.gameObject != null)
                    {
                        Debug.Log(modName + " enlarging map editor boundaries for larger islands");
                        Debug.Log("LevelEditorMain:: BoundaryChild object : " + tchild.gameObject.name + " / scale " + tchild.localScale + " / position " + tchild.localPosition);
                    }

                    tchild.localScale = new Vector3(200f, 150f, 100f);

                    if (tchild.localPosition.x < 0 && Math.Round(tchild.localPosition.z) == 0)
                    {
                        //(-210.0, 0.0, 0.0) 
                        tchild.localPosition = new Vector3(-210, 0, 210);
                        continue;
                    }
                    if (tchild.localPosition.x > 0 && Math.Round(tchild.localPosition.z) == 0)
                    {
                        //(210.0, 0.0, 0.0)    
                        tchild.localPosition = new Vector3(420, 0, 210);
                        continue;
                    }
                    if (tchild.localPosition.z > 0 && Math.Round(tchild.localPosition.x) == 0)
                    {
                        //(0.0, 0.0, 210.0)
                        tchild.localPosition = new Vector3(210, 0, 420);
                        continue;
                    }
                    if (tchild.localPosition.z < 0 && Math.Round(tchild.localPosition.x) == 0)
                    {
                        //(0.0, 0.0, -210.0) 
                        tchild.localPosition = new Vector3(210, 0, -210);
                        continue;
                    }
                }
                // Hide the buoy barrier
                Debug.Log(modName + " Hide the editor buoy barrier");
                Renderer[] renderers = Beam.Game.FindObjectsOfType<Renderer>();
                foreach (Renderer r in renderers)
                {
                    if (r.gameObject != null
                        && !r.gameObject.name.Contains("_Impostor")
                        && r.gameObject.name.Contains("Barrier"))
                    {
                        r.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LE_LevelEditorMain), "Initialize_InSecondUpdate")]
        class LE_LevelEditorMain_Initialize_InSecondUpdate_Patch
        {
            static void Postfix(LE_LevelEditorMain __instance)
            {
                try
                {
                    // boundary size change
                    EnlargeBoundaries(__instance);
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching postfix LE_LevelEditorMain.Initialize_InSecondUpdate : " + e);
                }
            }
        }

        [HarmonyPatch(typeof(LE_LevelEditorMain), "LE_LevelEditor.LEInput.LE_IInputHandler.MoveCamera")]
        class LE_LevelEditorMain_MoveCamera_Patch
        {
            static void Postfix(LE_LevelEditorMain __instance, Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords, bool scrolling)// = false)
            {
                try
                {
                    if (scrolling && __instance.IsMouseOverGUI())
                    {
                        return;
                    }
                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, -Main._editorCameraBoxWidth, Main._editorCameraBoxWidth * (float)IslandSizeRatio), Mathf.Clamp(cam.transform.position.y, -100f, 150f), Mathf.Clamp(cam.transform.position.z, -Main._editorCameraBoxWidth, Main._editorCameraBoxWidth * (float)IslandSizeRatio));
                    }
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_LevelEditorMain.LE_IInputHandler.MoveCamera : " + e);
                }
            }
        }

        [HarmonyPatch(typeof(LE_LevelEditorMain), "EstimateDistanceToLevel")]
        class LE_LevelEditorMain_EstimateDistanceToLevel_Patch
        {
            static void Postfix(LE_LevelEditorMain __instance, int p_mode, ref float __result)
            {
                try
                {
                    if (__result == 300f)
                    {
                        __result = 600f;
                    }
                    // skip original method
                    //return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching LE_LevelEditorMain.EstimateDistanceToLevel : " + e);
                }
                //return true;
            }
        }
    }
}
