using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common
{
    public static class LoginCredentials
    {
        public static USER CurrentUser { get; set; }

        public static bool hasPermission(string permissionName)
        {
            if (CurrentUser == null)
                return false;
            else if (CurrentUser.NAME == CommonResources.AdminUsername)
                return true;
            else if (CurrentUser.ROLE == null)
                return false;
            else if (CurrentUser.ROLE.ROLE_PERMISSION.Count == 0)
                return false;

            string permissionKey = PermissionDictionary.First(x => x.Value == permissionName).Key;
            return CurrentUser.ROLE.ROLE_PERMISSION.Any(x => x.PERMISSION == permissionKey);
        }

        public static Guid CurrentUserGuid()
        {
            if (CurrentUser == null)
                return Guid.Empty;

            return CurrentUser.GUID;
        }

        private static Dictionary<string, string> PermissionDictionary = GetPermissionLookUpInDictionary();
        public static Dictionary<string, string> GetPermissionLookUpInDictionary()
        {
            Dictionary<string, string> returnPermissions = new Dictionary<string, string>();
            ResourceSet resourceSet = PermissionResources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (System.Collections.DictionaryEntry permission in resourceSet)
            {
                returnPermissions.Add(permission.Key.ToString(), permission.Value.ToString());
            }

            return returnPermissions;
        }
    }
}
