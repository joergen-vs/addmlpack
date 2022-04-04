using AddmlPack.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddmlPack.FileTypes.Delimited
{
    public class DelimitedReader2
    {
        public string RecordSeparator { get; set; }
        public string FieldSeparator { get; set; }
        public string QuotationText { get; set; }
        public int RecordIdentifierIndex { get; set; }
        public List<string> Record { get; set; }
        private StreamReader Stream;

        /**
         * Add default encoding (UTF-8)
         */
        public DelimitedReader2(string path) : this(path, Encoding.UTF8) { }
        
        /**
         * Add default record- (newline) and field-separator (semicolon)
         */
        public DelimitedReader2(string path, Encoding encoding) :
            this(path, encoding, Environment.NewLine, ";")
        { }

        /**
         * Add default record-identifierindex (one record per file)
         */
        public DelimitedReader2(string path, Encoding encoding, string recordSeparator, string fieldSeparator) :
            this(path, encoding, recordSeparator, fieldSeparator, -1)
        { }

        /**
         * Add default quotation-text (no quotation)
         */
        public DelimitedReader2(string path, Encoding encoding, string recordSeparator, string fieldSeparator, int recordIdentifierIndex) :
            this(path, encoding, recordSeparator, fieldSeparator, recordIdentifierIndex, null)
        { }

        public DelimitedReader2(string path, Encoding encoding, string recordSeparator, string fieldSeparator, int recordIdentifierIndex, string quotationText)
        {
            RecordSeparator = recordSeparator;
            FieldSeparator = fieldSeparator;
            QuotationText = quotationText;
            RecordIdentifierIndex = recordIdentifierIndex;

            Stream = GetStream(path, encoding);
        }

        public long Read()
        {
            long recordCounter = 0;
            IRecordEnumerator recordEnumerator = new DelimiterFileFormatReader(RecordSeparator, FieldSeparator, QuotationText, RecordIdentifierIndex, Stream);

            List<Record> records = new List<Record>();

            while (recordEnumerator != null)
            {
                if (!recordEnumerator.MoveNext())
                    break;

                Record record = recordEnumerator.Current;
                recordCounter++;

                // validate field-datatypes

                // check if record-definition exists

            }
            return recordCounter;
        }

        private static StreamReader GetStream(string filePath, Encoding encoding)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            FileStream fileStream = fileInfo.OpenRead();
            StreamReader streamReader = new StreamReader(fileStream, encoding);
            return streamReader;
        }
    }
}
