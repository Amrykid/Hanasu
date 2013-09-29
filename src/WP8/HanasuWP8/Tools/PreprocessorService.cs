using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Core.Preprocessor.Preprocessors;
using Hanasu.Core.Preprocessor.Preprocessors.PLS;
using Hanasu.Core.Preprocessor.Preprocessors.M3U;
using System.Reflection;
using System.Threading.Tasks;
using Hanasu.Tools.Preprocessing.Preprocessors.ASX;

namespace Hanasu.Core.Preprocessor
{
    internal class PreprocessorService
    {
        static PreprocessorService()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Preprocessors = new ObservableCollection<IFileFormatPreprocessor>();

            RegisterPreprocessor(typeof(PLSPreprocessor));
            RegisterPreprocessor(typeof(M3UPreprocessor));
            RegisterPreprocessor(typeof(ASXPreprocessor));
        }

        public static void RegisterPreprocessor(Type processor)
        {
            if (Preprocessors == null)
                Initialize();

            if (processor.GetTypeInfo().BaseType == typeof(IFileFormatPreprocessor) || processor.GetTypeInfo().BaseType == typeof(BasePreprocessor) || processor.GetTypeInfo().BaseType.GetTypeInfo().BaseType == typeof(BasePreprocessor))
            {
                Preprocessors.Add(
                    (IFileFormatPreprocessor)Activator.CreateInstance(processor));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static async Task<bool> CheckIfPreprocessingIsNeeded(Uri url, string ExplicitExtension = "")
        {
            if (Preprocessors == null)
                Initialize();

            return await Task.Run(() => Preprocessors.Any((pre) =>
            {
                return pre.Supports(url) || pre.Extension == ExplicitExtension;
            }));
        }
        public static async Task<bool> CheckIfPreprocessingIsNeeded(string url, string ExplicitExtension = "")
        {
            return await CheckIfPreprocessingIsNeeded(new Uri(url), ExplicitExtension);
        }

        public static IFileFormatPreprocessor GetProcessor(Uri url, string ExplicitExtension = "")
        {
            if (Preprocessors == null)
                Initialize();

            foreach (IFileFormatPreprocessor p in Preprocessors)
                if (p.Supports(url) || p.Extension == ExplicitExtension)
                    return p;

            return null;
        }

        public static async Task<Uri> Process(Uri url)
        {
            if (Preprocessors == null)
                Initialize();

            var url2 = url;

            foreach (IPreprocessor p in Preprocessors)
                if (p.Supports(url2))
                    url2 = await p.Process(url2);

            return url2;
        }

        public static ObservableCollection<IFileFormatPreprocessor> Preprocessors { get; private set; }
    }
}
