using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Services.Preprocessor.Preprocessors;
using Hanasu.Services.Logging;
using Hanasu.Services.Preprocessor.Preprocessors.PLS;
using Hanasu.Services.Preprocessor.Preprocessors.M3U;

namespace Hanasu.Services.Preprocessor
{
    public class PreprocessorService : IStaticService
    {
        static PreprocessorService()
        {
            LogService.Instance.WriteLog(typeof(PreprocessorService),
    "Preprocessor Service initialized.");

            Preprocessors = new ObservableCollection<IFileFormatPreprocessor>();

            RegisterPreprocessor(typeof(PLSPreprocessor));
            RegisterPreprocessor(typeof(M3UPreprocessor));
        }

        public static void RegisterPreprocessor(Type processor)
        {
            if (processor.BaseType == typeof(IFileFormatPreprocessor) || processor.BaseType == typeof(BasePreprocessor) || processor.BaseType.BaseType == typeof(BasePreprocessor))
            {
                Preprocessors.Add(
                    (IFileFormatPreprocessor)Activator.CreateInstance(processor));

                LogService.Instance.WriteLog(typeof(PreprocessorService),
    "Preprocessor of type '" + processor.ToString() + "' registered.");
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static bool CheckIfPreprocessingIsNeeded(Uri url, string ExplicitExtension = "")
        {
            return Preprocessors.Any((pre) =>
            {
                return pre.Supports(url) || pre.Extension == ExplicitExtension;
            });
        }
        public static bool CheckIfPreprocessingIsNeeded(string url, string ExplicitExtension = "")
        {
            return CheckIfPreprocessingIsNeeded(new Uri(url), ExplicitExtension);
        }

        public static IFileFormatPreprocessor GetProcessor(Uri url, string ExplicitExtension = "")
        {
            foreach (IFileFormatPreprocessor p in Preprocessors)
                if (p.Supports(url) || p.Extension == ExplicitExtension)
                    return p;

            return null;
        }

        public static void Process(ref Uri url)
        {
            LogService.Instance.WriteLog(typeof(PreprocessorService),
    "Preprocessing url: " + url.ToString());

            foreach (IPreprocessor p in Preprocessors)
                if (p.Supports(url))
                    p.Process(ref url);
        }

        public static ObservableCollection<IFileFormatPreprocessor> Preprocessors { get; private set; }
    }
}
