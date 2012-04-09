using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Preprocessor
{
    public abstract class BasePreprocessor : IPreprocessor
    {
        public static implicit operator string(Uri url)
        {
            return url.ToString();
        }
        public static implicit operator Uri(string url)
        {
            return new Uri(url);
        }
    }
}
