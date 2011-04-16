using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVEInventionCalc
{
    public class EVEMaterial
    {
        public EVEMaterial(EVEItem item, int quantity, double damage, bool isExtra)
        {
            this.item = item;
            this.quantity = quantity;
            this.damage = damage;
            this.isExtra = isExtra;
        }

        public EVEItem item;
        public int quantity;
        public double damage;
        public bool isExtra;
    }
}
