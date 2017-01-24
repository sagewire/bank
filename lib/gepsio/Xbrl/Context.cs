using JeffFerguson.Gepsio.Xml.Interfaces;
using JeffFerguson.Gepsio.Xsd;
using System.Text;

namespace JeffFerguson.Gepsio
{
    /// <summary>
    /// An encapsulation of the XBRL element "context" as defined in the http://www.xbrl.org/2003/instance namespace. 
    /// </summary>
    public class Context
    {
        private INode thisContextNode;
        private INode thisInstantPeriodNode;
        private bool thisDurationPeriod;
        private INode thisStartDateDurationNode;
        private INode thisEndDateDurationNode;

        /// <summary>
        /// The ID of this context.
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Describes whether or not this context uses an instant period. Returns true if this context uses an instant
        /// period. Returns false is this context does not use an instant period.
        /// </summary>
        public bool InstantPeriod
        {
            get;
            private set;
        }

        /// <summary>
        /// Describes whether or not this context uses a duration period. Returns true if this context uses a duration
        /// period. Returns false is this context does not use a duration period.
        /// </summary>
        public bool DurationPeriod
        {
            get
            {
                if ((this.ForeverPeriod == true) || (thisDurationPeriod == true))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Describes whether or not this context uses a forever period. Returns true if this context uses a forever
        /// period. Returns false is this context does not use a forever period.
        /// </summary>
        public bool ForeverPeriod
        {
            get;
            private set;
        }

        /// <summary>
        /// The identifier for this context.
        /// </summary>
        public string Identifier
        {
            get;
            private set;
        }

        /// <summary>
        /// The identifier scheme for this context.
        /// </summary>
        public string IdentifierScheme
        {
            get;
            private set;
        }

        /// <summary>
        /// The segment node defined for this context. If this context was not marked up with a segment node, then
        /// this property will return null.
        /// </summary>
        public INode Segment
        {
            get;
            private set;
        }

        /// <summary>
        /// The scenario node defined for this context. If this context was not marked up with a scenario node, then
        /// this property will return null.
        /// </summary>
        internal INode Scenario
        {
            get;
            private set;
        }

        /// <summary>
        /// A reference to the <see cref="XbrlFragment"/> in which this context is found.
        /// </summary>
        public XbrlFragment Fragment
        {
            get;
            private set;
        }

        /// <summary>
        /// The start date of the period of this context. 
        /// </summary>
        /// <remarks>
        /// This value of this property should be considered valid only if this context uses a duration period.
        /// This can be checked using the context's <see cref="DurationPeriod"/> property:
        /// <code>
        /// var myDoc = new XbrlDocument();
        /// myDoc.Load("MyXbrlDoc.xml");
        /// foreach(var currentFragment in myDoc.Fragments)
        /// {
        ///     foreach(var currentContext in currentFragment.Contexts)
        ///     {
        ///         if(currentContext.DurationPeriod == true)
        ///         {
        ///             // value of currentContext.PeriodStartDate is valid
        ///         }
        ///         else
        ///         {
        ///             // value of currentContext.PeriodStartDate is undefined
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public System.DateTime PeriodStartDate
        {
            get;
            private set;
        }

        /// <summary>
        /// The end date of the period of this context. 
        /// </summary>
        /// <remarks>
        /// This value of this property should be considered valid only if this context uses a duration period.
        /// This can be checked using the context's <see cref="DurationPeriod"/> property:
        /// <code>
        /// var myDoc = new XbrlDocument();
        /// myDoc.Load("MyXbrlDoc.xml");
        /// foreach(var currentFragment in myDoc.Fragments)
        /// {
        ///     foreach(var currentContext in currentFragment.Contexts)
        ///     {
        ///         if(currentContext.DurationPeriod == true)
        ///         {
        ///             // value of currentContext.PeriodEndDate is valid
        ///         }
        ///         else
        ///         {
        ///             // value of currentContext.PeriodEndDate is undefined
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public System.DateTime PeriodEndDate
        {
            get;
            private set;
        }

        /// <summary>
        /// The date of the instant of this context. 
        /// </summary>
        /// <remarks>
        /// This value of this property should be considered valid only if this context uses an instant period.
        /// This can be checked using the context's <see cref="InstantPeriod"/> property:
        /// <code>
        /// var myDoc = new XbrlDocument();
        /// myDoc.Load("MyXbrlDoc.xml");
        /// foreach(var currentFragment in myDoc.Fragments)
        /// {
        ///     foreach(var currentContext in currentFragment.Contexts)
        ///     {
        ///         if(currentContext.InstantPeriod == true)
        ///         {
        ///             // value of currentContext.InstantDate is valid
        ///         }
        ///         else
        ///         {
        ///             // value of currentContext.InstantDate is undefined
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public System.DateTime InstantDate
        {
            get;
            private set;
        }

        internal Context(XbrlFragment Fragment, INode ContextNode)
        {
            this.Fragment = Fragment;
            thisContextNode = ContextNode;
            this.Id = thisContextNode.Attributes.FindAttribute("id").Value;
            this.PeriodStartDate = System.DateTime.MinValue;
            this.PeriodEndDate = System.DateTime.MinValue;
            foreach (INode CurrentChild in thisContextNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("period") == true)
                {
                    ProcessPeriod(CurrentChild);
                    ValidatePeriod();
                }
                else if (CurrentChild.LocalName.Equals("entity") == true)
                    ProcessEntity(CurrentChild);
                else if (CurrentChild.LocalName.Equals("scenario") == true)
                    ProcessScenario(CurrentChild);
            }
        }

        private void ProcessEntity(INode EntityNode)
        {
            this.Identifier = string.Empty;
            this.IdentifierScheme = string.Empty;
            this.Segment = null;
            this.Scenario = null;
            foreach (INode CurrentChild in EntityNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("identifier") == true)
                    ProcessIdentifier(CurrentChild);
                else if (CurrentChild.LocalName.Equals("segment") == true)
                    ProcessSegment(CurrentChild);

            }
        }

        private void ProcessScenario(INode ScenarioNode)
        {
            this.Scenario = ScenarioNode;
            ValidateScenario();
        }

        private void ValidateScenario()
        {
            foreach (INode CurrentChild in this.Scenario.ChildNodes)
                ValidateScenarioNode(CurrentChild);
        }

        private void ValidateScenarioNode(INode ScenarioNode)
        {
            if (ScenarioNode.NamespaceURI.Equals(XbrlDocument.XbrlNamespaceUri) == true)
            {
                string MessageFormat = AssemblyResources.GetName("ScenarioNodeUsingXBRLNamespace");
                StringBuilder MessageBuilder = new StringBuilder();
                MessageBuilder.AppendFormat(MessageFormat, this.Id, ScenarioNode.Name);
                this.Fragment.AddValidationError(new ContextValidationError(this, MessageBuilder.ToString()));
            }
            if (ScenarioNode.Prefix.Length > 0)
            {
                XbrlSchema NodeSchema = this.Fragment.GetXbrlSchemaForPrefix(ScenarioNode.Prefix);
                if (NodeSchema != null)
                {
                    Element NodeElement = NodeSchema.GetElement(ScenarioNode.LocalName);
                    if (NodeElement != null)
                    {
                        if (NodeElement.SubstitutionGroup != Element.ElementSubstitutionGroup.Unknown)
                        {
                            string MessageFormat = AssemblyResources.GetName("ScenarioNodeUsingSubGroupInXBRLNamespace");
                            StringBuilder MessageBuilder = new StringBuilder();
                            MessageBuilder.AppendFormat(MessageFormat, this.Id, ScenarioNode.Name, NodeSchema.Path);
                            this.Fragment.AddValidationError(new ContextValidationError(this, MessageBuilder.ToString()));
                        }
                    }
                }
            }
            foreach (INode CurrentChild in ScenarioNode.ChildNodes)
                ValidateScenarioNode(CurrentChild);
        }

        private void ProcessSegment(INode SegmentNode)
        {
            this.Segment = SegmentNode;
            ValidateSegment();
        }

        private void ValidateSegment()
        {
            foreach (INode CurrentChild in this.Segment.ChildNodes)
                ValidateSegmentNode(CurrentChild);
        }

        private void ValidateSegmentNode(INode SegmentNode)
        {
            ValidateSegmentNodeNamespace(SegmentNode);
            ValidateSegmentNodePrefix(SegmentNode);
            ValidateSegmentInnerText(SegmentNode);
            foreach (INode CurrentChild in SegmentNode.ChildNodes)
                ValidateSegmentNode(CurrentChild);
        }

        private void ValidateSegmentNodeNamespace(INode SegmentNode)
        {
            if (SegmentNode.NamespaceURI.Equals(XbrlDocument.XbrlNamespaceUri) == true)
            {
                string MessageFormat = AssemblyResources.GetName("SegmentNodeUsingXBRLNamespace");
                StringBuilder MessageBuilder = new StringBuilder();
                MessageBuilder.AppendFormat(MessageFormat, this.Id, SegmentNode.Name);
                this.Fragment.AddValidationError(new ContextValidationError(this, MessageBuilder.ToString()));
            }
        }

        private void ValidateSegmentNodePrefix(INode SegmentNode)
        {
            if (SegmentNode.Prefix.Length > 0)
            {
                XbrlSchema NodeSchema = this.Fragment.GetXbrlSchemaForPrefix(SegmentNode.Prefix);
                if (NodeSchema != null)
                {
                    Element NodeElement = NodeSchema.GetElement(SegmentNode.LocalName);
                    if (NodeElement != null)
                    {
                        if (NodeElement.SubstitutionGroup != Element.ElementSubstitutionGroup.Unknown)
                        {
                            string MessageFormat = AssemblyResources.GetName("SegmentNodeUsingSubGroupInXBRLNamespace");
                            StringBuilder MessageBuilder = new StringBuilder();
                            MessageBuilder.AppendFormat(MessageFormat, this.Id, SegmentNode.Name, NodeSchema.Path);
                            this.Fragment.AddValidationError(new ContextValidationError(this, MessageBuilder.ToString()));
                        }
                    }
                }
            }
        }

        private void ValidateSegmentInnerText(INode SegmentNode)
        {
            var text = SegmentNode.InnerText;
            if (string.IsNullOrEmpty(text) == true)
                return;
            if (this.Fragment.Schemas.Count == 0)
                return;
            var schema = this.Fragment.Schemas[0];
            var segmentNodeType = schema.GetNodeType(SegmentNode);
            if (segmentNodeType == null)
                return;
            if(segmentNodeType.CanConvert(text) == false)
            {
                string MessageFormat = AssemblyResources.GetName("SegmentTextNotConvertable");
                StringBuilder MessageBuilder = new StringBuilder();
                MessageBuilder.AppendFormat(MessageFormat, text);
                this.Fragment.AddValidationError(new ContextValidationError(this, MessageBuilder.ToString()));
            }
        }

        private void ProcessIdentifier(INode IdentifierNode)
        {
            this.Identifier = IdentifierNode.InnerText;
            if (IdentifierNode.Attributes["scheme"] != null)
                this.IdentifierScheme = IdentifierNode.Attributes["scheme"].Value;
        }

        private void ProcessPeriod(INode PeriodNode)
        {
            this.InstantPeriod = false;
            thisInstantPeriodNode = null;
            this.ForeverPeriod = false;
            thisDurationPeriod = false;
            thisStartDateDurationNode = null;
            thisEndDateDurationNode = null;
            foreach (INode CurrentChild in PeriodNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("instant") == true)
                {
                    this.InstantPeriod = true;
                    thisInstantPeriodNode = CurrentChild;
                    var parsedInstantDate = System.DateTime.MinValue;
                    System.DateTime.TryParse(thisInstantPeriodNode.InnerText, out parsedInstantDate);
                    this.InstantDate = parsedInstantDate;
                }
                else if (CurrentChild.LocalName.Equals("forever") == true)
                    this.ForeverPeriod = true;
                else if (CurrentChild.LocalName.Equals("startDate") == true)
                {
                    thisDurationPeriod = true;
                    thisStartDateDurationNode = CurrentChild;
                    var parsedStartDate = System.DateTime.MinValue;
                    System.DateTime.TryParse(thisStartDateDurationNode.InnerText, out parsedStartDate);
                    this.PeriodStartDate = parsedStartDate;
                }
                else if (CurrentChild.LocalName.Equals("endDate") == true)
                {
                    thisEndDateDurationNode = CurrentChild;
                    var parsedEndDate = System.DateTime.MinValue;
                    System.DateTime.TryParse(thisEndDateDurationNode.InnerText, out parsedEndDate);
                    this.PeriodEndDate = parsedEndDate;
                }
            }
        }

        private void ValidatePeriod()
        {
            if ((this.PeriodStartDate != System.DateTime.MinValue) && (this.PeriodEndDate != System.DateTime.MinValue))
            {
                if (this.PeriodEndDate < this.PeriodStartDate)
                {
                    string MessageFormat = AssemblyResources.GetName("PeriodEndDateLessThanPeriodStartDate");
                    StringBuilder MessageBuilder = new StringBuilder();
                    MessageBuilder.AppendFormat(MessageFormat, this.Id);
                    this.Fragment.AddValidationError(new ContextValidationError(this, MessageBuilder.ToString()));
                }
            }
        }

        //------------------------------------------------------------------------------------
        // Returns true if this context is Structure Equal (s-equal) to a supplied context,
        // and false otherwise. See section 4.10 of the XBRL 2.1 spec for more information.
        //------------------------------------------------------------------------------------
        internal bool StructureEquals(Context OtherContext)
        {
            if (PeriodStructureEquals(OtherContext) == false)
                return false;
            if (EntityStructureEquals(OtherContext) == false)
                return false;
            if (ScenarioStructureEquals(OtherContext) == false)
                return false;
            return true;
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool ScenarioStructureEquals(Context OtherContext)
        {
            if ((this.Scenario == null) && (OtherContext.Scenario == null))
                return true;
            if ((this.Scenario == null) && (OtherContext.Scenario != null))
                return true;
            if ((this.Scenario != null) && (OtherContext.Scenario == null))
                return true;
            return this.Scenario.StructureEquals(OtherContext.Scenario);
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool EntityStructureEquals(Context OtherContext)
        {
            if (this.Identifier.Equals(OtherContext.Identifier) == false)
                return false;
            if (SegmentStructureEquals(OtherContext) == false)
                return false;
            return true;
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool SegmentStructureEquals(Context OtherContext)
        {
            //--------------------------------------------------------------------------------
            // If neither context has a <segment> node, then the segments are considered
            // equal.
            //--------------------------------------------------------------------------------
            if ((this.Segment == null) && (OtherContext.Segment == null))
                return true;
            //--------------------------------------------------------------------------------
            // If this context does not have a <segment> node, but the other one does, then
            // the segments are equal only if the other <segment> is empty.
            //--------------------------------------------------------------------------------
            if ((this.Segment == null) && (OtherContext.Segment != null))
            {
                if (OtherContext.Segment.ChildNodes.Count > 0)
                    return false;
                return true;
            }
            //--------------------------------------------------------------------------------
            // If the other context does not have a <segment> node, but this one does, then
            // the segments are equal only if this <segment> is empty.
            //--------------------------------------------------------------------------------
            if ((this.Segment != null) && (OtherContext.Segment == null))
            {
                if (this.Segment.ChildNodes.Count > 0)
                    return false;
                return true;
            }
            //--------------------------------------------------------------------------------
            // At this point, both segments exist. Check to see if they have the same
            // structure.
            //--------------------------------------------------------------------------------
            return this.Segment.StructureEquals(OtherContext.Segment);
        }

        /// <summary>
        /// Compares the period type of this context with the period type of another context.
        /// </summary>
        /// <param name="OtherContext">
        /// The other context with which types should be compared.
        /// </param>
        /// <returns>
        /// True if the two contexts have the same period type; false otherwise.
        /// </returns>
        internal bool PeriodTypeEquals(Context OtherContext)
        {
            if (this.ForeverPeriod != OtherContext.ForeverPeriod)
                return false;
            if (this.InstantPeriod != OtherContext.InstantPeriod)
                return false;
            if (this.DurationPeriod != OtherContext.DurationPeriod)
                return false;
            return true;
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool PeriodStructureEquals(Context OtherContext)
        {
            if (PeriodTypeEquals(OtherContext) == false)
                return false;
            if (InstantPeriod == true)
            {
                if (this.InstantDate != OtherContext.InstantDate)
                    return false;
            }
            if (DurationPeriod == true)
            {
                if (this.PeriodStartDate != OtherContext.PeriodStartDate)
                    return false;
                if (this.PeriodEndDate != OtherContext.PeriodEndDate)
                    return false;
            }
            return true;
        }
    }
}
