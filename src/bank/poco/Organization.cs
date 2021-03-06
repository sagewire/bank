﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.enums;
using bank.extensions;
using bank.utilities;
using DapperExtensions.Mapper;
using Newtonsoft.Json;

namespace bank.poco
{
    public class Organization : Address
    {
        private static string[] _banks = new string[] { "NMB", "NAT", "SMB", "SSB", "FSB", "SAL", "MTC", "CPB", "FBO", "SMB" };

        public IList<PeerGroupCustom> CustomPeerGroups { get; set; }
        public int OrganizationId { get; set; }

        private string _entityCategory;
        public string EntityCategory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_entityCategory) && string.IsNullOrWhiteSpace(EntityType))
                {
                    return "org";

                }

                if (string.IsNullOrWhiteSpace(_entityCategory))
                {
                    if (_banks.Contains(EntityType.ToUpper()))
                    {
                        return "bank";
                    }
                    else
                    {
                        return "org";
                    }
                }
                else
                {
                    return _entityCategory.ToLower();
                }
            }
            set
            {
                _entityCategory = value;
            }
        }

        public OrganizationStatuses Status
        {
            get
            {

                if (Successor != null) return OrganizationStatuses.Acquired;

                if (Active) return OrganizationStatuses.Active;
                
                if (!Active) return OrganizationStatuses.Inactive;

                return OrganizationStatuses.Unknown;
            }
        }

        private List<TimelineItem> _timeline = null;
        public List<TimelineItem> Timeline
        {
            get
            {
                
                if (_timeline == null)
                {
                    var counter = 1;
                    var list = new List<TimelineItem>();

                    list.Add(new TimelineItem
                    {
                        Id = counter++,
                        Content = Name,
                        Start = Established.Value,
                        Type = "established"
                    });

                    foreach (var transformation in SucessorTransformations)
                    {
                        list.Add(new TimelineItem
                        {
                            Id = counter++,
                            Content = transformation.PredecessorOrganization.Name,
                            Start = transformation.D_DT_TRANS.Value,
                            Data = transformation,
                            Type = "acquirer"
                        });
                    }

                    foreach (var transformation in PredecessorTransformations)
                    {
                        list.Add(new TimelineItem
                        {
                            Id = counter++,
                            Content = transformation.SuccessorOrganization.Name,
                            Start = transformation.D_DT_TRANS.Value,
                            Data = transformation,
                            Type = "acquiree"
                        });
                    }


                    foreach (var relationship in ParentRelationships)
                    {
                        list.Add(new TimelineItem
                        {
                            Id = counter++,
                            Content = relationship.ParentOrganization.Name,
                            Start = relationship.DateRelationshipStart.Value,
                            End = relationship.DateRelationshipEnd,
                            Data = relationship,
                            Type = "parent"
                        });
                    }


                    foreach (var relationship in ChildRelationships.Where(x=>x.OffspringOrganization != null))
                    {
                        list.Add(new TimelineItem
                        {
                            Id = counter++,
                            Content = relationship.OffspringOrganization.Name,
                            Start = relationship.DateRelationshipStart.Value,
                            End = relationship.DateRelationshipEnd,
                            Data = relationship,
                            Type = "subsidiary"
                        });
                    }

                    list = list.OrderBy(x => x.Start).ToList();

                    var keys = new List<string>();
                    var cleanList = new List<TimelineItem>();

                    foreach(var item in list)
                    {
                        var key = string.Format("{0}-{1}", item.Content, item.Type);
                        if (!keys.Contains(key))
                        {
                            cleanList.Add(item);
                            keys.Add(key);
                        }
                    }

                    _timeline = cleanList;
                }

                return _timeline;
            }
        }

        public string EntityType { get; set; }
        //NAME
        public string ShortName { get; set; }
        public string Name { get; set; }
        //CERT
        public int FDIC_Cert { get; set; }
        //UNINUM
        public int FDIC_UniqueNumber { get; set; }
        //FED_RSSD
        public int ID_RSSD { get; set; }
        public DateTime? FirstReport { get; set; }
        public DateTime? Created { get; set; }
        //CHARTER
        public int OccCharterNumber { get; set; }
        //DOCKET
        public long OtsDocketNumber { get; set; }

        public string PrimaryAbaRoutingNumber { get; set; }
        public bool Active { get; set; }
        //BKCLASS
        public string BankClass { get; set; }
        //ASSET
        public long? TotalAssets { get; set; }
        //DEP
        public long? TotalDeposits { get; set; }
        //EQ
        public long? TotalEquityCapital { get; set; }
        //netinc
        public long? NetIncome { get; set; }
        //NETINCQ
        public long? NetIncomeQuarterly { get; set; }
        //ROA
        public decimal? ReturnOnAssets { get; set; }
        //ROAQ
        public decimal? ReturnOnAssetsQuarterly { get; set; }
        //ROW
        public decimal? ReturnOnEquity { get; set; }
        //ROEQ
        public decimal? ReturnOnEquityQuarterly { get; set; }
        //ROAPTX
        public decimal? PretaxReturnOnAssets { get; set; }
        //ROAPTXQ
        public decimal? PretaxReturnOnAssetsQuarterly { get; set; }
        //CHRTAGNT
        public string CharteringAgency { get; set; }
        //CMSA
        public string CMSA { get; set; }
        //CMSA_NO
        public int CMSA_Number { get; set; }
        //CBSA
        public string CBSA { get; set; }
        //CBSA_NO
        public int CBSA_NO { get; set; }
        //CBSA_METRO_NAME
        public string CBSA_MetroName { get; set; }
        //CBSA_METRO
        public int CBSA_Metro { get; set; }
        //CBSA_METRO_FLG
        public bool CBSA_MetroFlag { get; set; }
        //CBSA_MICRO_FLG
        public bool CBSA_MicroFlag { get; set; }
        //CBSA_DIV
        public string CBSA_Division { get; set; }
        //CBSA_DIV_NO
        public int? CBSA_Division_Number { get; set; }
        //CBSA_DIV_FLG
        public bool CBSA_DivisionFlag { get; set; }
        //CSA
        public string CSA { get; set; }
        //CSA_NO
        public string CSA_Number { get; set; }
        //CSA_FLG
        public bool CSA_Flag { get; set; }
        //MSA
        public string MSA { get; set; }
        //MSA_NO
        public int MSA_Number { get; set; }
        //DATEUPDT
        public DateTime? LastUpdate { get; set; }
        //ENDEFYMD
        public DateTime? EndUpdate { get; set; }
        //EFFDATE
        public DateTime? EffectiveStartDate { get; set; }
        // PROCDATE
        public DateTime? LastStructureChangeProcessDate { get; set; }
        //ESTYMD
        public DateTime? Established { get; set; }
        //RISDATE
        public DateTime? LastReportDate { get; set; }
        //FDICDBS
        public int FDICDBS { get; set; }
        //FDICSUPV
        public string FDICSUPV { get; set; }
        //FED
        public int Fed { get; set; }
        //FEDCHRTR
        public int FedCharter { get; set; }
        //FLDOFF
        public string FdicFieldOffice { get; set; }
        //DENOVO
        public bool Denovo { get; set; }
        //INSAGNT1
        public string InsuranceFundMembership { get; set; }
        //INSDATE
        public DateTime? InsuranceObtained { get; set; }
        //INSCOML
        public bool InsuredCommercialBank { get; set; }
        //STCHRTR
        public bool StateCharter { get; set; }
        //NEWCERT
        public int? NewFdicCert { get; set; }
        ////STALP
        //public string StateAlphaCode { get; set; }
        //REGAGNT
        public string Regulator { get; set; }
        //WEBADDR
        public string Url { get; set; }
        public string VerifiedUrl { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string ProfileBanner { get; set; }
        public string Avatar { get; set; }
        //OFFDOM
        public int? OfficesDomestic { get; set; }
        //OFFICES
        public int? Offices { get; set; }
        //OFFFOR
        public int? OfficesForeign { get; set; }
        //OFFOA
        public int? OfficesFdic { get; set; }
        //FORM31
        public bool? Form31 { get; set; }
        //MUTUAL
        public bool? OwnershipType { get; set; }
        //NAMEHCR
        public string BankHoldingCompany { get; set; }
        //RSSDHCR
        public int? HighRegulatoryHolder { get; set; }
        //STALPHCR
        public string HighRegulatoryHolderState { get; set; }
        //STMULT
        public bool? InterstateBranches { get; set; }
        //SUBCHAPS
        public bool? SubChapterS { get; set; }
        //TRUST
        public bool? Trust { get; set; }
        //SPECGRP
        public int? AssetConcentrationHierarchy { get; set; }
        //SPECGRPN
        public string AssetConcentrationHierarchyName { get; set; }
        //TRACT
        public bool? Tract { get; set; }
        //CB
        public bool? CommunityBank { get; set; }
        public DateTime? TwitterFriendUpdate { get; set; }
        public bool HasAvatar
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Avatar);
            }
        }

        public string StatePeerGroup
        {
            get
            {
                var bankType = "COM";

                switch (BankClass)
                {
                    case "SA":
                    case "SB":
                        bankType = "SVG";
                        break;
                }

                return string.Format("{0}{1}", State, bankType);
            }
        }

        public string ProfileUrl
        {
            get
            {
                return string.Format("/org/{0}.{1}", Name.CreateSlug(), Base26.Encode(OrganizationId));
            }
        }


        private Uri _websiteUri = null;
        public Uri WebsiteUri
        {
            get
            {
                if (_websiteUri == null)
                {
                    Uri.TryCreate(Url, UriKind.Absolute, out _websiteUri);
                }
                return _websiteUri;

            }
        }

        public string WebsiteDomainOnly
        {
            get
            {
                if (WebsiteUri != null)
                {
                    return string.Format("{0}", WebsiteUri.Host);
                }
                return null;
            }
        }

        public OrganizationFfiecTransformation Successor
        {
            get
            {
                if (PredecessorTransformations != null && PredecessorTransformations.Any())
                {
                    return PredecessorTransformations.OrderByDescending(x => x.D_DT_TRANS).FirstOrDefault();
                }
                return null;
            }
        }

        [JsonIgnore]
        public OrganizationFfiecRelationship HoldingCompany
        {
            get
            {
                if (ParentRelationships != null && ParentRelationships.Any())
                {
                    return ParentRelationships.OrderByDescending(x => x.D_DT_START.Value).FirstOrDefault();
                }
                return null;
            }
        }

        [JsonIgnore]
        public List<ReportImport> ReportImports { get; internal set; }

        [JsonIgnore]
        public List<OrganizationFfiecTransformation> SucessorTransformations { get; internal set; }
        
        [JsonIgnore]
        public List<OrganizationFfiecTransformation> FilteredTransformations
        {
            get
            {
                if (SucessorTransformations == null) return null;

                return SucessorTransformations
                    .Where(x => x.PredecessorOrganization != null && x.D_DT_TRANS.Value >= firstReport)
                    .OrderByDescending(x => x.PredecessorOrganization.TotalAssets).ToList();
            }
        }

        private DateTime firstReport
        {
            get
            {
                DateTime firstReport;
                if (!FirstReport.HasValue)
                {
                    firstReport = new DateTime(2002, 12, 31);
                }
                else
                {
                    firstReport = FirstReport.Value;
                }
                return firstReport;
            }
        }

        [JsonIgnore]
        public List<OrganizationFfiecTransformation> Transformations
        {
            get
            {
                return SucessorTransformations.Union(PredecessorTransformations).ToList();
            }
        }

        [JsonIgnore]
        public List<OrganizationFfiecRelationship> FilteredChildRelationships
        {
            get
            {
                if (ChildRelationships == null) return null;

                return ChildRelationships.Where(x => x.OffspringOrganization != null && x.DateRelationshipStart.Value >= firstReport)
                    .OrderByDescending(x => x.DateRelationshipStart)
                    .Take(999)
                    .ToList();

            }
        }

        [JsonIgnore]
        public List<OrganizationFfiecRelationship> FilteredParentRelationships
        {
            get
            {
                if (ParentRelationships == null) return null;



                return ParentRelationships.Where(x => x.ParentOrganization != null && x.DateRelationshipStart.Value >= firstReport)
                    .OrderByDescending(x => x.DateRelationshipStart)
                    .Take(999)
                    .ToList();

            }
        }


        [JsonIgnore]
        public List<OrganizationFfiecRelationship> Relationships
        {
            get
            {
                return ParentRelationships.Union(ChildRelationships).ToList();
            }
        }

        [JsonIgnore]
        public List<OrganizationFfiecRelationship> ChildRelationships { get; internal set; }
        [JsonIgnore]
        public List<OrganizationFfiecRelationship> ParentRelationships { get; internal set; }
        [JsonIgnore]
        public List<OrganizationFfiecTransformation> PredecessorTransformations { get; internal set; }
        public int? StatusCode { get; set; }

        //public string AvatarImageUrl
        //{
        //    get
        //    {
        //        return ProfileUrl + ".jpg";
        //    }
        //}
    }

}
