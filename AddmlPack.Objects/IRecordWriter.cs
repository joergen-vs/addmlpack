using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects
{
    public interface IRecordWriter
    {
        List<string> WriteRecord(List<string> fieldValues);
        void Flush();
        void Close();
    }
}
