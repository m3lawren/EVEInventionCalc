using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using EVEInventionCalc;
using EVEInventionCalc.DataContext;
using EVEInventionCalc.Logging;

namespace EVEInventionCalcConsole
{
    class Program
    {

        static void doWork(string typeName, TextWriter writer)
        {
            Logger.Log.InfoFormat("Processing {0}", typeName);

            Logger.Log.InfoFormat("Retrieving materials...");

            var baseMats = InventionCalc.ProductionBaseMaterials(typeName);
            var extraMats = InventionCalc.ProductionExtraMaterials(typeName);

            var mats = baseMats.ToList();
            mats.AddRange(extraMats);

            mats = InventionCalc.AdjustForME(mats, -4).OrderBy(x => x.item.typeName).OrderByDescending(x => x.quantity).Where(x => x.item.isSkill == false).ToList();

            Logger.Log.InfoFormat("Done.");

            decimal? manufactureCost = InventionCalc.GetMaterialCosts(mats);

            IEnumerable<EVEMaterial> inventionReqs = InventionCalc.GetInventionRequirements(typeName);

            decimal? inventionCost = InventionCalc.GetMaterialCosts(inventionReqs);

            double inventionChance = InventionCalc.GetBaseInventionChance(typeName) * (1.0 + 0.03) * (1 + 6 * 0.02);

            decimal? successfulInventionCost = inventionCost / (decimal)inventionChance / 10.0m;

            decimal? resultPrice = PricesHelper.JitaSellPrice(EVECache.GetItem(typeName));
            if (resultPrice.HasValue && inventionCost.HasValue && manufactureCost.HasValue)
            {
                decimal profitPerRun = (resultPrice - successfulInventionCost - manufactureCost).Value;

                decimal productionTime = InventionCalc.GetProductionTime(typeName, -4);

                decimal profitPerDay = 60 * 60 * 24 / productionTime * profitPerRun;

                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", typeName, inventionCost, inventionChance, successfulInventionCost, manufactureCost, resultPrice.Value, profitPerRun, profitPerDay));
            }

            Logger.Log.Debug("Processing completed.");
        }

        static void Main(string[] args)
        {
            Logger.Level["App"].Push("InvCalc");

            TextWriter writer = new StreamWriter("output.csv");

            writer.WriteLine("Name,InventionCost,InventionChance,SuccessfulInventionCost,ManufactureCost,ItemPrice,ProfitPerRun,ProfitPerDay");

            foreach (int typeID in PricesHelper.GetInventedTypes())
            {
                EVEItem item = EVECache.GetItem(typeID);

                doWork(item.typeName, writer);
            }

            writer.Flush();
            writer.Close();

            foreach (EVEItem i in InventionCalc.GetInventableProducts())
            {
                Logger.Log.InfoFormat("Item: {0}", i.typeName);
            }

            Logger.Level["App"].Clear();
            Logger.Level["App"].Push("Cache");

            Logger.Log.DebugFormat("Cached items: {0}", EVECache.CachedItemCount);
            Logger.Log.DebugFormat("Cached types: {0}", TypesHelper.CachedTypeCount);
            Logger.Log.DebugFormat("Cached groups: {0}", TypesHelper.CachedGroupCount);
            Logger.Log.DebugFormat("Cached buys: {0}", PricesHelper.CachedBuyCount);
            Logger.Log.DebugFormat("Cached sells: {0}", PricesHelper.CachedSellCount);            

        }
    }
}
