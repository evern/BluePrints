using BluePrints.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BluePrints.Common.Helpers
{
    public static class XMLHelpers
    {
        /// <summary>
        /// Retrieve the designated file path for xml
        /// </summary>
        public static string SettingsXMLFilePath(bool createDirectory)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string userFilePath = Path.Combine(localAppData, CommonResources.XMLDefaultDirectory);

            if (!Directory.Exists(userFilePath) && createDirectory)
                Directory.CreateDirectory(userFilePath);

            string destFilePath = Path.Combine(userFilePath, CommonResources.XMLFilename);

            return destFilePath;
        }

        /// <summary>
        /// Updates the database XML
        /// </summary>
        /// <returns>Connection string</returns>
        public static void UpdateSettingsXML(XMLSettings defaultSetting)
        {
            string xmlFilePath = SettingsXMLFilePath(true);
            string settingsRootName = "Settings";

            XDocument doc;
            if (File.Exists(xmlFilePath))
            {
                try
                {
                    doc = XDocument.Load(xmlFilePath);
                }
                catch  //if xml file fails to load recreate it
                {
                    File.Delete(xmlFilePath);
                    doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                            new XElement("Settings")
                        );
                }
            }
            else
                doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                        new XElement("Settings")
                    );

            if (!doc.Root.Descendants().Any(obj => obj.Name == settingsRootName))
            {
                doc.Root.Add(new XElement(settingsRootName,
                                new XAttribute("Username", defaultSetting.Username)
                            ));
            }
            else
            {
                XElement findDatabase = doc.Root.Descendants().FirstOrDefault(obj => obj.Name == settingsRootName);
                //dont need to check for null because we've already checked for any
                findDatabase.Attribute("Username").Value = defaultSetting.Username;
            }

            doc.Save(xmlFilePath);
        }

        public static string GetSettings_Username()
        {
            string xmlFilePath = SettingsXMLFilePath(true);
            string settingsRootName = "Settings";

            XDocument doc = null;
            if (File.Exists(xmlFilePath))
            {
                try
                {
                    doc = XDocument.Load(xmlFilePath);
                }
                catch  //if xml file fails to load recreate it
                {
                    return string.Empty;
                }
            }

            string username = string.Empty;
            if (doc != null)
            {
                XElement findDatabase = doc.Root.Descendants().FirstOrDefault(obj => obj.Name == settingsRootName);
                if (findDatabase != null)
                {
                    username = findDatabase.Attribute("Username").Value;
                }
            }

            return username;
        }
    }
}
