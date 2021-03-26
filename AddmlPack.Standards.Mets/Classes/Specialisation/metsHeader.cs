using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Standards.Mets.Classes.Specialisation
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.loc.gov/METS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.loc.gov/METS/", IsNullable = false)]
    public partial class metsTypeMetsHdr
    {

        private metsTypeMetsHdrAgent[] agentField;

        private metsTypeMetsHdrAltRecordID[] altRecordIDField;

        private metsTypeMetsHdrMetsDocumentID metsDocumentIDField;

        private string idField;

        private string aDMIDField;

        private System.DateTime cREATEDATEField;

        private bool cREATEDATEFieldSpecified;

        private System.DateTime lASTMODDATEField;

        private bool lASTMODDATEFieldSpecified;

        private string rECORDSTATUSField;

        private System.Xml.XmlAttribute[] anyAttrField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("agent")]
        public metsTypeMetsHdrAgent[] agent
        {
            get
            {
                return this.agentField;
            }
            set
            {
                this.agentField = value;
            }
        }
    }
}
