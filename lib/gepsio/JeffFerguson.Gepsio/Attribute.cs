using System;

namespace JeffFerguson.Gepsio
{
    internal class Attribute
    {
        private string thisName;
        private Type thisValueType;
        private bool thisRequired;

        public string Name
        {
            get
            {
                return thisName;
            }
        }

        public Type ValueType
        {
            get
            {
                return thisValueType;
            }
        }

        public bool Required
        {
            get
            {
                return thisRequired;
            }
        }

        internal Attribute(string Name, Type ValueType, bool Required)
        {
            thisName = Name;
            thisValueType = ValueType;
            thisRequired = Required;
        }
    }
}
