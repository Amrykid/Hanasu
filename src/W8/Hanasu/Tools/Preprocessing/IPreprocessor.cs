using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanasu.Core.Preprocessor
{
    public interface IPreprocessor
    {
        bool Supports(Uri url);
        Task<Uri> Process(Uri url);
    }
    public interface IFileFormatPreprocessor : IPreprocessor
    {
        string Extension { get; }
    }
}
