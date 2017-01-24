using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.data.repositories;
using bank.poco;

namespace bank.import.mdrm
{
    public static class Ubpr
    {
        private static Regex _section = new Regex(@"^.+?--Page .+", RegexOptions.Compiled);
        private static Regex _title = new Regex(@"(?<number>^\d+?)\s(?<text>.+)", RegexOptions.Compiled);
        private static Regex _subTitle = new Regex(@"^((?<mdrm>UBPR[\w\d]{3,4})|(?<line>\d+\.\d+?)\s(?<mdrm>.+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _description = new Regex(@"^DESCRIPTION", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _narrative = new Regex(@"^NARRATIVE", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _formula = new Regex(@"^FORMULA", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _skipUpdated = new Regex(@"^Updated .+? Page \d+ of \d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string _lastFlush = null;

        public static void Start()
        {
            var mdrmDefinition = new ConceptDefinition();

            Next next = Next.None;
            Next last = Next.None;

            var text = new StringBuilder();

            var file = @"D:\Repositories\git\bank\src\report-templates\ubpr.mdrm.txt";
            //var file = @"D:\Repositories\git\bank\src\report-templates\test.txt";

            var lines = File.ReadAllLines(file);
            ConceptDefinition nextMdrm = null;

            foreach (var line in lines)
            {
                //Console.WriteLine("######## {0}", line);
                if (_skipUpdated.IsMatch(line))
                {
                    continue;
                }
                else if (line == "EOF")
                {
                    next = Next.None;
                }
                else if (_description.IsMatch(line))
                {
                    next = Next.Description;
                }
                else if (_narrative.IsMatch(line))
                {
                    next = Next.Narrative;
                }
                else if (_formula.IsMatch(line))
                {
                    next = Next.Formula;
                }
                else if (_section.IsMatch(line))
                {
                    next = Next.None;

                    nextMdrm = new ConceptDefinition
                    {
                        Section = line
                    };
                }
                else if (_title.IsMatch(line))
                {
                    var titleMatch = _title.Match(line);
                    next = Next.None;
                    nextMdrm = new ConceptDefinition
                    {
                        Title = titleMatch.Groups["text"].Value
                    };
                }
                else if (_subTitle.IsMatch(line))
                {
                    var subTitleMatch = _subTitle.Match(line);
                    next = Next.None;

                    nextMdrm = new ConceptDefinition
                    {
                        SubTitle = line,
                        Mdrm = subTitleMatch.Groups["mdrm"].Value
                    };
                }


                if (next != last && last != Next.None)
                {
                    switch (last)
                    {
                        case Next.Description:
                            mdrmDefinition.Description = text.ToString();
                            break;
                        case Next.Narrative:
                            mdrmDefinition.Narrative = text.ToString();
                            break;
                        case Next.Formula:
                            mdrmDefinition.Formula = text.ToString();
                            break;
                    }
                    text.Clear();

                    if (next == Next.None)
                    {
                        Flush(mdrmDefinition);
                    }
                }
                else if (next != Next.None && last != Next.None)
                {
                    text.AppendFormat("{0} ", line);
                }

                mdrmDefinition.Section = nextMdrm.Section;
                mdrmDefinition.Title = nextMdrm.Title;
                mdrmDefinition.SubTitle = nextMdrm.SubTitle;
                mdrmDefinition.Mdrm = nextMdrm.Mdrm;

                last = next;

                //Console.WriteLine(line);
            }
        }

        static void Flush(ConceptDefinition mdrmDefinition)
        {
            if (string.IsNullOrWhiteSpace(mdrmDefinition.Mdrm))
            {
                return;
            }

            Console.WriteLine("Flushing {0} - {1}", mdrmDefinition.Mdrm, mdrmDefinition.Description);

            var repo = Repository<ConceptDefinition>.New();
            var existing = repo.Get(mdrmDefinition);

            if (existing == null)
            {
                repo.Insert(mdrmDefinition);
                _lastFlush = mdrmDefinition.Mdrm;
            }
            else
            {
                repo.Update(mdrmDefinition);
            }

            mdrmDefinition.Mdrm = null;

        }

        internal enum Next
        {
            None,
            Description,
            Narrative,
            Formula
        }
    }
}
