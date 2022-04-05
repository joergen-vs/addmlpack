using AddmlPack.FileTypes.Delimited;
using AddmlPack.Objects;
using AddmlPack.Standards.Addml.Classes.v8_3;
using AddmlPack.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AddmlPack.Scan.Delimited
{
    class DelimitedScan
    {
        private string Path { get; set; }
        private Project P { get; set; }
        private DelimitedReader2 Reader { get; set; }
        private long ScanLimit { get; set; }
        private Dictionary<string, string[]> FieldFormats;

        public DelimitedScan(string path, Project P) : this(
            path,
            Encoding.GetEncoding(P.Encoding),
            P.RecordSeparator,
            P.FieldSeparator,
            P.QuotationText,
            P.FieldFormats)
        { this.P = P; }

        public DelimitedScan
        (
            string path,
            Encoding encoding,
            string recordSeparator,
            string fieldSeparator,
            string quotationText,
            Dictionary<string, string[]> formats
        )
        {
            Path = path;
            Reader = new DelimitedReader2(path, encoding, recordSeparator, fieldSeparator, -1, quotationText);
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

            // Read all records

            long recordCount = Reader.Read();

            if(recordCount < 0)
            {
                // Some error..
                return "";
            }

            FlatFile.addProperty("numberOfOccurrences").value = $"{recordCount}";

            string ffdRef = GeneratorUtils.NewGUID();
            string fftRef = GeneratorUtils.NewGUID();

            var ffd = FlatFiles.addFlatFileDefinition(ffdRef, fftRef);

            var fft = FlatFiles.addFlatFileType(fftRef);
            fft.charset = P.Encoding;
            delimFileFormat _ = new delimFileFormat();
            _.fieldSeparatingChar = P.FieldSeparator;
            _.recordSeparator = P.RecordSeparator;
            _.quotingChar = P.QuotationText;
            fft.Item = _;

            return null;
        }
    }
}
