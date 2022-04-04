using AddmlPack.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AddmlPack.FileTypes.Delimited
{
    public class DelimiterFileFormatReader : IRecordEnumerator
    {
        private readonly string _recordDelimiter;
        private readonly string _fieldDelimiter;
        private readonly string _quotingChar;
        private readonly int _identifierIndex;
        private readonly IEnumerator<string> _lines;
        public long RecordNumber { get; set; }

        public Record Current => GetCurrentRecord();

        object IEnumerator.Current => Current;

        private string _currentName;

        public DelimiterFileFormatReader(string recorddelimiter, string fielddelimiter, string quotingchar, int recordidentifier, StreamReader streamReader)
        {
            _recordDelimiter = recorddelimiter;
            _fieldDelimiter = fielddelimiter;
            _quotingChar = quotingchar;
            _identifierIndex = recordidentifier;

            _lines = new DelimiterFileRecordEnumerable(streamReader, _recordDelimiter, _quotingChar).GetEnumerator();
        }

        private static StreamReader GetStream(string filePath, string charset)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            FileStream fileStream = fileInfo.OpenRead();
            Encoding encoding = Encoding.GetEncoding(charset);
            StreamReader streamReader = new StreamReader(fileStream, encoding);
            return streamReader;
        }

        private Record GetCurrentRecord()
        {
            List<Field> fields = new List<Field>();

            string currentLine = _lines.Current;

            string[] strings = JoinQuotedValues(Split(currentLine));

            for (int i = 0; i < strings.Length; i++)
            {
                fields.Add(new Field("fd"+(i+1), strings[i]));
            }

            return new Record("rd"+(RecordNumber+1), _fieldDelimiter, RecordNumber, _identifierIndex, fields);
        }

        public void Dispose()
        {
            _lines.Dispose();
        }

        public bool MoveNext()
        {
            if (!_lines.MoveNext())
                return false;

            RecordNumber++;
            return true;
        }

        public void Reset()
        {
            _lines.Reset();
        }

        private IEnumerable<string> Split(string stringToSplit)
        {
            var strings = new List<string>();
            var buffer = "";
            var fdIndex = 0;

            foreach (char c in stringToSplit)
            {
                buffer += c;

                if (c == _fieldDelimiter[fdIndex])
                {
                    fdIndex++;

                    if (fdIndex != _fieldDelimiter.Length)
                        continue;

                    strings.Add(buffer[..^_fieldDelimiter.Length]);
                    fdIndex = 0;
                    buffer = "";
                }
                else
                    fdIndex = 0;
            }

            strings.Add(buffer);

            return strings;
        }

        private string[] JoinQuotedValues(IEnumerable<string> splitFieldValues)
        {
            if (_quotingChar == null)
                return splitFieldValues.ToArray();

            var fieldValues = new List<string>();
            var fieldValue = "";
            var concatenating = false;
            foreach (string splitFieldValue in splitFieldValues)
            {
                if (concatenating)
                {
                    fieldValue += _fieldDelimiter;

                    if (EndsWithOddNumberOfQuotingChars(splitFieldValue))
                    {
                        fieldValue += TrimQuotingCharFromEnd(splitFieldValue);
                        fieldValues.Add(fieldValue);
                        fieldValue = "";
                        concatenating = false;
                    }
                    else
                        fieldValue += splitFieldValue;

                }
                else
                {
                    if (splitFieldValue.StartsWith(_quotingChar))
                    {
                        if (EndsWithOddNumberOfQuotingChars(splitFieldValue))
                            fieldValues.Add(TrimQuotingChar(splitFieldValue));
                        else
                        {
                            fieldValue = TrimQuotingCharFromStart(splitFieldValue);
                            concatenating = true;
                        }
                    }
                    else
                        fieldValues.Add(splitFieldValue);
                }
            }

            return fieldValues.ToArray();
        }

        private string TrimQuotingChar(string valueWithQuotingChar)
        {
            return valueWithQuotingChar[_quotingChar.Length..^_quotingChar.Length];
        }

        private string TrimQuotingCharFromStart(string valueWithQuotingChar)
        {
            return valueWithQuotingChar[_quotingChar.Length..];
        }

        private string TrimQuotingCharFromEnd(string valueWithQuotingChar)
        {
            return valueWithQuotingChar[..^_quotingChar.Length];
        }

        private bool EndsWithOddNumberOfQuotingChars(string value)
        {
            var numberOfQuotingChars = 0;
            string copy = value;
            while (copy.EndsWith(_quotingChar))
            {
                numberOfQuotingChars++;
                copy = TrimQuotingCharFromEnd(copy);
            }

            return numberOfQuotingChars % 2 == 1;
        }
    }
}
