using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Localization;
using System.Windows;

namespace Hanasu.ViewModel
{
    public class LanguageChooseViewModel: BaseViewModel
    {
        public LanguageChooseViewModel()
        {
            AvailableLocales = LocalizationManager.AvailableLocales;
            SelectedLocale = LocalizationManager.CurrentLocale;

            RegisterForMessages("AffirmativeButtonClicked");
            RegisterForMessages("NegativeButtonClicked");
        }

        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            switch (message.MessageString)
            {
                case "AffirmativeButtonClicked":
                    LocalizationManager.SwitchLocale(SelectedLocale);
                    CloseWindow();
                    return true;
                case "NegativeButtonClicked":
                    CloseWindow();
                    return true;
                default:
                    return base.ReceiveMessage(source, message);
            }
        }

        private void CloseWindow()
        {
            foreach (Window w in Application.Current.Windows)
                if (w.DataContext == this)
                {
                    w.Close();
                    break;
                }
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
