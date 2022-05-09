using DSPTransportStat.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Extensions
{
    static class StationStoreExtensions
    {
        static public string GetCountAsString (this StationStore stationStore)
        {
            return $"{stationStore.count}";
        }

        static public string GetTotalOrderAsString (this StationStore stationStore)
        {
            return $"{stationStore.totalOrdered}";
        }

        static public string GetMaxAsString (this StationStore stationStore)
        {
            return $"{stationStore.max}";
        }

        static public string GetLocalLogicAsString (this StationStore stationStore)
        {
            return stationStore.localLogic switch
            {
                ELogisticStorage.None => Strings.Common.StationStoreLogic.InPlanetStorage,
                ELogisticStorage.Supply => Strings.Common.StationStoreLogic.InPlanetSupply,
                ELogisticStorage.Demand => Strings.Common.StationStoreLogic.InPlanetDemand,
                _ => "Undefined Local Logic (" + stationStore.localLogic.ToString() + ")",
            };
        }

        static public string GetRemoteLogicAsString (this StationStore stationStore)
        {
            return stationStore.remoteLogic switch
            {
                ELogisticStorage.None => Strings.Common.StationStoreLogic.InterstellarStorage,
                ELogisticStorage.Supply => Strings.Common.StationStoreLogic.InterstellarSupply,
                ELogisticStorage.Demand => Strings.Common.StationStoreLogic.InterstellarDemand,
                _ => "Undefined Remote Logic (" + stationStore.remoteLogic.ToString() + ")",
            };
        }
    }
}
