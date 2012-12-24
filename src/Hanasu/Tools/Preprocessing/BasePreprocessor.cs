using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanasu.Core.Preprocessor
{
    public abstract class BasePreprocessor : IFileFormatPreprocessor
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

        public abstract Task<Uri> Process(Uri url);

        public abstract bool SupportsMultiples { get; }

        public abstract string Extension { get; }

    }
}
