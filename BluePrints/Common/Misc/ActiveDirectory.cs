using BaseModel.DataModel;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Resources;
using BluePrints.Data;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Timers;

namespace BluePrints.Common
{
    public static class ActiveDirectory
    {
        public static bool? Authenticate(string sUserName, string sPassword)
        {
            var domain = "primerogroup.com.au";
            var oPrincipalContext = new PrincipalContext
                (ContextType.Domain, domain);

            bool? result = null;

            try
            {
                result = oPrincipalContext.ValidateCredentials(sUserName, sPassword);
            }
            catch
            {

            }

            return result;
        }
        
        public static void ExchangeLoginAsync(string sUserName, string sPassword)
        {
#if DEBUG

#else
            try
            {
                string fullEmailAddress = sUserName + BluePrintsResources.DefaultAuthenticateDomain;
                //exService.Credentials = new WebCredentials(fullEmailAddress, sPassword);
                exService.UseDefaultCredentials = true;
                exService.AutodiscoverUrl(fullEmailAddress, RedirectionCallback);
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }
#endif

        }

        public static ExchangeService exService = new ExchangeService() { KeepAlive = true, PreAuthenticate = true };
        public static Timer EmailTimer = new Timer(60000) { AutoReset = true }; 
        public static void SendEmail(string fromName, string body, string subject, bool lowPriority = false, string toRecipient = "doc.control@primero.com.au")
        {
            if (lowPriority && EmailTimer.Enabled)
                return;

            EmailMessage msg = new EmailMessage(exService);
            msg.Subject = subject;
            msg.Body = body;
            msg.ToRecipients.Add(new Microsoft.Exchange.WebServices.Data.EmailAddress(toRecipient, toRecipient));

            //to prevent spamming
            if(!EmailTimer.Enabled)
            {
                EmailTimer.Elapsed -= EmailTimer_Elapsed;
                EmailTimer.Elapsed += EmailTimer_Elapsed;
                EmailTimer.Start();
            }

            try
            {
                if (msg.ToRecipients.Count > 0)
                    msg.SendAndSaveCopy();
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }
        }

        public static void SendEmailFromDocControl(string fromName, string body, string subject, string toRecipient)
        {
            string authenticationName = "document.control@primerogroup.com.au";
            string docControlName = "Doc.Control";
            string fullEmailAddress = docControlName + BluePrintsResources.DefaultAuthenticateDomain;
            ExchangeService docControlExService = new ExchangeService() { KeepAlive = true, PreAuthenticate = true };
            docControlExService.UseDefaultCredentials = false;
            docControlExService.Credentials = new WebCredentials(authenticationName, "dcPW2018++");
            docControlExService.AutodiscoverUrl(authenticationName, RedirectionCallback);

            EmailMessage msg = new EmailMessage(docControlExService);
            msg.Subject = subject;
            msg.Body = body;
            msg.ToRecipients.Add(new Microsoft.Exchange.WebServices.Data.EmailAddress(toRecipient, toRecipient));

            try
            {
                if (msg.ToRecipients.Count > 0)
                    msg.SendAndSaveCopy();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
        }

        static bool RedirectionCallback(string url)
        {
            // Return true if the URL is an HTTPS URL.
            return url.ToLower().StartsWith("https://");
        }

        private static void EmailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EmailTimer.Stop();
        }

        public static ArrayList Groups(string userDn, bool recursive)
        {
            var groupMemberships = new ArrayList();
            return AttributeValuesMultiString("memberOf", "LDAP-Server",
                groupMemberships, recursive);
        }

        public static void List_CurrentUser_Attr()
        {
            var name = Environment.UserName;
            // Get the currently connected LDAP context 
            var entry1 = new DirectoryEntry("LDAP://primerogroup.com.au");
            var domainContext = entry1.Properties["name"].Value as string;
            // Use the default naming context as the connected context may not work for searches
            var entry = new DirectoryEntry("LDAP://" + domainContext);
            var adSearch = new DirectorySearcher(entry);

            adSearch.Filter = "(&(objectClass=user)(anr=" + name + "))";

            // Go through all entries from the active directory.
            foreach (SearchResult singleADUser in adSearch.FindAll())
            {
                Console.WriteLine("The properties of the " + singleADUser.GetDirectoryEntry().Name + " are :");
                // Go through all the values found in the search
                foreach (string singleAttribute in ((ResultPropertyCollection) singleADUser.Properties).PropertyNames)
                {
                    Console.WriteLine(singleAttribute + " = ");
                    foreach (var singleValue in ((ResultPropertyCollection) singleADUser.Properties)[singleAttribute]
                    )
                        Console.WriteLine("\t" + singleValue);
                }
            }
        }

        public static List<USER> EnumerateOU(string OuDn)
        {
            var alObjects = new List<USER>();
            try
            {
                var directoryObject = new DirectoryEntry("LDAP://" + OuDn);
                foreach (PropertyValueCollection childProperty in directoryObject.Properties)
                    alObjects.Add(new USER() {NAME = childProperty.PropertyName, TITLE = childProperty[0].ToString()});
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
                var lstADUsers = new List<USER>();
                var DomainPath = "LDAP://primerogroup.com.au/DC=primerogroup,DC=com,DC=au";
                var searchRoot = new DirectoryEntry(DomainPath);
                var search = new DirectorySearcher(searchRoot);
                search.Filter = "(&(objectClass=user)(objectCategory=person))";
                search.PropertiesToLoad.Add("samaccountname");
                search.PropertiesToLoad.Add("path");
                search.PropertiesToLoad.Add("mail");
                //search.PropertiesToLoad.Add("usergroup");
                search.PropertiesToLoad.Add("usergroup");
                search.PropertiesToLoad.Add("department"); //first name
                search.PropertiesToLoad.Add("title");
                search.PropertiesToLoad.Add("description");
                search.PropertiesToLoad.Add("givenName");
                search.PropertiesToLoad.Add("sn");
                SearchResult result;
                var resultCol = search.FindAll();

                if (resultCol != null)
                    for (var counter = 0; counter < resultCol.Count; counter++)
                    {
                        var UserNameEmailString = string.Empty;
                        result = resultCol[counter];

                        lstADUsers.Add(new USER()
                        {
                            NAME =
                                result.Properties.Contains("samaccountname")
                                    ? (string) result.Properties["samaccountname"][0]
                                    : string.Empty,
                            DEPARTMENT =
                                result.Properties.Contains("department")
                                    ? (string) result.Properties["department"][0]
                                    : string.Empty,
                            TITLE =
                                result.Properties.Contains("title")
                                    ? (string) result.Properties["title"][0]
                                    : string.Empty,
                            DESCRIPTION =
                                result.Properties.Contains("description")
                                    ? (string) result.Properties["description"][0]
                                    : string.Empty,
                            FIRST_NAME =
                                    result.Properties.Contains("givenName")
                                    ? (string)result.Properties["givenName"][0]
                                    : string.Empty,
                            LAST_NAME =
                                    result.Properties.Contains("sn")
                                    ? (string)result.Properties["sn"][0]
                                    : string.Empty,
                        });
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
            var ent = new DirectoryEntry(objectDn);
            var ValueCollection = ent.Properties[attributeName];
            var en = ValueCollection.GetEnumerator();

            while (en.MoveNext())
                if (en.Current != null)
                    if (!valuesCollection.Contains(en.Current.ToString()))
                    {
                        valuesCollection.Add(en.Current.ToString());
                        if (recursive)
                            AttributeValuesMultiString(attributeName, "LDAP://" +
                                                                      en.Current.ToString(), valuesCollection, true);
                    }
            ent.Close();
            ent.Dispose();
            return valuesCollection;
        }
    }
}