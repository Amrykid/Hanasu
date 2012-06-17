using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Hanasu.Core
{
    public static class XElementExtensions
    {
        public static bool ContainsElement(this XElement element, XName name)
        {
            try
            {
                element.Element(name);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
