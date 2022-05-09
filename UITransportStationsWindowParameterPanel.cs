using DSPTransportStat.CacheObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DSPTransportStat.Extensions;
using DSPTransportStat.Global;
using DSPTransportStat.Translation;

namespace DSPTransportStat
{
    class UITransportStationsWindowParameterPanel : MonoBehaviour
    {
        /// <summary>
        /// 查询参数 - 是否显示行星内物流站点
        /// </summary>
        public bool ToggleInPlanet { get; set; }

        /// <summary>
        /// 查询参数 - 是否显示星际物流站点
        /// </summary>
        public bool ToggleInterstellar { get; set; }

        /// <summary>
        /// 查询参数 - 是否显示采集站
        /// </summary>
        public bool ToggleCollector { get; set; }

        /// <summary>
        /// 查询参数 - 过滤相关物品，只显示供应、需求或存储某项物品的物流站点
        /// 物品ID
        /// </summary>
        public int RelatedItemFilter { get; set; }

        /// <summary>
        /// 对外部窗口组件的引用，便于在查询参数发生变化时通知外部刷新界面内容
        /// </summary>
        private UITransportStationsWindow tsw;

        /// <summary>
        /// 创建左侧的查询参数面板
        /// </summary>
        static public UITransportStationsWindowParameterPanel Create (GameObject goTSWParamPanel, UITransportStationsWindow tsw)
        {
            UITransportStationsWindowParameterPanel cmpTSWParamPanel = goTSWParamPanel.AddComponent<UITransportStationsWindowParameterPanel>();

            // 传入外层界面的组件，方便在查询参数发生变化时通知外部刷新界面内容
            cmpTSWParamPanel.tsw = tsw;

            // 设置默认值

            // 设置大小和位置
            RectTransform cmpRectTransform = goTSWParamPanel.GetComponent<RectTransform>();
            cmpRectTransform.Zeroize();
            cmpRectTransform.anchorMax = new Vector2(0, 1);
            cmpRectTransform.anchorMin = new Vector2(0, 1);
            cmpRectTransform.offsetMax = new Vector2(10, -45);
            cmpRectTransform.offsetMin = new Vector2(-130, -400);

            // 创建 toggle-in-planet-label
            GameObject goToggleInPlanetLabel = new GameObject("toggle-in-planet-label", typeof(RectTransform), typeof(CanvasRenderer));
            goToggleInPlanetLabel.transform.SetParent(goTSWParamPanel.transform);

            RectTransform goToggleInPlanetLabel_cmpRectTransform = goToggleInPlanetLabel.GetComponent<RectTransform>();
            goToggleInPlanetLabel_cmpRectTransform.Zeroize();
            goToggleInPlanetLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInPlanetLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInPlanetLabel_cmpRectTransform.offsetMax = new Vector2(10, -10);
            goToggleInPlanetLabel_cmpRectTransform.offsetMin = new Vector2(10, -10);

            Text goToggleInPlanetLabel_cmpText = goToggleInPlanetLabel.AddComponent<Text>();
            goToggleInPlanetLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goToggleInPlanetLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goToggleInPlanetLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goToggleInPlanetLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ToggleInPlanetLabel;

            // 创建 toggle-in-planet
            GameObject goToggleInPlanet = Instantiate(NativeObjectCache.CheckBox, goTSWParamPanel.transform);
            goToggleInPlanet.name = "toggle-in-planet";

            // 比同一行的字符低2个单位，长宽均为20个单位
            RectTransform goToggleInPlanet_cmpRectTransform = goToggleInPlanet.GetComponent<RectTransform>();
            goToggleInPlanet_cmpRectTransform.Zeroize();
            goToggleInPlanet_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInPlanet_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInPlanet_cmpRectTransform.offsetMax = new Vector2(100, -12);
            goToggleInPlanet_cmpRectTransform.offsetMin = new Vector2(80, -32);

            Toggle goToggleInPlanet_cmpToggle = goToggleInPlanet.GetComponent<Toggle>();

            // UI 组件的默认状态
            goToggleInPlanet_cmpToggle.isOn = true;

            // 默认值
            cmpTSWParamPanel.ToggleInPlanet = true;

            // 监听
            goToggleInPlanet_cmpToggle.onValueChanged.AddListener(value =>
            {
                cmpTSWParamPanel.ToggleInPlanet = value;
                cmpTSWParamPanel.tsw.OnParameterChange();
            });

            // 创建 toggle-interstellar-label
            GameObject goToggleInterstellarLabel = Instantiate(goToggleInPlanetLabel, goTSWParamPanel.transform);
            goToggleInterstellarLabel.name = "toggle-interstellar-label";

            RectTransform goToggleInterstellarLabel_cmpRectTransform = goToggleInterstellarLabel.GetComponent<RectTransform>();
            goToggleInterstellarLabel_cmpRectTransform.Zeroize();
            goToggleInterstellarLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInterstellarLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInterstellarLabel_cmpRectTransform.offsetMax = new Vector2(10, -35);
            goToggleInterstellarLabel_cmpRectTransform.offsetMin = new Vector2(10, -35);

            Text goToggleInterstellarLabel_cmpText = goToggleInterstellarLabel.GetComponent<Text>();
            goToggleInterstellarLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goToggleInterstellarLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goToggleInterstellarLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goToggleInterstellarLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ToggleInterstellarLabel;

            // 创建 toggle-interstellar
            GameObject goToggleInterstellar = Instantiate(goToggleInPlanet, goTSWParamPanel.transform);
            goToggleInterstellar.name = "toggle-interstellar";

            // 比同一行的字符低2个单位，长宽均为20个单位
            RectTransform goToggleInterstellar_cmpRectTransform = goToggleInterstellar.GetComponent<RectTransform>();
            goToggleInterstellar_cmpRectTransform.Zeroize();
            goToggleInterstellar_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInterstellar_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInterstellar_cmpRectTransform.offsetMax = new Vector2(100, -37);
            goToggleInterstellar_cmpRectTransform.offsetMin = new Vector2(80, -57);

            Toggle goToggleInterstellar_cmpToggle = goToggleInterstellar.GetComponent<Toggle>();

            // UI 组件的默认状态
            goToggleInterstellar_cmpToggle.isOn = true;

            // 默认值
            cmpTSWParamPanel.ToggleInterstellar = true;

            // 监听
            goToggleInterstellar_cmpToggle.onValueChanged.AddListener(value =>
            {
                cmpTSWParamPanel.ToggleInterstellar = value;
                cmpTSWParamPanel.tsw.OnParameterChange();
            });

            // 创建 toggle-collector-label
            GameObject goToggleCollectorLabel = Instantiate(goToggleInPlanetLabel, goTSWParamPanel.transform);
            goToggleCollectorLabel.name = "toggle-collector-label";

            RectTransform goToggleCollectorLabel_cmpRectTransform = goToggleCollectorLabel.GetComponent<RectTransform>();
            goToggleCollectorLabel_cmpRectTransform.Zeroize();
            goToggleCollectorLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleCollectorLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleCollectorLabel_cmpRectTransform.offsetMax = new Vector2(10, -60);
            goToggleCollectorLabel_cmpRectTransform.offsetMin = new Vector2(10, -60);

            Text goToggleCollectorLabel_cmpText = goToggleCollectorLabel.GetComponent<Text>();
            goToggleCollectorLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goToggleCollectorLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goToggleCollectorLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goToggleCollectorLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ToggleCollectorLabel;

            // 创建 toggle-collector
            GameObject goToggleCollector = Instantiate(goToggleInPlanet, goTSWParamPanel.transform);
            goToggleCollector.name = "toggle-collector";

            // 比同一行的字符低2个单位，长宽均为20个单位
            RectTransform goToggleCollector_cmpRectTransform = goToggleCollector.GetComponent<RectTransform>();
            goToggleCollector_cmpRectTransform.Zeroize();
            goToggleCollector_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleCollector_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleCollector_cmpRectTransform.offsetMax = new Vector2(100, -62);
            goToggleCollector_cmpRectTransform.offsetMin = new Vector2(80, -82);

            Toggle goToggleCollector_cmpToggle = goToggleCollector.GetComponent<Toggle>();

            // UI 组件的默认状态
            goToggleCollector_cmpToggle.isOn = true;

            // 默认值
            cmpTSWParamPanel.ToggleCollector = true;

            // 监听
            goToggleCollector_cmpToggle.onValueChanged.AddListener(value =>
            {
                cmpTSWParamPanel.ToggleCollector = value;
                cmpTSWParamPanel.tsw.OnParameterChange();
            });

            // 创建 item-filter-label
            GameObject goItemFilterLabel = Instantiate(goToggleInPlanetLabel, goTSWParamPanel.transform);
            goItemFilterLabel.name = "item-filter-label";

            RectTransform goItemFilterLabel_cmpRectTransform = goItemFilterLabel.GetComponent<RectTransform>();
            goItemFilterLabel_cmpRectTransform.Zeroize();
            goItemFilterLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goItemFilterLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goItemFilterLabel_cmpRectTransform.offsetMax = new Vector2(10, -90);
            goItemFilterLabel_cmpRectTransform.offsetMin = new Vector2(10, -90);

            Text goItemFilterLabel_cmpText = goItemFilterLabel.GetComponent<Text>();
            goItemFilterLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goItemFilterLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goItemFilterLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goItemFilterLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ItemFilterLabel;

            // 创建 item-filter
            GameObject goItemFilter = Instantiate(ReassembledObjectCache.GOCircularItemFilterButton, goTSWParamPanel.transform);
            goItemFilter.name = "item-filter";

            RectTransform goItemFilter_cmpRectTransform = goItemFilter.GetComponent<RectTransform>();
            goItemFilter_cmpRectTransform.Zeroize();
            goItemFilter_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goItemFilter_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goItemFilter_cmpRectTransform.offsetMax = new Vector2(120, -85);
            goItemFilter_cmpRectTransform.offsetMin = new Vector2(80, -125);

            Image goItemFilter_childBg_cmpImage = goItemFilter.transform.Find("bg").GetComponent<Image>();

            // 监听选择过滤物品的事件
            // 依赖了 item-filter-clear ，提前声明
            GameObject goItemFilterClear = null;
            goItemFilter.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (UIItemPicker.isOpened)
                {
                    UIItemPicker.Close();
                    return;
                }
                UIItemPicker.Popup(new Vector2(-360f, 180f), (item) =>
                {
                    if (item == null)
                    {
                        goItemFilter_childBg_cmpImage.sprite = ResourceCache.Round54pxSlice;
                        cmpTSWParamPanel.RelatedItemFilter = Constants.NONE_ITEM_ID;
                        goItemFilterClear.SetActive(false);
                    }
                    else
                    {
                        goItemFilter_childBg_cmpImage.sprite = item.iconSprite;
                        cmpTSWParamPanel.RelatedItemFilter = item.ID;
                        goItemFilterClear.SetActive(true);
                    }
                    cmpTSWParamPanel.tsw.OnParameterChange();
                });
            });

            // 创建 item-filter-clear
            goItemFilterClear = Instantiate(ReassembledObjectCache.GOTextButton, goTSWParamPanel.transform);
            goItemFilterClear.name = "item-filter-clear";
            goItemFilterClear.SetActive(false);

            RectTransform goItemFilterClear_cmpRectTransform = goItemFilterClear.GetComponent<RectTransform>();
            goItemFilterClear_cmpRectTransform.Zeroize();
            goItemFilterClear_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goItemFilterClear_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goItemFilterClear_cmpRectTransform.offsetMax = new Vector2(140, -85);
            goItemFilterClear_cmpRectTransform.offsetMin = new Vector2(120, -105);

            goItemFilterClear.transform.Find("button-text").GetComponent<Text>().text = "X";

            goItemFilterClear.GetComponent<Button>().onClick.AddListener(() =>
            {
                goItemFilter_childBg_cmpImage.sprite = ResourceCache.Round54pxSlice;
                cmpTSWParamPanel.RelatedItemFilter = Constants.NONE_ITEM_ID;
                cmpTSWParamPanel.tsw.OnParameterChange();
                goItemFilterClear.SetActive(false);
            });

            return cmpTSWParamPanel;
        }
    }
}
