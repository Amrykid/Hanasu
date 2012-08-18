using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Core.Media
{
    public interface IMediaPlayer
    {
        void Initialize();
        void Play(Uri url);
        void Stop();
        bool IsPlaying { get; }
        void Shutdown();
        bool Supports(string extension);

        int Volume { get; set; }
    }
}
