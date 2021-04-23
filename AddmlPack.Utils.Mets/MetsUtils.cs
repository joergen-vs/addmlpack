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

            additionalElement agentElements = null;
            if (!_context.hasElement("agents"))
                agentElements = _context.addElement("agents");

            foreach (metsTypeMetsHdrAgent agent in metsHdr.agent)
            {
                additionalElement agentElement = null;
                string roleType = agent.TYPE == metsTypeMetsHdrAgentTYPE.OTHER ?
                    agent.OTHERTYPE.ToString() : agent.TYPE.ToString();
                string role = agent.ROLE == metsTypeMetsHdrAgentROLE.OTHER ?
                    agent.OTHERROLE.ToString() : agent.ROLE.ToString();

                foreach (additionalElement element in agentElements.getElements("agent"))
                {
                    if (element.getProperty("role").value == role &&
                        element.getProperty("type").value == roleType &&
                        element.getElement("name").value == agent.name)
                    {
                        agentElement = element;
                        break;
                    }
                }

                if (agentElement != null)
                {
                    Console.WriteLine($"name='{agent.name}'");
                    if (agent.note != null)
                        foreach (metsTypeMetsHdrAgentNote note in agent.note)
                            Console.WriteLine($"note='{note.Value}'");
                }
                else
                {
                    agentElement = agentElements.addElement("agent");

                    agentElement.addElement("name").value = agent.name;
                    agentElement.addProperty("type").value = roleType;
                    agentElement.addProperty("role").value = role;
                }

                if (agent.note != null)
                {
                    string notescontent = agent.note[agent.note.Length - 1].Value;
                    if (notescontent.StartsWith("notescontent:"))
                    {
                        var noteTypes = notescontent
                            .Replace("notescontent:", "")
                            .Split(',');

                        if (agentElement.hasElement("contact"))
                        {
                            for (int i = 0; i < noteTypes.Length; i++)
                            {
                                bool exists = false;
                                foreach (additionalElement contact in agentElement.getElements("contact"))
                                {

                                    if (contact.getProperty("type").value == noteTypes[i] &&
                                        contact.value == agent.note[i].Value)
                                    { exists = true; break; }
                                }

                                if (!exists)
                                {
                                    agentElement.addElement("contact", agent.note[i].Value)
                                        .addProperty("type").value = noteTypes[i];
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < noteTypes.Length; i++)
                            {
                                agentElement.addElement("contact", agent.note[i].Value)
                                    .addProperty("type").value = noteTypes[i];
                            }
                        }

                    }
                }

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
                if (agent.note != null)
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
