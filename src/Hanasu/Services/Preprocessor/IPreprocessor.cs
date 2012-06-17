using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Preprocessor
{
    public interface IPreprocessor
    {
        bool Supports(Uri url);
        void Process(ref Uri url);
    }
    public interface IFileFormatPreprocessor : IPreprocessor
    {
        string Extension { get; }
    }
}
