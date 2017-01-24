using JeffFerguson.Gepsio.IoC;
using JeffFerguson.Gepsio.Xml.Interfaces;
using JeffFerguson.Gepsio.Xsd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JeffFerguson.Gepsio
{
    /// <summary>
    /// A fragment of XBRL data. A collection of fragments is available in the <see cref="XbrlDocument"/>
    /// class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An XBRL fragment is a fragment of XBRL data having an xbrl tag as its root. In the generic case,
    /// an XBRL document will have an xbrl tag as the root tag of the XML document, and, in this case,
    /// the entire XBRL document is one large XBRL fragment. However, section 4.1 of the XBRL 2.1 Specification
    /// makes provisions for multiple XBRL fragments to be stored in a single document:
    /// </para>
    /// <para>
    /// "If multiple 'data islands' of XBRL mark-up are included in a larger document, the xbrl element is
    /// the container for each [fragment]."
    /// </para>
    /// <para>
    /// Gepsio supports this notion by defining an XBRL document containing a collection of one or more
    /// XBRL fragments, as in the following code sample:
    /// </para>
    /// <code>
    /// var myDocument = new XbrlDocument();
    /// myDocument.Load("myxbrldoc.xml");
    /// foreach(var currentFragment in myDocument.XbrlFragments)
    /// {
    ///     // XBRL data is available from the "currentFragment" variable
    /// }
    /// </code>
    /// <para>
    /// In the vast majority of cases, an XBRL document will be an XML document with the xbrl tag at its
    /// root, and, as a result, the <see cref="XbrlDocument"/> uses to load the XBRL document will have
    /// a single <see cref="XbrlFragment"/> in the document's fragments container. Consider, however, the
    /// possibility of having more than one fragment in a document, in accordance of the text in section
    /// 4.1 of the XBRL 2.1 Specification.
    /// </para>
    /// </remarks>
    public class XbrlFragment
    {
        #region Delegates

        /// <summary>
        /// The delegate used to handle events fired by the class.
        /// </summary>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        public delegate void XbrlEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        /// <summary>
        /// Event fired after a document has been loaded.
        /// </summary>
        public event XbrlEventHandler Loaded;

        /// <summary>
        /// Event fired after all XBRL validation has been completed.
        /// </summary>
        public event XbrlEventHandler Validated;

        #endregion

        #region Fields

        private INode thisXbrlRootNode;
        private List<Context> thisContexts;
        private IDictionary<string, Context> thisContextDictionary;
        private INamespaceManager thisNamespaceManager;
        private List<XbrlSchema> thisSchemas;
        private FactCollection thisFacts;
        private List<Unit> thisUnits;
        private List<FootnoteLink> thisFootnoteLinks;
        private List<ValidationError> thisValidationErrors;

        #endregion

        #region Properties

        /// <summary>
        /// A reference to the <see cref="XbrlDocument"/> instance in which the fragment
        /// was contained.
        /// </summary>
        public XbrlDocument Document
        {
            get;
            private set;
        }

        /// <summary>
        /// The root XML node for the XBRL fragment.
        /// </summary>
        internal INode XbrlRootNode
        {
            get
            {
                return thisXbrlRootNode;
            }
        }

        /// <summary>
        /// A collection of <see cref="Context"/> objects representing all contexts found in the fragment.
        /// </summary>
        public List<Context> Contexts
        {
            get
            {
                return thisContexts;
            }
        }

        /// <summary>
        /// A collection of <see cref="XbrlSchema"/> objects representing all schemas found in the fragment.
        /// </summary>
        public List<XbrlSchema> Schemas
        {
            get
            {
                return thisSchemas;
            }
        }

        /// <summary>
        /// A collection of <see cref="Fact"/> objects representing all facts found in the fragment.
        /// </summary>
        public FactCollection Facts
        {
            get
            {
                return thisFacts;
            }
        }

        /// <summary>
        /// A collection of <see cref="Unit"/> objects representing all units found in the fragment.
        /// </summary>
        public List<Unit> Units
        {
            get
            {
                return thisUnits;
            }
        }

        /// <summary>
        /// A collection of <see cref="FootnoteLink"/> objects representing all footnote links
        /// found in the fragment.
        /// </summary>
        public List<FootnoteLink> FootnoteLinks
        {
            get
            {
                return thisFootnoteLinks;
            }
        }

        /// <summary>
        /// Evaluates to true if the fragment contains no XBRL validation errors. Evaluates to
        /// false if the fragment contains at least one XBRL validation error.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (thisValidationErrors == null)
                    return true;
                if (thisValidationErrors.Count == 0)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// A collection of all validation errors found while validating the fragment.
        /// </summary>
        public List<ValidationError> ValidationErrors
        {
            get
            {
                return thisValidationErrors;
            }
        }

        internal INamespaceManager NamespaceManager
        {
            get;
            private set;
        }

        /// <summary>
        /// A collection of role references found in the fragment.
        /// </summary>
        public List<RoleReference> RoleReferences
        {
            get;
            private set;
        }

        /// <summary>
        /// A collection of arcrole references found in the fragment.
        /// </summary>
        public List<ArcroleReference> ArcroleReferences
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        internal XbrlFragment(XbrlDocument ParentDocument, INamespaceManager namespaceManager, INode XbrlRootNode)
        {
            this.Document = ParentDocument;
            this.NamespaceManager = namespaceManager;
            thisXbrlRootNode = XbrlRootNode;
            thisValidationErrors = new List<ValidationError>();
            CreateNamespaceManager();
            //---------------------------------------------------------------------------
            // Load.
            //---------------------------------------------------------------------------
            ReadTaxonomySchemaReferences();
            ReadRoleReferences();
            ReadArcroleReferences();
            ReadContexts();
            ReadUnits();
            ReadFacts();
            ReadFootnoteLinks();
            if (Loaded != null)
                Loaded(this, null);
            //---------------------------------------------------------------------------
            // Validate.
            //---------------------------------------------------------------------------
            ValidateRoleReferences();
            ValidateArcroleReferences();
            ValidateContextRefs();
            ValidateUnitRefs();
            ValidateContextTimeSpansAgainstPeriodTypes();
            ValidateFootnoteLocations();
            ValidateFootnoteArcs();
            ValidateItems();
            if (Validated != null)
                Validated(this, null);
        }

        #endregion

        internal void AddValidationError(ValidationError validationError)
        {
            thisValidationErrors.Add(validationError);
        }

        #region Public Methods

        /// <summary>
        /// Returns a reference to the context having the supplied context ID.
        /// </summary>
        /// <param name="ContextId">
        /// The ID of the context to return.
        /// </param>
        /// <returns>
        /// A reference to the context having the supplied context ID.
        /// A null is returned if no contexts with the supplied context ID is available.
        /// </returns>
        public Context GetContext(string ContextId)
        {
            foreach (Context CurrentContext in thisContexts)
            {
                if (CurrentContext.Id == ContextId)
                    return CurrentContext;
            }
            return null;
        }

        /// <summary>
        /// Returns a reference to the unit having the supplied unit ID.
        /// </summary>
        /// <param name="UnitId">
        /// The ID of the unit to return.
        /// </param>
        /// <returns>
        /// A reference to the unit having the supplied unit ID.
        /// A null is returned if no units with the supplied unit ID is available.
        /// </returns>
        public Unit GetUnit(string UnitId)
        {
            foreach (Unit CurrentUnit in thisUnits)
            {
                if (CurrentUnit.Id == UnitId)
                    return CurrentUnit;
            }
            return null;
        }

        /// <summary>
        /// Returns a reference to the fact having the supplied fact ID.
        /// </summary>
        /// <param name="FactId">
        /// The ID of the fact to return.
        /// </param>
        /// <returns>
        /// A reference to the fact having the supplied fact ID.
        /// A null is returned if no facts with the supplied fact ID is available.
        /// </returns>
        public Item GetFact(string FactId)
        {
            var matchingFact = thisFacts.GetFactById(FactId);
            if (matchingFact == null)
                return null;
            return matchingFact as Item;
        }

        /// <summary>
        /// Gets the URI associated with a given namespace prefix.
        /// </summary>
        /// <param name="Prefix">
        /// The namespace prefix whose URI should be returned.
        /// </param>
        /// <returns>
        /// The namespace URI associated with the specified namespace prefix.
        /// </returns>
        public string GetUriForPrefix(string Prefix)
        {
            return thisNamespaceManager.LookupNamespace(Prefix);
        }

        /// <summary>
        /// Gets the namespace prefix associated with a given URI.
        /// </summary>
        /// <param name="Uri">
        /// The URI whose namespace prefix should be returned.
        /// </param>
        /// <returns>
        /// The namespace prefix associated with the specified namespace URI.
        /// </returns>
        public string GetPrefixForUri(string Uri)
        {
            return thisNamespaceManager.LookupPrefix(Uri);
        }

        /// <summary>
        /// Gets the schema associated with a given namespace prefix.
        /// </summary>
        /// <param name="Prefix">
        /// The namespace prefix whose schema should be returned.
        /// </param>
        /// <returns>
        /// A reference to the XBRL schema containing the specified namespace prefix. A null
        /// is returned if the given namespace prefix is not found in any of the XBRL schemas.
        /// </returns>
        public XbrlSchema GetXbrlSchemaForPrefix(string Prefix)
        {
            string Uri = GetUriForPrefix(Prefix);
            if (Uri == null)
                return null;
            if (Uri.Length == 0)
                return null;
            foreach (XbrlSchema CurrentSchema in thisSchemas)
            {
                if (CurrentSchema.TargetNamespace.Equals(Uri) == true)
                    return CurrentSchema;
            }
            return null;
        }

        /// <summary>
        /// Finds the <see cref="RoleType"/> object having the given ID.
        /// </summary>
        /// <param name="RoleTypeId">
        /// The ID of the role type to find.
        /// </param>
        /// <returns>
        /// The <see cref="RoleType"/> object having the given ID, or null if no
        /// object can be found.
        /// </returns>
        public RoleType GetRoleType(string RoleTypeId)
        {
            foreach (var currentSchema in Schemas)
            {
                var roleTypeCandidate = currentSchema.GetRoleType(RoleTypeId);
                if (roleTypeCandidate != null)
                    return roleTypeCandidate;
            }
            return null;
        }

        /// <summary>
        /// Finds the <see cref="CalculationLink"/> object having the given role.
        /// </summary>
        /// <param name="CalculationLinkRole">
        /// The role type to find.
        /// </param>
        /// <returns>
        /// The <see cref="CalculationLink"/> object having the given role, or
        /// null if no object can be found.
        /// </returns>
        public CalculationLink GetCalculationLink(RoleType CalculationLinkRole)
        {
            foreach (var currentSchema in Schemas)
            {
                var calculationLinkCandidate = currentSchema.GetCalculationLink(CalculationLinkRole);
                if (calculationLinkCandidate != null)
                    return calculationLinkCandidate;
            }
            return null;
        }

        /// <summary>
        /// Returns a prestable fact tree.
        /// </summary>
        /// <returns>
        /// A presentable fact tree. A null will be returned if no presentation linkbase
        /// is available.
        /// </returns>
        public PresentableFactTree GetPresentableFactTree()
        {
            foreach (var currentSchema in Schemas)
            {
                if (currentSchema.PresentationLinkbase != null)
                    return new PresentableFactTree(currentSchema, this.Facts);
            }
            return null;
        }

        //===============================================================================
        #endregion
        //===============================================================================

        //===============================================================================
        #region Private Methods
        //===============================================================================

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadTaxonomySchemaReferences()
        {
            thisSchemas = new List<XbrlSchema>();
            string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix(XbrlDocument.XbrlLinkbaseNamespaceUri);
            StringBuilder XPathExpressionBuilder = new StringBuilder();
            XPathExpressionBuilder.AppendFormat("//{0}:schemaRef", LinkbaseNamespacePrefix);
            string XPathExpression = XPathExpressionBuilder.ToString();
            INodeList SchemaRefNodes = thisXbrlRootNode.SelectNodes(XPathExpression, thisNamespaceManager);
            foreach (INode SchemaRefNode in SchemaRefNodes)
                ReadTaxonomySchemaReference(SchemaRefNode);
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadRoleReferences()
        {
            RoleReferences = new List<RoleReference>();
            string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix(XbrlDocument.XbrlLinkbaseNamespaceUri);
            StringBuilder XPathExpressionBuilder = new StringBuilder();
            XPathExpressionBuilder.AppendFormat("//{0}:roleRef", LinkbaseNamespacePrefix);
            string XPathExpression = XPathExpressionBuilder.ToString();
            INodeList RoleRefNodes = thisXbrlRootNode.SelectNodes(XPathExpression, thisNamespaceManager);
            foreach (INode RoleRefNode in RoleRefNodes)
                this.RoleReferences.Add(new RoleReference(RoleRefNode));
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadArcroleReferences()
        {
            ArcroleReferences = new List<ArcroleReference>();
            string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix(XbrlDocument.XbrlLinkbaseNamespaceUri);
            StringBuilder XPathExpressionBuilder = new StringBuilder();
            XPathExpressionBuilder.AppendFormat("//{0}:arcroleRef", LinkbaseNamespacePrefix);
            string XPathExpression = XPathExpressionBuilder.ToString();
            INodeList ArcroleRefNodes = thisXbrlRootNode.SelectNodes(XPathExpression, thisNamespaceManager);
            foreach (INode ArcroleRefNode in ArcroleRefNodes)
                this.ArcroleReferences.Add(new ArcroleReference(ArcroleRefNode));
        }

        //-------------------------------------------------------------------------------
        // Validate role references.
        //
        // According to test 308.01 of the CR5 conformance suite, each role reference must
        // reference a unique URI.
        //-------------------------------------------------------------------------------
        private void ValidateRoleReferences()
        {
            var uniqueUris = new Dictionary<string, RoleReference>();

            foreach(var currentRoleReference in RoleReferences)
            {
                var currentRoleReferenceUriAsString = currentRoleReference.Uri.ToString();
                if(uniqueUris.ContainsKey(currentRoleReferenceUriAsString) == true)
                {
                    string MessageFormat = AssemblyResources.GetName("DuplicateRoleReferenceUri");
                    StringBuilder MessageBuilder = new StringBuilder();
                    MessageBuilder.AppendFormat(MessageFormat, currentRoleReferenceUriAsString);
                    AddValidationError(new RoleReferenceValidationError(currentRoleReference, MessageBuilder.ToString()));
                    return;
                }
                uniqueUris.Add(currentRoleReferenceUriAsString, currentRoleReference);
            }
        }

        //-------------------------------------------------------------------------------
        // Validate arcrole references.
        //
        // According to test 308.02 of the CR5 conformance suite, each arcrole reference
        // must reference a unique URI.
        //-------------------------------------------------------------------------------
        private void ValidateArcroleReferences()
        {
            var uniqueUris = new Dictionary<string, ArcroleReference>();

            foreach (var currentArcroleReference in ArcroleReferences)
            {
                var currentArcroleReferenceUriAsString = currentArcroleReference.Uri.ToString();
                if (uniqueUris.ContainsKey(currentArcroleReferenceUriAsString) == true)
                {
                    string MessageFormat = AssemblyResources.GetName("DuplicateArcroleReferenceUri");
                    StringBuilder MessageBuilder = new StringBuilder();
                    MessageBuilder.AppendFormat(MessageFormat, currentArcroleReferenceUriAsString);
                    AddValidationError(new ArcroleReferenceValidationError(currentArcroleReference, MessageBuilder.ToString()));
                    return;
                }
                uniqueUris.Add(currentArcroleReferenceUriAsString, currentArcroleReference);
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadTaxonomySchemaReference(INode SchemaRefNode)
        {
            string HrefAttributeValue = SchemaRefNode.GetAttributeValue(Xlink.XlinkNode.xlinkNamespace, "href");
            string Base = SchemaRefNode.GetAttributeValue(XbrlDocument.XmlNamespaceUri, "base");
            var newSchema = new XbrlSchema(this, HrefAttributeValue, Base);
            if(newSchema.SchemaRootNode != null)
                thisSchemas.Add(newSchema);
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void CreateNamespaceManager()
        {
            thisNamespaceManager = Container.Resolve<INamespaceManager>();
            thisNamespaceManager.AddNamespace("instance", thisXbrlRootNode.NamespaceURI);
            foreach (IAttribute CurrentAttribute in thisXbrlRootNode.Attributes)
            {
                if (CurrentAttribute.Prefix == "xmlns")
                    thisNamespaceManager.AddNamespace(CurrentAttribute.LocalName, CurrentAttribute.Value);
            }
        }

        /// <summary>
        /// Validate context references for all facts in the fragment.
        /// </summary>
        private void ValidateContextRefs()
        {
            thisContextDictionary = thisContexts.ToDictionary(context => context.Id);
            ValidateContextRefs(thisFacts);
        }

        /// <summary>
        /// Validate context references for all facts in the given fact collection.
        /// </summary>
        /// <param name="FactList">
        /// A collection of facts whose contexts should be validated.
        /// </param>
        private void ValidateContextRefs(FactCollection FactList)
        {
            foreach (Fact CurrentFact in FactList)
            {
                if (CurrentFact is Item)
                    ValidateContextRef(CurrentFact as Item);
                else if (CurrentFact is Tuple)
                {
                    var CurrentTuple = CurrentFact as Tuple;
                    ValidateContextRefs(CurrentTuple.Facts);
                }
            }
        }

        //-------------------------------------------------------------------------------
        // Validates the context reference for the given fact. Ensures that the context
        // ref can be tied to a defined context.
        //-------------------------------------------------------------------------------
        private void ValidateContextRef(Item ItemToValidate)
        {
            string ContextRefValue = ItemToValidate.ContextRefName;
            if (ContextRefValue.Length == 0)
                return;

            try
            {
                Context MatchingContext = this.thisContextDictionary[ContextRefValue];
                ItemToValidate.ContextRef = MatchingContext;
            }
            catch (KeyNotFoundException)
            {
                string MessageFormat = AssemblyResources.GetName("CannotFindContextForContextRef");
                StringBuilder MessageBuilder = new StringBuilder();
                MessageBuilder.AppendFormat(MessageFormat, ContextRefValue);
                AddValidationError(new ItemValidationError(ItemToValidate, MessageBuilder.ToString()));
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ValidateUnitRefs()
        {
            foreach (Fact CurrentFact in thisFacts)
            {
                if (CurrentFact is Item)
                    ValidateUnitRef(CurrentFact as Item);
            }
        }

        //-------------------------------------------------------------------------------
        // Validates the unit reference for the given fact. Ensures that the unit ref
        // can be tied to a defined unit.
        //-------------------------------------------------------------------------------
        private void ValidateUnitRef(Item ItemToValidate)
        {
            string UnitRefValue = ItemToValidate.UnitRefName;
            //-----------------------------------------------------------------------
            // According to section 4.6.2, non-numeric items must not have a unit
            // reference. So, if the fact's unit reference is blank, and this is a
            // non-numeric item, then there is nothing to validate.
            //-----------------------------------------------------------------------
            if (UnitRefValue.Length == 0)
            {
                if (ItemToValidate.SchemaElement == null)
                    return;
                if (ItemToValidate.Type == null)
                    return;
                if (ItemToValidate.Type.IsNumeric == false)
                    return;
            }
            //-----------------------------------------------------------------------
            // At this point, we have a unit ref should be matched to a unit.
            //-----------------------------------------------------------------------
            bool UnitFound = false;
            Unit MatchingUnit = null;
            foreach (Unit CurrentUnit in thisUnits)
            {
                if (CurrentUnit.Id == UnitRefValue)
                {
                    UnitFound = true;
                    MatchingUnit = CurrentUnit;
                    ItemToValidate.UnitRef = MatchingUnit;
                }
            }
            //-----------------------------------------------------------------------
            // Check to see if a unit is found.
            //-----------------------------------------------------------------------
            if (UnitFound == false)
            {
                string MessageFormat = AssemblyResources.GetName("CannotFindUnitForUnitRef");
                StringBuilder MessageBuilder = new StringBuilder();
                MessageBuilder.AppendFormat(MessageFormat, UnitRefValue);
                AddValidationError(new ItemValidationError(ItemToValidate, MessageBuilder.ToString()));
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadContexts()
        {
            thisContexts = new List<Context>();
            INodeList ContextNodes = thisXbrlRootNode.SelectNodes("//instance:context", thisNamespaceManager);
            foreach (INode ContextNode in ContextNodes)
                thisContexts.Add(new Context(this, ContextNode));
        }

        /// <summary>
        /// Reads all of the facts in the XBRL fragment and creates an object for each.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Element instances can be any of the following:
        /// </para>
        /// <para>
        /// <list type="bullet">
        /// <item>
        /// an item, represented in an XBRL schema by an element with a substitution group of "item"
        /// and represented by an <see cref="Item"/> object
        /// </item>
        /// <item>
        /// a tuple, represented in an XBRL schema by an element with a substitution group of "tuple"
        /// and represented by an <see cref="Tuple"/> object
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        private void ReadFacts()
        {
            thisFacts = new FactCollection();
            thisFacts.Capacity = thisXbrlRootNode.ChildNodes.Count;
            foreach (INode CurrentChild in thisXbrlRootNode.ChildNodes)
            {
                var CurrentFact = Fact.Create(this, CurrentChild);
                if (CurrentFact != null)
                    thisFacts.Add(CurrentFact);
            }
            thisFacts.TrimExcess();
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private bool IsTaxonomyNamespace(string CandidateNamespace)
        {
            foreach (XbrlSchema CurrentSchema in this.Schemas)
            {
                if (CandidateNamespace == CurrentSchema.TargetNamespace)
                    return true;
            }
            return false;
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ValidateContextTimeSpansAgainstPeriodTypes()
        {
            foreach (Fact CurrentFact in thisFacts)
            {
                if (CurrentFact is Item)
                {
                    var CurrentItem = CurrentFact as Item;
                    switch (CurrentItem.SchemaElement.PeriodType)
                    {
                        case Element.ElementPeriodType.Duration:
                            if (CurrentItem.ContextRef != null)
                            {
                                if (CurrentItem.ContextRef.DurationPeriod == false)
                                {
                                    StringBuilder MessageBuilder = new StringBuilder();
                                    string StringFormat = AssemblyResources.GetName("ElementSchemaDefinesDurationButUsedWithNonDurationContext");
                                    MessageBuilder.AppendFormat(StringFormat, CurrentItem.SchemaElement.Schema.Path, CurrentItem.Name, CurrentItem.ContextRef.Id);
                                    AddValidationError(new ItemValidationError(CurrentItem, MessageBuilder.ToString()));
                                }
                            }
                            break;
                        case Element.ElementPeriodType.Instant:
                            if (CurrentItem.ContextRef != null)
                            {
                                if (CurrentItem.ContextRef.InstantPeriod == false)
                                {
                                    StringBuilder MessageBuilder = new StringBuilder();
                                    string StringFormat = AssemblyResources.GetName("ElementSchemaDefinesInstantButUsedWithNonInstantContext");
                                    MessageBuilder.AppendFormat(StringFormat, CurrentItem.SchemaElement.Schema.Path, CurrentItem.Name, CurrentItem.ContextRef.Id);
                                    AddValidationError(new ItemValidationError(CurrentItem, MessageBuilder.ToString()));
                                }
                            }
                            break;
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadUnits()
        {
            thisUnits = new List<Unit>();
            INodeList UnitNodes = thisXbrlRootNode.SelectNodes("//instance:unit", thisNamespaceManager);
            foreach (INode UnitNode in UnitNodes)
                thisUnits.Add(new Unit(this, UnitNode, thisNamespaceManager));
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ReadFootnoteLinks()
        {
            thisFootnoteLinks = new List<FootnoteLink>();
            string LinkbaseNamespacePrefix = thisNamespaceManager.LookupPrefix(XbrlDocument.XbrlLinkbaseNamespaceUri);
            StringBuilder XPathExpressionBuilder = new StringBuilder();
            XPathExpressionBuilder.AppendFormat("//{0}:footnoteLink", LinkbaseNamespacePrefix);
            string XPathExpression = XPathExpressionBuilder.ToString();
            INodeList FootnoteLinkNodes = thisXbrlRootNode.SelectNodes(XPathExpression, thisNamespaceManager);
            foreach (INode FootnoteLinkNode in FootnoteLinkNodes)
                thisFootnoteLinks.Add(new FootnoteLink(this, FootnoteLinkNode));
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ValidateFootnoteArcs()
        {
            foreach (FootnoteLink CurrentFootnoteLink in thisFootnoteLinks)
            {
                foreach (FootnoteArc CurrentArc in CurrentFootnoteLink.FootnoteArcs)
                    ValidateFootnoteArc(CurrentArc);
            }
        }

        //-------------------------------------------------------------------------------
        // Validate all of the facts found in the fragment. Multiple activities happen
        // here:
        //
        // * each fact is validated against its data type described in its definition
        //   via an <element> tag in a taxonomy schema
        // * any facts that participate in an arc role are checked
        //-------------------------------------------------------------------------------
        private void ValidateItems()
        {
            foreach (Fact CurrentFact in thisFacts)
            {
                var CurrentItem = CurrentFact as Item;
                if (CurrentItem != null)
                    CurrentItem.Validate();
            }
            ValidateItemsReferencedInDefinitionArcRoles();
            ValidateSummationConcepts();
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private bool UrlReferencesFragmentDocument(HyperlinkReference Href)
        {
            if (Href.UrlSpecified == false)
                return false;
            string DocFullPath = Path.GetFullPath(this.Document.Filename);
            string HrefFullPathString;
            if (Href.Url.IndexOf(Path.DirectorySeparatorChar) == -1)
                HrefFullPathString = this.Document.Path + Path.DirectorySeparatorChar + Href.Url;
            else
                HrefFullPathString = Href.Url;
            string HrefFullPath = Path.GetFullPath(HrefFullPathString);
            if (DocFullPath.Equals(HrefFullPath) == true)
                return true;
            return false;
        }

        //===============================================================================
        #endregion
        //===============================================================================

        //===============================================================================
        #region Definition Arc Role Validation
        //===============================================================================

        //-------------------------------------------------------------------------------
        // Searches the associated XBRL schemas, looking for facts that are referenced
        // in arc roles.
        //-------------------------------------------------------------------------------
        private void ValidateItemsReferencedInDefinitionArcRoles()
        {
            foreach (var currentSchema in Schemas)
            {
                var currentDefinitionLinkbaseDocument = currentSchema.DefinitionLinkbase;
                if (currentDefinitionLinkbaseDocument != null)
                {
                    foreach (DefinitionLink CurrentDefinitionLink in currentDefinitionLinkbaseDocument.DefinitionLinks)
                        ValidateFactsReferencedInDefinitionArcRoles(CurrentDefinitionLink);
                }
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ValidateFactsReferencedInDefinitionArcRoles(DefinitionLink CurrentDefinitionLink)
        {
            foreach (DefinitionArc CurrentDefinitionArc in CurrentDefinitionLink.DefinitionArcs)
            {
                switch (CurrentDefinitionArc.Role)
                {
                    case DefinitionArc.RoleEnum.EssenceAlias:
                        ValidateEssenceAliasedFacts(CurrentDefinitionArc);
                        break;
                    case DefinitionArc.RoleEnum.RequiresElement:
                        ValidateRequiresElementFacts(CurrentDefinitionArc);
                        break;
                    default:
                        break;
                }
            }
        }

        //-------------------------------------------------------------------------------
        // Validate the "requires element" connection between two facts referenced in a
        // definition arc.
        //-------------------------------------------------------------------------------
        private void ValidateRequiresElementFacts(DefinitionArc RequiresElementDefinitionArc)
        {
            Locator CurrentFromLocator = RequiresElementDefinitionArc.FromLocator;
            Locator CurrentToLocator = RequiresElementDefinitionArc.ToLocator;
            int FromFactCount = CountFactInstances(CurrentFromLocator.HrefResourceId);
            int ToFactCount = CountFactInstances(CurrentToLocator.HrefResourceId);
            if (FromFactCount > ToFactCount)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("NotEnoughToFactsInRequiresElementRelationship");
                MessageBuilder.AppendFormat(StringFormat, CurrentFromLocator.HrefResourceId, CurrentToLocator.HrefResourceId);
                AddValidationError(new DefinitionArcValidationError(RequiresElementDefinitionArc, MessageBuilder.ToString()));
            }
        }

        //-------------------------------------------------------------------------------
        // Returns a count of the number of facts with the given name.
        //-------------------------------------------------------------------------------
        private int CountFactInstances(string FactName)
        {
            int Count = 0;

            foreach (Fact CurrentFact in thisFacts)
            {
                if (CurrentFact.Name.Equals(FactName) == true)
                    Count++;
            }
            return Count;
        }

        /// <summary>
        /// Validate the essence alias between two facts referenced in a definition arc using
        /// the set of all facts in the fragment. 
        /// </summary>
        /// <param name="EssenceAliasDefinitionArc">
        /// The definition arc defining the essence alias.
        /// </param>
        private void ValidateEssenceAliasedFacts(DefinitionArc EssenceAliasDefinitionArc)
        {
            ValidateEssenceAliasedFacts(EssenceAliasDefinitionArc, thisFacts);
        }

        /// <summary>
        /// Validate the essence alias between two facts referenced in a definition arc using
        /// the set of all facts in the fragment. 
        /// </summary>
        /// <param name="EssenceAliasDefinitionArc">
        /// The definition arc defining the essence alias.
        /// </param>
        /// <param name="FactList">
        /// A collection of <see cref="Fact"/> objects defined in the fragment.
        /// </param>
        private void ValidateEssenceAliasedFacts(DefinitionArc EssenceAliasDefinitionArc, FactCollection FactList)
        {
            Locator CurrentFromLocator = EssenceAliasDefinitionArc.FromLocator;
            if (CurrentFromLocator == null)
                throw new NullReferenceException("FromLocator is NULL in ValidateEssenceAliasedFacts()");
            Locator CurrentToLocator = EssenceAliasDefinitionArc.ToLocator;

            foreach (Fact CurrentFact in FactList)
            {
                if (CurrentFact is Item)
                {
                    var CurrentItem = CurrentFact as Item;
                    if (CurrentItem.Name.Equals(CurrentFromLocator.HrefResourceId) == true)
                        ValidateEssenceAliasedFacts(CurrentItem, FactList, CurrentToLocator.HrefResourceId);
                }
                else if (CurrentFact is Tuple)
                {
                    var CurrentTuple = CurrentFact as Tuple;
                    ValidateEssenceAliasedFacts(EssenceAliasDefinitionArc, CurrentTuple.Facts);
                }
            }
        }

        /// <summary>
        /// Validate the essence alias between a given fact and all other facts with the given fact name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An essence alias is a relationship between a "from" item and a "to" item. The "from" item and
        /// the "to" item must be identical. This method is given the "from" item and must search for the
        /// corresponding "to" item.
        /// </para>
        /// <para>
        /// The scoping of the search for the corresponding "to" item is important. In the simple case, an
        /// XBRL fragment has only items, and the search for the corresponding "to" item can be conducted
        /// in the list all of all items in the fragment.
        /// </para>
        /// <para>
        /// However, if the "from" item is found in a tuple, then the list of items from which the "to" item
        /// is to be found must be restricted to the other items in the tuple and not simply the set of all
        /// items in the fragment.
        /// </para>
        /// </remarks>
        /// <param name="FromItem">
        /// The item that represents the "from" end of the essence alias relationship.
        /// </param>
        /// <param name="FactList">
        /// The list of facts that should be searched to find the item that represents the "to" end of the essence alias relationship.
        /// </param>
        /// <param name="ToItemName">
        /// The name of the item that represents the "to" end of the essence alias relationship.
        /// </param>
        private void ValidateEssenceAliasedFacts(Item FromItem, FactCollection FactList, string ToItemName)
        {
            foreach (Fact CurrentFact in FactList)
            {
                var CurrentItem = CurrentFact as Item;
                if (CurrentItem != null)
                {
                    if (CurrentFact.Name.Equals(ToItemName) == true)
                        ValidateEssenceAliasedFacts(FromItem, CurrentItem);
                }
            }
        }

        //-------------------------------------------------------------------------------
        // Validate the essence alias between two given facts.
        //-------------------------------------------------------------------------------
        private void ValidateEssenceAliasedFacts(Item FromItem, Item ToItem)
        {

            // Essence alias checks for c-equals items are a bit tricky, according to the
            // XBRL-CONF-CR3-2007-03-05 conformance suite. Test 392.11 says that it is valid
            // to have two items with contexts having the same structure but different
            // period values is valid; however, test 392.13 says that it is invalid two have
            // two items with contexts having a different structure.

            if (FromItem.ContextEquals(ToItem) == false)
            {
                if ((FromItem.ContextRef != null) && (ToItem.ContextRef != null))
                {
                    if (FromItem.ContextRef.PeriodTypeEquals(ToItem.ContextRef) == false)
                    {
                        StringBuilder MessageBuilder = new StringBuilder();
                        string StringFormat = AssemblyResources.GetName("EssenceAliasFactsNotContextEquals");
                        MessageBuilder.AppendFormat(StringFormat, FromItem.Name, ToItem.Name, FromItem.Id, ToItem.Id);
                        var validationError = new ItemsValidationError(MessageBuilder.ToString());
                        validationError.AddItem(FromItem);
                        validationError.AddItem(ToItem);
                        AddValidationError(validationError);
                        return;
                    }
                }
                return;
            }
            if (FromItem.ParentEquals(ToItem) == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("EssenceAliasFactsNotParentEquals");
                MessageBuilder.AppendFormat(StringFormat, FromItem.Name, ToItem.Name, FromItem.Id, ToItem.Id);
                var validationError = new ItemsValidationError(MessageBuilder.ToString());
                validationError.AddItem(FromItem);
                validationError.AddItem(ToItem);
                AddValidationError(validationError);
                return;
            }
            if (FromItem.UnitEquals(ToItem) == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("EssenceAliasFactsNotUnitEquals");
                MessageBuilder.AppendFormat(StringFormat, FromItem.Name, ToItem.Name, FromItem.Id, ToItem.Id);
                var validationError = new ItemsValidationError(MessageBuilder.ToString());
                validationError.AddItem(FromItem);
                validationError.AddItem(ToItem);
                AddValidationError(validationError);
                return;
            }

            // At this point, the valies of the items need to be compared. Check the item's type
            // to ensure that the correct value is being compared.

            var ItemValuesMatch = true;
            if (FromItem.SchemaElement.TypeName.Name.Equals("stringItemType") == true)
            {
                ItemValuesMatch = FromItem.Value.Equals(ToItem.Value);
            }
            else
            {
                if (FromItem.RoundedValue != ToItem.RoundedValue)
                    ItemValuesMatch = false;
            }
            if (ItemValuesMatch == false)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("EssenceAliasFactsHaveDifferentRoundedValues");
                MessageBuilder.AppendFormat(StringFormat, FromItem.Name, ToItem.Name, FromItem.Id, FromItem.RoundedValue.ToString(), ToItem.Id, ToItem.RoundedValue.ToString());
                var validationError = new ItemsValidationError(MessageBuilder.ToString());
                validationError.AddItem(FromItem);
                validationError.AddItem(ToItem);
                AddValidationError(validationError);
                return;
            }
        }

        //===============================================================================
        #endregion
        //===============================================================================

        //===============================================================================
        #region Footnote Validation
        //===============================================================================

        //-------------------------------------------------------------------------------
        // Validates a footnote arc.
        //
        // Validation is handled differently, depending on the arc's role. Note that
        // the XBRL specification discusses this:
        //
        // 4.11.1.3.1 @xlink:arcrole attributes on <footnoteArc> elements
        // The value of the @xlink:arcrole attribute MUST be a URI that indicates the
        // meaning of the arc. One standard arc role value has been defined for arc role
        // values on <footnoteArc> elements. Its value is:
        //
        // http://www.xbrl.org/2003/arcrole/fact-footnote
        //
        // This arc role value is for use on a <footnoteArc> from item or tuple Locators
        // to footnote resources and it indicates that the <footnote> conveys human-readable
        // information about the fact or facts.
        //
        // For more information, see the blog post at http://gepsio.wordpress.com/2014/07/09/better-validation-coming-for-footnote-arcs-and-arc-roles/.
        //-------------------------------------------------------------------------------
        private void ValidateFootnoteArc(FootnoteArc CurrentArc)
        {
            FootnoteLocator Locator = CurrentArc.Link.GetLocator(CurrentArc.From);
            if (Locator == null)
            {
                if (CurrentArc.StandardArcRole == true)
                {
                    StringBuilder MessageBuilder = new StringBuilder();
                    string StringFormat = AssemblyResources.GetName("CannotFindFootnoteLocator");
                    MessageBuilder.AppendFormat(StringFormat, CurrentArc.Title, CurrentArc.From);
                    AddValidationError(new FootnoteArcValidationError(CurrentArc, MessageBuilder.ToString()));
                    return;
                }
                var fromFootnote = CurrentArc.Link.GetFootnote(CurrentArc.From);
                if(fromFootnote == null)
                {
                    StringBuilder MessageBuilder = new StringBuilder();
                    string StringFormat = AssemblyResources.GetName("CannotFindFootnoteLocatorOrFootnote");
                    MessageBuilder.AppendFormat(StringFormat, CurrentArc.Title, CurrentArc.From);
                    AddValidationError(new FootnoteArcValidationError(CurrentArc, MessageBuilder.ToString()));
                    return;
                }
                CurrentArc.FromFootnote = fromFootnote;
            }
            else
            {
                if ((Locator.Href.UrlSpecified == true) && (UrlReferencesFragmentDocument(Locator.Href) == false))
                {
                    StringBuilder MessageBuilder = new StringBuilder();
                    string StringFormat = AssemblyResources.GetName("FootnoteReferencesFactInExternalDoc");
                    MessageBuilder.AppendFormat(StringFormat, Locator.Href.ElementId, Locator.Href.Url);
                    AddValidationError(new FootnoteArcValidationError(CurrentArc, MessageBuilder.ToString()));
                    return;
                }
                CurrentArc.FromItem = GetFact(Locator.Href.ElementId);
                if (CurrentArc.FromItem == null)
                {
                    StringBuilder MessageBuilder = new StringBuilder();
                    string StringFormat = AssemblyResources.GetName("CannotFindFactForFootnoteArc");
                    MessageBuilder.AppendFormat(StringFormat, CurrentArc.Title, Locator.Href);
                    AddValidationError(new FootnoteArcValidationError(CurrentArc, MessageBuilder.ToString()));
                    return;
                }
            }
            CurrentArc.ToFootnote = CurrentArc.Link.GetFootnote(CurrentArc.To);
            if (CurrentArc.ToFootnote == null)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("CannotFindFootnoteForFootnoteArc");
                MessageBuilder.AppendFormat(StringFormat, CurrentArc.Title, CurrentArc.To);
                AddValidationError(new FootnoteArcValidationError(CurrentArc, MessageBuilder.ToString()));
                return;
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ValidateFootnoteLocations()
        {
            foreach (FootnoteLink CurrentFootnoteLink in thisFootnoteLinks)
            {
                foreach (FootnoteLocator CurrentLocation in CurrentFootnoteLink.FootnoteLocators)
                    ValidateFootnoteLocation(CurrentLocation.Href.ElementId);   // TODO
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void ValidateFootnoteLocation(string FootnoteLocationReference)
        {
            HyperlinkReference Reference = new HyperlinkReference(FootnoteLocationReference);
            if (Reference.UrlSpecified == true)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("FootnoteReferencesFactInExternalDoc");
                MessageBuilder.AppendFormat(StringFormat, Reference.ElementId, Reference.Url);
                AddValidationError(new HyperlinkReferenceValidationError(Reference, MessageBuilder.ToString()));
                return;
            }
            if (GetFact(Reference.ElementId) == null)
            {
                StringBuilder MessageBuilder = new StringBuilder();
                string StringFormat = AssemblyResources.GetName("NoFactForFootnoteReference");
                MessageBuilder.AppendFormat(StringFormat, FootnoteLocationReference);
                AddValidationError(new HyperlinkReferenceValidationError(Reference, MessageBuilder.ToString()));
                return;
            }
        }

        //===============================================================================
        #endregion
        //===============================================================================

        //===============================================================================
        #region Summation Concept Validation
        //===============================================================================

        /// <summary>
        /// Validates all of the summation concepts in this fragment.
        /// </summary>
        private void ValidateSummationConcepts()
        {
            var validator = new SummationConceptValidator(this);
            validator.Validate();
        }

        //===============================================================================
        #endregion
        //===============================================================================

        //===============================================================================
        #region Location Support
        //===============================================================================

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private Item LocateFact(Locator FactLocator)
        {
            if (FactLocator == null)
                return null;
            foreach (Item CurrentFact in thisFacts)
            {
                if (CurrentFact.Name.Equals(FactLocator.HrefResourceId) == true)
                    return CurrentFact;
            }
            return null;
        }

        //===============================================================================
        #endregion
        //===============================================================================

    }
}
