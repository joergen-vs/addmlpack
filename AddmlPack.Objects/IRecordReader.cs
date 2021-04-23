using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects
{
    public interface IRecordReader
    {
        bool Next();
        List<string> Peek();
    }
}
