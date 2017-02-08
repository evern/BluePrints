using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using DevExpress.Utils;
using DevExpress.Utils.Zip;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.RichEdit;
using DevExpress.Xpf.SpellChecker;
using DevExpress.XtraRichEdit;
using DevExpress.XtraSpellChecker;

namespace BluePrints.Common
{
    public class DemoUtils
    {
        public static readonly string PathToDemoData = "BluePrints.Data";
        public static readonly string PathToDictionaries = PathToDemoData + ".Dictionaries";

        public static string GetPathToResource(string path, string name)
        {
            return string.Format("{0}.{1}", path, name);
        }

        public static Stream GetDataStream(string path, string name)
        {
            var fullPath = GetPathToResource(path, name);
            if (!string.IsNullOrEmpty(fullPath))
                return Assembly.GetExecutingAssembly().GetManifestResourceStream(fullPath);
            return null;
        }

        public static void ShowDialog(string title, string text, FrameworkElement owner)
        {
            var textBox = new TextBlock() {Text = text};
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.VerticalAlignment = VerticalAlignment.Center;
            textBox.HorizontalAlignment = HorizontalAlignment.Center;
            var dialogControl = new DialogControl() {DialogContent = textBox, UseContentIndents = true};
            dialogControl.CancelButton.Visibility = Visibility.Collapsed;
            FloatingContainer.ShowDialog(dialogControl, owner, Size.Empty, new FloatingContainerParameters()
            {
                AllowSizing = false,
                CloseOnEscape = true,
                Title = title
            });
        }

        public static BitmapImage GetBitmapImage(string fileName)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = GetDataStream(PathToDemoData, fileName);
            bmp.EndInit();
            return bmp;
        }
    }

    public static class SpellCheckerHelper
    {
        public static void RegisterDefaultDictionaries(SpellChecker spellChecker)
        {
            spellChecker.Dictionaries.Add(GetDefaultDictionary());
            spellChecker.Dictionaries.Add(GetCustomDictionary());
        }

        public static void RegisterHunspellDictionaries(SpellChecker spellChecker)
        {
            spellChecker.Dictionaries.Add(CreateHunspellDictionaries(new CultureInfo("en-US")));
            spellChecker.Dictionaries.Add(CreateHunspellDictionaries(new CultureInfo("de-DE")));
            spellChecker.Dictionaries.Add(CreateHunspellDictionaries(new CultureInfo("ru-RU")));
        }

        private static HunspellDictionary CreateHunspellDictionaries(CultureInfo culture)
        {
            var parts = culture.Name.Split('-');
            var result = new HunspellDictionary();
            var uriPath = string.Format("pack://application:,,,/BluePrints;component//Data/Dictionaries/{0}/",
                parts[0]);
            var dictionaryStream =
                Application.GetResourceStream(new Uri(string.Format("{0}{1}_{2}.dic", uriPath, parts[0], parts[1])))
                    .Stream;
            var grammarStream =
                Application.GetResourceStream(new Uri(string.Format("{0}{1}_{2}.aff", uriPath, parts[0], parts[1])))
                    .Stream;
            try
            {
                result.LoadFromStream(dictionaryStream, grammarStream);
            }
            catch
            {
            }
            finally
            {
                dictionaryStream.Close();
                grammarStream.Close();
            }
            result.Culture = culture;
            return result;
        }

        private static Stream GetFileStream(InternalZipFileCollection files, string name)
        {
            var stream =
                files.Find(delegate(InternalZipFile file) { return file.FileName.IndexOf(name) >= 0; }).FileDataStream;
            try
            {
                return CreateMemoryStream(stream);
            }
            finally
            {
                stream.Close();
            }
        }

        private static Stream CreateMemoryStream(Stream stream)
        {
            var result = new MemoryStream();
            for (;;)
            {
                var readedByte = stream.ReadByte();
                if (readedByte < 0)
                    break;
                result.WriteByte((byte) readedByte);
            }
            result.Flush();
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        private static ISpellCheckerDictionary GetDefaultDictionary()
        {
            var dic = new SpellCheckerISpellDictionary();
            using (var stream = DemoUtils.GetDataStream(DemoUtils.PathToDictionaries, "default.zip"))
            {
                var files = InternalZipArchive.Open(stream);
                var dictionaryStream = GetFileStream(files, "american.xlg");
                var grammarStream = GetFileStream(files, "english.aff");
                var alphabetStream = DemoUtils.GetDataStream(DemoUtils.PathToDictionaries, "EnglishAlphabet.txt");
                try
                {
                    dic.LoadFromStream(dictionaryStream, grammarStream, alphabetStream);
                }
                catch
                {
                }
                finally
                {
                    dictionaryStream.Close();
                    grammarStream.Close();
                    alphabetStream.Close();
                }
            }
            dic.Culture = new CultureInfo("en-US");
            return dic;
        }

        private static ISpellCheckerDictionary GetCustomDictionary()
        {
            var result = new SpellCheckerCustomDictionary();
            var dictionaryStream = DemoUtils.GetDataStream(DemoUtils.PathToDictionaries, "CustomEnglish.dic");
            var alphabetStream = DemoUtils.GetDataStream(DemoUtils.PathToDictionaries, "EnglishAlphabet.txt");
            try
            {
                result.Load(dictionaryStream, alphabetStream);
            }
            catch
            {
            }
            finally
            {
                dictionaryStream.Close();
                alphabetStream.Close();
            }
            result.Culture = new CultureInfo("en-US");
            return result;
        }
    }
}