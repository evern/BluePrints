using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Collections;
using BluePrints.Data;

namespace BluePrints.Common
{
    public static class ActiveDirectory
    {
        public static bool Authenticate(string sUserName, string sPassword)
        {
            string domain = "primerogroup.com.au";
            PrincipalContext oPrincipalContext = new PrincipalContext
            (ContextType.Domain, domain);

            return oPrincipalContext.ValidateCredentials(sUserName, sPassword);
        }

        public static ArrayList Groups(string userDn, bool recursive)
        {
            ArrayList groupMemberships = new ArrayList();
            return AttributeValuesMultiString("memberOf", "LDAP-Server",
                groupMemberships, recursive);
        }

        public static void List_CurrentUser_Attr()
        {
            string name = Environment.UserName;
            // Get the currently connected LDAP context 
            DirectoryEntry entry1 = new DirectoryEntry("LDAP://primerogroup.com.au");
            string domainContext = entry1.Properties["name"].Value as string;
            // Use the default naming context as the connected context may not work for searches
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + domainContext);
            DirectorySearcher adSearch = new DirectorySearcher(entry);

            adSearch.Filter = "(&(objectClass=user)(anr=" + name + "))";

            // Go through all entries from the active directory.
            foreach (SearchResult singleADUser in adSearch.FindAll())
            {
                Console.WriteLine("The properties of the " + singleADUser.GetDirectoryEntry().Name + " are :");
                // Go through all the values found in the search
                foreach (string singleAttribute in ((ResultPropertyCollection)singleADUser.Properties).PropertyNames)
                {
                    Console.WriteLine(singleAttribute + " = ");
                    foreach (Object singleValue in ((ResultPropertyCollection)singleADUser.Properties)[singleAttribute])
                    {
                        Console.WriteLine("\t" + singleValue);
                    }
                }
            }
        }

        public static List<USER> EnumerateOU(string OuDn)
        {
            List<USER> alObjects = new List<USER>();
            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + OuDn);
                foreach (PropertyValueCollection childProperty in directoryObject.Properties)
                {
                    alObjects.Add(new USER() { NAME = childProperty.PropertyName, TITLE = childProperty[0].ToString() });
                }
                directoryObject.Close();
                directoryObject.Dispose();
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message.ToString());
            }
            return alObjects;
        }

        public static List<USER> GetUSERS()
        {
            try
            {
                List<USER> lstADUsers = new List<USER>();
                string DomainPath = "LDAP://primerogroup.com.au/DC=primerogroup,DC=com,DC=au";
                DirectoryEntry searchRoot = new DirectoryEntry(DomainPath);
                DirectorySearcher search = new DirectorySearcher(searchRoot);
                search.Filter = "(&(objectClass=user)(objectCategory=person))";
                search.PropertiesToLoad.Add("samaccountname");
                search.PropertiesToLoad.Add("path");
                search.PropertiesToLoad.Add("mail");
                //search.PropertiesToLoad.Add("usergroup");
                search.PropertiesToLoad.Add("usergroup");
                search.PropertiesToLoad.Add("department");//first name
                search.PropertiesToLoad.Add("title");
                search.PropertiesToLoad.Add("description");
                SearchResult result;
                SearchResultCollection resultCol = search.FindAll();

                if (resultCol != null)
                {
                    for (int counter = 0; counter < resultCol.Count; counter++)
                    {

                        string UserNameEmailString = string.Empty;
                        result = resultCol[counter];

                        lstADUsers.Add(new USER()
                        {
                            NAME = result.Properties.Contains("samaccountname") ? (String)result.Properties["samaccountname"][0] : string.Empty,
                            DEPARTMENT = result.Properties.Contains("department") ? (String)result.Properties["department"][0] : string.Empty,
                            TITLE = result.Properties.Contains("title") ? (String)result.Properties["title"][0] : string.Empty,
                            DESCRIPTION = result.Properties.Contains("description") ? (String)result.Properties["description"][0] : string.Empty
                        });
                    }
                }

                return lstADUsers;
            }
            catch
            {
                return new List<USER>();
            }
        }

        public static ArrayList AttributeValuesMultiString(string attributeName,
        string objectDn, ArrayList valuesCollection, bool recursive)
        {
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            PropertyValueCollection ValueCollection = ent.Properties[attributeName];
            IEnumerator en = ValueCollection.GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current != null)
                {
                    if (!valuesCollection.Contains(en.Current.ToString()))
                    {
                        valuesCollection.Add(en.Current.ToString());
                        if (recursive)
                        {
                            AttributeValuesMultiString(attributeName, "LDAP://" +
                            en.Current.ToString(), valuesCollection, true);
                        }
                    }
                }
            }
            ent.Close();
            ent.Dispose();
            return valuesCollection;
        }
    }
}
