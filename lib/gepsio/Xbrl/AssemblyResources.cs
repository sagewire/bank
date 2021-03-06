using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;

namespace JeffFerguson.Gepsio
{
    internal class AssemblyResources
    {
        static ResourceManager staticResourceManager;

        static AssemblyResources()
        {
#if NETFX_CORE
            string ResourceFile = typeof(AssemblyResources).GetTypeInfo().Assembly.GetName().Name + ".Gepsio";
            staticResourceManager = new ResourceManager(ResourceFile, typeof(AssemblyResources).GetTypeInfo().Assembly);
#else
            string ResourceFile = typeof(AssemblyResources).Assembly.GetName().Name + ".Gepsio";
            staticResourceManager = new ResourceManager(ResourceFile, typeof(AssemblyResources).Assembly);
#endif
        }

        public static string GetName(string Key)
        {
            return staticResourceManager.GetString(Key);
        }

        public static string BuildMessage(string Key, params object[] Parameters)
        {
            StringBuilder Message = new StringBuilder();
            string MessageFormat = GetName(Key);
            Message.AppendFormat(MessageFormat, Parameters);
            return Message.ToString();
        }
    }
}
