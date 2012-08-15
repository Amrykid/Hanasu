using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Hanasu.Core.Media;

namespace Hanasu.Core
{
    public class PluginImporterInstance
    {
        internal PluginImporterInstance()
        {
            
        }

        [ImportMany(typeof(IMediaPlayer))]
        public IEnumerable<IMediaPlayer> Players;
    }
}
