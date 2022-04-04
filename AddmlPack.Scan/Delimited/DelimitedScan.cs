using AddmlPack.FileTypes.Delimited;
using AddmlPack.Objects;
using AddmlPack.Standards.Addml.Classes.v8_3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AddmlPack.Scan.Delimited
{
    class DelimitedScan
    {
        private string Path { get; set; }
        private DelimitedReader Reader { get; set; }
        private DelimitedReader2 Reader2 { get; set; }
        private long ScanLimit { get; set; }
        private Dictionary<string, string[]> FieldFormats;

        public DelimitedScan(Project P)
        {
        }

        public DelimitedScan
        (
            string path,
            Encoding encoding,
            string recordSeparator,
            string fieldSeparator,
            string quotationText,
            Dictionary<string, string[]> formats,
            long scanLimit
        )
        {
            Reader = new DelimitedReader(path, encoding, recordSeparator, fieldSeparator, quotationText, 4 << 10);
            FieldFormats = formats;
        }

        public DelimitedScan
        (
            Stream stream,
            Encoding encoding,
            string recordSeparator,
            string fieldSeparator,
            string quotationText,
            Dictionary<string, string[]> formats,
            long scanLimit
        )
        {
            Reader = new DelimitedReader(stream, encoding, recordSeparator, fieldSeparator, quotationText, 4 << 10);
            FieldFormats = formats;
        }

        public string Scan(addml aml)
        {
            flatFiles FlatFiles = aml.dataset?[0].flatFiles;
            flatFile FlatFile = null;
            foreach (flatFile file in FlatFiles.flatFile)
            {
                // flatfile-path is stored in flatfile.property[fileName].value
                if (file.getProperty("fileName") != null)
                {
                    if (Path.Contains(file.getProperty("fileName").value))
                    {
                        FlatFile = file;
                        break;
                    }
                }
            }

            // File not found, returning
            if(FlatFile == null)
            {
                return "No such file among listed flatfiles.";
            }

            long recordCount = 0;

            // Read all records
            if (ScanLimit == -1)
            {
                while (Reader.Next())
                {
                    recordCount += 1;
                }
            }

            // Read number of records or all if a higher number is given
            else
            {
                bool EndOfFile = false;
                for (long i = 0; i < ScanLimit && !EndOfFile; i++)
                {
                    recordCount += 1;
                }
            }

            return null;
        }
    }
}
