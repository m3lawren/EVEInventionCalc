using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EVEInventionCalc.DataContext;

namespace EVEInventionCalc
{
    public class EVEItem
    {
        public EVEItem(int typeID, string typeName, bool isSkill, int groupID)
        {
            this.typeID = typeID;
            this.typeName = typeName;
            this.isSkill = isSkill;
            this.groupID = groupID;
        }

        public int typeID;
        public string typeName;
        public bool isSkill;
        public int groupID;
    }
}
