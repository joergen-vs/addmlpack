using AddmlPack.Standards.Addml.Classes.v8_3;
using AddmlPack.Standards.Mets;
using System;
using System.IO;
using System.Xml.Serialization;

namespace AddmlPack.Utils.Mets
{
    public class MetsUtils
    {
        public static void AppendMetsInfo(addml aml, metsTypeMetsHdr metsHdr)
        {
            context _context = aml.dataset[0].reference?.context;

            if (_context == null)
            {
                if (aml.dataset == null || aml.dataset.Length == 0)
                {
                    aml.addDataset(GeneratorUtils.NewGUID());
                }
                if (aml.dataset[0].reference == null)
                {
                    aml.dataset[0].reference = new reference();
                }

                _context = aml.dataset[0].reference.context = new context();
            }

            additionalElement agentElements = _context.addElement("agents");
            additionalElement agentElement = null;

            foreach (metsTypeMetsHdrAgent agent in metsHdr.agent)
            {
                foreach (additionalElement element in agentElements.getElements("agent"))
                {
                    //if ()
                }

                if (agentElement == null)
                    agentElement = agentElements.addElement("agent");

                string type = agent.TYPE == metsTypeMetsHdrAgentTYPE.OTHER ?
                    agent.OTHERTYPE.ToString() : agent.TYPE.ToString();
                string role = agent.ROLE == metsTypeMetsHdrAgentROLE.OTHER ?
                    agent.OTHERROLE.ToString() : agent.ROLE.ToString();

                agentElement.addProperty("type").value = type;
                agentElement.addProperty("role").value = role;

                agentElement.value = agent.name;

                Console.WriteLine($"name='{agent.name}'");
                foreach (metsTypeMetsHdrAgentNote note in agent.note)
                    Console.WriteLine($"note='{note.Value}'");
            }
            return;
        }
        public static metsTypeMetsHdr getAgents(string path)
        {
            var metsHdr = readHeader(path);

            foreach (metsTypeMetsHdrAgent agent in metsHdr.agent)
            {
                string type = agent.TYPE == metsTypeMetsHdrAgentTYPE.OTHER ?
                    agent.OTHERTYPE.ToString() : agent.TYPE.ToString();
                string role = agent.ROLE == metsTypeMetsHdrAgentROLE.OTHER ?
                    agent.OTHERROLE.ToString() : agent.ROLE.ToString();

                System.Diagnostics.Debug.WriteLine($"name='{agent.name}', role='{role}', type='{type}'");
                foreach (metsTypeMetsHdrAgentNote note in agent.note)
                    System.Diagnostics.Debug.WriteLine($"note='{note.Value}'");
            }

            return metsHdr;
        }

        private static metsTypeMetsHdr readHeader(string path)
        {
            if (!File.Exists(path))
                Console.WriteLine($"File {path} doesnt exist.");

            var mySerializer = new XmlSerializer(typeof(mets));
            var metsObject = (mets)mySerializer.Deserialize(new StringReader(FromFile(path)));
            return metsObject.metsHdr;
            //return (mets)mySerializer.Deserialize(new StringReader(FromFile(path)));
        }

        public static string FromFile(string path)
        {
            int index, count;
            index = 0;
            count = 200; // or whatever number you think is better
            char[] buffer = new char[count];
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            System.IO.StreamReader sr = new System.IO.StreamReader(fs);

            string text = "";

            while (sr.Read(buffer, index, count) > 0)
            {
                text += new string(buffer);
                //int start = text.IndexOf("<metsHdr>") > -1 ?
                //    text.IndexOf("<metsHdr") :
                //    text.IndexOf("<mets:metsHdr");
                int end = text.IndexOf("</metsHdr>") > -1 ?
                     text.IndexOf("</metsHdr>") :
                     text.IndexOf("</mets:metsHdr>");


                //if (start > -1)
                //{
                //    text = text.Substring(start);
                //    start = -1;
                //}

                if (end > -1)
                {
                    text = text.Substring(0, end) + (text.IndexOf("</metsHdr>") > -1 ?
                     "</metsHdr></mets>" : "</mets:metsHdr></mets:mets>");

                    //text = text.Replace("<mets:", "<");
                    //text = text.Replace("</mets:", "</");
                    //text = text.Replace("<metsHdr ", "<metsHdr xmlns=\"http://www.loc.gov/METS/\" ");
                    //text = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + text;

                    System.Diagnostics.Debug.WriteLine(text);
                    return text;
                }
                else
                    System.Diagnostics.Debug.WriteLine(text);
            }

            return "";

        }
    }
}
