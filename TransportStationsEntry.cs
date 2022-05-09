using DSPTransportStat.CacheObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DSPTransportStat.Extensions;
using DSPTransportStat.Translation;

namespace DSPTransportStat
{
    /// <summary>
    /// 物流站点窗口中的列表项，每一个 TransportStationsEntry 对应一个预先创建好的 transport-stations-entry GameObject ，通过对 UI 组件的引用来控制显示的内容，其具体位置在外部（ UITrasnportStationsWindow ）通过虚拟滚动进行控制
    /// 
    /// 主要功能是根据传入的 StationComponent 更新 UI 组件中显示的内容
    /// </summary>
    class TransportStationsEntry
    {
        // 相关对象的引用
        public RectTransform RectTransform { get; set; } = null;

        public StationComponent StationComponent { get; set; } = null;

        public StarData StarData { get; set; } = null;

        public PlanetData PlanetData { get; set; } = null;

        // 常量
        static public Color PositiveColor = new Color(0.3804f, 0.8471f, 1f, 0.698f);

        static public Color NegativeColor = new Color(0.9922f, 0.5882f, 0.3686f, 0.698f);

        // UI控件
        public Text Star { get; set; } = null;

        public Text Planet { get; set; } = null;

        public Text StationType { get; set; } = null;

        public Text Name { get; set; } = null;

        public Image Item01Icon { get; set; } = null;

        public Text Item01CurrentAmount { get; set; } = null;

        public GameObject Item01CurrentLabelGO { get; set; } = null;

        public GameObject Item01MaxLabelGO { get; set; } = null;

        public Text Item01OrderAmount { get; set; } = null;

        public Text Item01MaxAmount { get; set; } = null;

        public Text Item01InPlanetStorageUsage { get; set; } = null;

        public Text Item01InterstellarStorageUsage { get; set; } = null;

        public Image Item02Icon { get; set; } = null;

        public Text Item02CurrentAmount { get; set; } = null;

        public Text Item02OrderAmount { get; set; } = null;

        public Text Item02MaxAmount { get; set; } = null;

        public Text Item02InPlanetStorageUsage { get; set; } = null;

        public Text Item02InterstellarStorageUsage { get; set; } = null;

        public GameObject Item02CurrentLabelGO { get; set; } = null;

        public GameObject Item02MaxLabelGO { get; set; } = null;

        public Image Item03Icon { get; set; } = null;

        public Text Item03CurrentAmount { get; set; } = null;

        public Text Item03OrderAmount { get; set; } = null;

        public Text Item03MaxAmount { get; set; } = null;

        public Text Item03InPlanetStorageUsage { get; set; } = null;

        public Text Item03InterstellarStorageUsage { get; set; } = null;

        public GameObject Item03CurrentLabelGO { get; set; } = null;

        public GameObject Item03MaxLabelGO { get; set; } = null;

        public Image Item04Icon { get; set; } = null;

        public Text Item04CurrentAmount { get; set; } = null;

        public Text Item04OrderAmount { get; set; } = null;

        public Text Item04MaxAmount { get; set; } = null;

        public Text Item04InPlanetStorageUsage { get; set; } = null;

        public Text Item04InterstellarStorageUsage { get; set; } = null;

        public GameObject Item04CurrentLabelGO { get; set; } = null;

        public GameObject Item04MaxLabelGO { get; set; } = null;

        public Image Item05Icon { get; set; } = null;

        public Text Item05CurrentAmount { get; set; } = null;

        public Text Item05OrderAmount { get; set; } = null;

        public Text Item05MaxAmount { get; set; } = null;

        public Text Item05InPlanetStorageUsage { get; set; } = null;

        public Text Item05InterstellarStorageUsage { get; set; } = null;

        public GameObject Item05CurrentLabelGO { get; set; } = null;

        public GameObject Item05MaxLabelGO { get; set; } = null;

        public void Update ()
        {
            if (StationComponent == null)
            {
                return;
            }

            Star.text = StarData.displayName;

            Planet.text = PlanetData.displayName;

            if (StationComponent.isCollector)
            {
                if (StationComponent.isStellar)
                {
                    StationType.text = Strings.Common.StationType.InterstellarCollector;
                }
                else
                {
                    StationType.text = Strings.Common.StationType.InPlanetCollector;
                }
            }
            else
            {
                if (StationComponent.isStellar)
                {
                    StationType.text = Strings.Common.StationType.Interstrllar;
                }
                else
                {
                    StationType.text = Strings.Common.StationType.InPlanet;
                }
            }

            // StationComponent 的 name 字段不一定有值，没有值的情况下使用 gid 和 id 拼凑名称
            //Name.text = StationComponent.name;
            if (string.IsNullOrEmpty(StationComponent.name))
            {
                if (StationComponent.isStellar)
                {
                    Name.text = "星际站点号".Translate() + StationComponent.gid.ToString();
                }
                else
                {
                    Name.text = "本地站点号".Translate() + StationComponent.id.ToString();
                }
            }
            else
            {
                Name.text = StationComponent.name;
            }

            if (StationComponent.storage.Length < 1)
            {
                // item01 不存在（没有槽位），隐藏整个 item 对象
                Item01Icon.gameObject.SetActive(false);
                Item01Icon.transform.parent.gameObject.SetActive(false);
            }
            else if (StationComponent.storage[0].itemId == 0)
            {
                // 有槽位，但是没有设置物品，显示 item 对象，但是不显示物品图标和相关的文本
                Item01Icon.gameObject.SetActive(false);
                Item01Icon.transform.parent.gameObject.SetActive(true);
                Item01CurrentLabelGO.SetActive(false);
                Item01MaxLabelGO.SetActive(false);

                Item01CurrentAmount.text = "";
                Item01OrderAmount.text = "";
                Item01MaxAmount.text = "";
                Item01InPlanetStorageUsage.text = "";
                Item01InterstellarStorageUsage.text = "";
            }
            else
            {
                Item01Icon.gameObject.SetActive(true);
                Item01Icon.transform.parent.gameObject.SetActive(true);
                Item01CurrentLabelGO.SetActive(true);
                Item01MaxLabelGO.SetActive(true);

                StationStore stationStore = StationComponent.storage[0];
                ItemProto itemProto = LDB.items.Select(stationStore.itemId);

                Item01Icon.sprite = itemProto.iconSprite;
                Item01CurrentAmount.text = stationStore.GetCountAsString();

                int totalOrder = stationStore.totalOrdered;
                if (totalOrder == 0)
                {
                    Item01OrderAmount.text = Strings.Common.NoOrder;
                    Item01OrderAmount.color = Color.white;
                    Item01OrderAmount.material = ResourceCache.MaterialDefaultUIMaterial;
                }
                else if (totalOrder < 0)
                {
                    Item01OrderAmount.text = $"{totalOrder}";
                    Item01OrderAmount.color = NegativeColor;
                    Item01OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }
                else
                {
                    Item01OrderAmount.text = $"+{totalOrder}";
                    Item01OrderAmount.color = PositiveColor;
                    Item01OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }

                Item01MaxAmount.text = stationStore.GetMaxAsString();
                Item01InPlanetStorageUsage.text = stationStore.GetLocalLogicAsString();
                Item01InterstellarStorageUsage.text = stationStore.GetRemoteLogicAsString();
            }

            if (StationComponent.storage.Length < 2)
            {
                // item02 不存在
                Item02Icon.gameObject.SetActive(false);
                Item02Icon.transform.parent.gameObject.SetActive(false);
            }
            else if (StationComponent.storage[1].itemId == 0)
            {
                // 有槽位，但是没有设置物品，显示 item 对象，但是不显示物品图标
                Item02Icon.gameObject.SetActive(false);
                Item02Icon.transform.parent.gameObject.SetActive(true);
                Item02CurrentLabelGO.SetActive(false);
                Item02MaxLabelGO.SetActive(false);

                Item02CurrentAmount.text = "";
                Item02OrderAmount.text = "";
                Item02MaxAmount.text = "";
                Item02InPlanetStorageUsage.text = "";
                Item02InterstellarStorageUsage.text = "";
            }
            else
            {
                Item02Icon.gameObject.SetActive(true);
                Item02Icon.transform.parent.gameObject.SetActive(true);
                Item02CurrentLabelGO.SetActive(true);
                Item02MaxLabelGO.SetActive(true);

                StationStore stationStore = StationComponent.storage[1];
                ItemProto itemProto = LDB.items.Select(stationStore.itemId);

                Item02Icon.sprite = itemProto.iconSprite;
                Item02CurrentAmount.text = stationStore.GetCountAsString();

                int totalOrder = stationStore.totalOrdered;
                if (totalOrder == 0)
                {
                    Item02OrderAmount.text = Strings.Common.NoOrder;
                    Item02OrderAmount.color = Color.white;
                    Item02OrderAmount.material = ResourceCache.MaterialDefaultUIMaterial;
                }
                else if (totalOrder < 0)
                {
                    Item02OrderAmount.text = $"{totalOrder}";
                    Item02OrderAmount.color = NegativeColor;
                    Item02OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }
                else
                {
                    Item02OrderAmount.text = $"+{totalOrder}";
                    Item02OrderAmount.color = PositiveColor;
                    Item02OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }

                Item02MaxAmount.text = stationStore.GetMaxAsString();
                Item02InPlanetStorageUsage.text = stationStore.GetLocalLogicAsString();
                Item02InterstellarStorageUsage.text = stationStore.GetRemoteLogicAsString();
            }

            if (StationComponent.storage.Length < 3)
            {
                // item03 不存在
                Item03Icon.gameObject.SetActive(false);
                Item03Icon.transform.parent.gameObject.SetActive(false);
            }
            else if (StationComponent.storage[2].itemId == 0)
            {
                // 有槽位，但是没有设置物品，显示 item 对象，但是不显示物品图标
                Item03Icon.gameObject.SetActive(false);
                Item03Icon.transform.parent.gameObject.SetActive(true);
                Item03CurrentLabelGO.SetActive(false);
                Item03MaxLabelGO.SetActive(false);

                Item03CurrentAmount.text = "";
                Item03OrderAmount.text = "";
                Item03MaxAmount.text = "";
                Item03InPlanetStorageUsage.text = "";
                Item03InterstellarStorageUsage.text = "";
            }
            else
            {
                Item03Icon.gameObject.SetActive(true);
                Item03Icon.transform.parent.gameObject.SetActive(true);
                Item03CurrentLabelGO.SetActive(true);
                Item03MaxLabelGO.SetActive(true);

                StationStore stationStore = StationComponent.storage[2];
                ItemProto itemProto = LDB.items.Select(stationStore.itemId);

                Item03Icon.sprite = itemProto.iconSprite;
                Item03CurrentAmount.text = stationStore.GetCountAsString();

                int totalOrder = stationStore.totalOrdered;
                if (totalOrder == 0)
                {
                    Item03OrderAmount.text = Strings.Common.NoOrder;
                    Item03OrderAmount.color = Color.white;
                    Item03OrderAmount.material = ResourceCache.MaterialDefaultUIMaterial;
                }
                else if (totalOrder < 0)
                {
                    Item03OrderAmount.text = $"{totalOrder}";
                    Item03OrderAmount.color = NegativeColor;
                    Item03OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }
                else
                {
                    Item03OrderAmount.text = $"+{totalOrder}";
                    Item03OrderAmount.color = PositiveColor;
                    Item03OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }

                Item03MaxAmount.text = stationStore.GetMaxAsString();
                Item03InPlanetStorageUsage.text = stationStore.GetLocalLogicAsString();
                Item03InterstellarStorageUsage.text = stationStore.GetRemoteLogicAsString();
            }

            if (StationComponent.storage.Length < 4)
            {
                // item04 不存在
                Item04Icon.gameObject.SetActive(false);
                Item04Icon.transform.parent.gameObject.SetActive(false);
            }
            else if (StationComponent.storage[3].itemId == 0)
            {
                // 有槽位，但是没有设置物品，显示 item 对象，但是不显示物品图标
                Item04Icon.gameObject.SetActive(false);
                Item04Icon.transform.parent.gameObject.SetActive(true);
                Item04CurrentLabelGO.SetActive(false);
                Item04MaxLabelGO.SetActive(false);

                Item04CurrentAmount.text = "";
                Item04OrderAmount.text = "";
                Item04MaxAmount.text = "";
                Item04InPlanetStorageUsage.text = "";
                Item04InterstellarStorageUsage.text = "";
            }
            else
            {
                Item04Icon.gameObject.SetActive(true);
                Item04Icon.transform.parent.gameObject.SetActive(true);
                Item04CurrentLabelGO.SetActive(true);
                Item04MaxLabelGO.SetActive(true);

                StationStore stationStore = StationComponent.storage[3];
                ItemProto itemProto = LDB.items.Select(stationStore.itemId);

                Item04Icon.sprite = itemProto.iconSprite;
                Item04CurrentAmount.text = stationStore.GetCountAsString();

                int totalOrder = stationStore.totalOrdered;
                if (totalOrder == 0)
                {
                    Item04OrderAmount.text = Strings.Common.NoOrder;
                    Item04OrderAmount.color = Color.white;
                    Item04OrderAmount.material = ResourceCache.MaterialDefaultUIMaterial;
                }
                else if (totalOrder < 0)
                {
                    Item04OrderAmount.text = $"{totalOrder}";
                    Item04OrderAmount.color = NegativeColor;
                    Item04OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }
                else
                {
                    Item04OrderAmount.text = $"+{totalOrder}";
                    Item04OrderAmount.color = PositiveColor;
                    Item04OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }

                Item04MaxAmount.text = stationStore.GetMaxAsString();
                Item04InPlanetStorageUsage.text = stationStore.GetLocalLogicAsString();
                Item04InterstellarStorageUsage.text = stationStore.GetRemoteLogicAsString();
            }

            if (StationComponent.storage.Length < 5)
            {
                // item05 不存在
                Item05Icon.gameObject.SetActive(false);
                Item05Icon.transform.parent.gameObject.SetActive(false);
            }
            else if (StationComponent.storage[4].itemId == 0)
            {
                // 有槽位，但是没有设置物品，显示 item 对象，但是不显示物品图标
                Item05Icon.gameObject.SetActive(false);
                Item05Icon.transform.parent.gameObject.SetActive(true);
                Item05CurrentLabelGO.SetActive(false);
                Item05MaxLabelGO.SetActive(false);

                Item05CurrentAmount.text = "";
                Item05OrderAmount.text = "";
                Item05MaxAmount.text = "";
                Item05InPlanetStorageUsage.text = "";
                Item05InterstellarStorageUsage.text = "";
            }
            else
            {
                Item05Icon.gameObject.SetActive(true);
                Item05Icon.transform.parent.gameObject.SetActive(true);
                Item05CurrentLabelGO.SetActive(true);
                Item05MaxLabelGO.SetActive(true);

                StationStore stationStore = StationComponent.storage[4];
                ItemProto itemProto = LDB.items.Select(stationStore.itemId);

                Item05Icon.sprite = itemProto.iconSprite;
                Item05CurrentAmount.text = stationStore.GetCountAsString();

                int totalOrder = stationStore.totalOrdered;
                if (totalOrder == 0)
                {
                    Item05OrderAmount.text = Strings.Common.NoOrder;
                    Item05OrderAmount.color = Color.white;
                    Item05OrderAmount.material = ResourceCache.MaterialDefaultUIMaterial;
                }
                else if (totalOrder < 0)
                {
                    Item05OrderAmount.text = $"{totalOrder}";
                    Item05OrderAmount.color = NegativeColor;
                    Item05OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }
                else
                {
                    Item05OrderAmount.text = $"+{totalOrder}";
                    Item05OrderAmount.color = PositiveColor;
                    Item05OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }

                Item05MaxAmount.text = stationStore.GetMaxAsString();
                Item05InPlanetStorageUsage.text = stationStore.GetLocalLogicAsString();
                Item05InterstellarStorageUsage.text = stationStore.GetRemoteLogicAsString();
            }
        }
    }
}
