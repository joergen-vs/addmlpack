using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AddmlPack.Objects;
using AddmlPack.Utils;

namespace AddmlPack.FileTypes.Text
{
    public class BasicWriter : IWriter
    {
        public string Path { get; set; }
        public Encoding Encoding { get; set; }
        private StreamWriter streamWriter { get; set; }

        public BasicWriter(string path) : this(path, "UTF-8") { }

        public BasicWriter(string path, string encoding)
        {
            Path = path;
            Encoding = TextUtils.GetEncoding(encoding);

            streamWriter = new StreamWriter(new FileStream(
                path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            ), Encoding);
        }

        public void Write(string Data)
        {
            streamWriter.Write(Data);
        }

        public void Flush()
        {
            streamWriter.Flush();
        }

        public void Close()
        {
            streamWriter.Close();
        }
    }
}
