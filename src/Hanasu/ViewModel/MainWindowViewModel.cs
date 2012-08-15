using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Hanasu.Core;

namespace Hanasu.ViewModel
{
    public class MainWindowViewModel: BaseViewModel
    {
        public MainWindowViewModel()
        {
            GlobalHanasuCore.Initialize();
        }
    }
}
