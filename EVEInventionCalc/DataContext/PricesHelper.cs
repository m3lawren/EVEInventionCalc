using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EVEInventionCalc.DataContext;
using EVEInventionCalc.Logging;

namespace EVEInventionCalc.DataContext
{
    public class PricesHelper
    {
        protected static PricesDataContext _pricesContext = new PricesDataContext();

        protected static int _jitaStationID = 60003760;

        protected static Dictionary<int, decimal> _jitaSellCache = new Dictionary<int, decimal>();
        protected static Dictionary<int, decimal> _jitaBuyCache = new Dictionary<int, decimal>();

        public static int CachedSellCount { get { return _jitaSellCache.Count; } }
        public static int CachedBuyCount { get { return _jitaBuyCache.Count; } }

        public static decimal? JitaBuyPrice(EVEItem item)
        {
            if (_jitaBuyCache.ContainsKey(item.TypeID))
                return _jitaBuyCache[item.TypeID];

            decimal? result = (from t in _pricesContext.vwBestPrices
                               where (t.typeId == item.TypeID && t.stationId == _jitaStationID)
                               select t.buyPrice).FirstOrDefault();

            if (result.HasValue)
                _jitaBuyCache.Add(item.TypeID, result.Value);
            
            AddItem(item);

            return result;
        }

        public static decimal? JitaSellPrice(EVEItem item)
        {
            if (_jitaSellCache.ContainsKey(item.TypeID))
                return _jitaSellCache[item.TypeID];

            decimal? result = (from t in _pricesContext.vwBestPrices
                               where (t.typeId == item.TypeID && t.stationId == _jitaStationID)
                               select t.sellPrice).FirstOrDefault();

            if (result.HasValue)
                _jitaSellCache.Add(item.TypeID, result.Value);
            
            AddItem(item);
            
            return result;

        }

        public static void AddItem(EVEItem item)
        {
            var result = (from t in _pricesContext.tblActiveItems
                          where (t.itemID == item.TypeID)
                          select t).ToList();

            if (result.Count == 0)
            {
                tblActiveItem newItem = new tblActiveItem();
                newItem.itemID = item.TypeID;
                newItem.isActive = true;
                newItem.lastUpdated = null;

                _pricesContext.tblActiveItems.InsertOnSubmit(newItem);

                _pricesContext.SubmitChanges();
            }
            else
            {
                tblActiveItem foundItem = result.First();
                if (foundItem.isActive == false)
                {
                    foundItem.isActive = true;
                    _pricesContext.SubmitChanges();
                }
            }

        }

        public static IEnumerable<int> GetInventedTypes()
        {
            return (from t in _pricesContext.tblInventedItems
                    where t.isActive
                    select t.typeID);
        }
    }
}
