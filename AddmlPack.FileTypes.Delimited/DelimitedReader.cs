using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AddmlPack.FileTypes.Text;
using AddmlPack.Objects;

namespace AddmlPack.FileTypes.Delimited
{
    public class DelimitedReader
    {
        public string RecordSeparator { get; set; }
        public string FieldSeparator { get; set; }
        public string QuotationText { get; set; }
        private int MaxSeparatorLength { get; set; }
        public BasicReader Reader { get; set; }
        private int BufferSize { get; set; }
        public int Start { get; set; }
        public List<string> Record { get; set; }

        public DelimitedReader(string path, int buffer) : this(path, Encoding.UTF8, buffer) { }

        public DelimitedReader(string path, Encoding encoding, int buffer) :
            this(path, encoding, Environment.NewLine, ";", buffer)
        { }

        public DelimitedReader(string path, Encoding encoding, string recordSeparator, string fieldSeparator, int buffer) :
            this(path, encoding, recordSeparator, fieldSeparator, null, buffer)
        { }

        public DelimitedReader(string path, Encoding encoding, string recordSeparator, string fieldSeparator, string quotationText, int buffer)
        {
            RecordSeparator = recordSeparator;
            FieldSeparator = fieldSeparator;
            QuotationText = quotationText;

            Start = 0;
            BufferSize = buffer;
            Record = new List<string>();

            MaxSeparatorLength = 0;
            if (RecordSeparator.Length > MaxSeparatorLength)
                MaxSeparatorLength = RecordSeparator.Length;
            if (FieldSeparator.Length > MaxSeparatorLength)
                MaxSeparatorLength = FieldSeparator.Length;
            if (QuotationText != null &&
                QuotationText.Length > MaxSeparatorLength)
                MaxSeparatorLength = QuotationText.Length;

            Reader = new BasicReader(path, encoding);
        }

        public DelimitedReader(Stream stream, int buffer) : this(stream, Encoding.UTF8, buffer)
        { }

        public DelimitedReader(Stream stream, Encoding encoding, int buffer) :
            this(stream, encoding, Environment.NewLine, ";", buffer)
        { }

        public DelimitedReader(Stream stream, Encoding encoding, string recordSeparator, string fieldSeparator, int buffer) :
            this(stream, encoding, recordSeparator, fieldSeparator, null, buffer)
        { }

        public DelimitedReader(Stream stream, Encoding encoding, string recordSeparator, string fieldSeparator, string quotationText, int buffer)
        {
            RecordSeparator = recordSeparator;
            FieldSeparator = fieldSeparator;
            QuotationText = quotationText;

            Start = 0;
            BufferSize = buffer;
            Record = new List<string>();

            MaxSeparatorLength = 0;
            if (RecordSeparator.Length > MaxSeparatorLength)
                MaxSeparatorLength = RecordSeparator.Length;
            if (FieldSeparator.Length > MaxSeparatorLength)
                MaxSeparatorLength = FieldSeparator.Length;
            if (QuotationText != null &&
                QuotationText.Length > MaxSeparatorLength)
                MaxSeparatorLength = QuotationText.Length;

            Reader = new BasicReader(stream, encoding);
        }

        public bool Next()
        {
            Record.Clear();

            bool insideQuote = false;
            int fieldQuotes = 0;
            int FieldStart = Start;

            if (Reader.Data == null)
            {
                Fill(0);
            }

            while (!Reader.DoneReading && Start < Reader.Data.Length)
            {
                for (int i = Start + 1; i < Reader.Data.Length; i++)
                {
                    //Console.WriteLine($"Building record, {ToString(Record)} <= Reader.Data.Substring({FieldStart + fieldQuotes}={FieldStart} + {fieldQuotes},{i - FieldStart - fieldQuotes * 2}={i} - {FieldStart} - {fieldQuotes * 2}) = '{Reader.Data.Substring(FieldStart + fieldQuotes,i - FieldStart - fieldQuotes * 2)}'");
                    
                    // Found quotation
                    if (QuotationText != null &&
                        foundSeparator(QuotationText, i))
                    {
                        insideQuote = !insideQuote;
                        fieldQuotes = QuotationText.Length;
                    }
                    else

                    // Found Record-separator
                    if (!insideQuote &&
                        foundSeparator(RecordSeparator, i))
                    {
                        Record.Add(Reader.Data.Substring(
                            FieldStart + fieldQuotes,
                            i - FieldStart - fieldQuotes*2 - RecordSeparator.Length
                        ));

                        Start = i;
                        return true;
                    }
                    else

                    // Found Field-separator
                    if (!insideQuote &&
                        foundSeparator(FieldSeparator,i))
                    {
                        Record.Add(Reader.Data.Substring(
                            FieldStart + fieldQuotes,
                            i - FieldStart - fieldQuotes * 2 - FieldSeparator.Length
                        ));

                        fieldQuotes = 0;
                        FieldStart = i;
                    }
                }

                fieldQuotes = 0;
                Start = MaxSeparatorLength;
                Fill(FieldStart);
                FieldStart = MaxSeparatorLength;
                insideQuote = false;

                if (Reader.Data.Length >= 10 * BufferSize)
                {
                    return false;
                }
            }

            try
            {
                Record.Add(Reader.Data.Substring(
                FieldStart + fieldQuotes,
                Reader.Data.Length - FieldStart - fieldQuotes * 2 - 1
            ));
            } catch (Exception)
            {
                Console.WriteLine($"Error\r\n" +
                    $"Reader.Data.Length={Reader.Data.Length}\r\n" +
                    $"Reader.Data.Substring(" +
                    $"{FieldStart + fieldQuotes}," +
                    $"{Reader.Data.Length - FieldStart - fieldQuotes * 2 - 1})");
            }

            return false;
        }

        private void Fill(int bufferStart)
        {
            //Console.WriteLine($"Fill({(bufferStart - MaxSeparatorLength >= 0 ? bufferStart - MaxSeparatorLength : 0)})");
            Reader.Read(BufferSize,
                bufferStart - MaxSeparatorLength>=0 ?
                bufferStart - MaxSeparatorLength : 0);
        }

        public List<string> Peek()
        {
            int _Start = Start;
            Next();
            Start = _Start;

            return Record;
        }

        public int CountRecords()
        {
            int NumberOfRecords = 0;
            bool insideQuote = false;

            if (Reader.Data == null)
            {
                Fill(0);
            }

            while (!Reader.DoneReading && Start < Reader.Data.Length)
            {
                for (int i = Start + 1; i < Reader.Data.Length; i++)
                {
                    //Console.WriteLine($"Building record, {ToString(Record)} <= Reader.Data.Substring({FieldStart + fieldQuotes}={FieldStart} + {fieldQuotes},{i - FieldStart - fieldQuotes * 2}={i} - {FieldStart} - {fieldQuotes * 2}) = '{Reader.Data.Substring(FieldStart + fieldQuotes,i - FieldStart - fieldQuotes * 2)}'");

                    // Found quotation
                    if (QuotationText != null &&
                        foundSeparator(QuotationText, i))
                    {
                        insideQuote = !insideQuote;
                    }
                    else

                    // Found Record-separator
                    if (!insideQuote &&
                        foundSeparator(RecordSeparator, i))
                    {
                        NumberOfRecords += 1;

                        Start = i;
                    }
                }

                Fill(Start);
                Start = MaxSeparatorLength;
                insideQuote = false;

                if (Reader.Data.Length >= 10 * BufferSize)
                {
                    return -1;
                }
            }

            return NumberOfRecords;
        }

        public void Reset()
        {
            Start = 0;
            Reader.Reset();
        }

        private bool foundSeparator(string separator, int position)
        {
            if (separator.Length + position >= Reader.Data.Length ||
                position - separator.Length < 0)
                return false;

            for(int i=0; i<separator.Length; i++)
            {
                if (Reader.Data[position - separator.Length + i] != separator[i])
                    return false;
            }

            return true;
        }

        private string ToString(List<string> record)
        {
            if (record.Count == 0)
                return "( )";
            string res = "('" + record[0] + "'";
            for (int i = 1; i < record.Count; i++)
                res += ", '" + record[i] + "'";
            return res + ")";
        }
    }
}
