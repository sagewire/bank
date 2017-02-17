using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JeffFerguson.Gepsio;
using DapperExtensions;
using System.IO;
using System.Data;

using bank;
using bank.data.repositories;
using bank.reports;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using System.ServiceModel;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace poc
{
    class Program
    {
        //private const string ConnString = @"Data Source=.\SQL2014;Initial Catalog=BankData;Integrated Security=True";
        static void Main(string[] args)
        {

            //OrganizationCollection orgs = null;
            //string path = @"c:\temp\20161231_ATTRIBUTES_ACTIVE.xml";

            //XmlSerializer serializer = new XmlSerializer(typeof(OrganizationCollection));

            //StreamReader reader = new StreamReader(path);
            //orgs = (OrganizationCollection)serializer.Deserialize(reader);
            //reader.Close();


            //foreach(var org in orgs)
            //{

            //}

            //FormatXml(@"c:\temp\20161231_ATTRIBUTES_ACTIVE.xml");
            //TestFfiecWebservice();
            Console.ReadKey();
        }

        static void FormatXml(string filename)
        {
            //XDocument doc = XDocument.Load(filename);
            //File.WriteAllText(filename.Replace(".xml", ".pretty.xml"), doc.ToString());

            var saveTo = filename.Replace(".xml", ".pretty.xml");

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(saveTo, null);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);
        }

        //static void TestFfiecWebservice()
        //{
        //    var userToken = new UsernameToken("sagewire", "oNWaFfn9THI6aYyJLwHB", PasswordOption.SendHashed);

        //    //WSHttpBinding b = new WSHttpBinding();
        //    //b.Security.Mode = SecurityMode.Message;

        //    var ffiec = new gov.ffiec.cdr.RetrievalService();
        //    ffiec.RequestSoapContext.Security.Tokens.Add(userToken);
        //    //var result = ffiec.TestUserAccess();


        //    //var result = ffiec.RetrievePanelOfReporters(gov.ffiec.cdr.ReportingDataSeriesName.Call, "2016-12-31");

        //    //var result = ffiec.RetrieveFilersSubmissionDateTime(gov.ffiec.cdr.ReportingDataSeriesName.Call, "2016-12-31", "2016-12-31");

        //    var result = ffiec.RetrieveFacsimile(
        //        gov.ffiec.cdr.ReportingDataSeriesName.Call,
        //        "2002-12-31",
        //        gov.ffiec.cdr.FinancialInstitutionIDType.ID_RSSD,
        //        688556,
        //        gov.ffiec.cdr.FacsimileFormat.XBRL);

        //    //var result = ffiec.RetrieveUBPRXBRLFacsimile("2002-12-31", gov.ffiec.cdr.FinancialInstitutionIDType.ID_RSSD, 688556);

        //    File.WriteAllBytes(@"c:\temp\test\688556.xml", result);
        //}

        //static void TestReport()
        //{

        //    var report = new bank.reports.Report();

        //    report.Columns.Add(new CompanyColumn
        //    {
        //        OrganizationId = 5517
        //    });

        //    report.Template = "call";
        //    //report.Template = "deposit-composition";
        //    //report.Template = "loan-composition";
        //    //report.Parse("income-statement");
        //    report.Template = "financial-highlights";
        //    report.Parse();

        //    //Report.PopulateReport(report);

        //    Write(report);
        //}

        //static void Write(Report report)
        //{
        //    foreach(var section in report.Sections)
        //    {
        //        WriteLineItem(report, section);
        //    }
        //}

        //static void WriteLineItem(Report report, LineItem lineItem, int level = 0)
        //{

        //    if (lineItem.LineItemType == LineItemTypes.Table)
        //    {
        //        foreach(var column in report.Columns)
        //        {
        //            Report.GetCell(lineItem, column);
        //        }
        //    }

        //    Console.WriteLine("{0}{1}\t{2}\t{3}", 
        //        string.Empty.PadRight(level), 
        //        lineItem.GetType().Name,
        //        lineItem.Id,
        //        lineItem.Label);

        //    if (lineItem.LineItems == null) return;

        //    foreach(var child in lineItem.LineItems)
        //    {
        //        WriteLineItem(report, child, level+1);
        //    }

        //    var section = lineItem as Section;

        //    if (section != null && section.SubSections != null)
        //    {
        //        foreach (var subsection in section.SubSections)
        //        {
        //            WriteLineItem(report, subsection, level + 1);
        //        }
        //    }
        //}

    }
}
