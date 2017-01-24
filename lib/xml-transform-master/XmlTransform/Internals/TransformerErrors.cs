namespace XmlTransform.Internals
{
    internal static class TransformerErrors
    {
        internal static readonly string SourceIsNullOrWhiteSpace = "Source cannot be null, empty or consist of whitespace only.";
        internal static readonly string TransformsIsNull = "Transforms cannot be null.";
        internal static readonly string TransformsIsNullOrWhiteSpaceElements = "Transforms cannot consist of only null, empty or whitespace elements.";
        internal static readonly string ErrorWhileApplyingTransformFrom = "An error occured while applying transformation from transform #";
        internal static readonly string FailedToLoadSource = "Failed to load source.";
    }
}
