using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bank.extensions
{
    public static class XElementExtensions
    {
        public static bool? SafeBoolAttributeValue(this XElement element, string name)
        {
            bool b;
            var text = SafeAttributeValue(element, name);

            if (bool.TryParse(text, out b))
            {
                return b;
            }
            return null;
        }

        public static double? SafeDoubleAttributeValue(this XElement element, string name)
        {
            double b;
            var text = SafeAttributeValue(element, name);

            if (double.TryParse(text, out b))
            {
                return b;
            }
            return null;
        }

        public static long? SafeLongAttributeValue(this XElement element, string name)
        {
            long b;
            var text = SafeAttributeValue(element, name);

            if (long.TryParse(text, out b))
            {
                return b;
            }
            return null;
        }

        public static string SafeAttributeValue(this XElement element, string name)
        {
            var attrib = element.Attribute(name);

            if (attrib != null)
            {
                return attrib.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
