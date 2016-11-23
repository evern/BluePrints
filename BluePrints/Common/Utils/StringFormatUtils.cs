using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Helpers
{
    public static class StringFormatUtils
    {
        public static string GetEntityNameByEntitiesType(object entities)
        {
            return entities.GetType().ToString().Replace("BluePrints.Data.", "").Replace("[]", "");
        }

        public static string GetEntityNameByType(Type type)
        {
            return type.ToString().Replace("BluePrints.Data.", "").Replace("[]", "");
        }
    }
}
