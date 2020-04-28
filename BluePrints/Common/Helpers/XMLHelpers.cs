using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BluePrints.Common.Helpers
{
    public static class XMLHelpers
    {
        //change settingsrootname to revise XML settings attributes when attributes has been added/deleted
        public static string SettingsRootName = "Settings_v2";
        public static string UsernameElementName = "Username";
        public static string PasswordElementName = "Password";
        public static string LastChangeLogDisplayElementName = "LastChangeLogDisplayDate";

        /// <summary>
        /// Retrieve the designated file path for xml
        /// </summary>
        public static string SettingsXMLFilePath(bool createDirectory)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var userFilePath = Path.Combine(localAppData, BluePrintsResources.Default_XML_Directory);

            if (!Directory.Exists(userFilePath) && createDirectory)
                Directory.CreateDirectory(userFilePath);

            var destFilePath = Path.Combine(userFilePath, BluePrintsResources.Default_XML_Filename);

            return destFilePath;
        }

        public static void ClearSettings()
        {
            UpdateSettingsXML(null);
        }

        /// <summary>
        /// Updates the database XML
        /// </summary>
        /// <returns>Connection string</returns>
        public static void UpdateSettingsXML(XMLSettings defaultSetting)
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

            if(!doc.Root.Descendants().Any(obj => obj.Name.LocalName == LastChangeLogDisplayElementName))
            {
                doc.Root.Add(new XElement(LastChangeLogDisplayElementName, string.Empty));
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

        public static DateTime? GetSettings_LastChangeLogDisplayDate()
        {
            XDocument doc = GetSettingsXML();
            DateTime? lastChangeLogDisplayDate = null;
            if (doc != null)
            {
                var findLastChangeLogDisplayDate = doc.Root.Elements().FirstOrDefault(obj => obj.Name.LocalName == LastChangeLogDisplayElementName);
                if (findLastChangeLogDisplayDate != null)
                {
                    if(findLastChangeLogDisplayDate.Value != string.Empty)
                        lastChangeLogDisplayDate = DateTime.Parse(findLastChangeLogDisplayDate.Value);
                }
            }

            return lastChangeLogDisplayDate;
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
    }
}