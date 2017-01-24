using System.Collections.Generic;

namespace XmlTransform
{
    /// <summary>
    /// Interface for a wrapper around TransformXml task to apply transforms in memory.
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Applies <paramref name="transform" /> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">A string representing an XML config file.</param>
        /// <param name="transform">A string representing an XML transform config file.</param>
        /// <returns>The result of the transformation as a string.</returns>
        /// <remarks>Calls <see cref="ApplyTransforms"/>.</remarks>
        string ApplyTransform(string source, string transform);

        /// <summary>
        /// Applies <paramref name="transforms" /> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">A string representing an XML config file.</param>
        /// <param name="transforms">A series of strings representing XML transform config files.</param>
        /// <returns>The result of the transformation as a string.</returns>
        /// <remarks>Transforms are applied in the order in which <paramref name="transforms"/> are enumerated.</remarks>
        string ApplyTransforms(string source, IEnumerable<string> transforms);
    }
}
