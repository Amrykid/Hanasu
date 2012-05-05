using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Services.Preprocessor.Preprocessors;
using Hanasu.Services.Logging;

namespace Hanasu.Services.Preprocessor
{
    public class PreprocessorService : IStaticService
    {
        static PreprocessorService()
        {
            LogService.Instance.WriteLog(typeof(PreprocessorService),
    "Preprocessor Service initialized.");

            Preprocessors = new ObservableCollection<IPreprocessor>();

            RegisterPreprocessor(typeof(PLSPreprocessor));
        }

        public static void RegisterPreprocessor(Type processor)
        {
            if (processor.BaseType == typeof(IPreprocessor) || processor.BaseType == typeof(BasePreprocessor))
            {
                Preprocessors.Add(
                    (IPreprocessor)Activator.CreateInstance(processor));

                LogService.Instance.WriteLog(typeof(PreprocessorService),
    "Preprocessor of type '" + processor.ToString() + "' registered.");
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
            LogService.Instance.WriteLog(typeof(PreprocessorService),
    "Preprocessing url: " + url.ToString());

            foreach (IPreprocessor p in Preprocessors)
                if (p.Supports(url))
                    p.Process(ref url);
        }

        public static ObservableCollection<IPreprocessor> Preprocessors { get; private set; }
    }
}
