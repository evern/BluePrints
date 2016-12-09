using BluePrints.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data.Helpers
{
    public static class MorphUtils<TFromEntity, TToEntity>
    {
        public static TToEntity ShallowCopy(TToEntity copyObject, TFromEntity objectToCopy, bool copyVirtualProperties = false)
        {
            IEnumerable<PropertyInfo> objectToCopyProperties = objectToCopy.GetType().GetProperties().Where(p => (copyVirtualProperties == true || !p.GetGetMethod().IsVirtual) && !p.GetCustomAttributes().Any(attr => attr.GetType() == typeof(ProjectionPropertyAttribute)));
            foreach (var objectToCopyProperty in objectToCopyProperties)
            {
                if (!objectToCopyProperty.CanWrite || !objectToCopyProperty.CanRead)
                    continue;

                var objectToCopyValue = objectToCopyProperty.GetValue(objectToCopy);
                PropertyInfo copyObjectProperty = copyObject.GetType().GetProperty(objectToCopyProperty.Name);

                copyObjectProperty.SetValue(copyObject, objectToCopyValue);
            }

            return copyObject;
        }
    }

    public static class DataUtils
    {
        public static object ShallowCopy(object copyObject, object objectToCopy, bool copyVirtualProperties = false)
        {
            IEnumerable<PropertyInfo> objectToCopyProperties = objectToCopy.GetType().GetProperties().Where(p => (copyVirtualProperties == true || !p.GetGetMethod().IsVirtual) && !p.GetCustomAttributes().Any(attr => attr.GetType() == typeof(ProjectionPropertyAttribute)));
            foreach(var objectToCopyProperty in objectToCopyProperties)
            {
                if (!objectToCopyProperty.CanWrite || !objectToCopyProperty.CanRead)
                    continue;

                var objectToCopyValue = objectToCopyProperty.GetValue(objectToCopy);
                PropertyInfo copyObjectProperty = copyObject.GetType().GetProperty(objectToCopyProperty.Name);
                
                copyObjectProperty.SetValue(copyObject, objectToCopyValue);
            }

            return copyObject;
        }

        public static PropertyInfo GetKeyPropertyInfo(Type type)
        {
            try
            {
                PropertyInfo keyPropertyInfo = type.GetProperties().Single(property => property.GetCustomAttributes().Any(attr => attr.GetType() == typeof(KeyAttribute)));
                return keyPropertyInfo;
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<PropertyInfo> GetProjectionPropertyInfos(Type type)
        {
            try
            {
                List<PropertyInfo> projectionPropertyInfos = type.GetProperties().Where(property => property.GetCustomAttributes().Any(attr => attr.GetType() == typeof(ProjectionPropertyAttribute))).ToList();
                return projectionPropertyInfos;
            }
            catch
            {
                return null;
            }
        }

        public static PropertyInfo GetFilterNamePropertyInfo(Type type)
        {
            try
            {
                PropertyInfo filterPropertyInfo = type.GetProperties().Single(property => property.GetCustomAttributes().Any(attr => attr.GetType() == typeof(FilterNameAttribute)));
                return filterPropertyInfo;
            }
            catch
            {
                return null;
            }
        }

        public static PropertyInfo GetFilterValuePropertyInfo(Type type)
        {
            try
            {
                PropertyInfo filterPropertyInfo = type.GetProperties().Single(property => property.GetCustomAttributes().Any(attr => attr.GetType() == typeof(FilterValueAttribute)));
                return filterPropertyInfo;
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<string> GetConstraintPropertyStrings(Type type)
        {
            ConstraintAttributes TypeSpecificConstraintAttribute = (ConstraintAttributes)Attribute.GetCustomAttribute(type, typeof(ConstraintAttributes), false);
            if (TypeSpecificConstraintAttribute != null)
                return TypeSpecificConstraintAttribute.ColumnNames;

            return null;
        }

        public static IEnumerable<string> GetRequiredPropertyStrings(Type type)
        {
            List<string> requiredPropertyStrings = new List<string>();
            PropertyInfo[] props = type.GetProperties();
            foreach(PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    RequiredAttribute requiredAttr = attr as RequiredAttribute;
                    if (requiredAttr != null)
                        requiredPropertyStrings.Add(prop.Name);
                }
            }

            return requiredPropertyStrings;
        }


        /// <summary>
        /// Recurse member instance to change its value
        /// </summary>
        /// <param name="propertyString">Property string to change</param>
        /// <param name="parentInstance">Instance to modify</param>
        /// <param name="value">Value to modify</param>
        public static void SetNestedValue(string propertyString, object parentInstance, object value)
        {
            string[] propertyNames = propertyString.Split('.');
            string firstPropertyName = propertyNames.First();
            var childInstance = parentInstance.GetType().GetProperty(firstPropertyName).GetValue(parentInstance);

            if (!propertyString.Contains("."))
                parentInstance.GetType().GetProperty(firstPropertyName).SetValue(parentInstance, value);
            else
            {
                propertyString = propertyString.Replace(firstPropertyName + ".", string.Empty);
                SetNestedValue(propertyString, childInstance, value);
            }
        }

        /// <summary>
        /// Recurse member instance to get its value
        /// </summary>
        /// <param name="propertyString">Property string to get</param>
        /// <param name="parentInstance">Instance to get</param>
        public static object GetNestedValue(string propertyString, object parentInstance)
        {
            string[] propertyNames = propertyString.Split('.');
            string firstPropertyName = propertyNames.First();
            var childInstance = parentInstance.GetType().GetProperty(firstPropertyName).GetValue(parentInstance);

            if (!propertyString.Contains("."))
                return parentInstance.GetType().GetProperty(firstPropertyName).GetValue(parentInstance);
            else
            {
                propertyString = propertyString.Replace(firstPropertyName + ".", string.Empty);
                return GetNestedValue(propertyString, childInstance);
            }
        }

        public static PropertyInfo GetNestedPropertyInfo(string propertyString, object parentInstance)
        {
            string[] propertyNames = propertyString.Split('.');
            string firstPropertyName = propertyNames.First();
            var childInstance = parentInstance.GetType().GetProperty(firstPropertyName).GetValue(parentInstance);

            if (!propertyString.Contains("."))
                return parentInstance.GetType().GetProperty(firstPropertyName);
            else
            {
                propertyString = propertyString.Replace(firstPropertyName + ".", string.Empty);
                return GetNestedPropertyInfo(propertyString, childInstance);
            }
        }
    }
}
