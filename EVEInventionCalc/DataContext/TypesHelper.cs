using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVEInventionCalc.DataContext
{
    public class TypesHelper
    {
        protected static TypesDataContext _typesContext = new TypesDataContext();

        protected static Dictionary<string, invType> _cachedTypesByName = new Dictionary<string, invType>();
        protected static Dictionary<int, invType> _cachedTypesByID = new Dictionary<int, invType>();

        protected static Dictionary<short, invGroup> _cachedGroupsByID = new Dictionary<short, invGroup>();

        public static int CachedTypeCount { get { return _cachedTypesByID.Count; } }
        public static int CachedGroupCount { get { return _cachedGroupsByID.Count; } }

        public static invBlueprintType GetBlueprintType(invType type)
        {
            return GetBlueprintType(type.typeID);
        }

        public static invBlueprintType GetBlueprintType(int typeID)
        {
            return (from b in _typesContext.invBlueprintTypes
                    where b.productTypeID == typeID
                    select b).FirstOrDefault();
        }

        public static IEnumerable<ramTypeRequirement> GetRamTypeRequirements(invBlueprintType blueprintType)
        {
            return GetRamTypeRequirements(blueprintType.blueprintTypeID);
        }

        public static IEnumerable<ramTypeRequirement> GetRamTypeRequirements(int blueprintTypeID)
        {
            return (from r in _typesContext.ramTypeRequirements
                    where r.typeID == blueprintTypeID
                    select r);
        }

        public static invType GetType(string typeName)
        {
            if (_cachedTypesByName.ContainsKey(typeName))                            
                return _cachedTypesByName[typeName];            

            invType loadedType = (from t in _typesContext.invTypes
                                  where t.typeName == typeName
                                  select t).FirstOrDefault();

            if (loadedType != null)
            {
                _cachedTypesByName.Add(loadedType.typeName, loadedType);
                _cachedTypesByID.Add(loadedType.typeID, loadedType);
            }

            return loadedType;
        }

        public static invType GetType(int typeID)
        {
            if (_cachedTypesByID.ContainsKey(typeID))                            
                return _cachedTypesByID[typeID];                        

            invType loadedType = (from t in _typesContext.invTypes
                                  where t.typeID == typeID
                                  select t).FirstOrDefault();

            if (loadedType != null)
            {
                _cachedTypesByID.Add(loadedType.typeID, loadedType);
                _cachedTypesByName.Add(loadedType.typeName, loadedType);
            }

            return loadedType;
        }

        public static invGroup GetGroup(invType type)
        {
            if (type.groupID == null)
                return null;
            return GetGroup(type.groupID.Value);
        }

        public static invGroup GetGroup(short groupID)
        {
            if (_cachedGroupsByID.ContainsKey(groupID))                            
                return _cachedGroupsByID[groupID];

            invGroup loadedGroup = (from g in _typesContext.invGroups
                                    where g.groupID == groupID
                                    select g).FirstOrDefault();
            if (loadedGroup != null)
            {
                _cachedGroupsByID.Add(loadedGroup.groupID, loadedGroup);
            }

            return loadedGroup;
        }

        public static IEnumerable<invTypeMaterial> GetTypeMaterials(invType type)
        {
            return GetTypeMaterials(type.typeID);
        }

        public static IEnumerable<invTypeMaterial> GetTypeMaterials(int typeID)
        {
            return (from m in _typesContext.invTypeMaterials
                    where m.typeID == typeID
                    select m);
        }

        public static invType GetMeta0Type(invType type)
        {
            return GetMeta0Type(type.typeID);
        }

        public static invType GetMeta0Type(int typeID)
        {
            return (from t in _typesContext.invTypes
                    join mt in _typesContext.invMetaTypes on t.typeID equals mt.typeID
                    select t).FirstOrDefault();
                        
        }

        public static invType GetMeta0Type(string typeName)
        {
            return (from t2 in _typesContext.invTypes
                    join mt in _typesContext.invMetaTypes on t2.typeID equals mt.typeID
                    join t1 in _typesContext.invTypes on mt.parentTypeID equals t1.typeID
                    where t2.typeName == typeName
                    select t1).FirstOrDefault();
        }

        public static IEnumerable<invMetaType> GetMetaVariants(int typeID)
        {
            return (from mt0 in _typesContext.invMetaTypes
                    join mt1 in _typesContext.invMetaTypes on mt0.parentTypeID equals mt1.parentTypeID
                    where mt0.typeID == typeID
                    select mt1);
                    
        }

    }
}
