using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.data.repositories;
using bank.poco;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace bank.import.fdic
{
    public class ImportInstitutions
    {
        private static TaskPool<Organization> _taskPool = new TaskPool<Organization>();

        public static void Start(int threads)
        {
            InitializeTaskPool(threads);

            var filename = @"c:/data/institutions2.csv";

            var reader = new StreamReader(filename);
            
            var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.RegisterClassMap<ClassMap>();

            try {
                var records = csv.GetRecords<Organization>().ToList();
                _taskPool.Enqueue(records);
            }
            catch(CsvHelper.TypeConversion.CsvTypeConverterException csve)
            {
                Console.WriteLine(csve);

                foreach(System.Collections.DictionaryEntry item in csve.Data)
                {
                    Console.WriteLine(item.Value);
                }
            }
        }

        static void InitializeTaskPool(int threads)
        {
            Console.WriteLine("Starting fdic institution import pool");
            _taskPool.MaxWorkers = threads;
            _taskPool.NextTask += _taskPool_NextTask;
        }

        private static void _taskPool_NextTask(Organization record)
        {
            var orgRepo = new OrganizationRepository();

            var existing = orgRepo.LookupByRssd(record.ID_RSSD);

            if (existing != null)
            {
                record.OrganizationId = existing.OrganizationId;
                record.VerifiedUrl = existing.VerifiedUrl;
                record.Twitter = existing.Twitter;
                record.Facebook = existing.Facebook;
                record.ProfileBanner = existing.ProfileBanner;
                record.Avatar = existing.Avatar;

                orgRepo.Update(record);
                Console.WriteLine("Updated {0}", record.Name);
            }
            else
            {
                orgRepo.Insert(record);
                Console.WriteLine("Inserted {0}", record.Name);
            }
        }
    }

    class ClassMap : CsvClassMap<bank.poco.Organization>
    {
        public ClassMap()
        {
            Map(m => m.Active).Name("ACTIVE");
            Map(m => m.FDIC_Cert).Name("CERT");
            Map(m => m.Name).Name("NAME");
            Map(m => m.FDIC_UniqueNumber).Name("UNINUM");
            Map(m => m.ID_RSSD).Name("FED_RSSD");

            Map(m => m.Street1).Name("ADDRESS");
            Map(m => m.City).Name("CITY");
            Map(m => m.State).Name("STALP");
            Map(m => m.ZIP).Name("ZIP");


            Map(m => m.OccCharterNumber).Name("CHARTER");
            Map(m => m.OtsDocketNumber).Name("DOCKET");
            Map(m => m.BankClass).Name("BKCLASS");
            Map(m => m.TotalAssets).Name("ASSET").TypeConverterOption(NumberStyles.Any);
            Map(m => m.TotalDeposits).Name("DEP").TypeConverterOption(NumberStyles.Any);
            Map(m => m.TotalEquityCapital).Name("EQ").TypeConverterOption(NumberStyles.Any);
            Map(m => m.NetIncome).Name("NETINC").TypeConverterOption(NumberStyles.Number);
            Map(m => m.NetIncomeQuarterly).Name("NETINCQ").TypeConverterOption(NumberStyles.Number);
            Map(m => m.ReturnOnAssets).Name("ROA").TypeConverterOption(NumberStyles.Number);
            Map(m => m.ReturnOnAssetsQuarterly).Name("ROAQ").TypeConverterOption(NumberStyles.Number);
            Map(m => m.ReturnOnEquity).Name("ROEQ").TypeConverterOption(NumberStyles.Number);
            Map(m => m.ReturnOnEquityQuarterly).Name("ROAPTX").TypeConverterOption(NumberStyles.Number);
            Map(m => m.PretaxReturnOnAssets).Name("ROAPTXQ").TypeConverterOption(NumberStyles.Number);
            Map(m => m.CharteringAgency).Name("CHRTAGNT");
            Map(m => m.CMSA).Name("CMSA");
            Map(m => m.CMSA_Number).Name("CMSA_NO");
            Map(m => m.CBSA).Name("CBSA");
            Map(m => m.CBSA_NO).Name("CBSA_NO");
            Map(m => m.CBSA_MetroName).Name("CBSA_METRO_NAME");
            Map(m => m.CBSA_Metro).Name("CBSA_METRO");
            Map(m => m.CBSA_MetroFlag).Name("CBSA_METRO_FLG");
            Map(m => m.CBSA_MicroFlag).Name("CBSA_MICRO_FLG");
            Map(m => m.CBSA_Division).Name("CBSA_DIV");
            Map(m => m.CBSA_Division_Number).Name("CBSA_DIV_NO");
            Map(m => m.CBSA_DivisionFlag).Name("CBSA_DIV_FLG");
            Map(m => m.CSA).Name("CSA");
            Map(m => m.CSA_Number).Name("CSA_NO");
            Map(m => m.CSA_Flag).Name("CSA_FLG");
            Map(m => m.MSA).Name("MSA");
            Map(m => m.MSA_Number).Name("MSA_NO");
            Map(m => m.LastUpdate).Name("DATEUPDT");
            Map(m => m.EndUpdate).Name("ENDEFYMD");
            Map(m => m.EffectiveStartDate).Name("EFFDATE");
            Map(m => m.LastStructureChangeProcessDate).Name("PROCDATE");
            Map(m => m.Established).Name("ESTYMD");
            Map(m => m.LastReportDate).Name("RISDATE");
            Map(m => m.FDICDBS).Name("FDICDBS");
            Map(m => m.FDICSUPV).Name("FDICSUPV");
            Map(m => m.Fed).Name("FED");
            Map(m => m.FedCharter).Name("FEDCHRTR");
            Map(m => m.FdicFieldOffice).Name("FLDOFF");
            Map(m => m.Denovo).Name("DENOVO");
            Map(m => m.InsuranceFundMembership).Name("INSAGNT1");
            Map(m => m.InsuranceObtained).Name("INSDATE");
            Map(m => m.InsuredCommercialBank).Name("INSCOML");
            Map(m => m.StateCharter).Name("STCHRTR");
            Map(m => m.NewFdicCert).Name("NEWCERT");
            //Map(m => m.StateAlphaCode).Name("STALP");
            Map(m => m.Regulator).Name("REGAGNT");
            Map(m => m.Url).Name("WEBADDR");
            Map(m => m.OfficesDomestic).Name("OFFDOM");
            Map(m => m.Offices).Name("OFFICES");
            Map(m => m.OfficesForeign).Name("OFFFOR");
            Map(m => m.OfficesFdic).Name("OFFOA");
            Map(m => m.Form31).Name("FORM31");
            Map(m => m.OwnershipType).Name("MUTUAL");
            Map(m => m.BankHoldingCompany).Name("NAMEHCR");
            Map(m => m.HighRegulatoryHolder).Name("RSSDHCR");
            Map(m => m.HighRegulatoryHolderState).Name("STALPHCR");
            Map(m => m.InterstateBranches).Name("STMULT");
            Map(m => m.SubChapterS).Name("SUBCHAPS");
            Map(m => m.Trust).Name("TRUST");
            Map(m => m.AssetConcentrationHierarchy).Name("SPECGRP");
            Map(m => m.AssetConcentrationHierarchyName).Name("SPECGRPN");
            Map(m => m.Tract).Name("TRACT");
            Map(m => m.CommunityBank).Name("CB");
            
        }
    }

}
