using System.Collections.Generic;
using System.Windows;
using DevExpress.Xpf.SpellChecker;
using DevExpress.XtraSpellChecker.Native;
using DevExpress.XtraRichEdit.SpellChecker;
using System.Globalization;
using DevExpress.XtraRichEdit;
using DevExpress.XtraSpellChecker;

namespace BluePrints.Common
{
    public class SpellCheckerModule
    {
        static SpellCheckerModule()
        {
            RegisterRichEditControlController();
            RegisterRichEditControlUndoManager();
        }

        private static void RegisterRichEditControlController()
        {
            SpellCheckTextControllersManager.Default.RegisterClass(typeof(RichEditControl),
                typeof(RichEditSpellCheckController));
        }

        private static void RegisterRichEditControlUndoManager()
        {
            UndoControllerRepository.Default.Register(typeof(RichEditControl), typeof(RichEditUndoController));
        }

        public SpellCheckerModule()
        {
            SpellChecker = CreateDefaultSpellCheckerControl();
        }

        public SpellChecker SpellChecker { get; set; }

        protected virtual List<FrameworkElement> CheckingElements
        {
            get { return null; }
        }

        protected string XamlSuffix
        {
            get { return ".xaml"; }
        }

        protected string DefaultXamlSuffix
        {
            get { return XamlSuffix; }
        }

        private void CheckingElement_Loaded(object sender, RoutedEventArgs e)
        {
            ApplySpellCheckMode(true);
        }

        public void ApplySpellCheckMode(bool isAsYouType)
        {
            if (isAsYouType)
                SpellChecker.SpellCheckMode = SpellCheckMode.AsYouType;
            else
                SpellChecker.SpellCheckMode = SpellCheckMode.OnDemand;
        }

        protected virtual SpellChecker CreateSpellCheckerControl()
        {
            return null;
        }

        private SpellChecker CreateDefaultSpellCheckerControl()
        {
            var result = new SpellChecker();
            SpellCheckerHelper.RegisterDefaultDictionaries(result);
            result.Culture = new CultureInfo("en-US");
            return result;
        }

        protected object GetModuleDataContext()
        {
            return SpellChecker;
        }
    }
}