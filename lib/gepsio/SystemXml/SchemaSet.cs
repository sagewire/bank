using JeffFerguson.Gepsio.Xml.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace JeffFerguson.Gepsio.Xml.Implementation.SystemXml
{
    internal class SchemaSet : ISchemaSet
    {
        private XmlSchemaSet thisSchemaSet;
        private Dictionary<IQualifiedName, ISchemaElement> thisGlobalElements;
        private Dictionary<IQualifiedName, ISchemaType> thisGlobalTypes;

        public Dictionary<IQualifiedName, ISchemaElement> GlobalElements
        {
            get
            {
                if (thisGlobalElements == null)
                {
                    var newDictionary = new Dictionary<IQualifiedName, ISchemaElement>();
                    foreach (DictionaryEntry currentEntry in thisSchemaSet.GlobalElements)
                    {
                        var key = new QualifiedName(currentEntry.Key as XmlQualifiedName);
                        var value = currentEntry.Value as XmlSchemaElement;
                        var convertedValue = new SchemaElement(value);
                        newDictionary.Add(key, convertedValue);
                    }
                    thisGlobalElements = newDictionary;
                }
                return thisGlobalElements;
            }
        }

        public Dictionary<IQualifiedName, ISchemaType> GlobalTypes
        {
            get
            {
                if (thisGlobalTypes == null)
                {
                    var newDictionary = new Dictionary<IQualifiedName, ISchemaType>();
                    foreach (DictionaryEntry currentEntry in thisSchemaSet.GlobalTypes)
                    {
                        var key = new QualifiedName(currentEntry.Key as XmlQualifiedName);
                        var value = currentEntry.Value as XmlSchemaType;
                        var convertedValue = new SchemaType(value);
                        newDictionary.Add(key, convertedValue);
                    }
                    thisGlobalTypes = newDictionary;
                }
                return thisGlobalTypes;
            }
        }

        public void Add(ISchema schemaToAdd)
        {
            var schemaImplementation = schemaToAdd as Schema;
            thisSchemaSet.Add(schemaImplementation.XmlSchema);
        }

        public void Compile()
        {
            thisSchemaSet.Compile();
        }

        public SchemaSet()
        {
            thisSchemaSet = new XmlSchemaSet();
            thisGlobalElements = null;
            thisGlobalTypes = null;
        }
    }
}
