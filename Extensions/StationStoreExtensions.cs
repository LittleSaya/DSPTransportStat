using DSPTransportStat.Global;
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
                ELogisticStorage.None => Translations.Common.StationStoreLogic.InPlanetStorage,
                ELogisticStorage.Supply => Translations.Common.StationStoreLogic.InPlanetSupply,
                ELogisticStorage.Demand => Translations.Common.StationStoreLogic.InPlanetDemand,
                _ => "Undefined Local Logic (" + stationStore.localLogic.ToString() + ")",
            };
        }

        static public string GetRemoteLogicAsString (this StationStore stationStore)
        {
            return stationStore.remoteLogic switch
            {
                ELogisticStorage.None => Translations.Common.StationStoreLogic.InterstellarStorage,
                ELogisticStorage.Supply => Translations.Common.StationStoreLogic.InterstellarSupply,
                ELogisticStorage.Demand => Translations.Common.StationStoreLogic.InterstellarDemand,
                _ => "Undefined Remote Logic (" + stationStore.remoteLogic.ToString() + ")",
            };
        }
    }
}
