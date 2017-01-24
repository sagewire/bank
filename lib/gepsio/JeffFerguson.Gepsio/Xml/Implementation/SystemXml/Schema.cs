using JeffFerguson.Gepsio.Xml.Interfaces;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Net;
using System.IO;
using System.Net.Http;
using System;

namespace JeffFerguson.Gepsio.Xml.Implementation.SystemXml
{
    internal class Schema : ISchema
    {
        private XmlSchema thisSchema;
        private List<IQualifiedName> thisNamespaceList;

        internal XmlSchema XmlSchema
        {
            get
            {
                return thisSchema;
            }
        }

        public List<IQualifiedName> Namespaces
        {
            get
            {
                if (thisNamespaceList == null)
                {
                    var newList = new List<IQualifiedName>();
                    var xmlNamespaces = thisSchema.Namespaces.ToArray();
                    newList.Capacity = xmlNamespaces.Length;
                    foreach (var entry in xmlNamespaces)
                    {
                        var newItem = new QualifiedName(entry);
                        newList.Add(newItem);
                    }
                    thisNamespaceList = newList;
                }
                return thisNamespaceList;
            }
        }

        public Schema()
        {
            thisSchema = null;
            thisNamespaceList = null;
        }

        internal Schema(XmlSchema schema)
        {
            thisSchema = schema;
            thisNamespaceList = null;
        }

        public bool Read(string path)
        {            
            try
            {
                //MP
                var uri = new Uri(path);
                XmlReader reader;

                if (uri.IsFile)
                {
                    reader = new XmlTextReader(uri.AbsolutePath);
                }
                else
                {
                    var client = new HttpClient();
                    var webTask = client.GetStreamAsync(path);

                    webTask.Wait();

                    reader = XmlTextReader.Create(webTask.Result);
                }
                
                thisSchema = XmlSchema.Read(reader, null);
                return true;
            }
            catch(XmlSchemaException)
            {
                return false;
            }
        }
    }
}
