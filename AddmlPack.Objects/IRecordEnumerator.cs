using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddmlPack.Objects
{
    public interface IRecordEnumerator : IEnumerator<Record>
    {
        public long RecordNumber { get; set; }
    }
}
