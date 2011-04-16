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
            EVEBlueprint bp = InventionCalc.GetBlueprint(typeName);

            var mats = baseMats.ToList();
            mats.AddRange(extraMats);

            mats = InventionCalc.AdjustForME(mats, bp.MaterialEfficiency).OrderBy(x => x.item.TypeName).OrderByDescending(x => x.quantity).Where(x => x.item.IsSkill == false).ToList();

            Logger.Log.InfoFormat("Done.");

            decimal? manufactureCost = InventionCalc.GetMaterialCosts(mats);

            IEnumerable<EVEMaterial> inventionReqs = InventionCalc.GetInventionRequirements(typeName);

            decimal? inventionCost = InventionCalc.GetMaterialCosts(inventionReqs);

            double inventionChance = InventionCalc.GetBaseInventionChance(typeName) * (1.0 + 0.04) * (1 + 8 * 0.02);

            decimal? successfulInventionCost = inventionCost / (decimal)inventionChance / (decimal)bp.NumRuns;

            decimal? resultPrice = PricesHelper.JitaSellPrice(EVECache.GetItem(typeName));
            if (resultPrice.HasValue && inventionCost.HasValue && manufactureCost.HasValue)
            {
                decimal profitPerRun = (resultPrice - successfulInventionCost - manufactureCost).Value;

                decimal productionTime = (decimal)(Math.Ceiling((double)(bp.BaseProductionTime * 2.0m * 0.8m * 0.75m * bp.NumRuns) / (12.0 * 60.0 * 60.0)) * 12.0 * 60.0 * 60.0);

                decimal profitPerDay = 60 * 60 * 24 / (productionTime / bp.NumRuns) * profitPerRun;

                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", typeName, inventionCost, inventionChance, successfulInventionCost, manufactureCost, resultPrice.Value, profitPerRun, profitPerDay, productionTime));
            }

            Logger.Log.Debug("Processing completed.");
        }

        static void doInventionPhase(string typeName)
        {
            // Calculates the following:
            //   * invention materials
            //   * invention chance
            //   * resulting BPO stats
            int encryptionSkill = 3;
            int scienceSkill1 = 4;
            int scienceSkill2 = 4;

            IEnumerable<EVEMaterial> inventionMats = InventionCalc.GetInventionRequirements(typeName);
            double inventionChance = InventionCalc.GetBaseInventionChance(typeName) * (1.0 + 0.01 * encryptionSkill) * (1 + (scienceSkill1 + scienceSkill2) * 0.02);

            EVEBlueprint bp = InventionCalc.GetBlueprint(typeName);
            bp.ProductionEfficiency = bp.MaterialEfficiency = -4;
            bp.NumRuns = bp.MaxRuns / 10;

        }

        static void Main(string[] args)
        {
            Logger.Level["App"].Push("InvCalc");

            //doInventionPhase("800mm Repeating Artillery II");

            TextWriter writer = new StreamWriter("output.csv");

            writer.WriteLine("Name,InventionCost,InventionChance,SuccessfulInventionCost,ManufactureCost,ItemPrice,ProfitPerRun,ProfitPerDay,ProductionTime");

            foreach (int typeID in PricesHelper.GetInventedTypes())
            {
                EVEItem item = EVECache.GetItem(typeID);

                doWork(item.TypeName, writer);
            }

            writer.Flush();
            writer.Close();

            Logger.Log.InfoFormat("Cached items: {0}", EVECache.CachedItemCount);            
        }
    }
}
