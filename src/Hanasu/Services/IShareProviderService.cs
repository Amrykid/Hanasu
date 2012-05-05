using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services
{
    public interface IShareProviderService
    {
        string SiteName { get; } //I.e. Facebook or Twitter
        bool Share(object data);
        bool NeedsToAuth { get; }
    }
}
