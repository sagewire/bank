﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.data.repositories;
using bank.poco;

namespace bank.reports
{
    public class ReportFactory
    {
        public List<Organization> Organizations { get; set; } = new List<Organization>();
        public List<int> OrganizationIds { get; set; } = new List<int>();
        public List<PeerGroupStandard> PeerGroups { get; set; } = new List<PeerGroupStandard>();
        public List<PeerGroupCustom> CustomPeerGroups { get; set; } = new List<PeerGroupCustom>();
        public string Template { get; set; }
        public string SectionFilter { get; set; }
        public DateTime Period { get; set; }

        public Layout Build()
        {
            var layout = new Layout();
            layout.SectionFilter = SectionFilter;
            layout.Load(Template);
            
            //get columns
            layout.DataColumns = GetColumns();

            //populate facts and orgs
            var tasks = new List<Task>();


            var populateTask = Task.Run(() => layout.Populate(Period));
            var orgTask = Task.Run(() => GetOrganizations());


            tasks.Add(orgTask);
            tasks.Add(populateTask);


            Task.WaitAll(tasks.ToArray());

            SetColumns(layout);


            //set columns
            return layout;
        }

        private void GetOrganizations()
        {
            
            if (OrganizationIds.Any()) {
                var orgRepo = new OrganizationRepository();
                var orgs = orgRepo.GetOrganizations(OrganizationIds);
                Organizations.AddRange(orgs);
            }

        }

        private void SetColumns(Layout layout)
        {

            foreach (var column in layout.DataColumns)
            {
                var companyColumn = column as CompanyColumn;

                if (companyColumn != null)
                {
                    if (OrganizationIds != null)
                    {
                        companyColumn.Organization = Organizations.Single(x => x.OrganizationId == companyColumn.OrganizationId);
                    }
                }
            }
        }

        private IList<Column> GetColumns()
        {
            var columns = new List<Column>();

            var orgIds = new List<int>();
            orgIds.AddRange(OrganizationIds);
            orgIds.AddRange(Organizations.Select(x => x.OrganizationId));

            foreach (var orgId in orgIds)
            {
                columns.Add(new CompanyColumn
                {
                    OrganizationId = orgId
                });
            }

            
            foreach(var peerGroup in PeerGroups)
            {
                columns.Add(new PeerGroupColumn
                {
                    PeerGroup = peerGroup.Code,
                    HeaderText = peerGroup.Name
                });
            }

            
            foreach (var custom in CustomPeerGroups)
            {
                columns.Add(new PeerGroupCustomColumn
                {
                    PeerGroupCustom = custom,
                    HeaderText = custom.PeerGroupCode
                });
            }

            return columns;
        }
    }
}