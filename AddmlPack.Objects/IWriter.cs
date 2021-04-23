using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects
{
    public interface IWriter
    {
        void Write(string Data);
        void Flush();
        void Close();
    }
}
