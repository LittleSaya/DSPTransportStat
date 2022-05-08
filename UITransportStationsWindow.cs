using DSPTransportStat.CacheObjects;
using DSPTransportStat.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DSPTransportStat.Extensions;
using DSPTransportStat.Global;

namespace DSPTransportStat
{
    class UITransportStationsWindow : ManualBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 是否已经创建过窗口，防止重复创建窗口
        /// </summary>
        static private bool isCreated = false;

        /// <summary>
        /// 鼠标指针是否在窗口内
        /// </summary>
        public bool IsPointerInside { get; set; }

        /// <summary>
        /// 预先创建好的列表项
        /// </summary>
        private TransportStationsEntry[] transportStationsEntries;

        /// <summary>
        /// content RectTransform 的引用，便于修改内容尺寸
        /// </summary>
        private RectTransform contentRectTransform;

        /// <summary>
        /// viewport RectTransform 的引用，便于修改视口的尺寸
        /// </summary>
        private RectTransform viewportRectTransform;

        /// <summary>
        /// 参数面板组件，方便在查询参数发生变化时获取新的查询参数
        /// </summary>
        private UITransportStationsWindowParameterPanel uiTSWParameterPanel = null;

        /// <summary>
        /// 物流运输站列表，包含 StationInfo 、 StarData 和 PlanetData
        /// </summary>
        private readonly List<StationInfoBundle> stations = new List<StationInfoBundle>();

        /// <summary>
        /// 排序参数 - 位置和名称
        /// </summary>
        private SortOrder locationAndNameSortOrder = SortOrder.NONE;

        /// <summary>
        /// 查询参数 - 搜索字符串
        /// </summary>
        private string searchString = "";

        /// <summary>
        /// 创建物流运输站窗口
        /// </summary>
        /// <returns></returns>
        static public UITransportStationsWindow Create ()
        {
            if (isCreated)
            {
                throw new Exception("UITransportStationsWindow has beed created before");
            }
            isCreated = true;

            // 克隆原来的统计面板
            UIStatisticsWindow statWindow = UIRoot.instance.uiGame.statWindow;
            GameObject statWindowGO = statWindow.gameObject;

            UIStatisticsWindow clonedStatWindow = Instantiate(statWindow, statWindowGO.transform.parent);
            GameObject transportStationsWindowGO = clonedStatWindow.gameObject;

            UITransportStationsWindow uiTSW = transportStationsWindowGO.AddComponent<UITransportStationsWindow>();
            uiTSW._Create();

            uiTSW.IsPointerInside = false;
            uiTSW.transportStationsEntries = new TransportStationsEntry[Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT]
            {
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry(),
                new TransportStationsEntry()
            };

            // 调整大小
            transportStationsWindowGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 880);

            // 修改名称
            transportStationsWindowGO.name = "DSPTransportStat_TransportStationsWindow";

            // 修改标题
            Transform titleTextTransform = transportStationsWindowGO.transform.Find("panel-bg/title-text");
            Destroy(titleTextTransform.GetComponent<Localizer>());
            titleTextTransform.GetComponent<Text>().text = Translations.TransportStationsWindow.Title;

            // 通过克隆的 statWindow 删除不需要的对象
            // 删除左侧菜单
            Destroy(clonedStatWindow.verticalTab.gameObject);
            // 保留水平标签列表，删除里面的按钮
            // Destroy(clonedStatWindow.horizontalTab.gameObject);
            for (int i = 0; i < clonedStatWindow.horizontalTab.transform.childCount; ++i)
            {
                Destroy(clonedStatWindow.horizontalTab.transform.GetChild(i).gameObject);
            }
            // 将水平标签列表的名称修改为 parameter-panel ，在此基础上构造查询参数面板
            clonedStatWindow.horizontalTab.name = "parameter-panel";

            uiTSW.uiTSWParameterPanel = UITransportStationsWindowParameterPanel.Create(clonedStatWindow.horizontalTab, uiTSW);

            // 删除里程碑、电力和研究等面板
            Destroy(clonedStatWindow.achievementPanelUI.gameObject);
            Destroy(clonedStatWindow.dysonPanel);
            Destroy(clonedStatWindow.milestonePanelUI.gameObject);
            Destroy(clonedStatWindow.performancePanel);
            Destroy(clonedStatWindow.powerPanel);
            // 不删除生产面板，需要在生产面板的基础上构造物流运输站点的面板
            // Destroy(clonedStatWindow.productPanel);
            Destroy(clonedStatWindow.propertyPanelUI.gameObject);
            Destroy(clonedStatWindow.researchPanel);

            // 修改 productPanel
            // 修改组件名称
            clonedStatWindow.productPanel.name = "transport-stations-bg";

            // 留一个引用，方便后面创建表头时使用
            GameObject goTransportStationsBg = clonedStatWindow.productPanel;

            // 兼容 LSTM
            // 有些 mod 会给原来的 product-bg 添加新的按钮，如果那些 mod 先于本 mod 初始化，这里就会把其他 mod 的按钮复制过来
            // 所以这里要把除了 scroll-view 之外的其他子对象都删除
            for (int i = 0; i < goTransportStationsBg.transform.childCount; ++i)
            {
                if (goTransportStationsBg.transform.GetChild(i).gameObject.name != "scroll-view")
                {
                    Destroy(goTransportStationsBg.transform.GetChild(i).gameObject);
                }
            }

            // 设置为激活状态
            clonedStatWindow.productPanel.SetActive(true);

            // 删除 top 对象
            Destroy(clonedStatWindow.productPanel.transform.Find("top").gameObject);

            // 调整 scroll-view 的上边距，使之适应没有 top 对象后的内容高度
            // 40 是标题栏的高度
            clonedStatWindow.productPanel.transform.Find("scroll-view").GetComponent<RectTransform>().offsetMax = new Vector2(0, -40);

            // 删除所有的 product-entry
            GameObject content = clonedStatWindow.productPanel.transform.Find("scroll-view/viewport/content").gameObject;
            for (int i = 0; i < content.transform.childCount; ++i)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }

            // 给 viewport 和 content 的 RectTransform 留一个引用
            uiTSW.viewportRectTransform = clonedStatWindow.productPanel.transform.Find("scroll-view/viewport").GetComponent<RectTransform>();
            uiTSW.contentRectTransform = content.GetComponent<RectTransform>();

            // ==========
            // 开始创建UI组件（表体）
            // ==========

            // 创建 transport-stations-entry
            GameObject transportStationsEntry = new GameObject("transport-stations-entry", typeof(RectTransform), typeof(CanvasRenderer));
            transportStationsEntry.transform.SetParent(content.transform);

            // 调整大小和位置
            RectTransform transportStationsEntry_cmpRectTransform = transportStationsEntry.GetComponent<RectTransform>();
            transportStationsEntry_cmpRectTransform.Zeroize();
            transportStationsEntry_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_cmpRectTransform.anchorMin = new Vector2(0, 1);

            // 给每一项 transport-stations-entry 的 RectTransform 留一个引用
            uiTSW.transportStationsEntries[0].RectTransform = transportStationsEntry_cmpRectTransform;

            // 创建 transport-stations-entry > star
            GameObject transportStationsEntry_childStar = new GameObject("star", typeof(RectTransform), typeof(CanvasRenderer));
            transportStationsEntry_childStar.transform.SetParent(transportStationsEntry.transform);

            // 调整大小和位置
            RectTransform transportStationsEntry_childStar_cmpRectTransform = transportStationsEntry_childStar.GetComponent<RectTransform>();
            transportStationsEntry_childStar_cmpRectTransform.Zeroize();
            transportStationsEntry_childStar_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childStar_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childStar_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childStar_cmpRectTransform.offsetMax = transportStationsEntry_childStar_cmpRectTransform.offsetMin = new Vector2(10, -10);

            // 设置文本框属性
            Text transportStationsEntry_childStar_cmpText = transportStationsEntry_childStar.AddComponent<Text>();
            transportStationsEntry_childStar_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            transportStationsEntry_childStar_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            transportStationsEntry_childStar_cmpText.font = ResourceCache.FontSAIRASB;
            transportStationsEntry_childStar_cmpText.text = "STAR_NAME";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Star = transportStationsEntry_childStar_cmpText;

            // 创建 transport-stations-entry > planet
            GameObject transportStationsEntry_childPlanet = Instantiate(transportStationsEntry_childStar, transportStationsEntry.transform);
            transportStationsEntry_childPlanet.name = "planet";

            // 调整大小和位置
            RectTransform transportStationsEntry_childPlanet_cmpRectTransform = transportStationsEntry_childPlanet.GetComponent<RectTransform>();
            transportStationsEntry_childPlanet_cmpRectTransform.Zeroize();
            transportStationsEntry_childPlanet_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childPlanet_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childPlanet_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childPlanet_cmpRectTransform.offsetMax = transportStationsEntry_childPlanet_cmpRectTransform.offsetMin = new Vector2(10, -35);

            // 设置文本框属性
            Text transportStationsEntry_childPlanet_cmpText = transportStationsEntry_childPlanet.GetComponent<Text>();
            transportStationsEntry_childPlanet_cmpText.text = "PLANET_NAME";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Planet = transportStationsEntry_childPlanet_cmpText;

            // 创建 transport-stations-entry > station-type
            GameObject transportStationsEntry_childStationType = Instantiate(transportStationsEntry_childStar, transportStationsEntry.transform);
            transportStationsEntry_childStationType.name = "station-type";

            // 调整大小和位置
            RectTransform transportStationsEntry_childStationType_cmpRectTransform = transportStationsEntry_childStationType.GetComponent<RectTransform>();
            transportStationsEntry_childStationType_cmpRectTransform.Zeroize();
            transportStationsEntry_childStationType_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childStationType_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childStationType_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childStationType_cmpRectTransform.offsetMax = transportStationsEntry_childStationType_cmpRectTransform.offsetMin = new Vector2(10, -60);

            // 设置文本框属性
            Text transportStationsEntry_childStationType_cmpText = transportStationsEntry_childStationType.GetComponent<Text>();
            transportStationsEntry_childStationType_cmpText.text = "STATION_TYPE";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].StationType = transportStationsEntry_childStationType_cmpText;

            // 创建 transport-stations-entry > name
            GameObject transportStationsEntry_childName = Instantiate(transportStationsEntry_childStar, transportStationsEntry.transform);
            transportStationsEntry_childName.name = "name";

            // 调整大小和位置
            RectTransform transportStationsEntry_childName_cmpRectTransform = transportStationsEntry_childName.GetComponent<RectTransform>();
            transportStationsEntry_childName_cmpRectTransform.Zeroize();
            transportStationsEntry_childName_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childName_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childName_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childName_cmpRectTransform.offsetMax = transportStationsEntry_childName_cmpRectTransform.offsetMin = new Vector2(10, -85);

            // 设置文本框属性
            Text transportStationsEntry_childName_cmpText = transportStationsEntry_childName.GetComponent<Text>();
            transportStationsEntry_childName_cmpText.text = "STATION_NAME";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Name = transportStationsEntry_childName_cmpText;

            // 创建 transport-stations-entry > sep-line-1 和 transport-stations-entry > sep-line-0
            GameObject transportStationsEntry_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, transportStationsEntry.transform);

            RectTransform transportStationsEntry_childSepLine1_cmpRectTransform = transportStationsEntry_childSepLine1.GetComponent<RectTransform>();
            transportStationsEntry_childSepLine1_cmpRectTransform.Zeroize();
            transportStationsEntry_childSepLine1_cmpRectTransform.anchorMax = new Vector2(1, 0);
            transportStationsEntry_childSepLine1_cmpRectTransform.offsetMax = new Vector2(-1, 1);
            transportStationsEntry_childSepLine1_cmpRectTransform.offsetMin = new Vector2(1, 0);

            GameObject transportStationsEntry_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, transportStationsEntry.transform);

            RectTransform transportStationsEntry_childSepLine0_cmpRectTransform = transportStationsEntry_childSepLine0.GetComponent<RectTransform>();
            transportStationsEntry_childSepLine0_cmpRectTransform.Zeroize();
            transportStationsEntry_childSepLine0_cmpRectTransform.anchorMax = new Vector2(1, 0);
            transportStationsEntry_childSepLine0_cmpRectTransform.offsetMax = new Vector2(-1, 0);
            transportStationsEntry_childSepLine0_cmpRectTransform.offsetMin = new Vector2(1, -1);

            // 创建 transport-stations-entry > item01
            GameObject transportStationsEntry_childItem01 = new GameObject("item01", typeof(RectTransform), typeof(CanvasRenderer));
            transportStationsEntry_childItem01.transform.SetParent(transportStationsEntry.transform);

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_cmpRectTransform = transportStationsEntry_childItem01.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem01_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_cmpRectTransform.offsetMax = new Vector2(150, 0);
            transportStationsEntry_childItem01_cmpRectTransform.offsetMin = new Vector2(150, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 创建 transport-stations-entry > item01 > icon-bg
            GameObject transportStationsEntry_childItem01_childIconBg = new GameObject("icon-bg", typeof(RectTransform), typeof(CanvasRenderer));
            transportStationsEntry_childItem01_childIconBg.transform.SetParent(transportStationsEntry_childItem01.transform);

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childIconBg_cmpRectTransform = transportStationsEntry_childItem01_childIconBg.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childIconBg_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childIconBg_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem01_childIconBg_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childIconBg_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childIconBg_cmpRectTransform.offsetMax = new Vector2(55, -5);
            transportStationsEntry_childItem01_childIconBg_cmpRectTransform.offsetMin = new Vector2(5, -55);

            // 设置物品图标的背景色
            Image transportStationsEntry_childItem01_childIconBg_cmpImage = transportStationsEntry_childItem01_childIconBg.AddComponent<Image>();
            transportStationsEntry_childItem01_childIconBg_cmpImage.sprite = ResourceCache.SpriteRound256;
            transportStationsEntry_childItem01_childIconBg_cmpImage.color = new Color(0, 0, 0, 0.5f);

            // 创建 transport-stations-entry > item01 > icon
            GameObject transportStationsEntry_childItem01_childIcon = Instantiate(transportStationsEntry_childItem01_childIconBg, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childIcon.name = "icon";

            RectTransform transportStationsEntry_childItem01_childIcon_cmpRectTransform = transportStationsEntry_childItem01_childIcon.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childIcon_cmpRectTransform.offsetMax = new Vector2(50, -10);
            transportStationsEntry_childItem01_childIcon_cmpRectTransform.offsetMin = new Vector2(10, -50);

            // 设置物品图标（只设置颜色，此时 LDB 中的物品图标尚未初始化）
            Image transportStationsEntry_childItem01_childIcon_cmpImage = transportStationsEntry_childItem01_childIcon.GetComponent<Image>();
            transportStationsEntry_childItem01_childIcon_cmpImage.color = Color.white;

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01Icon = transportStationsEntry_childItem01_childIcon_cmpImage;

            // 创建 transport-stations-entry > item01 > current-label
            GameObject transportStationsEntry_childItem01_childCurrentLabel = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childCurrentLabel.name = "current-label";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childCurrentLabel_cmpRectTransform = transportStationsEntry_childItem01_childCurrentLabel.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childCurrentLabel_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childCurrentLabel_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childCurrentLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childCurrentLabel_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childCurrentLabel_cmpRectTransform.anchoredPosition = new Vector2(60, -10);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childCurrentLabel_cmpText = transportStationsEntry_childItem01_childCurrentLabel.GetComponent<Text>();
            transportStationsEntry_childItem01_childCurrentLabel_cmpText.text = Translations.TransportStationsWindow.CurrentLabel;

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01CurrentLabelGO = transportStationsEntry_childItem01_childCurrentLabel;

            // 创建 transport-stations-entry > item01 > current-amount
            GameObject transportStationsEntry_childItem01_childCurrentAmount = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childCurrentAmount.name = "current-amount";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childCurrentAmount_cmpRectTransform = transportStationsEntry_childItem01_childCurrentAmount.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childCurrentAmount_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childCurrentAmount_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childCurrentAmount_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childCurrentAmount_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childCurrentAmount_cmpRectTransform.anchoredPosition = new Vector2(110, -10);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childCurrentAmount_cmpText = transportStationsEntry_childItem01_childCurrentAmount.GetComponent<Text>();
            transportStationsEntry_childItem01_childCurrentAmount_cmpText.text = "000000";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01CurrentAmount = transportStationsEntry_childItem01_childCurrentAmount_cmpText;

            // 创建 transport-stations-entry > item01 > order-amount
            GameObject transportStationsEntry_childItem01_childOrderAmount = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childOrderAmount.name = "order-amount";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childOrderAmount_cmpRectTransform = transportStationsEntry_childItem01_childOrderAmount.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childOrderAmount_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childOrderAmount_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childOrderAmount_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childOrderAmount_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childOrderAmount_cmpRectTransform.anchoredPosition = new Vector2(160, -10);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childOrderAmount_cmpText = transportStationsEntry_childItem01_childOrderAmount.GetComponent<Text>();
            transportStationsEntry_childItem01_childOrderAmount_cmpText.text = "+000000";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01OrderAmount = transportStationsEntry_childItem01_childOrderAmount_cmpText;

            // 创建 transport-stations-entry > item01 > max-label
            GameObject transportStationsEntry_childItem01_childMaxLabel = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childMaxLabel.name = "max-label";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childMaxLabel_cmpRectTransform = transportStationsEntry_childItem01_childMaxLabel.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childMaxLabel_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childMaxLabel_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childMaxLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childMaxLabel_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childMaxLabel_cmpRectTransform.anchoredPosition = new Vector2(60, -35);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childMaxLabel_cmpText = transportStationsEntry_childItem01_childMaxLabel.GetComponent<Text>();
            transportStationsEntry_childItem01_childMaxLabel_cmpText.text = Translations.TransportStationsWindow.MaxLabel;

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01MaxLabelGO = transportStationsEntry_childItem01_childMaxLabel;

            // 创建 transport-stations-entry > item01 > max-amount
            GameObject transportStationsEntry_childItem01_childMaxAmount = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childMaxAmount.name = "max-amount";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childMaxAmount_cmpRectTransform = transportStationsEntry_childItem01_childMaxAmount.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childMaxAmount_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childMaxAmount_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childMaxAmount_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childMaxAmount_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childMaxAmount_cmpRectTransform.anchoredPosition = new Vector2(110, -35);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childMaxAmount_cmpText = transportStationsEntry_childItem01_childMaxAmount.GetComponent<Text>();
            transportStationsEntry_childItem01_childMaxAmount_cmpText.text = "000000";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01MaxAmount = transportStationsEntry_childItem01_childMaxAmount_cmpText;

            // 创建 transport-stations-entry > item01 > in-planet-storage-usage
            GameObject transportStationsEntry_childItem01_childInPlanetStorageUsage = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childInPlanetStorageUsage.name = "in-planet-storage-usage";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpRectTransform = transportStationsEntry_childItem01_childInPlanetStorageUsage.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpRectTransform.anchoredPosition = new Vector2(10, -60);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpText = transportStationsEntry_childItem01_childInPlanetStorageUsage.GetComponent<Text>();
            transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpText.text = "IN_PLANET_STORAGE_USAGE";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01InPlanetStorageUsage = transportStationsEntry_childItem01_childInPlanetStorageUsage_cmpText;

            // 创建 transport-stations-entry > item01 > interstellar-storage-usage
            GameObject transportStationsEntry_childItem01_childInterstellarStorageUsage = Instantiate(transportStationsEntry_childStar, transportStationsEntry_childItem01.transform);
            transportStationsEntry_childItem01_childInterstellarStorageUsage.name = "interstellar-storage-usage";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpRectTransform = transportStationsEntry_childItem01_childInterstellarStorageUsage.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpRectTransform.anchorMax = new Vector2(1, 1);
            transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpRectTransform.anchoredPosition = new Vector2(10, -85);

            // 设置文本框属性
            Text transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpText = transportStationsEntry_childItem01_childInterstellarStorageUsage.GetComponent<Text>();
            transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpText.text = "INTERSTELLAR_STORAGE_USAGE";

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item01InterstellarStorageUsage = transportStationsEntry_childItem01_childInterstellarStorageUsage_cmpText;

            // 创建 item01 > sep-line-1 和 item01 > sep-line-0
            GameObject transportStationsEntry_childItem01_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, transportStationsEntry_childItem01.transform);

            RectTransform transportStationsEntry_childItem01_childSepLine1_cmpRectTransform = transportStationsEntry_childItem01_childSepLine1.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childSepLine1_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childSepLine1_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem01_childSepLine1_cmpRectTransform.offsetMax = new Vector2(0, -1);
            transportStationsEntry_childItem01_childSepLine1_cmpRectTransform.offsetMin = new Vector2(-1, 1);

            GameObject transportStationsEntry_childItem01_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, transportStationsEntry_childItem01.transform);

            RectTransform transportStationsEntry_childItem01_childSepLine0_cmpRectTransform = transportStationsEntry_childItem01_childSepLine0.GetComponent<RectTransform>();
            transportStationsEntry_childItem01_childSepLine0_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem01_childSepLine0_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem01_childSepLine0_cmpRectTransform.offsetMax = new Vector2(1, -1);
            transportStationsEntry_childItem01_childSepLine0_cmpRectTransform.offsetMin = new Vector2(0, 1);

            // 创建 transport-stations-entry > item02
            GameObject transportStationsEntry_childItem02 = Instantiate(transportStationsEntry_childItem01, transportStationsEntry.transform);
            transportStationsEntry_childItem02.name = "item02";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem02_cmpRectTransform = transportStationsEntry_childItem02.GetComponent<RectTransform>();
            transportStationsEntry_childItem02_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem02_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem02_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem02_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem02_cmpRectTransform.offsetMax = new Vector2(380, 0);
            transportStationsEntry_childItem02_cmpRectTransform.offsetMin = new Vector2(380, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item02Icon = transportStationsEntry_childItem02_cmpRectTransform.Find("icon").GetComponent<Image>();
            uiTSW.transportStationsEntries[0].Item02CurrentLabelGO = transportStationsEntry_childItem02_cmpRectTransform.Find("current-label").gameObject;
            uiTSW.transportStationsEntries[0].Item02CurrentAmount = transportStationsEntry_childItem02_cmpRectTransform.Find("current-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item02OrderAmount = transportStationsEntry_childItem02_cmpRectTransform.Find("order-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item02MaxLabelGO = transportStationsEntry_childItem02_cmpRectTransform.Find("max-label").gameObject;
            uiTSW.transportStationsEntries[0].Item02MaxAmount = transportStationsEntry_childItem02_cmpRectTransform.Find("max-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item02InPlanetStorageUsage = transportStationsEntry_childItem02_cmpRectTransform.Find("in-planet-storage-usage").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item02InterstellarStorageUsage = transportStationsEntry_childItem02_cmpRectTransform.Find("interstellar-storage-usage").GetComponent<Text>();

            // 创建 transport-stations-entry > item03
            GameObject transportStationsEntry_childItem03 = Instantiate(transportStationsEntry_childItem01, transportStationsEntry.transform);
            transportStationsEntry_childItem03.name = "item03";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem03_cmpRectTransform = transportStationsEntry_childItem03.GetComponent<RectTransform>();
            transportStationsEntry_childItem03_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem03_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem03_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem03_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem03_cmpRectTransform.offsetMax = new Vector2(610, 0);
            transportStationsEntry_childItem03_cmpRectTransform.offsetMin = new Vector2(610, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item03Icon = transportStationsEntry_childItem03_cmpRectTransform.Find("icon").GetComponent<Image>();
            uiTSW.transportStationsEntries[0].Item03CurrentLabelGO = transportStationsEntry_childItem03_cmpRectTransform.Find("current-label").gameObject;
            uiTSW.transportStationsEntries[0].Item03CurrentAmount = transportStationsEntry_childItem03_cmpRectTransform.Find("current-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item03OrderAmount = transportStationsEntry_childItem03_cmpRectTransform.Find("order-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item03MaxLabelGO = transportStationsEntry_childItem03_cmpRectTransform.Find("max-label").gameObject;
            uiTSW.transportStationsEntries[0].Item03MaxAmount = transportStationsEntry_childItem03_cmpRectTransform.Find("max-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item03InPlanetStorageUsage = transportStationsEntry_childItem03_cmpRectTransform.Find("in-planet-storage-usage").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item03InterstellarStorageUsage = transportStationsEntry_childItem03_cmpRectTransform.Find("interstellar-storage-usage").GetComponent<Text>();

            // 创建 transport-stations-entry > item04
            GameObject transportStationsEntry_childItem04 = Instantiate(transportStationsEntry_childItem01, transportStationsEntry.transform);
            transportStationsEntry_childItem04.name = "item04";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem04_cmpRectTransform = transportStationsEntry_childItem04.GetComponent<RectTransform>();
            transportStationsEntry_childItem04_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem04_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem04_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem04_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem04_cmpRectTransform.offsetMax = new Vector2(840, 0);
            transportStationsEntry_childItem04_cmpRectTransform.offsetMin = new Vector2(840, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item04Icon = transportStationsEntry_childItem04_cmpRectTransform.Find("icon").GetComponent<Image>();
            uiTSW.transportStationsEntries[0].Item04CurrentLabelGO = transportStationsEntry_childItem04_cmpRectTransform.Find("current-label").gameObject;
            uiTSW.transportStationsEntries[0].Item04CurrentAmount = transportStationsEntry_childItem04_cmpRectTransform.Find("current-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item04OrderAmount = transportStationsEntry_childItem04_cmpRectTransform.Find("order-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item04MaxLabelGO = transportStationsEntry_childItem04_cmpRectTransform.Find("max-label").gameObject;
            uiTSW.transportStationsEntries[0].Item04MaxAmount = transportStationsEntry_childItem04_cmpRectTransform.Find("max-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item04InPlanetStorageUsage = transportStationsEntry_childItem04_cmpRectTransform.Find("in-planet-storage-usage").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item04InterstellarStorageUsage = transportStationsEntry_childItem04_cmpRectTransform.Find("interstellar-storage-usage").GetComponent<Text>();

            // 创建 transport-stations-entry > item05
            GameObject transportStationsEntry_childItem05 = Instantiate(transportStationsEntry_childItem01, transportStationsEntry.transform);
            transportStationsEntry_childItem05.name = "item05";

            // 调整大小和位置
            RectTransform transportStationsEntry_childItem05_cmpRectTransform = transportStationsEntry_childItem05.GetComponent<RectTransform>();
            transportStationsEntry_childItem05_cmpRectTransform.Zeroize();
            transportStationsEntry_childItem05_cmpRectTransform.anchorMax = new Vector2(0, 1);
            transportStationsEntry_childItem05_cmpRectTransform.anchorMin = new Vector2(0, 1);
            transportStationsEntry_childItem05_cmpRectTransform.localScale = Vector3.one;
            transportStationsEntry_childItem05_cmpRectTransform.offsetMax = new Vector2(1070, 0);
            transportStationsEntry_childItem05_cmpRectTransform.offsetMin = new Vector2(1070, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 填充对组件的引用
            uiTSW.transportStationsEntries[0].Item05Icon = transportStationsEntry_childItem05_cmpRectTransform.Find("icon").GetComponent<Image>();
            uiTSW.transportStationsEntries[0].Item05CurrentLabelGO = transportStationsEntry_childItem05_cmpRectTransform.Find("current-label").gameObject;
            uiTSW.transportStationsEntries[0].Item05CurrentAmount = transportStationsEntry_childItem05_cmpRectTransform.Find("current-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item05OrderAmount = transportStationsEntry_childItem05_cmpRectTransform.Find("order-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item05MaxLabelGO = transportStationsEntry_childItem05_cmpRectTransform.Find("max-label").gameObject;
            uiTSW.transportStationsEntries[0].Item05MaxAmount = transportStationsEntry_childItem05_cmpRectTransform.Find("max-amount").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item05InPlanetStorageUsage = transportStationsEntry_childItem05_cmpRectTransform.Find("in-planet-storage-usage").GetComponent<Text>();
            uiTSW.transportStationsEntries[0].Item05InterstellarStorageUsage = transportStationsEntry_childItem05_cmpRectTransform.Find("interstellar-storage-usage").GetComponent<Text>();

            // 再克隆若干份 transport-stations-entry ，填满剩余几项
            // 从 1 开始，跳过第一个，因为第一个刚才已经创建好了
            for (int i = 1; i < Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT; ++i)
            {
                transportStationsEntry = Instantiate(transportStationsEntry, content.transform);
                transportStationsEntry.name = "transport-stations-entry";

                // 给每一项 transport-stations-entry 的 RectTransform 留一个引用
                uiTSW.transportStationsEntries[i].RectTransform = transportStationsEntry.GetComponent<RectTransform>();

                // 填充对组件的引用
                uiTSW.transportStationsEntries[i].Star = transportStationsEntry.transform.Find("star").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Planet = transportStationsEntry.transform.Find("planet").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].StationType = transportStationsEntry.transform.Find("station-type").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Name = transportStationsEntry.transform.Find("name").GetComponent<Text>();

                uiTSW.transportStationsEntries[i].Item01Icon = transportStationsEntry.transform.Find("item01/icon").GetComponent<Image>();
                uiTSW.transportStationsEntries[i].Item01CurrentLabelGO = transportStationsEntry.transform.Find("item01/current-label").gameObject;
                uiTSW.transportStationsEntries[i].Item01CurrentAmount = transportStationsEntry.transform.Find("item01/current-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item01OrderAmount = transportStationsEntry.transform.Find("item01/order-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item01MaxLabelGO = transportStationsEntry.transform.Find("item01/max-label").gameObject;
                uiTSW.transportStationsEntries[i].Item01MaxAmount = transportStationsEntry.transform.Find("item01/max-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item01InPlanetStorageUsage = transportStationsEntry.transform.Find("item01/in-planet-storage-usage").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item01InterstellarStorageUsage = transportStationsEntry.transform.Find("item01/interstellar-storage-usage").GetComponent<Text>();

                uiTSW.transportStationsEntries[i].Item02Icon = transportStationsEntry.transform.Find("item02/icon").GetComponent<Image>();
                uiTSW.transportStationsEntries[i].Item02CurrentLabelGO = transportStationsEntry.transform.Find("item02/current-label").gameObject;
                uiTSW.transportStationsEntries[i].Item02CurrentAmount = transportStationsEntry.transform.Find("item02/current-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item02OrderAmount = transportStationsEntry.transform.Find("item02/order-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item02MaxLabelGO = transportStationsEntry.transform.Find("item02/max-label").gameObject;
                uiTSW.transportStationsEntries[i].Item02MaxAmount = transportStationsEntry.transform.Find("item02/max-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item02InPlanetStorageUsage = transportStationsEntry.transform.Find("item02/in-planet-storage-usage").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item02InterstellarStorageUsage = transportStationsEntry.transform.Find("item02/interstellar-storage-usage").GetComponent<Text>();

                uiTSW.transportStationsEntries[i].Item03Icon = transportStationsEntry.transform.Find("item03/icon").GetComponent<Image>();
                uiTSW.transportStationsEntries[i].Item03CurrentLabelGO = transportStationsEntry.transform.Find("item03/current-label").gameObject;
                uiTSW.transportStationsEntries[i].Item03CurrentAmount = transportStationsEntry.transform.Find("item03/current-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item03OrderAmount = transportStationsEntry.transform.Find("item03/order-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item03MaxLabelGO = transportStationsEntry.transform.Find("item03/max-label").gameObject;
                uiTSW.transportStationsEntries[i].Item03MaxAmount = transportStationsEntry.transform.Find("item03/max-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item03InPlanetStorageUsage = transportStationsEntry.transform.Find("item03/in-planet-storage-usage").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item03InterstellarStorageUsage = transportStationsEntry.transform.Find("item03/interstellar-storage-usage").GetComponent<Text>();

                uiTSW.transportStationsEntries[i].Item04Icon = transportStationsEntry.transform.Find("item04/icon").GetComponent<Image>();
                uiTSW.transportStationsEntries[i].Item04CurrentLabelGO = transportStationsEntry.transform.Find("item04/current-label").gameObject;
                uiTSW.transportStationsEntries[i].Item04CurrentAmount = transportStationsEntry.transform.Find("item04/current-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item04OrderAmount = transportStationsEntry.transform.Find("item04/order-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item04MaxLabelGO = transportStationsEntry.transform.Find("item04/max-label").gameObject;
                uiTSW.transportStationsEntries[i].Item04MaxAmount = transportStationsEntry.transform.Find("item04/max-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item04InPlanetStorageUsage = transportStationsEntry.transform.Find("item04/in-planet-storage-usage").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item04InterstellarStorageUsage = transportStationsEntry.transform.Find("item04/interstellar-storage-usage").GetComponent<Text>();

                uiTSW.transportStationsEntries[i].Item05Icon = transportStationsEntry.transform.Find("item05/icon").GetComponent<Image>();
                uiTSW.transportStationsEntries[i].Item05CurrentLabelGO = transportStationsEntry.transform.Find("item05/current-label").gameObject;
                uiTSW.transportStationsEntries[i].Item05CurrentAmount = transportStationsEntry.transform.Find("item05/current-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item05OrderAmount = transportStationsEntry.transform.Find("item05/order-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item05MaxLabelGO = transportStationsEntry.transform.Find("item05/max-label").gameObject;
                uiTSW.transportStationsEntries[i].Item05MaxAmount = transportStationsEntry.transform.Find("item05/max-amount").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item05InPlanetStorageUsage = transportStationsEntry.transform.Find("item05/in-planet-storage-usage").GetComponent<Text>();
                uiTSW.transportStationsEntries[i].Item05InterstellarStorageUsage = transportStationsEntry.transform.Find("item05/interstellar-storage-usage").GetComponent<Text>();
            }

            // ==========
            // UI组件创建完成（表体）
            // ==========

            // ==========
            // 开始创建UI组件（表头）
            // ==========

            // 创建 headers
            GameObject goHeaders = new GameObject("headers", typeof(RectTransform), typeof(CanvasRenderer));
            goHeaders.transform.parent = goTransportStationsBg.transform;

            RectTransform goHeaders_cmpRectTransform = goHeaders.GetComponent<RectTransform>();
            goHeaders_cmpRectTransform.Zeroize();
            goHeaders_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goHeaders_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goHeaders_cmpRectTransform.offsetMin = new Vector2(0, -40);

            // 创建 headers > location-and-name
            GameObject goHeaders_childLocationAndName = new GameObject("location-and-name", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            goHeaders_childLocationAndName.transform.parent = goHeaders.transform;

            RectTransform goHeaders_childLocationAndName_cmpRectTransform = goHeaders_childLocationAndName.GetComponent<RectTransform>();
            goHeaders_childLocationAndName_cmpRectTransform.Zeroize();
            goHeaders_childLocationAndName_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goHeaders_childLocationAndName_cmpRectTransform.offsetMax = new Vector2(100, -10);
            goHeaders_childLocationAndName_cmpRectTransform.offsetMin = new Vector2(10, 0);

            Text goHeaders_childLocationAndName_cmpText = goHeaders_childLocationAndName.GetComponent<Text>();
            goHeaders_childLocationAndName_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goHeaders_childLocationAndName_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goHeaders_childLocationAndName_cmpText.font = ResourceCache.FontSAIRASB;
            goHeaders_childLocationAndName_cmpText.text = Translations.TransportStationsWindow.LocationAndName;

            // 创建 headers > location-and-name > sort
            GameObject goHeaders_childLocationAndName_childSort = Instantiate(ReassembledObjectCache.GOTextButton, goHeaders_childLocationAndName.transform);
            goHeaders_childLocationAndName_childSort.name = "sort";
            goHeaders_childLocationAndName_childSort.SetActive(true);

            RectTransform goHeaders_childLocationAndName_childSort_cmpRectTransform = goHeaders_childLocationAndName_childSort.GetComponent<RectTransform>();
            goHeaders_childLocationAndName_childSort_cmpRectTransform.Zeroize();
            goHeaders_childLocationAndName_childSort_cmpRectTransform.anchorMin = new Vector2(1, 0);
            goHeaders_childLocationAndName_childSort_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goHeaders_childLocationAndName_childSort_cmpRectTransform.offsetMax = new Vector2(40, 0);
            goHeaders_childLocationAndName_childSort_cmpRectTransform.offsetMin = new Vector2(0, 5);

            GameObject goHeaders_childLocationAndName_childSort_childButtonText = goHeaders_childLocationAndName_childSort.transform.Find("button-text").gameObject;
            Text goHeaders_childLocationAndName_childSort_childButtonText_cmpText = goHeaders_childLocationAndName_childSort_childButtonText.GetComponent<Text>();

            // 默认的位置和名称顺序为升序
            goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Translations.TransportStationsWindow.ASC;
            uiTSW.locationAndNameSortOrder = SortOrder.ASC;

            goHeaders_childLocationAndName_childSort.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (uiTSW.locationAndNameSortOrder == SortOrder.ASC)
                {
                    uiTSW.locationAndNameSortOrder = SortOrder.DESC;
                    goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Translations.TransportStationsWindow.DESC;
                }
                else if (uiTSW.locationAndNameSortOrder == SortOrder.DESC)
                {
                    uiTSW.locationAndNameSortOrder = SortOrder.ASC;
                    goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Translations.TransportStationsWindow.ASC;
                }
                else
                {
                    uiTSW.locationAndNameSortOrder = SortOrder.ASC;
                    goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Translations.TransportStationsWindow.ASC;
                }
                uiTSW.OnSort();
            });

            // 创建 headers > item-slots
            GameObject goHeaders_childItemSlots = new GameObject("item-slots", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            goHeaders_childItemSlots.transform.parent = goHeaders.transform;

            RectTransform goHeaders_childItemSlots_cmpRectTransform = goHeaders_childItemSlots.GetComponent<RectTransform>();
            goHeaders_childItemSlots_cmpRectTransform.Zeroize();
            goHeaders_childItemSlots_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goHeaders_childItemSlots_cmpRectTransform.offsetMin = new Vector2(150, 0);

            Text goHeaders_childItemSlots_cmpText = goHeaders_childItemSlots.GetComponent<Text>();
            goHeaders_childItemSlots_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goHeaders_childItemSlots_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goHeaders_childItemSlots_cmpText.font = ResourceCache.FontSAIRASB;
            goHeaders_childItemSlots_cmpText.text = Translations.TransportStationsWindow.ItemSlots;
            goHeaders_childItemSlots_cmpText.alignment = TextAnchor.MiddleCenter;

            // 创建 headers > item-slots > sep-line-1 和 headers > item-slots > sep-line-0
            GameObject goHeaders_childItemSlots_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, goHeaders_childItemSlots.transform);

            RectTransform goHeaders_childItemSlots_childSepLine1_cmpRectTransform = goHeaders_childItemSlots_childSepLine1.GetComponent<RectTransform>();
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.Zeroize();
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.offsetMax = new Vector2(0, -1);
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.offsetMin = new Vector2(-1, 1);

            GameObject goHeaders_childItemSlots_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, goHeaders_childItemSlots.transform);

            RectTransform goHeaders_childItemSlots_childSepLine0_cmpRectTransform = goHeaders_childItemSlots_childSepLine0.GetComponent<RectTransform>();
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.Zeroize();
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.offsetMax = new Vector2(1, -1);
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.offsetMin = new Vector2(0, 1);

            // 创建 headers > sep-line-1 和 headers > sep-line-0
            GameObject goHeaders_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, goHeaders.transform);

            RectTransform goHeaders_childSepLine1_cmpRectTransform = goHeaders_childSepLine1.GetComponent<RectTransform>();
            goHeaders_childSepLine1_cmpRectTransform.Zeroize();
            goHeaders_childSepLine1_cmpRectTransform.anchorMax = new Vector2(1, 0);
            goHeaders_childSepLine1_cmpRectTransform.offsetMax = new Vector2(-1, 1);
            goHeaders_childSepLine1_cmpRectTransform.offsetMin = new Vector2(1, 0);

            GameObject goHeaders_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, goHeaders.transform);

            RectTransform goHeaders_childSepLine0_cmpRectTransform = goHeaders_childSepLine0.GetComponent<RectTransform>();
            goHeaders_childSepLine0_cmpRectTransform.Zeroize();
            goHeaders_childSepLine0_cmpRectTransform.anchorMax = new Vector2(1, 0);
            goHeaders_childSepLine0_cmpRectTransform.offsetMax = new Vector2(-1, 0);
            goHeaders_childSepLine0_cmpRectTransform.offsetMin = new Vector2(1, -1);

            // ==========
            // UI组件创建完成（表头）
            // ==========

            // ==========
            // 开始创建UI组件（标题栏）
            // ==========

            GameObject goPanelBg = uiTSW.transform.Find("panel-bg").gameObject;

            // 创建 panel-bg > search-label
            GameObject goPanelBg_childSearchLabel = Instantiate(transportStationsEntry_childStar, goPanelBg.transform);
            goPanelBg_childSearchLabel.name = "search-label";

            RectTransform goPanelBg_childSearchLabel_cmpRectTransform = goPanelBg_childSearchLabel.GetComponent<RectTransform>();
            goPanelBg_childSearchLabel_cmpRectTransform.Zeroize();
            goPanelBg_childSearchLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goPanelBg_childSearchLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearchLabel_cmpRectTransform.offsetMin = new Vector2(200, -40);
            goPanelBg_childSearchLabel_cmpRectTransform.offsetMax = new Vector2(250, -15);

            Text goPanelBg_childSearchLabel_cmpText = goPanelBg_childSearchLabel.GetComponent<Text>();
            goPanelBg_childSearchLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goPanelBg_childSearchLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goPanelBg_childSearchLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goPanelBg_childSearchLabel_cmpText.text = Translations.TransportStationsWindow.SearchLabel;

            // 创建 panel-bg > search
            GameObject goPanelBg_childSearch = Instantiate(ReassembledObjectCache.GOInputField, goPanelBg.transform);
            goPanelBg_childSearch.name = "search";

            RectTransform goPanelBg_childSearch_cmpRectTransform = goPanelBg_childSearch.GetComponent<RectTransform>();
            goPanelBg_childSearch_cmpRectTransform.Zeroize();
            goPanelBg_childSearch_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goPanelBg_childSearch_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearch_cmpRectTransform.offsetMin = new Vector2(250, -37);
            goPanelBg_childSearch_cmpRectTransform.offsetMax = new Vector2(400, -12);

            InputField goPanelBg_childSearch_cmpInputField = goPanelBg_childSearch.GetComponent<InputField>();

            // 监听编辑结束的事件
            // 由于依赖了 search-clear ，这里对其进行提前声明
            GameObject goPanelBg_childSearchClear = null;
            goPanelBg_childSearch_cmpInputField.onEndEdit.AddListener((value) =>
            {
                uiTSW.searchString = value;
                uiTSW.OnSearch();

                // 搜索字符串不为空时才显示 search-clear 按钮
                goPanelBg_childSearchClear.SetActive(value.Length > 0);
            });

            UIButton goPanelBg_childSearch_cmpUIButton = goPanelBg_childSearch.GetComponent<UIButton>();
            goPanelBg_childSearch_cmpUIButton.transitions[0].normalColor = new Color(1, 1, 1, 0.15f);
            goPanelBg_childSearch_cmpUIButton.transitions[0].mouseoverColor = new Color(1, 1, 1, 0.2f);
            goPanelBg_childSearch_cmpUIButton.transitions[0].pressedColor = new Color(1, 1, 1, 0.05f);

            Image goPanelBg_childSearch_cmpImage = goPanelBg_childSearch.GetComponent<Image>();
            goPanelBg_childSearch_cmpImage.color = new Color(1, 1, 1, 0.15f);

            // 创建 panel-bg > search-clear
            goPanelBg_childSearchClear = Instantiate(ReassembledObjectCache.GOTextButton, goPanelBg.transform);
            goPanelBg_childSearchClear.name = "search-clear";
            goPanelBg_childSearchClear.SetActive(false);

            RectTransform goPanelBg_childSearchClear_cmpRectTransform = goPanelBg_childSearchClear.GetComponent<RectTransform>();
            goPanelBg_childSearchClear_cmpRectTransform.Zeroize();
            goPanelBg_childSearchClear_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goPanelBg_childSearchClear_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearchClear_cmpRectTransform.offsetMin = new Vector2(375, -32);
            goPanelBg_childSearchClear_cmpRectTransform.offsetMax = new Vector2(395, -15);

            goPanelBg_childSearchClear.transform.Find("button-text").GetComponent<Text>().text = "X";

            goPanelBg_childSearchClear.GetComponent<Button>().onClick.AddListener(() =>
            {
                uiTSW.searchString = goPanelBg_childSearch_cmpInputField.text = "";
                uiTSW.OnSearch();

                goPanelBg_childSearchClear.SetActive(false);
            });

            // 创建 panel-bg > search-button
            GameObject goPanelBg_childSearchButton = Instantiate(ReassembledObjectCache.GOTextButton, goPanelBg.transform);
            goPanelBg_childSearchButton.name = "search-button";

            RectTransform goPanelBg_childSearchButton_cmpRectTransform = goPanelBg_childSearchButton.GetComponent<RectTransform>();
            goPanelBg_childSearchButton_cmpRectTransform.Zeroize();
            goPanelBg_childSearchButton_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goPanelBg_childSearchButton_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearchButton_cmpRectTransform.offsetMin = new Vector2(400, -37);
            goPanelBg_childSearchButton_cmpRectTransform.offsetMax = new Vector2(450, -12);

            goPanelBg_childSearchButton.transform.Find("button-text").GetComponent<Text>().text = Translations.TransportStationsWindow.SearchButton;

            goPanelBg_childSearchButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                // do nothing
            });

            // ==========
            // UI组件创建完成（标题栏）
            // ==========

            // 删除克隆的 statWindow 组件
            Destroy(clonedStatWindow);

            // 为右上角的关闭按钮添加事件
            transportStationsWindowGO.transform.Find("panel-bg/x").GetComponent<Button>().onClick.AddListener(uiTSW._Close);

            return uiTSW;
        }

        /// <summary>
        /// 加载所有的物流运输站、根据当前的物流运输站数量设置 content 的高度并重置滚动条的位置
        /// </summary>
        public void ComputeTransportStationsWindow_LoadStations ()
        {
            bool toggleInPlanet = uiTSWParameterPanel.ToggleInPlanet;
            bool toggleInterstellar = uiTSWParameterPanel.ToggleInterstellar;
            bool toggleCollector = uiTSWParameterPanel.ToggleCollector;
            int relatedItemFilter = uiTSWParameterPanel.RelatedItemFilter;

            stations.Clear();

            // 遍历每一个恒星中的每一个行星中的每一个物流运输站
            for (int i = 0; i < GameMain.galaxy.stars.Length; ++i)
            {
                StarData star = GameMain.galaxy.stars[i];

                for (int j = 0; j < star.planets.Length; ++j)
                {
                    PlanetData planet = star.planets[j];

                    // 玩家未抵达的行星上没有工厂
                    if (planet.factory?.transport?.stationPool == null)
                    {
                        continue;
                    }

                    for (int k = 0; k < planet.factory.transport.stationPool.Length; ++k)
                    {
                        StationComponent station = planet.factory.transport.stationPool[k];

                        // 如果拆除物流站点的话，会出现 station 不为 null 但是 entityId 为 0 的情况
                        if (station == null || station.entityId == 0)
                        {
                            continue;
                        }

                        // 是否显示行星内物流站
                        if (!toggleInPlanet && !station.isStellar)
                        {
                            continue;
                        }

                        // 是否显示星际物流运输站
                        if (!toggleInterstellar && station.isStellar)
                        {
                            continue;
                        }

                        // 是否显示采集站
                        if (!toggleCollector && station.isCollector)
                        {
                            continue;
                        }

                        // 通过搜索字符串对站点进行过滤
                        if (!string.IsNullOrWhiteSpace(searchString) && !star.name.Contains(searchString) && !planet.name.Contains(searchString) && !station.GetStationName().Contains(searchString))
                        {
                            continue;
                        }

                        // 过滤相关物品
                        if (relatedItemFilter != Constants.NONE_ITEM_ID)
                        {
                            // 该站点至少有一个槽位包含用户选择的物品
                            int ii = 0;
                            for (; ii < station.storage.Length; ++ii)
                            {
                                if (station.storage[ii].itemId == relatedItemFilter)
                                {
                                    break;
                                }
                            }
                            if (ii == station.storage.Length)
                            {
                                continue;
                            }
                        }

                        stations.Add(new StationInfoBundle(star, planet, station));
                    }
                }
            }

            // 加载站点列表时排一次序
            OnSort();

            // 设置 content 的高度为：物流运输站的数量 x 物流运输站的高度
            contentRectTransform.offsetMin = new Vector2(0, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT * stations.Count);

            // 重置滚动条
            contentRectTransform.offsetMax = new Vector2(0, 0);
        }

        /// <summary>
        /// 根据当前 content 的高度、当前 content 的位置、当前 viewport 的高度以及每一项的高度，计算哪些物流运输站的数据需要被实际显示，
        /// 并设置 transport-stations-entry 的位置，使之恰好处于可视的 viewport 的范围内
        /// 
        /// 虚拟滚动
        /// </summary>
        private void ComputeTransportStationsWindow_VirtualScroll ()
        {
            // 内容高度
            float contentHeight = contentRectTransform.rect.height;

            // 内容向上滚动的距离就是当前内容的位置
            float contentPosition = contentRectTransform.offsetMax.y;

            // 视口高度
            float viewportHeight = viewportRectTransform.rect.height;

            int firstEntryIndex = (int)Math.Floor((double)contentPosition / (double)Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);
            if (firstEntryIndex < 0)
            {
                // 视口位置太靠上，视口的上边缘超过内容的上边缘
                firstEntryIndex = 0;
            }

            float firstEntryPosition = firstEntryIndex * Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT;
            int lastEntryIndex = (int)Math.Floor(((double)contentPosition + (double)viewportHeight) / (double)Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 最后一个项目的索引必须在范围之内
            if (lastEntryIndex < 0)
            {
                // 视口位置太太靠上，视口的下边缘超过内容的上边缘
                lastEntryIndex = 0;
            }
            if (lastEntryIndex > stations.Count - 1)
            {
                // 视口位置太靠下，或者现有的站点数量太少，最后一个站点的位置触不到视口的下边缘
                // 如果没有站点的话， lastEntryIndex 会被置为 -1
                lastEntryIndex = stations.Count - 1;
            }

            if (lastEntryIndex < 0)
            {
                // 没有站点，不进行计算，将所有的 entry 都 SetActive(false)
                for (int i = 0; i < transportStationsEntries.Length; ++i)
                {
                    GameObject go = transportStationsEntries[i].RectTransform.gameObject;
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
                return;
            }

            // 万一预先创建的 entry 对象不够多，也要限制最后一个项目的索引位置
            if (lastEntryIndex - firstEntryIndex + 1 > Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT)
            {
                Plugin.Instance.Logger.LogWarning($"Insufficient pre-created transport-stations-entry objects.");
                Plugin.Instance.Logger.LogWarning($"    Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT: {Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT}");
                Plugin.Instance.Logger.LogWarning($"    Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT: {Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT}");
                Plugin.Instance.Logger.LogWarning($"    contentHeight: {contentHeight}, contentPosition: {contentPosition}, viewportHeight: {viewportHeight}");
                Plugin.Instance.Logger.LogWarning($"    firstEntryIndex: {firstEntryIndex}, firstEntryPosition: {firstEntryPosition}, lastEntryIndex: {lastEntryIndex}");

                lastEntryIndex = Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT + firstEntryIndex - 1;

                Plugin.Instance.Logger.LogWarning($"    lastEntryIndex(modified): {lastEntryIndex}");
            }

            // 准备使用的 entry 对象个数
            int entriesToUseCount = lastEntryIndex - firstEntryIndex + 1;

            for (int i = 0; i < entriesToUseCount; ++i)
            {
                int entryIndex = firstEntryIndex + i;
                float entryPosition = firstEntryPosition + i * Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT;

                // 设置 stationComponent 和相关引用
                transportStationsEntries[i].StationComponent = stations[entryIndex].Station;
                transportStationsEntries[i].StarData = stations[entryIndex].Star;
                transportStationsEntries[i].PlanetData = stations[entryIndex].Planet;

                // 设置大小和位置
                transportStationsEntries[i].RectTransform.Zeroize();
                transportStationsEntries[i].RectTransform.anchorMax = new Vector2(1, 1);
                transportStationsEntries[i].RectTransform.anchorMin = new Vector2(0, 1);
                transportStationsEntries[i].RectTransform.offsetMax = new Vector2(0, -entryPosition); // y 轴正方向朝上
                transportStationsEntries[i].RectTransform.offsetMin = new Vector2(0, -entryPosition - Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

                // 设置激活状态
                transportStationsEntries[i].RectTransform.gameObject.SetActive(true);
            }

            for (int i = entriesToUseCount; i < transportStationsEntries.Length; ++i)
            {
                // 剩余取消激活状态
                transportStationsEntries[i].RectTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 查询参数发生变化
        /// </summary>
        public void OnParameterChange ()
        {
            ComputeTransportStationsWindow_LoadStations();
        }

        /// <summary>
        /// 使用排序参数对 stations 进行排序
        /// </summary>
        public void OnSort ()
        {
            if (locationAndNameSortOrder == SortOrder.ASC)
            {
                stations.Sort(StationInfoBundle.CompareByLocationAndNameASC);
            }
            else if (locationAndNameSortOrder == SortOrder.DESC)
            {
                stations.Sort(StationInfoBundle.CompareByLocationAndNameDESC);
            }
            else
            {
                // NONE 不进行排序
            }
        }

        /// <summary>
        /// 用户输入搜索字符串
        /// </summary>
        public void OnSearch ()
        {
            ComputeTransportStationsWindow_LoadStations();
        }

        private void Update ()
        {
            ComputeTransportStationsWindow_VirtualScroll();
            for (int i = 0; i < transportStationsEntries.Length; ++i)
            {
                transportStationsEntries[i].Update();
            }
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            IsPointerInside = true;
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            IsPointerInside = false;
        }

        protected override bool _OnInit ()
        {
            return true;
        }

        protected override void _OnClose ()
        {
            IsPointerInside = false;
            base._OnClose();
        }
    }
}
