using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using bank.data.repositories;
using bank.poco;

namespace bank.import
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "fbauth":
                    crawl.Facebook.Auth(args[1]);
                    break;
                case "crawl":
                    crawl.Import.Start();
                    break;
                case "ffiec":
                    ffiec.Import.Start();
                    break;
                case "fdic":
                    fdic.ImportInstitutions.Start();
                    break;
                case "form":
                    form();
                    break;
                case "indexorg":
                    index.IndexOrganization.Start();
                    break;
                case "ubprmdrm":
                    mdrm.Ubpr.Start();
                    break;
            }

            Console.ReadKey();
        }

        static void form()
        {
            var pres = @"C:\Data\call-data\09302016_Form041\call-report041-2016-09-30-v129-pres.xml";


            var xml = XDocument.Load(pres);
            var nav = xml.CreateNavigator();
            nav.MoveToFollowing(XPathNodeType.Element);

            var namespaces = nav.GetNamespacesInScope(XmlNamespaceScope.Local);

            var nsmgr = new XmlNamespaceManager(nav.NameTable);

            foreach (var ns in namespaces)
            {
                nsmgr.AddNamespace(ns.Key, ns.Value);
            }

            var xpath = "/linkbase/presentationLink";
            var results = nav.Evaluate(xpath, nsmgr);

            //var xml = new XmlDocument();
            //xml.Load(pres);

            //var nsmgr = new XmlNamespaceManager(xml.NameTable);
            //var ns = nsmgr.GetNamespacesInScope(XmlNamespaceScope.All);


        }


        //static void ProcessCompanyList(string file)
        //{
        //    var lines = File.ReadAllLines(file).ToList();

        //    DataTable table = new DataTable();

        //    var header = lines.First().Split('\t').ToList();

        //    header.ForEach(headerItem => table.Columns.Add(headerItem));

        //    lines.Skip(1)
        //        .ToList()
        //        .ForEach(line => table.Rows.Add(line.Split('\t')));


        //    foreach (DataRow row in table.Rows)
        //    {
        //        var ffiec = int.Parse(row["\"IDRSSD\""].ToString());

        //        var repo = (OrganizationRepository)Repository<Organization>.New();
        //        var existing = repo.LookupByFFIEC(ffiec);

        //        var organization = new Organization
        //        {
        //            Name = row["Financial Institution Name"].ToString(),
        //            FFIEC = ffiec,
        //            FDIC_Cert = int.Parse(row["FDIC Certificate Number"].ToString()),
        //            OccCharterNumber = int.Parse(row["OCC Charter Number"].ToString()),
        //            OtsDocketNumber = int.Parse(row["OTS Docket Number"].ToString()),
        //            PrimaryAbaRoutingNumber = row["Primary ABA Routing Number"].ToString(),
        //            State = row["Financial Institution State"].ToString()
        //        };

        //        if (existing != null)
        //        {
        //            existing.Name = organization.Name;
        //            existing.FFIEC = organization.FFIEC;
        //            existing.FDIC_Cert = organization.FDIC_Cert;
        //            existing.OccCharterNumber = organization.OccCharterNumber;
        //            existing.OtsDocketNumber = organization.OtsDocketNumber;
        //            existing.PrimaryAbaRoutingNumber = organization.PrimaryAbaRoutingNumber;
        //            existing.State = organization.State;

        //            repo.Update(existing);
        //        }
        //        else
        //        {
        //            repo.Insert(organization);
        //        }

        //        Console.WriteLine(organization.Name);


        //    }

        //}


    }
}
