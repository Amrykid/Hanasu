using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Preprocessor
{
    public abstract class MultiStreamPreprocessor: BasePreprocessor
    {
        //public abstract bool Supports(Uri url);

        //public abstract void Process(ref Uri url);

        public override bool SupportsMultiples
        {
            get { return true; }
        }

        //public abstract string Extension { get; }

        public abstract IMultiStreamEntry[] Parse(Uri url);
    }
    public interface IMultiStreamEntry
    {
        string File { get; set; }
        string Title { get; set; }
        int Length { get; set; }
        int ID { get; set; }
    }
}
