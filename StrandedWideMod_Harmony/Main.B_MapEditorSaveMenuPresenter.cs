using Beam;
using Beam.AccountServices;
using Beam.Language;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Terrain;
using Beam.UI;
using Beam.UI.MapEditor;
using Funlabs;
using HarmonyLib;
using LE_LevelEditor;
using LE_LevelEditor.Core;
using Rewired;
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
        static FieldInfo fi_view = AccessTools.Field(typeof(MapEditorSaveMenuPresenter), "_view");
        static MethodInfo mi_OnSaved = AccessTools.Method(typeof(MapEditorSaveMenuPresenter), "OnSaved");

        [HarmonyPatch(typeof(MapEditorSaveMenuPresenter), "SaveMap")]
        class MapEditorSaveMenuPresenter_SaveMap_Patch
        {
            static bool Prefix(MapEditorSaveMenuPresenter __instance)
            {
                try
                {
                    __instance.CurrentMap.HeightmapData = Singleton<LE_ZoneTileGenerator>.Instance.terrain.terrainData.GetHeights(0, 0, IslandSize + 1, IslandSize + 1);
                    JObject jobject = new JObject();
                    foreach (LE_Object le_Object in Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer.GetComponentsInChildren<LE_Object>(true))
                    {
                        JObject jobject2 = new JObject();
                        jobject2.AddField("prefab", Prefabs.ConvertPrefabIdToLegacy(le_Object.Id));
                        jobject2.AddField("reference", MiniGuid.New().ToString());
                        JObject data = JSerializer.Serialize(le_Object.transform);
                        jobject2.AddField("Transform", data);
                        jobject.Add(jobject2);
                    }
                    __instance.CurrentMap.ObjectData = jobject;
                    __instance.CurrentMap.EditorData.Author = GAccountService.Get().LocalUser.UserInfo.DisplayName;
                    IMapEditorSaveMenuViewAdapter view = fi_view.GetValue(__instance) as IMapEditorSaveMenuViewAdapter;
                    __instance.CurrentMap.EditorData.Name = view.NameInputField.Text.Trim();
                    __instance.CurrentMap.EditorData.Description = view.DescriptionInputField.Text.Trim();
                    __instance.CurrentMap.EditorData.VersionNumber++;
                    bool flag = false;
                    string text = "";
                    try
                    {
                        Maps.SaveMap(__instance.CurrentMap, FilePath.LOCAL_MAPS_FOLDER);
                        flag = true;
                    }
                    catch (Exception ex)
                    {

                        flag = false;
                        text = ex.Message;
                    }
                    if (flag)
                    {
                        mi_OnSaved.Invoke(__instance, new object[] { });

                    }
                    else
                    {
                        Maps.DeleteMap(__instance.CurrentMap);
                        string message = Localization.GetLanguageHandler().Localize("MAP_EDITOR_SAVE_MENU_DB_ERROR_SAVING_MAP_DESC", new string[]
                        {
                    text
                        });
                        DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning, DialogueType.Base);
                    }
                    __instance.Hide();
                    // skip original method
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : error while patching MapEditorSaveMenuPresenter.SaveMap : " + e);
                }
                return true;
            }
        }
    }
}
