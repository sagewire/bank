using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace bank.import.ffiec
{
    public static class ParseCallConcepts
    {
        public static void Start()
        {
            var file = @"C:\Data\call-data\09302016_Form041\concepts.xsd";
            var text = File.ReadAllText(file);
            var output = @"c:\temp\output.txt";
            var sb = new StringBuilder();

            var matches = Regex.Matches(text, @"(?<=name="")(?<name>[\w\d]{8})"" .+? xbrli:balance=""(?<balance>\w+?)""");

            foreach(Match match in matches)
            {
                var name = match.Groups["name"].Value;
                var balance = match.Groups["balance"].Value;

                var format = "insert into tempdef values('" + name + "', '" + balance + "')";

                sb.AppendFormat(format, name, balance);
                sb.AppendLine();

                Console.WriteLine(format, name, balance);
            }

            File.WriteAllText(output, sb.ToString());
        }
    }
}
