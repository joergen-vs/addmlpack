using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddmlPack.Objects
{
    public class Record
    {
        public string Name { get; set; }
        public string RecordIdentifierValue { get; set; }
        public string FieldSeparator { get; set; }
        public List<Field> Fields { get; }
        public long LineNumber { get; }
        public long LineCount { get; set; }
        public string Value
        {
            get
            {
                return string.Join(FieldSeparator ?? "", Fields.Select(f => f.Value));
            }
        }

        public Record(string name, string fieldSeparator, long lineNumber, int recordIdentifierIndex, List<Field> fields)
        {
            Name = name;
            FieldSeparator = fieldSeparator;
            Fields = fields;
            LineNumber = lineNumber;
            LineCount = 1;

            if (recordIdentifierIndex>0 && recordIdentifierIndex - 1 < Fields.Count)
                RecordIdentifierValue = Fields[recordIdentifierIndex - 1].Value;
            else
                RecordIdentifierValue = null;
        }

        public Record(string name, string fieldSeparator, long lineNumber, int recordIdentifierIndex, params Field[] fields)
        {
            Name=name;
            FieldSeparator = fieldSeparator;
            Fields = fields.ToList();
            LineNumber = lineNumber;
            LineCount = 1;

            if (recordIdentifierIndex > 0 && recordIdentifierIndex - 1 < Fields.Count)
                RecordIdentifierValue = Fields[recordIdentifierIndex - 1].Value;
            else
                RecordIdentifierValue = null;
        }

        public void validate(Record other)
        {
            for(int i=0; i<Fields.Count; i++)
            {
                Fields[i].validate(other.Fields[i].Value);
            }
            LineCount += 1;
        }
    }
}
