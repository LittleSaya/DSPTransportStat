using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DSPTransportStat.CacheObjects;
using DSPTransportStat.Translation;

namespace DSPTransportStat
{
    [BepInPlugin(__GUID__, __NAME__, __VERSION__)]
    public class Plugin : BaseUnityPlugin
    {
        public const string __NAME__ = "DSPTransportStat";
        public const string __GUID__ = "IndexOutOfRange.DSPTransportStat";
        public const string __VERSION__ = "0.0.6";

        static public Plugin Instance { get; set; } = null;

        /// <summary>
        /// 插件日志对象
        /// </summary>
        new public ManualLogSource Logger { get => base.Logger; }

        private KeyboardShortcut transportStationsWindowShortcut;

        private UITransportStationsWindow uiTransportStationsWindow;

        private void Awake ()
        {
            Instance = this;

            transportStationsWindowShortcut = KeyboardShortcut.Deserialize("F + LeftControl");

            Harmony harmony = new Harmony(__GUID__);
            harmony.PatchAll(typeof(Patch));
        }

        private void Update ()
        {
            if (!GameMain.isRunning || GameMain.isPaused || GameMain.instance.isMenuDemo || VFInput.inputing)
            {
                return;
            }

            if (VFInput.inputing)
            {
                return;
            }

            if (transportStationsWindowShortcut.IsDown())
            {
                ToggleTransportStationsWindow();
            }
            //else if (VFInput._closePanelE)
            //{
            //    if (uiTransportStationsWindow.active)
            //    {
            //        uiTransportStationsWindow._Close();
            //    }
            //}
        }

        private void ToggleTransportStationsWindow ()
        {
            if (uiTransportStationsWindow.active)
            {
                uiTransportStationsWindow._Close();
            }
            else
            {
                uiTransportStationsWindow._Open();
                uiTransportStationsWindow.transform.SetAsLastSibling();
                uiTransportStationsWindow.ComputeTransportStationsWindow_LoadStations();
            }
        }

        static class Patch
        {
            static private bool isUIGameCreated = false;

            static private bool isUIGameInitialized = false;

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            static private void UIGame__OnCreate_Postfix ()
            {
                if (!isUIGameCreated)
                {
                    isUIGameCreated = true;
                    ResourceCache.InitializeResourceCache();
                    bool success = NativeObjectCache.InitializeNativeObjectCache(out string missingInfo);
                    if (!success)
                    {
                        Instance.Logger.LogError(missingInfo);
                    }
                    Strings.InitializeTranslations(UIRoot.instance.optionWindow.GetTempOption().language);
                    ReassembledObjectCache.InitializeReassembledObjectCache();
                    Instance.uiTransportStationsWindow = UITransportStationsWindow.Create();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnFree")]
            static public void UIGame__OnFree_Postfix ()
            {
                Instance.uiTransportStationsWindow._Free();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnUpdate")]
            static public void UIGame__OnUpdate_Postfix ()
            {
                if (GameMain.isPaused || !GameMain.isRunning)
                {
                    return;
                }
                Instance.uiTransportStationsWindow._Update();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnDestroy")]
            static public void UIGame__OnDestroy_Postfix ()
            {
                Instance.uiTransportStationsWindow._Destroy();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnInit")]
            static private void UIGame__OnInit_Postfix (UIGame __instance)
            {
                if (!isUIGameInitialized)
                {
                    isUIGameInitialized = true;
                    Instance.uiTransportStationsWindow._Init(Instance.uiTransportStationsWindow.data);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
            public static void UIGame_ShutAllFunctionWindow_Postfix ()
            {
                Instance.uiTransportStationsWindow._Close();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(VFInput), "_cameraZoomIn", MethodType.Getter)]
            static private void VFInput__cameraZoomIn_Postfix (ref float __result)
            {
                if (Instance.uiTransportStationsWindow != null && Instance.uiTransportStationsWindow.IsPointerInside)
                {
                    __result = 0f;
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(VFInput), "_cameraZoomOut", MethodType.Getter)]
            static private void VFInput__cameraZoomOut_Postfix (ref float __result)
            {
                if (Instance.uiTransportStationsWindow != null && Instance.uiTransportStationsWindow.IsPointerInside)
                {
                    __result = 0f;
                }
            }
        }
    }
}
