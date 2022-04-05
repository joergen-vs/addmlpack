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
        public bool hasHeader { get; set; }
        public Dictionary<string, Record> Records { get; set; }

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

            Records = new Dictionary<string, Record>();

            Stream = GetStream(path, encoding);
        }

        public long Read()
        {
            long recordCounter = 0;
            IRecordEnumerator recordEnumerator = new DelimiterFileFormatReader(RecordSeparator, FieldSeparator, QuotationText, RecordIdentifierIndex, Stream);



            // Find header
            while (hasHeader && recordEnumerator != null)
            {
                if (!recordEnumerator.MoveNext())
                    break;
                Record record = recordEnumerator.Current;
                Console.WriteLine($"{0}" + String.Join(",", record.Fields));
                Records["header"] = record;
                break;
            }

            recordEnumerator.Reset();

            while (recordEnumerator != null)
            {
                if (!recordEnumerator.MoveNext())
                    break;
                break;
            }

            // Find data
            while (recordEnumerator != null)
            {
                if (!recordEnumerator.MoveNext())
                    break;

                Record record = recordEnumerator.Current;
                recordCounter++;

                // To get som progress-output
                if (recordCounter % 10000 == 0)
                    Console.WriteLine(recordCounter);

                var recordKey = "";
                if (RecordIdentifierIndex != -1)
                    recordKey = record.Fields[RecordIdentifierIndex].Value;

                // check if record-definition exists
                if (Records.ContainsKey(recordKey))
                {
                    if(Records[recordKey].Fields.Count != record.Fields.Count)
                    {
                        Console.WriteLine($"Records are of different lengths, " +
                            $"expected {Records[recordKey].Fields.Count}, found {record.Fields.Count}");
                        return -1;
                    }

                    // validate field-datatypes
                    Records[recordKey].validate(record);

                    record = Records[recordKey];
                }
                else
                {
                    Records[recordKey] = record;

                    // validate field-datatypes
                    foreach (Field field in record.Fields)
                        field.validate();
                }
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
