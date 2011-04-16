using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EVEInventionCalc.DataContext;
using EVEInventionCalc.Logging;

namespace EVEInventionCalc
{
    public class EVECache
    {
        protected static TypesDataContext _typesContext = new TypesDataContext();

        #region Caches

        protected static Dictionary<int, EVEItem> _itemsByID = new Dictionary<int, EVEItem>();
        protected static Dictionary<string, EVEItem> _itemsByName = new Dictionary<string, EVEItem>();

        #endregion 

        #region Statistics

        public static int CachedItemCount { get { return _itemsByID.Count; } }

        #endregion

        #region Cache Population

        protected static EVEItem _loadItem(string typeName, int? typeID)
        {
            var x = (from t in _typesContext.invTypes
                     join g in _typesContext.invGroups on t.groupID equals g.groupID
                     where (typeID.HasValue && t.typeID == typeID) || (typeName != null && t.typeName == typeName)
                     select new EVEItem(t.typeID, t.typeName, g.categoryID == 16, g.groupID)).FirstOrDefault();

            _itemsByID.Add(x.TypeID, x);
            _itemsByName.Add(x.TypeName, x);

            return x;
        }

        #endregion

        #region Item Loading

        public static EVEItem GetItem(int typeID)
        {
            if (_itemsByID.ContainsKey(typeID))
                return _itemsByID[typeID];

            return _loadItem(null, typeID);
        }

        public static EVEItem GetItem(string typeName)
        {
            if (_itemsByName.ContainsKey(typeName))
                return _itemsByName[typeName];

            return _loadItem(typeName, null);
        }

        public static EVEItem GetItem(invType type)
        {
            return GetItem(type.typeID);
        }

        #endregion
    }
}
