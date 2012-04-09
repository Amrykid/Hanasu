using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Hanasu.Services.Preprocessor
{
    public static class PreprocessorService
    {
        static PreprocessorService()
        {
            Preprocessors = new ObservableCollection<IPreprocessor>();
        }

        public static void RegisterPreprocessor(Type processor)
        {
            if (processor is IPreprocessor)
            {
                Preprocessors.Add(
                    (IPreprocessor)Activator.CreateInstance(processor));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static bool CheckIfPreprocessingIsNeeded(Uri url)
        {
            return Preprocessors.Any((pre) =>
            {
                return pre.Supports(url);
            });
        }
        public static bool CheckIfPreprocessingIsNeeded(string url)
        {
            return CheckIfPreprocessingIsNeeded(new Uri(url));
        }

        public static void Process(ref Uri url)
        {
            foreach (IPreprocessor p in Preprocessors)
                if (p.Supports(url))
                    p.Process(ref url);
        }

        public static ObservableCollection<IPreprocessor> Preprocessors { get; private set; }
    }
}
