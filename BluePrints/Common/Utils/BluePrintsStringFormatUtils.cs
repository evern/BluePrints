using System;

namespace BluePrints.Common.Utils
{
    public static class BluePrintsStringFormatUtils
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