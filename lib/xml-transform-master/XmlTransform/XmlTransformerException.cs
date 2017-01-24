using System;
using System.Runtime.Serialization;

namespace XmlTransform
{
#pragma warning disable 1591
    /// <summary>
    /// Wrapper exception thrown from <see cref="XmlTransformer.ApplyTransforms"/>.
    /// </summary>
    /// <remarks>
    /// If an inner exception is present then the exception originated from applying a transformation or loading an invalid source string.
    /// </remarks>
    public class XmlTransformerException : Exception

    {
        public XmlTransformerException(string message) : base(message)
        {
        }

        public XmlTransformerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected XmlTransformerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
#pragma warning restore 1591
}
