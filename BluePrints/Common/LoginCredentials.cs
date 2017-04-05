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
        static USER currentUser;
        public static USER CurrentUser
        {
            get { return currentUser; }
            set
            {
                currentUser = value;
                List<ROLE_PERMISSION> user_permission = CurrentUser.ROLE.ROLE_PERMISSION.ToList();
                CurrentUserPermission = new List<ROLE_PERMISSION>(user_permission);
            }
        }

        public static List<ROLE_PERMISSION> CurrentUserPermission { get; set; }

        public static string CurrentHWID { get; set; }

        public static bool hasPermission(string permissionName)
        {
            if (CurrentUser == null)
                return false;
            else if (CurrentUser.NAME == CommonResources.AdminUsername)
                return true;
            else if (CurrentUserPermission == null || CurrentUserPermission.Count == 0)
                return false;

            var permissionKey = PermissionDictionary.First(x => x.Value == permissionName).Key;
            return CurrentUserPermission.Any(x => x.PERMISSION == permissionKey);
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
            var returnPermissions = new Dictionary<string, string>();
            var resourceSet = PermissionResources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture,
                true, true);
            foreach (System.Collections.DictionaryEntry permission in resourceSet)
                returnPermissions.Add(permission.Key.ToString(), permission.Value.ToString());

            return returnPermissions;
        }
    }
}