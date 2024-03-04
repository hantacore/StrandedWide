using Beam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace StrandedWideMod_Harmony
{
    static partial class Main
    {
        private static float splashCanvasDefaultScreenWitdh = 1024f;
        private static float splashCanvasDefaultScreenHeight = 768f;

        private static GameObject canvas;
        private static Image imgBackground;

        private static void UpdateSplashCanvas()
        {
            if (canvas != null)
            {
                bool showCanvas = (Beam.Game.State == GameState.MAIN_MENU);
                if (showCanvas)
                {
                    Beam.UI.UCartographerMenuViewAdapter cartoview = Beam.Game.FindObjectOfType<Beam.UI.UCartographerMenuViewAdapter>();
                    if (cartoview != null)
                    {
                        showCanvas = showCanvas && !cartoview.Visible;
                    }
                    Beam.UI.UOptionsMenuViewAdapter optionsview = Beam.Game.FindObjectOfType<Beam.UI.UOptionsMenuViewAdapter>();
                    if (optionsview != null)
                    {
                        showCanvas = showCanvas && !optionsview.Visible;
                    }
                    Beam.UI.ULeaderboardsMenuViewAdapter leaderbview = Beam.Game.FindObjectOfType<Beam.UI.ULeaderboardsMenuViewAdapter>();
                    if (leaderbview != null)
                    {
                        showCanvas = showCanvas && !leaderbview.Visible;
                    }
                    Beam.UI.NewGameMenuViewAdapterBase newGameView = Beam.Game.FindObjectOfType<Beam.UI.NewGameMenuViewAdapterBase>();
                    if (newGameView != null)
                    {
                        showCanvas = showCanvas && !newGameView.Visible;
                    }
                    Beam.UI.OptionsMenuViewAdapterBase optionsView = Beam.Game.FindObjectOfType<Beam.UI.OptionsMenuViewAdapterBase>();
                    if (optionsView != null)
                    {
                        showCanvas = showCanvas && !optionsView.Visible;
                    }
                    Beam.UI.LobbyMenuViewAdapterBase lobbyView = Beam.Game.FindObjectOfType<Beam.UI.LobbyMenuViewAdapterBase>();
                    if (lobbyView != null)
                    {
                        showCanvas = showCanvas && !lobbyView.Visible;
                    }
                }

                canvas.SetActive(showCanvas);
                return;
            }

            Debug.Log(modName + " init splash canvas");

            //Create main Canvas
            canvas = createCanvas(false);

            //Create your Image GameObject
            GameObject bgGO = new GameObject("SWideSplashBackground_Sprite");

            //Make the GameObject child of the Canvas
            bgGO.transform.SetParent(canvas.transform);

            //Add Image Component to it(This will add RectTransform as-well)
            imgBackground = bgGO.AddComponent<Image>();
            imgBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 132);
            imgBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 254);

            Texture2D backgroundImage = new Texture2D(509, 265, TextureFormat.ARGB32, false);
            backgroundImage.LoadImage(ExtractResource("StrandedWideMod_Harmony.assets.Splash.stranded_wide_white_cutout.png"));
            Sprite bgSprite = Sprite.Create(backgroundImage, new Rect(0, 0, 509, 265), new Vector2(254, 132));
            imgBackground.sprite = bgSprite;

            //Center Image to screen
            bgGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(300, -170);

            // default hide
            if (canvas != null)
                canvas.SetActive(true);
        }

        public static byte[] ExtractResource(String filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (System.IO.Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        #region Canvas instanciation

        //Creates Hidden GameObject and attaches Canvas component to it
        private static GameObject createCanvas(bool hide, string name = "SWideModCanvas")
        {
            //Create Canvas GameObject
            GameObject tempCanvas = new GameObject(name);
            if (hide)
            {
                tempCanvas.hideFlags = HideFlags.HideAndDontSave;
            }

            //Create and Add Canvas Component
            Canvas cnvs = tempCanvas.AddComponent<Canvas>();
            cnvs.renderMode = RenderMode.ScreenSpaceOverlay;
            cnvs.pixelPerfect = false;

            //Set Cavas sorting order to be above other Canvas sorting order
            cnvs.sortingOrder = 12;

            cnvs.targetDisplay = 0;

            CanvasGroup cg = tempCanvas.AddComponent<CanvasGroup>();
            cg.alpha = 0.25f;

            addCanvasScaler(tempCanvas);
            addGraphicsRaycaster(tempCanvas);
            return tempCanvas;
        }

        //Adds CanvasScaler component to the Canvas GameObject 
        private static void addCanvasScaler(GameObject parentaCanvas)
        {
            CanvasScaler cvsl = parentaCanvas.AddComponent<CanvasScaler>();
            cvsl.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cvsl.referenceResolution = new Vector2(splashCanvasDefaultScreenWitdh, splashCanvasDefaultScreenHeight);
            cvsl.matchWidthOrHeight = 0.5f;
            cvsl.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cvsl.referencePixelsPerUnit = 100f;
        }

        //Adds GraphicRaycaster component to the Canvas GameObject 
        private static void addGraphicsRaycaster(GameObject parentaCanvas)
        {
            GraphicRaycaster grcter = parentaCanvas.AddComponent<GraphicRaycaster>();
            grcter.ignoreReversedGraphics = true;
            grcter.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        }

        #endregion
    }
}
