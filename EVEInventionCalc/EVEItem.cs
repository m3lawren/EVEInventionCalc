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
            TypeID = typeID;
            TypeName = typeName;
            IsSkill = isSkill;
            GroupID = groupID;
        }

        public int TypeID { get; private set; }
        public string TypeName { get; private set; }
        public bool IsSkill { get; private set; }
        public int GroupID { get; private set; }
    }
}
