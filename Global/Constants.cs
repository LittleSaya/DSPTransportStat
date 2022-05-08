using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Global
{
    static public class Constants
    {
        /// <summary>
        /// 实际存在于场景中，用于显示的物流运输站条目数量
        /// </summary>
        public const int DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT = 10;

        /// <summary>
        /// 每一个物流运输条目的高度
        /// </summary>
        public const int TRANSPORT_STATIONS_ENTRY_HEIGHT = 120;

        /// <summary>
        /// 表示没有物品的物品ID
        /// </summary>
        public const int NONE_ITEM_ID = int.MinValue;
    }
}
