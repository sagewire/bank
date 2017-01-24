using System.Globalization;
using System.Collections.Generic;
using System.Text;
using JeffFerguson.Gepsio.Xml.Interfaces;
using JeffFerguson.Gepsio.Xsd;

namespace JeffFerguson.Gepsio
{
    /// <summary>
    /// A definition of a unit of measure used by numeric or fractional facts within the
    /// XBRL document. XBRL allows more complex units to be defined if necessary. Facts
    /// of a monetary nature must use a unit from the ISO 4217 namespace.
    /// </summary>
    public class Unit
    {
        private INode thisUnitNode;
        private List<QualifiedName> thisRatioNumeratorQualifiedNames;
        private List<QualifiedName> thisRatioDenominatorQualifiedNames;

        /// <summary>
        /// The ID of this unit.
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// A collection of <see cref="QualifiedName"/> objects representing the set of measure qualified names for this unit.
        /// </summary>
        public List<QualifiedName> MeasureQualifiedNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Region information for this unit.
        /// </summary>
        public RegionInfo RegionInformation
        {
            get;
            private set;
        }

        /// <summary>
        /// Culture information for this unit.
        /// </summary>
        public CultureInfo CultureInformation
        {
            get;
            private set;
        }

        /// <summary>
        /// Describes whether or not this unit represents a ratio. Returns true if this unit represents a ratio.
        /// Returns false if this unit does not represent a ratio.
        /// </summary>
        public bool Ratio
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="XbrlFragment"/> which contains the unit.
        /// </summary>
        public XbrlFragment Fragment
        {
            get;
            private set;
        }

        private INamespaceManager namespaceManager;

        internal Unit(XbrlFragment fragment, INode UnitNode, INamespaceManager namespaceManager)
        {
            this.Fragment = fragment;
            this.RegionInformation = null;
            thisUnitNode = UnitNode;
            this.Id = thisUnitNode.Attributes["id"].Value;
            this.MeasureQualifiedNames = new List<QualifiedName>();
            this.Ratio = false;
            thisRatioNumeratorQualifiedNames = new List<QualifiedName>();
            thisRatioDenominatorQualifiedNames = new List<QualifiedName>();
            this.namespaceManager = namespaceManager;
            foreach (INode CurrentChild in UnitNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("measure") == true)
                    this.MeasureQualifiedNames.Add(new QualifiedName(CurrentChild, namespaceManager));
                else if (CurrentChild.LocalName.Equals("divide") == true)
                {
                    ProcessDivideChildElement(CurrentChild);
                    CheckForMeasuresCommonToNumeratorsAndDenominators();
                    this.Ratio = true;
                }
            }
        }

        private void CheckForMeasuresCommonToNumeratorsAndDenominators()
        {
            foreach (QualifiedName CurrentNumeratorMeasure in thisRatioNumeratorQualifiedNames)
            {
                if (thisRatioDenominatorQualifiedNames.Contains(CurrentNumeratorMeasure) == true)
                {
                    string MessageFormat = AssemblyResources.GetName("UnitRatioUsesSameMeasureInNumeratorAndDenominator");
                    StringBuilder MessageFormatBuilder = new StringBuilder();
                    MessageFormatBuilder.AppendFormat(MessageFormat, this.Id, CurrentNumeratorMeasure.ToString());
                    this.Fragment.AddValidationError(new UnitValidationError(this, MessageFormatBuilder.ToString()));
                    return;
                }
            }
        }

        private void ProcessDivideChildElement(INode UnitDivideNode)
        {
            foreach (INode CurrentChild in UnitDivideNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("unitNumerator") == true)
                    ProcessUnitNumerators(CurrentChild);
                else if (CurrentChild.LocalName.Equals("unitDenominator") == true)
                    ProcessUnitDenominators(CurrentChild);
            }
        }

        private void ProcessUnitDenominators(INode UnitDivideDenominatorNode)
        {
            foreach (INode CurrentChild in UnitDivideDenominatorNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("measure") == true)
                    thisRatioDenominatorQualifiedNames.Add(new QualifiedName(CurrentChild, namespaceManager));
            }
        }

        private void ProcessUnitNumerators(INode UnitDivideNumeratorNode)
        {
            foreach (INode CurrentChild in UnitDivideNumeratorNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("measure") == true)
                    thisRatioNumeratorQualifiedNames.Add(new QualifiedName(CurrentChild, namespaceManager));
            }
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        internal void SetCultureAndRegionInfoFromISO4217Code(string Iso4217Code)
        {
            //--------------------------------------------------------------------------------
            // See if any obsolete ISO 4217 codes are being used and support those separately.
            //--------------------------------------------------------------------------------
            if (Iso4217Code.Equals("DEM") == true)
            {
                SetCultureAndRegionInfoFromRegionInfoName("DE");
                return;
            }
            //--------------------------------------------------------------------------------
            // Get a list of all cultures and find one whose region information specifies the
            // given ISO 4217 code as its currency symbol.
            //--------------------------------------------------------------------------------
            CultureInfo[] AllSpecificCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CurrentCultureInfo in AllSpecificCultures)
            {
                RegionInfo CurrentRegionInfo = new RegionInfo(CurrentCultureInfo.LCID);
                if (CurrentRegionInfo.ISOCurrencySymbol == Iso4217Code)
                {
                    this.CultureInformation = CurrentCultureInfo;
                    this.RegionInformation = CurrentRegionInfo;
                    return;
                }
            }
        }

        //------------------------------------------------------------------------------------
        // This method is a bit of a hack so that Gepsio passes unit test 304.24 in the
        // XBRL-CONF-CR3-2007-03-05 conformance suite. The XBRL document in 304.24 uses a unit
        // of measure called iso4217:DEM, which is an obsolete ISO 4217 currency code for the
        // German Mark. This has been replaced in favor of the Euro.
        //
        // This method searches for appropriate CultureInfo and RegionInfo settings given the
        // name of a region.
        //------------------------------------------------------------------------------------
        private void SetCultureAndRegionInfoFromRegionInfoName(string RegionInfoName)
        {
            CultureInfo[] AllSpecificCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CurrentCultureInfo in AllSpecificCultures)
            {
                RegionInfo CurrentRegionInfo = new RegionInfo(CurrentCultureInfo.LCID);
                if (CurrentRegionInfo.Name == RegionInfoName)
                {
                    this.CultureInformation = CurrentCultureInfo;
                    this.RegionInformation = CurrentRegionInfo;
                    return;
                }
            }
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        internal bool StructureEquals(Unit OtherUnit)
        {
            if (thisUnitNode == null)
                return false;
            if (OtherUnit.thisUnitNode == null)
                return false;
            if (thisUnitNode.StructureEquals(OtherUnit.thisUnitNode) == false)
                return false;
            if (this.Ratio == false)
                return NonRatioStructureEquals(OtherUnit);
            else
                return RatioStructureEquals(OtherUnit);
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool RatioStructureEquals(Unit OtherUnit)
        {
            if (QualifiedNameListsStructureEquals(thisRatioNumeratorQualifiedNames, OtherUnit.thisRatioNumeratorQualifiedNames) == false)
                return false;
            if (QualifiedNameListsStructureEquals(thisRatioDenominatorQualifiedNames, OtherUnit.thisRatioDenominatorQualifiedNames) == false)
                return false;
            return true;
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool NonRatioStructureEquals(Unit OtherUnit)
        {
            return QualifiedNameListsStructureEquals(MeasureQualifiedNames, OtherUnit.MeasureQualifiedNames);
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private bool QualifiedNameListsStructureEquals(List<QualifiedName> List1, List<QualifiedName> List2)
        {
            if (List1.Count != List2.Count)
                return false;
            foreach (QualifiedName CurrentQualifiedName in List1)
            {
                if (List2.Contains(CurrentQualifiedName) == false)
                    return false;
            }
            return true;
        }
    }
}
