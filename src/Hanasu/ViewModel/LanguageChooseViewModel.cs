using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Localization;
using System.Windows;
using Crystal.Messaging;

namespace Hanasu.ViewModel
{
    public class LanguageChooseViewModel : BaseViewModel
    {
        public LanguageChooseViewModel()
        {
            AvailableLocales = LocalizationManager.AvailableLocales;
            SelectedLocale = LocalizationManager.CurrentLocale;
        }

        [MessageHandler("AffirmativeButtonClicked")]
        public void HandleOkayButton(object data)
        {
            LocalizationManager.SwitchLocale(SelectedLocale);
            ViewModelOperations.CloseWindow(this, true);
        }

        [MessageHandler("NegativeButtonClicked")]
        public void HandleCancelButton(object data)
        {
            ViewModelOperations.CloseWindow(this, false);
        }

        public IList<LocaleDataFrame> AvailableLocales
        {
            get { return (IList<LocaleDataFrame>)this.GetProperty("AvailableLocales"); }
            set { this.SetProperty("AvailableLocales", value); }
        }

        public LocaleDataFrame SelectedLocale
        {
            get { return (LocaleDataFrame)this.GetProperty("SelectedLocale"); }
            set { this.SetProperty("SelectedLocale", value); }
        }
    }
}
