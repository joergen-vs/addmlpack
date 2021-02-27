using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace AddmlPack.Utils
{
    public class MetsUtils
    {
        public static MetsHdr getAgents(string path)
        {
            return readHeader(path);
        }

        private static MetsHdr readHeader(string path)
        {
            if (!File.Exists(path))
                Console.WriteLine($"File {path} doesnt exist.");

            var mySerializer = new XmlSerializer(typeof(MetsHdr));
            var myFileStream = new FileStream(path, FileMode.Open);
            return (MetsHdr)mySerializer.Deserialize(new StringReader(FromFile(path)));
        }

        public static string FromFile(string path)
        {
            int index, count;
            index = 0;
            count = 200; // or whatever number you think is better
            char[] buffer = new char[count];
            System.IO.StreamReader sr = new System.IO.StreamReader(path);

            string text = "";

            while (sr.Read(buffer, index, count) > 0)
            {
                text += new string(buffer);
                int start = text.IndexOf("<metsHdr>") > -1 ?
                    text.IndexOf("<metsHdr>") :
                    text.IndexOf("<mets:metsHdr>");
                int end = text.IndexOf("</metsHdr>") > -1 ?
                     text.IndexOf("</metsHdr>") :
                     text.IndexOf("</mets:metsHdr>");

                if(start > -1)
                {
                    text = text.Substring(start);
                }

                if(end > -1)
                {
                    text = text.Substring(0, end);
                    return text;
                }
            }

            return "";

        }
    }
}
