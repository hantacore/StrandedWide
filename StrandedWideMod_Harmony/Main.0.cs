using Beam;
using Beam.UI;
using Beam.Utilities;
using HarmonyLib;
using StrandedDeepModsUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace StrandedWideMod_Harmony
{
    public static partial class Main
    {
        // known issues
        //nutmeg521 : You don't seem to be able to place items all the way to the edge of the build-able area.
        //Also, if you try to move an object after placing it down, it glitches to the middle point of the island and you can then only move it within one quarter of the map.


        private static Harmony harmony;
        public static TMPLabelViewAdapter _versionNumberLabel = null;
        public static bool labelIsDone = false;
        public static bool optionsReloaded = false;
        public static bool worldReloaded = false;
        public static string modEditionName = "Wide";
        public static string modName = "Stranded Wide (Harmony edition)";
        public static string sdeepOptionsFileOriginalPath = "";
        public static string _infojsonlocation = "https://raw.githubusercontent.com/hantacore/StrandedDeepMods/main/StrandedWide/StrandedWide/StrandedWideMod_Harmony/Info.json";

        public static bool mixProceduralAndCustom = true;
        public static float customIslandsRatio = 0.25f;

        public static string ModEditionName
        {
            get
            {
                return modEditionName;
            }
        }

        public static string _modVersion = UnityModManager.FindMod("StrandedWideMod").Version.ToString();//"Beta 6";

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                ////Debug.Log(modName + " current directory = " + Directory.GetCurrentDirectory());
                //UnityModManager.FindMod("StrandedWideMod").Path
                //string utilitiesPath = Path.Combine(Directory.GetCurrentDirectory(), @"Mods\" + Assembly.GetExecutingAssembly().GetName().Name + @"\StrandedDeepModsUtilities.dll");
                ////Debug.Log(modName + " utilities directory = " + utilitiesPath);
                //if (File.Exists(utilitiesPath))
                //{
                //    try
                //    {
                //        //Assembly a = Assembly.LoadFile(utilitiesPath);
                //        //Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"Stranded_Deep_Data\Managed\"));
                //        //Debug.Log(modName + " utilities assembly name : " + a.GetName());
                //        StrandedDeepModsUtilities.ModUtilities.IsStrandedWide();
                //    }
                //    catch (Exception e)
                //    {
                //        Debug.Log(modName + " utilities not loaded " + e);
                //    }
                //}

                //foreach(MethodInfo mi in typeof(LE_LevelEditor.LE_LevelEditorMain).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                //{
                //    Debug.Log("Stranded Wide (Harmony edition) : LE_LevelEditorMain method name = " + mi.Name);
                //}

                //foreach(Type type in typeof(PiscusFollower).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                //{
                //    Debug.Log("Stranded Wide (Harmony edition) : PiscusFollower nested type name = " + type.Name);
                //}

                modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);
                modEntry.OnGUI = Main.OnGUI;
                modEntry.OnHideGUI = OnHideGUI;
                modEntry.OnUnload = Main.Unload;
                modEntry.OnToggle = Main.OnToggle;

                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                sdeepOptionsFileOriginalPath = FilePath.OPTIONS_FILE;
                PatchSaveFolders();

                string newOptionsFile = AccessTools.Field(typeof(FilePath), "OPTIONS_FILE").GetValue(null) as string;

                // Clone Standard SD options
                if (Directory.Exists(FilePath.SAVE_FOLDER))
                {
                    if (File.Exists(sdeepOptionsFileOriginalPath)
                        && !File.Exists(newOptionsFile))
                    {
                        // copy file from SDeep
                        Debug.Log("Stranded Wide (Harmony edition) : copying options from Stranded Deep vanilla");
                        Directory.CreateDirectory(Path.GetDirectoryName(newOptionsFile));
                        File.Copy(sdeepOptionsFileOriginalPath, newOptionsFile, true);
                        Debug.Log("Stranded Wide (Harmony edition) : options successfully copied from Stranded Deep vanilla");
                    }

                    // cloning mod config if does not already exists
                    string modConfigsDir = Path.GetDirectoryName(sdeepOptionsFileOriginalPath);
                    if (Directory.Exists(modConfigsDir))
                    {
                        foreach (string modconfigfilename in Directory.GetFiles(modConfigsDir))
                        {
                            string targetFilename = modconfigfilename.Replace(Path.GetDirectoryName(sdeepOptionsFileOriginalPath), Path.GetDirectoryName(newOptionsFile));
                            if (!File.Exists(targetFilename))
                            {
                                Debug.Log("Stranded Wide (Harmony edition) : cloning vanilla mod config : " + targetFilename);
                                File.Copy(modconfigfilename, targetFilename, false);
                            }
                        }
                    }
                }

                VersionChecker.CheckVersion(modEntry, _infojsonlocation);

                ReadConfig();

                Main._zoneLoadDistance = IslandSize - 6;
                Debug.Log("Stranded Wide (Harmony edition) : Load _zoneLoadDistance = " + _zoneLoadDistance);
                Main._zoneUnloadDistance = _zoneLoadDistance - 10;
                Debug.Log("Stranded Wide (Harmony edition) : Load _zoneUnloadDistance = " + _zoneUnloadDistance);

                Debug.Log(modName + " Successfully started. ");

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Stranded Wide (Harmony edition) : error on Load : " + e);
            }
            return false;
        }

        private static void UpdateGameVersionLabel()
        {
            // show Stranded Wide label
            if (_versionNumberLabel == null)
            {
                UMainMenuViewAdapter mainMenu = Game.FindObjectOfType<UMainMenuViewAdapter>();
                if (mainMenu != null
                    && mainMenu.Visible)
                {

                    FieldInfo fi_versionNumberLabel = AccessTools.Field(typeof(UMainMenuViewAdapter), "_versionNumberLabel");
                    _versionNumberLabel = fi_versionNumberLabel.GetValue(mainMenu) as TMPLabelViewAdapter;
                    if (_versionNumberLabel != null
                        && !String.IsNullOrEmpty(_versionNumberLabel.Text)
                        && !_versionNumberLabel.Text.Contains("Wide"))
                    {
                        _versionNumberLabel.Text = _versionNumberLabel.Text + " - Stranded Wide (" + Main._modVersion + " Harmony edition)";
                        labelIsDone = true;
                    }
                }
            }
        }

        private static void PatchSaveFolders()
        {
            try
            {
                string newSaveFolder = PathTools.Combine(new string[]
                {
                    FilePath.USER_FOLDER.Replace("Deep", modEditionName),
                    FilePath.SAVE_ROOT
                });

                PatchFolder("SAVE_FOLDER", newSaveFolder);

                PatchFolder("OPTIONS_FILE", PathTools.Combine(new string[]
                {
                    newSaveFolder,
                    "Options.json"
                }));

                PatchFolder("WORLD_FOLDER", PathTools.Combine(new string[]
                {
                    newSaveFolder,
                    FilePath.GetSlotFolderName(0),
                    "World"
                }));

                PatchFolder("HEIGHTMAPS_FOLDER", PathTools.Combine(new string[]
                {
                    newSaveFolder,
                    FilePath.GetSlotFolderName(0),
                    "Heightmaps"
                }));

                PatchFolder("LOCAL_MAPS_FOLDER", PathTools.Combine(new string[]
                {
                    newSaveFolder,
                    "Maps"
                }));

                PatchFolder("PREFS_FALLBACK_FOLDER", PathTools.Combine(new string[]
                {
                    newSaveFolder,
                    ".prefs/"
                }));
            }
            catch (Exception e)
            {
                Debug.Log("Stranded Wide (Harmony edition) : error while patching FilePath : " + e);
            }
        }

        private static void PatchFolder(string folder, string path)
        {
            Debug.Log("Stranded Wide (Harmony edition) : " + folder + " new value " + path);

            //FieldInfo field = AccessTools.Field(typeof(FilePath), folder);
            FieldInfo field = typeof(FilePath).GetField(folder, BindingFlags.Static | BindingFlags.Public);
            field.SetValue(null, path);

            Debug.Log("Stranded Wide (Harmony edition) : " + folder + " patched to " + field.GetValue(null));
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            if (value)
            {
                return true;
            }

            modEntry.Active = true;
            modEntry.Enabled = true;
            return false; // If true, the mod will switch the state. If not, the state will not change.
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("<b>Stranded Wide mod by Hantacore and Stranded Wide Team</b>");
            GUILayout.Label("Team Discord : https://discord.gg/UKwtkzZgEU");
            if (Game.State != GameState.LOAD_GAME 
                && Game.State != GameState.NEW_GAME
                && Game.State != GameState.INTRO)
            {
                bool mustRegenerate = (IslandSizeBuffer != IslandSize || ZoneSpacing != ZoneSpacingBuffer || IslandsCount != IslandsCountBuffer);

                GUILayout.Label("<color=orange>Changing these values demand a new world generation</color>" + (mustRegenerate ? " <color=red>Regeneration needed !</color>" : ""));
                mixProceduralAndCustom = GUILayout.Toggle(mixProceduralAndCustom, "Mix procedural islands and custom islands");
                GUILayout.Label("Custom islands density = " + customIslandsRatio);
                customIslandsRatio = GUILayout.HorizontalSlider(customIslandsRatio, 0.1f, 1.0f);
                GUILayout.Label("<b><color=orange>Don't change those values mid-game, I mean : really DON'T.</color></b>");

                GUILayout.Label("Island size = " + IslandSizeBuffer.ToString() + " (vanilla is " + (StrandedWorld.ZONE_HEIGHTMAP_SIZE - 1) + ")");
                IslandSizeBuffer = (int)Math.Pow(2, (int)GUILayout.HorizontalSlider((float)(Math.Log(IslandSizeBuffer / (StrandedWorld.ZONE_HEIGHTMAP_SIZE - 1)) / Math.Log(2)), 0, 2)) * (StrandedWorld.ZONE_HEIGHTMAP_SIZE - 1);

                GUILayout.Label("Island spacing = " + ZoneSpacingBuffer.ToString() + " (vanilla is " + StrandedWorld.ZONE_SPACING + ")");
                ZoneSpacingBuffer = GUILayout.HorizontalSlider(ZoneSpacingBuffer, IslandSizeBuffer >= 1024 ? 1.25f : 1.0f, 2.0f);

                GUILayout.Label("Island count = " + IslandsCountBuffer.ToString() + " (vanilla is " + StrandedWorld.WORLD_ZONES_SQUARED + ")");
                IslandsCountBuffer = (int)GUILayout.HorizontalSlider(IslandsCountBuffer, 10, 49);
            }
        }

        static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            WriteConfig();
        }

        public static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            try
            {
                UpdateSplashCanvas();

                if (!optionsReloaded
                    && FilePath.OPTIONS_FILE.Contains(modEditionName))
                {
                    // Force reload options for swide
                    if (Directory.Exists(FilePath.SAVE_FOLDER)
                        && File.Exists(FilePath.OPTIONS_FILE))
                    {
                        Debug.Log("Stranded Wide (Harmony edition) : reloading options from " + FilePath.OPTIONS_FILE);
                        Options.Load();
                    }

                    Debug.Log("Stranded Wide (Harmony edition) : reloading options last slot = " + Options.GeneralSettings.LastSaveSlotUsed);
                    SaveManager.ChangeCurrentSlot(Options.GeneralSettings.LastSaveSlotUsed);

                    optionsReloaded = true;
                }

                if (optionsReloaded
                    && !worldReloaded)
                {
                    Debug.Log("Stranded Wide (Harmony edition) : reloading world");
                    MainMenuPresenter mmp = Game.FindObjectOfType<MainMenuPresenter>();
                    if (mmp != null)
                    {
                        MethodInfo mi_ChangeSaveSlotMenuPresenter_SlotChanged = AccessTools.Method(typeof(MainMenuPresenter), "ChangeSaveSlotMenuPresenter_SlotChanged");
                        mi_ChangeSaveSlotMenuPresenter_SlotChanged.Invoke(mmp, new object[] { });

                        worldReloaded = true;
                    }
                }

                if (!labelIsDone
                    && Beam.Game.State == GameState.MAIN_MENU)
                {
                    UpdateGameVersionLabel();
                }

                if (Beam.Game.State == GameState.NEW_GAME
                    || Beam.Game.State == GameState.LOAD_GAME)
                {
                    labelIsDone = false;
                    //if (WorldUtilities.IsWorldLoaded())
                    //{
                    //    Debug.Log("Stranded Wide (Harmony edition) : Update StrandedWorld Zones count : " + StrandedWorld.Instance.Zones.Length);
                    //    Debug.Log("Stranded Wide (Harmony edition) : Update World.MapList count : " + Beam.Terrain.World.MapList.Length);
                    //}
                }
                else
                {
                    Reset();
                }
            }
            catch (Exception e)
            {
                Debug.Log("Stranded Wide (Harmony edition) : error OnUpdate : " + e);
            }
        }

        private static void Reset()
        {
            
        }

        private static bool Unload(UnityModManager.ModEntry modEntry)
        {
            harmony.UnpatchAll(modEntry.Info.Id);
            return true;
        }

        //private static void AddShortcutToLogFile(string linkName)
        //{
        //    string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        //    using (StreamWriter writer = new StreamWriter(deskDir + "\\" + linkName + ".url"))
        //    {
        //        string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
        //        writer.WriteLine("[InternetShortcut]");
        //        writer.WriteLine("URL=file:///" + app);
        //        writer.WriteLine("IconIndex=0");
        //        string icon = app.Replace('\\', '/');
        //        writer.WriteLine("IconFile=" + icon);
        //    }
        //}
    }
}
