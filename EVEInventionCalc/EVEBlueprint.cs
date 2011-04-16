using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVEInventionCalc
{
    public class EVEBlueprint
    {        
        public EVEItem Blueprint { get; set; }
        public EVEItem Product { get; set; }

        public int MaterialEfficiency { get; set; }
        public int ProductionEfficiency { get; set; }
        public int NumRuns { get; set; }

        public int MaxRuns { get; set; }
        public int BaseProductionTime { get; set; }
        public int BaseInventionTime { get; set; }        
    }
}
