using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystal.Core;
using Crystal.Navigation;
using Hanasu.ViewModel;

namespace Hanasu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseCrystalApplication
    {
        protected override void PreStartup()
        {
            EnableCrystalLocalization = true;
            EnableDeepReflectionCaching = true;
            EnableSelfAssemblyResolution = true;

            base.PreStartup();
        }

        protected override void PostStartup()
        {
            PlaybackEngine.Initialize();

            NavigationService.ShowWindow<MainWindowViewModel>();

            base.PostStartup();
        }
    }
}
