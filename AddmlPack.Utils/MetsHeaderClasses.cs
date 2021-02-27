using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace AddmlPack.Utils
{
	[XmlRoot(ElementName = "agent")]
	public class Agent
	{
		[XmlElement(ElementName = "name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "ROLE")]
		public string ROLE { get; set; }
		[XmlAttribute(AttributeName = "TYPE")]
		public string TYPE { get; set; }
		[XmlElement(ElementName = "note")]
		public List<string> Note { get; set; }
		[XmlAttribute(AttributeName = "OTHERROLE")]
		public string OTHERROLE { get; set; }
		[XmlAttribute(AttributeName = "OTHERTYPE")]
		public string OTHERTYPE { get; set; }
	}

	[XmlRoot(ElementName = "altRecordID")]
	public class AltRecordID
	{
		[XmlAttribute(AttributeName = "TYPE")]
		public string TYPE { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "metsHdr")]
	public class MetsHdr
	{
		[XmlElement(ElementName = "agent")]
		public List<Agent> Agent { get; set; }
		[XmlElement(ElementName = "altRecordID")]
		public List<AltRecordID> AltRecordID { get; set; }
		[XmlAttribute(AttributeName = "CREATEDATE")]
		public string CREATEDATE { get; set; }
	}
}
