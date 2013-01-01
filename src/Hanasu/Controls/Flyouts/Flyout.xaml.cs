using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Hanasu.Controls.Flyouts
{
    public partial class Flyout : UserControl
    {
        //Heavily modified from: http://code.msdn.microsoft.com/windowsapps/C-FlyOut-example-e1a0f2c5/

        public Flyout(Action backButtonAction = null)
        {
            this.InitializeComponent();
            _backButtonAction = backButtonAction;
        }

        private Action _backButtonAction = null;

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(Flyout), new PropertyMetadata(""));


        public new object FlyoutContent
        {
            get { return GetValue(FlyoutContentProperty); }
            set { SetValue(FlyoutContentProperty, value); }
        }
        public static readonly DependencyProperty FlyoutContentProperty = DependencyProperty.Register("FlyoutContent", typeof(object), typeof(Flyout), new PropertyMetadata(null));

        #region Unmodified/original code from the sample
        // FlyoutState - is open or closed
        public bool flState = false;

        public void OnClosingEvent(object sender)
        {
            try
            {
                flyoutPopup.IsOpen = false;
                Canvas.SetLeft(flyoutPopup, Window.Current.Bounds.Width + 10);
                flState = false;
            }
            catch { }
        }


        public void Show()
        {

            double ScreenW = Window.Current.Bounds.Width;
            double ScreenH = Window.Current.Bounds.Height;
            mainBorder.Width = 400;
            mainBorder.Height = ScreenH;


            flyoutPopup.IsOpen = true;
            Canvas.SetLeft(flyoutPopup, ScreenW-400);
            flState = true;

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.Activated += OnWindowActivated;
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                Hide();
            }
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            Hide();
        }

        public void Hide()
        {
            flState = false;
            OnClosingEvent(this);
            Window.Current.Activated -= OnWindowActivated;
            Canvas.SetLeft(flyoutPopup, Window.Current.Bounds.Width);

            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Window.Current.Activated -= OnWindowActivated;
        }

        private void OnPopupClosed(object sender, object e)
        {
            Hide();           
        }
#endregion
        protected virtual void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            if (_backButtonAction != null)
                _backButtonAction();
        }

        public static readonly Action ShowSettingsCharmBackButtonAction = () =>
            {
                SettingsPane.Show();
            };
    }
}
