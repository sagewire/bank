using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using bank.data.repositories;
using bank.poco;
using bank.extensions;
using Neo4j.Driver.V1;

namespace bank.import.graph
{
    public static class ImportGraph
    {
        private static TaskPool<OrganizationFfiecTransformation> _taskPool = new TaskPool<OrganizationFfiecTransformation>();

        public static void Start(int threads)
        {
            //InitializeTaskPool(threads);
            var orgRepo = new OrganizationRepository();
            var relationshipRepo = new OrganizationFfiecRelationshipRepository();
            var transformationRepo = new OrganizationFfiecTransformationRepository();

            //var all = orgRepo.GetOrganizationsForGraph();
            var orgs = orgRepo.GetOrganizationsForGraph();
            //var relationships = relationshipRepo.GetRelationshipsForGraph();
            var transformations = transformationRepo.GetTransformationsForGraph();

            using (var driver = GraphDatabase.Driver("bolt://localhost:7687",
                            AuthTokens.Basic("neo4j", "letmein123"),
                            Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig()))
            {




                foreach (var org in orgs)
                {
                    using (var session = driver.Session())
                    {
                        var orgSql = new StringBuilder();
                        orgSql.AppendLine("MERGE (a:Organization { id: {id} })");
                        orgSql.AppendLine("ON CREATE SET a.id = {id}, a.title = {name}, a.name = {name}, a.assets = {assets}, a.url = {url}, a.active = {active}");
                        orgSql.AppendLine("ON MATCH SET a.title = {name}, a.name = {name}, a.assets = {assets}, a.url = {url}, a.active = {active}");

                        var props = new Dictionary<string, object>();
                        props.Add("id", org.OrganizationId);
                        props.Add("name", org.Name);
                        props.Add("title", org.Name);
                        props.Add("assets", org.TotalAssets);
                        props.Add("url", org.ProfileUrl);
                        props.Add("active", org.Active);

                        var result = session.Run(orgSql.ToString(), props);

                        Console.WriteLine("{0}", org.Name);
                    }
                }

                return;


                //foreach (var relationship in relationships)
                //{
                //    using (var session = driver.Session())
                //    {
                //        var props = new Dictionary<string, object>();
                //        props.Add("parent", relationship.ParentOrganizationId);
                //        props.Add("offspring", relationship.OffspringOrganizationId);

                //        var sql = new StringBuilder();
                //        sql.AppendLine("MATCH(parent:Organization { id: {parent} }), (offspring:Organization { id: {offspring} })");
                //        sql.AppendLine("MERGE (parent)-[:IS_PARENT]->(offspring)");

                //        var result = session.Run(sql.ToString(), props);
                //    }
                //}

                var transformationSql = new StringBuilder();
                transformationSql.AppendLine("MATCH(predecessor:Organization { id: {predecessor} }), (successor:Organization { id: {successor} })");
                transformationSql.AppendLine("MERGE (successor)-[a:Acquired]->(predecessor)");
                transformationSql.AppendLine("ON CREATE SET a.date = {date}, a.type = {type}");
                transformationSql.AppendLine("ON MATCH SET a.date = {date}, a.type = {type}");

                foreach (var transformation in transformations)
                {
                    using (var session = driver.Session())
                    {

                        var props = new Dictionary<string, object>();
                        props.Add("predecessor", transformation.PredecessorOrganizationId);
                        props.Add("successor", transformation.SuccessorOrganizationId);
                        props.Add("date", transformation.D_DT_TRANS.Value.ToMillisecondsSince1970());
                        props.Add("type", transformation.TRNSFM_CD);

                        var result = session.Run(transformationSql.ToString(), props);

                        Console.WriteLine("{0}->{1}", transformation.ID_RSSD_PREDECESSOR, transformation.ID_RSSD_SUCCESSOR);
                    }
                }

            }
        }


        static void InitializeTaskPool(int threads)
        {
            Console.WriteLine("Starting ffiec transformations import pool");
            _taskPool.MaxWorkers = threads;
            _taskPool.NextTask += _taskPool_NextTask;
        }

        private static void _taskPool_NextTask(OrganizationFfiecTransformation task)
        {
            throw new NotImplementedException();
        }
    }
}
