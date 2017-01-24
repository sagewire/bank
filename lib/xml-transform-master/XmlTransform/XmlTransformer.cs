using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Web.XmlTransform;
using XmlTransform.Internals;

namespace XmlTransform
{
    /// <summary>
    /// Wrapper around TransformXml task to apply transforms in memory.
    /// </summary>
    public sealed class XmlTransformer : ITransformer
    {
        /// <summary>
        /// Flag indicating whether whitespace should be preserved or not.
        /// </summary>
        public bool PreserveWhiteSpace { get; set; }

        /// <summary>
        /// Constructs an XmlTransformer that preserves whitespace.
        /// </summary>
        public XmlTransformer() : this(true)
        {

        }

        /// <summary>
        /// Constructs an XmlTransformer.
        /// </summary>
        /// <param name="preserveWhiteSpace">Flag indicating whether whitespace should be preserved or not.</param>
        public XmlTransformer(bool preserveWhiteSpace)
        {
            PreserveWhiteSpace = preserveWhiteSpace;
        }

        /// <summary>
        /// Applies <paramref name="transform" /> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">A string representing an XML config file.</param>
        /// <param name="transform">A string representing an XML transform config file.</param>
        /// <returns>The result of the transformation as a string.</returns>
        /// <remarks>Calls <see cref="ApplyTransforms"/>.</remarks>
        public string ApplyTransform(string source, string transform)
        {
            return ApplyTransforms(source, new[] { transform });
        }

        /// <summary>
        /// Applies <paramref name="transforms" /> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">A string representing an XML config file.</param>
        /// <param name="transforms">A series of strings representing XML transform config files.</param>
        /// <returns>The result of the transformation as a string.</returns>
        /// <remarks>Transforms are applied in the order in which <paramref name="transforms"/> are enumerated.</remarks>
        /// <exception cref="XmlTransform.XmlTransformerException"></exception>
        public string ApplyTransforms(string source, IEnumerable<string> transforms)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new XmlTransformerException(TransformerErrors.SourceIsNullOrWhiteSpace);
            }

            if (transforms == null)
            {
                throw new XmlTransformerException(TransformerErrors.TransformsIsNull);
            }

            var transformArray = transforms.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToArray();

            if (transformArray.Length == 0)
            {
                throw new XmlTransformerException(TransformerErrors.TransformsIsNullOrWhiteSpaceElements);
            }

            XmlTransformableDocument document;
            try
            {
                document = GetStringAsXmlTransformableDocument(source);
            }
            catch (Exception ex)
            {
                throw new XmlTransformerException($"{TransformerErrors.FailedToLoadSource} {ex.Message}",  ex.InnerException);
            }

            var transformCounter = 1;
            foreach (var transform in transformArray)
            {
                try
                {
                    using (XmlTransformation xmlTransformation = new XmlTransformation(transform, false, null))
                    {
                        xmlTransformation.Apply(document);
                    }
                }
                catch (Exception ex)
                {
                    throw new XmlTransformerException($"{TransformerErrors.ErrorWhileApplyingTransformFrom}{transformCounter}. Reason: {ex.Message}", ex);
                }
                ++transformCounter;
            }

            var xmlContent = document.InnerXml;
            return xmlContent;
        }

        private XmlTransformableDocument GetStringAsXmlTransformableDocument(string source)
        {
            XmlTransformableDocument transformableDocument = new XmlTransformableDocument
            {
                PreserveWhitespace = PreserveWhiteSpace
            };
            transformableDocument.LoadXml(source);
            return transformableDocument;
        }

    }
}
