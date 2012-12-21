using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Hanasu.UI;
using Crystal.Localization;
using System.ComponentModel;
using Hanasu.ViewModel;
using System.Threading.Tasks;
using System.Threading;
using Crystal.Core;

namespace Hanasu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            this.Title = "Hanasu v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
#endif

            System.Timers.Timer treeViewTimer = new System.Timers.Timer();
            treeViewTimer.Elapsed += new System.Timers.ElapsedEventHandler(treeViewTimer_Elapsed);
            treeViewTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
            treeViewTimer.Start();

            InitializeViews();
        }

        void treeViewTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
                Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                {
                    if (LibraryTreeView.SelectedItem == null)
                        StationsListAdorner.IsAdornerVisible = true;
                    else
                        if (((TreeViewItem)LibraryTreeView.SelectedItem).DataContext == null)
                            StationsListAdorner.IsAdornerVisible = true;
                        else
                            StationsListAdorner.IsAdornerVisible = StationListView.Items.Count == 0;

                }));
        }

        #region Related to view selection
        private ViewBase _CurrentSelectedView = null;
        public ViewBase CurrentSelectedView
        {
            get { return _CurrentSelectedView; }
            set
            {
                _CurrentSelectedView = value;

                OnPropertyChanged("CurrentSelectedView");

                IsOnDefaultGridView = value == GridViewObject;
            }
        }

        private Style _SelectedStyle = null;
        public Style SelectedStyle
        {
            get { return _SelectedStyle; }
            set
            {
                _SelectedStyle = value;

                OnPropertyChanged("SelectedStyle");

                IsOnDefaultGridView = value == GridViewStyle;
            }
        }

        private bool _IsOnDefaultGridView = false;
        public bool IsOnDefaultGridView
        {
            get { return _IsOnDefaultGridView; }
            set
            {
                _IsOnDefaultGridView = value;

                OnPropertyChanged("IsOnDefaultGridView");

                SwitchListViewView();
            }

        }

        private void SwitchListViewView()
        {
            ViewBase view = IsOnDefaultGridView == true ? (ViewBase)GridViewObject : (ViewBase)ImageViewObject;
            Style style = IsOnDefaultGridView == true ? GridViewStyle : ImageViewStyle;

            if (view == CurrentSelectedView)
                return;
            if (style == SelectedStyle)
                return;

            SelectedStyle = style;
            CurrentSelectedView = view;
        }

        public GridView GridViewObject { get; set; }
        public ImageHeaderView ImageViewObject { get; set; }

        private void InitializeViews()
        {
            GridViewObject = new GridView();
            //GridViewObject.ColumnHeaderContainerStyle = new Style(typeof(GridViewColumnHeader));
            try
            {
                GridViewObject.Columns.Add(new GridViewColumn() { Header = LocalizationManager.GetLocalizedValue("StationNameColumn"), DisplayMemberBinding = new Binding("Name") });
                GridViewObject.Columns.Add(new GridViewColumn() { Header = LocalizationManager.GetLocalizedValue("StationLanguageColumn"), DisplayMemberBinding = new Binding("Language") });

                GridViewObject.Columns.Add(new GridViewColumn()
                {
                    Header = LocalizationManager.GetLocalizedValue("StationNowPlayingColumn"),
                    DisplayMemberBinding = new Binding()
                    {
                        Path = new PropertyPath("DetectedNowPlaying"),
                        UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                    }
                });
            }
            catch (Exception)
            {
                GridViewObject.Columns.Clear();


                GridViewObject.Columns.Add(new GridViewColumn() { Header = "StationNameColumn", DisplayMemberBinding = new Binding("Name") });
                GridViewObject.Columns.Add(new GridViewColumn() { Header = "StationLanguageColumn", DisplayMemberBinding = new Binding("Language") });

                GridViewObject.Columns.Add(new GridViewColumn()
                {
                    Header = "StationNowPlayingColumn",
                    DisplayMemberBinding = new Binding()
                    {
                        Path = new PropertyPath("DetectedNowPlaying"),
                        UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                    }
                });
            }
            //GridViewObject.ColumnHeaderContainerStyle = (Style)Application.Current.FindResource("GridViewColumnHeaderGripper");

            ImageViewObject = new ImageHeaderView();

            CurrentSelectedView = GridViewObject;

            GridViewStyle = new Style();
            GridViewStyle.Setters.Add(new Setter(ListView.ViewProperty, GridViewObject));

            //http://stackoverflow.com/questions/1406982/get-the-style-of-a-control-staticresource-xtype-textblock-in-code-behind

            ImageViewStyle = new Style(typeof(ListView),
                (Style)Application.Current.FindResource(typeof(ListBox)));
            ImageViewStyle.Setters.Add(new Setter(ListView.ViewProperty, ImageViewObject));

            SelectedStyle = GridViewStyle;

            IsOnDefaultGridView = true;
        }

        public Style ImageViewStyle
        {
            get;
            set;
        }
        public Style GridViewStyle { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void StationListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsOnDefaultGridView)
            {
                UIElement control = (UIElement)e.MouseDevice.DirectlyOver;

                if (control.TryFindParent<GridViewColumnHeader>() != null)
                {
                    //is a header.
                    e.Handled = true;

                    //HandleSort(sender, e, control);
                }
            }
        }

        private void StationListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            
            //if (LibraryTreeView.SelectedItem == null)
            //    StationsListAdorner.IsAdornerVisible = true;
            //else
            //    if (((TreeViewItem)LibraryTreeView.SelectedItem).DataContext == null)
            //        StationsListAdorner.IsAdornerVisible = true;
            //    else
            //        StationsListAdorner.IsAdornerVisible = StationListView.Items.Count == 0;

        }

        private void LibraryTreeView_Expanded(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as TreeViewItem).IsSelected = true;
        }

        private void StationListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            return;
        }
    }
}
