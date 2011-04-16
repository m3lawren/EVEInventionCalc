using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EVEInventionCalc.DataContext;
using EVEInventionCalc.Logging;

namespace EVEInventionCalc
{
    public class InventionCalc
    {
        public static IEnumerable<EVEMaterial> ProductionExtraMaterials(string typeName)
        {
            invType dcType = TypesHelper.GetType(typeName);

            List<EVEMaterial> matInfo = new List<EVEMaterial>();
            invBlueprintType bpType = TypesHelper.GetBlueprintType(dcType);
            
            if (bpType != null)
            {
                foreach (ramTypeRequirement m in TypesHelper.GetRamTypeRequirements(bpType).Where(x => x.activityID == 1))
                {
                    matInfo.Add(new EVEMaterial(EVECache.GetItem(m.requiredTypeID), m.quantity.Value, m.damagePerJob.Value, true));
                }
            }

            return matInfo;
        }

        public static IEnumerable<EVEMaterial> ProductionBaseMaterials(string typeName)
        {
            invType dcType = TypesHelper.GetType(typeName);

            Dictionary<int, EVEMaterial> matInfo = new Dictionary<int, EVEMaterial>();
            
            foreach (invTypeMaterial m in TypesHelper.GetTypeMaterials(dcType))
            {
                matInfo[m.materialTypeID] = new EVEMaterial(EVECache.GetItem(m.materialTypeID), m.quantity, 1.0, false);
            }

            invBlueprintType bpType = TypesHelper.GetBlueprintType(dcType);
            if (bpType != null)
            {
                foreach (ramTypeRequirement m in TypesHelper.GetRamTypeRequirements(bpType).Where(x => x.activityID == 1))
                {
                    if (m.recycle.Value)
                    {
                        IEnumerable<EVEMaterial> subMats = ProductionBaseMaterials(TypesHelper.GetType(m.requiredTypeID).typeName);
                        foreach (EVEMaterial subMat in subMats)
                        {
                            if (matInfo.ContainsKey(subMat.item.typeID))
                                matInfo[subMat.item.typeID].quantity -= subMat.quantity;
                        }
                    }
                }
            }

            return matInfo.Values.Where(x => x.quantity > 0).OrderByDescending(x => x.quantity).ToList();
        }

        public static IEnumerable<EVEMaterial> AdjustForME(IEnumerable<EVEMaterial> materials, int matEfficiency)
        {
            List<EVEMaterial> result = new List<EVEMaterial>();

            foreach (EVEMaterial m in materials)
            {
                if (m.isExtra)
                    result.Add(m);
                else
                {
                    EVEMaterial newMat = new EVEMaterial(m.item, m.quantity, m.damage, m.isExtra);
                    int waste;
                    if (matEfficiency >= 0)
                        waste = (int)Math.Floor(0.5 + newMat.quantity * 0.1 * (1 / (matEfficiency + 1)));
                    else
                    {
                        waste = (int)Math.Floor(0.5 + newMat.quantity * 0.1 * (1 - matEfficiency));
                    }
                    newMat.quantity += waste;
                    result.Add(newMat);
                }
            }

            return result;
        }

        public static IEnumerable<EVEMaterial> GetInventionRequirements(string t2ItemName)
        {
            List<EVEMaterial> result = new List<EVEMaterial>();

            invType t1type = TypesHelper.GetMeta0Type(t2ItemName);
            invBlueprintType bpType = TypesHelper.GetBlueprintType(t1type);
            IEnumerable<ramTypeRequirement> ramReqs = TypesHelper.GetRamTypeRequirements(bpType).Where(x => x.activityID == 8);            

            foreach (ramTypeRequirement r in ramReqs)
            {
                invType reqType = TypesHelper.GetType(r.requiredTypeID);
                EVEMaterial newMat = new EVEMaterial(EVECache.GetItem(reqType), r.quantity.Value, r.damagePerJob.Value, true);

                if (reqType.groupID == 716)
                    newMat.damage = 0;

                result.Add(newMat);
            }

            return result;
        }

        public static decimal GetProductionTime(string typeName, int PE)
        {
            invBlueprintType blueprint = TypesHelper.GetBlueprintType(TypesHelper.GetType(typeName));

            decimal baseProdTime = (decimal)blueprint.productionTime.Value;
            decimal prodMod = (decimal)blueprint.productivityModifier.Value;            

            if (PE < 0)
            {
                return baseProdTime * 0.8m * (1 - (prodMod / baseProdTime) * (PE - 1));
            }
            else
            {
                return baseProdTime * 0.8m * (1 - (prodMod / baseProdTime) * (PE / (1 + PE)));
            }
        }

        public static IEnumerable<EVEItem> GetInventableProducts()
        {
            TypesDataContext context = new TypesDataContext();

            List<int> knownSkills = new List<int>();
            knownSkills.Add(20423);
            knownSkills.Add(20424);
            knownSkills.Add(20418);

            foreach (int typeID in (from req in context.ramTypeRequirements
                                    where req.activityID == 8
                                    select req.typeID).Distinct())
            {
                var x = (from req in context.ramTypeRequirements
                         where req.activityID == 8 && req.typeID == typeID && knownSkills.Contains(req.requiredTypeID)
                         select req.typeID).Count();
                if (x == 2)
                    yield return EVECache.GetItem(typeID);
            }
        }

        public static decimal? GetMaterialCosts(IEnumerable<EVEMaterial> materials)
        {
            decimal result = 0;
            bool unknownPrice = false;

            foreach (EVEMaterial m in materials)
            {
                decimal? price = PricesHelper.JitaSellPrice(m.item);
                if (price.HasValue)
                    result += price.Value * (decimal)m.damage * m.quantity;
                else
                    unknownPrice = true;                       
            }

            return (unknownPrice ? (decimal?)null : result);
        }

        public static double GetBaseInventionChance(string typeName)
        {
            EVEItem item = EVECache.GetItem(typeName);
            switch (item.groupID)
            {
                case 25: case 420: case 513: case 893: case 902: case 324: case 830: case 831: case 541: case 834:
                    return 0.3;
                case 26: case 28: case 894: case 906: case 833: case 358: case 832: case 380:
                    return 0.25;
                case 27: case 419: case 898: case 900: case 540:
                    return 0.2;
                default:
                    return 0.4;
            }
        }
    }
}
