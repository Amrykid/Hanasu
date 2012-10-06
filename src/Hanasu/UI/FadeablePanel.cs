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

namespace Hanasu.UI
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FadeablePanelTest"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FadeablePanelTest;assembly=FadeablePanelTest"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:FadeablePanel/>
    ///
    /// </summary>
    public class FadeablePanel : Control
    {
        static FadeablePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FadeablePanel), new FrameworkPropertyMetadata(typeof(FadeablePanel)));
        }

        public FadeablePanel()
        {
            this.Loaded += FadeablePanel_Loaded;
            this.Unloaded += FadeablePanel_Unloaded;
        }

        void FadeablePanel_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= FadeablePanel_Loaded;
            this.Unloaded -= FadeablePanel_Unloaded;
        }

        void FadeablePanel_Loaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "UpperPanelFocus", true);

            PART_LowerPanel = (ContentControl)this.Template.FindName("PART_LowerPanel", this);
            PART_UpperPanel = (ContentControl)this.Template.FindName("PART_UpperPanel", this);
        }

        private ContentControl PART_UpperPanel = null;
        private ContentControl PART_LowerPanel = null;

        public UIElement UpperPanel
        {
            get { return (UIElement)this.GetValue(UpperPanelProperty); }
            set { this.SetValue(UpperPanelProperty, value); }
        }

        public UIElement LowerPanel
        {
            get { return (UIElement)this.GetValue(LowerPanelProperty); }
            set { this.SetValue(LowerPanelProperty, value); }
        }

        public FadeablePanelState PanelState
        {
            get { return (FadeablePanelState)this.GetValue(PanelStateProperty); }
            set { this.SetValue(PanelStateProperty, value); }
        }

        private delegate void EmptyDelegate();
        void HandleStateChangedInternal()
        {
            Dispatcher.Invoke(new EmptyDelegate(() =>
                {
                    switch (PanelState)
                    {
                        case FadeablePanelState.UpperFocus:
                            PART_LowerPanel.IsEnabled = false;
                            PART_UpperPanel.IsEnabled = true;
                            PART_LowerPanel.IsHitTestVisible = false;
                            PART_UpperPanel.SetValue(Panel.ZIndexProperty, 100);
                            break;
                        case FadeablePanelState.LowerFocus:
                            PART_UpperPanel.IsEnabled = false;
                            PART_LowerPanel.IsEnabled = true;
                            PART_LowerPanel.IsHitTestVisible = true;
                            PART_UpperPanel.SetValue(Panel.ZIndexProperty, 50);
                            break;
                    }
                }));
        }

        static void HandleStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FadeablePanel panel = ((FadeablePanel)obj);
            switch ((FadeablePanelState)(e.NewValue))
            {
                case FadeablePanelState.LowerFocus: VisualStateManager.GoToState((FrameworkElement)obj, "LowerPanelFocus", true);
                    panel.PART_UpperPanel.IsHitTestVisible = false;
                    panel.PART_LowerPanel.Focus();
                    break;
                case FadeablePanelState.UpperFocus: VisualStateManager.GoToState((FrameworkElement)obj, "UpperPanelFocus", true);
                    panel.PART_UpperPanel.IsHitTestVisible = true;
                    panel.PART_UpperPanel.Focus();
                    break;
            }

        }

        public static readonly DependencyProperty LowerPanelProperty = DependencyProperty.Register("LowerPanel", typeof(UIElement), typeof(FadeablePanel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty UpperPanelProperty = DependencyProperty.Register("UpperPanel", typeof(UIElement), typeof(FadeablePanel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty PanelStateProperty = DependencyProperty.Register("PanelState", typeof(FadeablePanelState), typeof(FadeablePanel), 
            new UIPropertyMetadata(FadeablePanelState.UpperFocus,new PropertyChangedCallback(HandleStateChanged)));


    }
    public enum FadeablePanelState
    {
        UpperFocus = 0,
        LowerFocus = 1
    }
}
