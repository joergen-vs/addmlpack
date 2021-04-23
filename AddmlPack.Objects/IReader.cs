using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AddmlPack.Objects
{
    public interface IReader
    {
        string Path { get; set; }
        Encoding Encoding { get; set; }
        StreamReader Stream { get; set; }
        string Data { get; set; }
        string Read(int NumberOfCharsToRead, int bufferStart);
    }
}
