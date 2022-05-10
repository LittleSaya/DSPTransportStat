using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Translation
{
    static class Strings
    {
        static public Language Language { get => language; }

        static private Language language = Language.enUS;

        static public void InitializeTranslations (Language lang)
        {
            language = lang;
        }

        static public class TransportStationsWindow
        {
            static public string Title
            {
                get => language switch
                {
                    Language.enUS => "Transport Stations",
                    Language.zhCN => "物流运输站",
                    _ => "Transport Stations"
                };
            }

            static public string CurrentLabel
            {
                get => language switch
                {
                    Language.enUS => "Current:",
                    Language.zhCN => "当前：",
                    _ => "Current:"
                };
            }

            static public string MaxLabel
            {
                get => language switch
                {
                    Language.enUS => "Max:",
                    Language.zhCN => "最大：",
                    _ => "Max:"
                };
            }

            static public string LocationAndName
            {
                get => language switch
                {
                    Language.enUS => "Location & Name",
                    Language.zhCN => "位置和名称",
                    _ => "Location & Name"
                };
            }

            static public string ASC
            {
                get => language switch
                {
                    Language.enUS => "ASC",
                    Language.zhCN => "升序",
                    _ => "ASC"
                };
            }

            static public string DESC
            {
                get => language switch
                {
                    Language.enUS => "DESC",
                    Language.zhCN => "降序",
                    _ => "DESC"
                };
            }

            static public string ItemSlots
            {
                get => language switch
                {
                    Language.enUS => "Item Slots",
                    Language.zhCN => "物品槽位",
                    _ => "Item Slots"
                };
            }

            static public string SearchLabel
            {
                get => language switch
                {
                    Language.enUS => "Search:",
                    Language.zhCN => "搜索：",
                    _ => "Search:"
                };
            }

            static public string SearchButton
            {
                get => language switch
                {
                    Language.enUS => "Search",
                    Language.zhCN => "搜索",
                    _ => "Search"
                };
            }

            static public string ItemConfigPanel
            {
                get => language switch
                {
                    Language.enUS => "Config Panel",
                    Language.zhCN => "配置面板",
                    _ => "Config Panel"
                };
            }

            static public string ItemHomePanel
            {
                get => language switch
                {
                    Language.enUS => "Home Panel",
                    Language.zhCN => "主页",
                    _ => "Home Panel"
                };
            }

            static public class ParameterPanel
            {
                static public string ToggleInPlanetLabel
                {
                    get => language switch
                    {
                        Language.enUS => "Planetary Station",
                        Language.zhCN => "行星物流站",
                        _ => "Planetary Station"
                    };
                }

                static public string ToggleInterstellarLabel
                {
                    get => language switch
                    {
                        Language.enUS => "Interstellar Station",
                        Language.zhCN => "星际物流站",
                        _ => "Interstellar Station"
                    };
                }

                static public string ToggleCollectorLabel
                {
                    get => language switch
                    {
                        Language.enUS => "Collector",
                        Language.zhCN => "采集站",
                        _ => "Collector"
                    };
                }

                static public string ItemFilterLabel
                {
                    get => language switch
                    {
                        Language.enUS => "Item Filter",
                        Language.zhCN => "过滤物品",
                        _ => "Item Filter"
                    };
                }
            }
        }

        static public class Common
        {
            static public string NoOrder
            {
                get => language switch
                {
                    Language.enUS => "No Order",
                    Language.zhCN => "无订单",
                    _ => "No Order"
                };
            }
            static public class StationType
            {
                static public string InPlanet
                {
                    get => language switch
                    {
                        Language.enUS => "Planetary Station",
                        Language.zhCN => "行星物流站",
                        _ => "Planetary Station"
                    };
                }

                static public string InPlanetCollector
                {
                    get => language switch
                    {
                        Language.enUS => "Planetary Collector",
                        Language.zhCN => "行星采集站",
                        _ => "Planetary Collector"
                    };
                }

                static public string Interstrllar
                {
                    get => language switch
                    {
                        Language.enUS => "Interstellar Station",
                        Language.zhCN => "星际物流站",
                        _ => "Interstellar Station"
                    };
                }

                static public string InterstellarCollector
                {
                    get => language switch
                    {
                        Language.enUS => "Orbital Collector",
                        Language.zhCN => "轨道采集站",
                        _ => "Orbital Collector"
                    };
                }
            }

            static public class StationStoreLogic
            {
                static public string InPlanetStorage
                {
                    get => language switch
                    {
                        Language.enUS => "Local Storage",
                        Language.zhCN => "本地仓储",
                        _ => "Local Storage"
                    };
                }

                static public string InPlanetSupply
                {
                    get => language switch
                    {
                        Language.enUS => "Local Supply",
                        Language.zhCN => "本地供应",
                        _ => "Local Supply"
                    };
                }

                static public string InPlanetDemand
                {
                    get => language switch
                    {
                        Language.enUS => "Local Demand",
                        Language.zhCN => "本地需求",
                        _ => "Local Demand"
                    };
                }
                static public string InterstellarStorage
                {
                    get => language switch
                    {
                        Language.enUS => "Remote Storage",
                        Language.zhCN => "星际仓储",
                        _ => "Remote Storage"
                    };
                }

                static public string InterstellarSupply
                {
                    get => language switch
                    {
                        Language.enUS => "Remote Supply",
                        Language.zhCN => "星际供应",
                        _ => "Remote Supply"
                    };
                }

                static public string InterstellarDemand
                {
                    get => language switch
                    {
                        Language.enUS => "Remote Demand",
                        Language.zhCN => "星际需求",
                        _ => "Remote Demand"
                    };
                }
            }
        }
    }
}
