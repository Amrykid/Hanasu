using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Crystal.Localization;
using Crystal.Command;

namespace Hanasu.ViewModel
{
    public class MessageBoxWindowViewModel : BaseViewModel
    {
        public MessageBoxWindowViewModel()
        {
            OKCommand = new CrystalCommand(this, true, (o) =>
            {
                ViewModelOperations.CloseWindow(this, true);
            });
            CancelCommand = OKCommand = new CrystalCommand(this, true, (o) =>
            {
                ViewModelOperations.CloseWindow(this, false);
            });
        }

        public string Title
        {
            get { return (string)this.GetProperty("Title"); }
            set { this.SetProperty("Title", value); }
        }

        public string Message
        {
            get { return (string)this.GetProperty("Message"); }
            set { this.SetProperty("Message", value); }
        }

        public MessageBoxButton Options
        {
            get { return (MessageBoxButton)this.GetProperty("Options"); }
            set
            {
                this.SetProperty("Options", value);

                GenerateButtons();
            }
        }

        public ObservableCollection<Button> Buttons
        {
            get { return (ObservableCollection<Button>)this.GetProperty("Buttons"); }
            set { this.SetProperty("Buttons", value); }
        }

        public CrystalCommand OKCommand { get; set; }
        public CrystalCommand CancelCommand { get; set; }

        private void GenerateButtons()
        {
            var x = new ObservableCollection<Button>();
            switch (Options)
            {
                case MessageBoxButton.OKCancel:
                    x.Add(new Button()
                    {
                        Content = LocalizationManager.GetLocalizedValue("GenericAffirmativeButton"),
                        Command = OKCommand
                    });
                    x.Add(new Button()
                    {
                        Content = LocalizationManager.GetLocalizedValue("GenericNegativeButton"),
                        Command = CancelCommand
                    });
                    break;
                case MessageBoxButton.OK:
                    x.Add(new Button()
                    {
                        Content = LocalizationManager.GetLocalizedValue("GenericAffirmativeButton"),
                        Command = OKCommand,
                        Width = 60
                    });
                    break;
            }

            Buttons = x;
        }
    }
}
