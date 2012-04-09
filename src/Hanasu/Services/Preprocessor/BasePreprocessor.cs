using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Preprocessor
{
    public abstract class BasePreprocessor : IPreprocessor
    {
        //public static implicit operator string(Uri url)
        //{
        //    return (string)url.ToString();
        //}
        //public static implicit operator Uri(string url)
        //{
        //    return new Uri(url);
        //}
        public abstract bool Supports(Uri url);

        public abstract void Process(ref Uri url);
    }
}
