using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Preprocessor.Preprocessors
{
    public class PLSPreprocessor: BasePreprocessor
    {
        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(".pls");
        }

        public override void Process(ref Uri url)
        {
            throw new NotImplementedException();
        }
    }
}
