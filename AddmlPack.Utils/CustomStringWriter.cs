using System;
using System.IO;
using System.Text;

namespace AddmlPack.Utils
{
    public class CustomStringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }
}
