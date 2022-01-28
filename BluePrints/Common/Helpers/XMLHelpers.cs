using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BluePrints.Common.Helpers
{
    public static class XMLHelpers
    {
        //change settingsrootname to revise XML settings attributes when attributes has been added/deleted
        public static string SettingsRootName = "Settings_v2";
        public static string UsernameElementName = "Username";
        public static string PasswordElementName = "Password";

        public static string IntegrationAppElementName = "BluePrints";
        public static string IntegrationProjectElementName = "Project";
        public static string IntegrationAutoInvokeProjectElementName = "AutoInvokeProject";
        public static string LastChangeLogDisplayVersionElementName = "LastChangeLogDisplayVersion";

        /// <summary>
        /// Retrieve the designated file path for xml
        /// </summary>
        public static string SettingsXMLFilePath(bool createDirectory)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string userFilePath = Path.Combine(localAppData, BluePrintsResources.Default_XML_Directory);

            if (!Directory.Exists(userFilePath) && createDirectory)
                Directory.CreateDirectory(userFilePath);

            string destFilePath = Path.Combine(userFilePath, BluePrintsResources.Default_XML_Filename);

            return destFilePath;
        }

        public static string IntegrationSettingsXMLFilePath()
        {
            string localIntegrationAppData = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string intgrationXMLPath = Path.Combine(localIntegrationAppData, BluePrintsResources.IntegrationXMLDirectory);

            if (!Directory.Exists(intgrationXMLPath))
                Directory.CreateDirectory(intgrationXMLPath);

            var integrationXMLFilePath = Path.Combine(intgrationXMLPath, BluePrintsResources.IntegrationXMLFilename);

            return integrationXMLFilePath;
        }

        public static void ClearSettings()
        {
            UpdateSettingsXMLCredentials(null);
        }

        /// <summary>
        /// Updates the database XML
        /// </summary>
        /// <returns>Connection string</returns>
        public static void UpdateSettingsXMLCredentials(XMLSettings defaultSetting)
        {
            XDocument doc = GetSettingsXML();
            var xmlFilePath = SettingsXMLFilePath(true);
            string username = defaultSetting == null ? string.Empty : defaultSetting.Username;
            string password = defaultSetting == null ? string.Empty : defaultSetting.Password;

            //remove old xml file
            if (doc.Root.Name != SettingsRootName)
            {
                File.Delete(xmlFilePath);
                doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                    new XElement(SettingsRootName)
                );
                doc.Save(xmlFilePath);
            }

            if (!doc.Root.Descendants().Any(obj => obj.Name.LocalName == UsernameElementName))
            {
                doc.Root.Add(new XElement(UsernameElementName, username));
                if(!doc.Root.Descendants().Any(obj => obj.Name.LocalName == PasswordElementName))
                    doc.Root.Add(new XElement(PasswordElementName, password));
            }
            else
            {
                var findUsernameElement = doc.Root.Descendants().First(obj => obj.Name.LocalName == UsernameElementName);
                findUsernameElement.Value = username;
                var findPasswordElement = doc.Root.Descendants().First(obj => obj.Name.LocalName == PasswordElementName);
                findPasswordElement.Value = password;
            }

            doc.Save(xmlFilePath);
        }

        public static XDocument GetSettingsXML()
        {
            var xmlFilePath = SettingsXMLFilePath(true);
            XDocument doc;
            if (File.Exists(xmlFilePath))
                try
                {
                    doc = XDocument.Load(xmlFilePath);
                }
                catch //if xml file fails to load recreate it
                {
                    File.Delete(xmlFilePath);
                    doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                        new XElement(SettingsRootName)
                    );
                    doc.Save(xmlFilePath);
                }
            else
            {
                doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                    new XElement(SettingsRootName)
                );
                doc.Save(xmlFilePath);
            }

            return doc;
        }

        public static XDocument GetIntegrationSettingsXML()
        {
            var xmlFilePath = IntegrationSettingsXMLFilePath();
            XDocument doc = null;
            if (File.Exists(xmlFilePath))
                    doc = XDocument.Load(xmlFilePath);

            return doc;
        }

        public static string GetSettings_LastChangeLogDisplayVersion()
        {
            XDocument doc = GetSettingsXML();
            string lastChangeLogDisplayVersionStr = string.Empty;
            if (doc != null)
            {
                var findLastChangeLogDisplayVersionElement = doc.Root.Elements().FirstOrDefault(obj => obj.Name.LocalName == LastChangeLogDisplayVersionElementName);
                if (findLastChangeLogDisplayVersionElement != null)
                {
                    if (findLastChangeLogDisplayVersionElement.Value != string.Empty)
                        lastChangeLogDisplayVersionStr = findLastChangeLogDisplayVersionElement.Value;
                }
                else
                {
                    doc.Root.Add(new XElement(LastChangeLogDisplayVersionElementName, string.Empty));
                    var xmlFilePath = SettingsXMLFilePath(true);
                    doc.Save(xmlFilePath);
                }
            }

            return lastChangeLogDisplayVersionStr;
        }

        /// <summary>
        /// Updates the database XML change log display date
        /// </summary>
        public static void UpdateSettingsXMLChangeLogDisplayVersion(Version version)
        {
            XDocument doc = GetSettingsXML();
            var xmlFilePath = SettingsXMLFilePath(true);

            string changeLogVersionString = version == null ? string.Empty : version.ToString();
            if (!doc.Root.Descendants().Any(obj => obj.Name.LocalName == LastChangeLogDisplayVersionElementName))
            {
                doc.Root.Add(new XElement(LastChangeLogDisplayVersionElementName, changeLogVersionString));
            }
            else
            {
                var findChangeLogDisplayVersionElement = doc.Root.Descendants().First(obj => obj.Name.LocalName == LastChangeLogDisplayVersionElementName);
                findChangeLogDisplayVersionElement.Value = changeLogVersionString;
            }

            doc.Save(xmlFilePath);
        }

        public static string GetSettings_Username()
        {
            XDocument doc = GetSettingsXML();
            var username = string.Empty;
            if (doc != null)
            {
                var findUsername = doc.Root.Elements().FirstOrDefault(obj => obj.Name.LocalName == UsernameElementName);
                if (findUsername != null)
                    username = findUsername.Value;
            }

            return username;
        }

        public static string GetSettings_Password()
        {
            XDocument doc = GetSettingsXML();
            var password = string.Empty;
            if (doc != null)
            {
                var findPassword = doc.Root.Elements().FirstOrDefault(obj => obj.Name.LocalName == PasswordElementName);
                if (findPassword != null)
                {
                    password = findPassword.Value;
                    if (password != string.Empty)
                        password = BluePrintsUtils.Decrypt(password, true);
                }
            }

            return password;
        }

        public static bool GetIntegrationSettings_AutoInvokeProject()
        {
            XDocument doc = GetIntegrationSettingsXML();
            if (doc != null)
            {
                XElement findAppElement = doc.Root.Element(IntegrationAppElementName);
                if (findAppElement != null)
                {
                    XElement findAutoInvokeElement = findAppElement.Element(IntegrationAutoInvokeProjectElementName);
                    if (findAutoInvokeElement != null && findAutoInvokeElement.Value.ToString() == "1")
                    {
                        //save it as disabled because it's acknowledged to auto invoke once
                        findAutoInvokeElement.Value = "0";
                        doc.Save(IntegrationSettingsXMLFilePath());
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetIntegrationSettings_ProjectNumber()
        {
            XDocument doc = GetIntegrationSettingsXML();
            string projectNumber = string.Empty;
            if (doc != null)
            {
                XElement findProjectElement = doc.Root.Element(IntegrationProjectElementName);
                if (findProjectElement != null)
                {
                    projectNumber = findProjectElement.Value;
                    string regexString = @"\d{5}";
                    projectNumber = Regex.Match(projectNumber, regexString).Value;
                }
            }

            return projectNumber;
        }
    }
}