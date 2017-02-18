using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco.graph;
using Neo4j.Driver.V1;

namespace bank.data.graph
{
    public class Cypher
    {
        public static Data Acquisitions()
        {

            using (var driver = GraphDatabase.Driver("bolt://localhost:7687",
                            AuthTokens.Basic("neo4j", "letmein123"),
                            Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig()))
            {

                var nodes = new Dictionary<long, poco.graph.INode>();
                var edges = new Dictionary<string, IEdge>();

                using (var session = driver.Session())
                {
                    var cypher = new StringBuilder();
                    cypher.AppendLine("MATCH p=(n:Organization { id: {id} })-[r:Acquired*1..20]-() RETURN p LIMIT 50");

                    var props = new Dictionary<string, object>();
                    props.Add("id", 24359);
                    
                    var result = session.Run(cypher.ToString(), props);

                    foreach(var record in result)
                    {
                        foreach(var value in record.Values)
                        {
                            var path = value.Value as Neo4j.Driver.V1.IPath;
                            
                            foreach(var node in path.Nodes)
                            {
                                var nodeObj = new OrganizationNode
                                {
                                    NodeId = node.Id,
                                    OrganizationId = int.Parse(node.Properties["id"].ToString()),
                                    Name = node.Properties["name"].ToString()
                                };

                                if (!nodes.ContainsKey(nodeObj.NodeId))
                                {
                                    nodes.Add(nodeObj.NodeId, nodeObj);
                                }

                            }


                            foreach (var relationship in path.Relationships)
                            {
                                var edgeObj = new AcquiredEdge
                                {
                                    SourceId = relationship.StartNodeId,
                                    TargetId = relationship.EndNodeId
                                };

                                if (!edges.ContainsKey(edgeObj.Key))
                                {
                                    edges.Add(edgeObj.Key, edgeObj);
                                }
                            }

                        }
                    }

                }

                return new Data
                {
                    Nodes = nodes.Values.ToList(),
                    Edges = edges.Values.ToList()
                };
            }
        }
    }
}
