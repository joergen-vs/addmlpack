using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AddmlPack.Objects;
using AddmlPack.Utils;

namespace AddmlPack.FileTypes.Text
{
    public class BasicReader : IReader
    {
        public string Path { get; set; }
        public Encoding Encoding { get; set; }
        public StreamReader Stream { get; set; }
        private int NumberOfCharsToRead { get; set; }
        public bool DoneReading { get; set; }
        //public char[] RawData { get; set; }
        public string Data { get; set; }

        public BasicReader(string path) : this(path, Encoding.UTF8) { }

        public BasicReader(string path, Encoding encoding)
        {
            Path = path;
            Encoding = encoding;

            Stream = new StreamReader(new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            ), Encoding);
        }

        public BasicReader(Stream stream) : this(stream, Encoding.UTF8) { }

        public BasicReader(Stream stream, Encoding encoding)
        {
            Path = null;
            Encoding = encoding;

            Stream = new StreamReader(stream, Encoding);
        }

        public void Reset()
        {
            if(Path == null)
            {
                Stream _stream = Stream.BaseStream;
                _stream.Seek(0, SeekOrigin.Begin);
                Stream = new StreamReader(_stream, Encoding);
            } else
            {
                Stream = new StreamReader(new FileStream(
                    Path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                ), Encoding);
            }
            DoneReading = false;
        }

        public List<string> Scan(int NumberOfRowsToScan)
        {
            // Not implemented for ordinary text-files
            return null;
        }

        public string Read(int numberOfCharsToRead, int bufferStart)
        {
            var buffer = "";
            if (Data != null)
                buffer = Data.Substring(bufferStart);

            NumberOfCharsToRead = numberOfCharsToRead;
            char[] RawData = new char[NumberOfCharsToRead];

            int status = Stream.Read(RawData, 0, NumberOfCharsToRead);
            if(status == 0)
                DoneReading = true;
            
            Data = buffer + new string(RawData).Replace("\0","");

            //Console.WriteLine($"Buffer = '{Buffer}'");
            //Console.WriteLine($"Data = '{Data}'");
            return null;
        }

        public string sub(int start, int end)
        {
            string s = Data.Substring(start, end - start);
            return s;
        }
    }
}
