using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat
{
    /// <summary>
    /// 使用特殊方式（反射、反编译）实现的功能，更容易在游戏版本更新时失效
    /// </summary>
    static class Hacking
    {
        static private UIStationWindow currentStationWindow = null;

        /// <summary>
        /// 打开任意一个物流运输站的站点窗口
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="stationId"></param>
        static public void OpenStationWindowOfAnyStation (PlanetFactory factory, int stationId)
        {
            UIStationWindow win = UIRoot.instance.uiGame.stationWindow;

            // 模拟对 ManualBehaviour._Open 和 UIStationWindow._OnOpen 的调用
            // 这个模拟调用的过程与原有过程不同，以实现打开任意目标站点面板的功能（即使玩家与目标站点不在同一个星球上）
            if (win.inited && win.active)
            {
                win._Close();
            }

            if (win.inited && !win.active)
            {
                typeof(ManualBehaviour).GetProperty("active").SetValue(win, true);
                if (!win.gameObject.activeSelf)
                {
                    win.gameObject.SetActive(value: true);
                }

                try
                {
                    win.factory = factory;
                    win.factorySystem = factory.factorySystem;
                    win.player = GameMain.mainPlayer;
                    win.powerSystem = factory.powerSystem;
                    win.transport = factory.transport;
                    win.stationId = stationId;

                    if (win.active)
                    {
                        win.nameInput.onValueChanged.AddListener((s) => typeof(UIStationWindow).GetMethod("OnNameInputSubmit").Invoke(win, new object[1] { s }));
                        win.nameInput.onEndEdit.AddListener((s) => typeof(UIStationWindow).GetMethod("OnNameInputSubmit").Invoke(win, new object[1] { s }));
                        currentStationWindow = win;
                        win.player.onIntendToTransferItems += OnPlayerIntendToTransferItems;
                    }
                    win.transform.SetAsLastSibling();
                }
                catch (Exception message)
                {
                    Plugin.Instance.Logger.LogError(message);
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UIStationWindow), "_OnClose")]
        static void UIStationWindow__OnClose_Prefix ()
        {
            if (currentStationWindow != null && currentStationWindow.player != null)
            {
                currentStationWindow.player.onIntendToTransferItems -= OnPlayerIntendToTransferItems;
            }
        }

        static private void OnPlayerIntendToTransferItems (int _itemId, int _itemCount, int _itemInc)
        {
            MethodInfo method = typeof(UIStationWindow).GetMethod("OnPlayerIntendToTransferItems", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(currentStationWindow, new object[3] { _itemId, _itemCount, _itemInc });
        }
    }
}
