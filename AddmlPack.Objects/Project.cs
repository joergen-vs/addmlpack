using AddmlPack.Standards.Addml.Classes.v8_3;
using AddmlPack.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace AddmlPack.Objects
{
    public class Project
    {
        private addml _aml { get; set; }
        public addml Addml { get { return _aml; } set { _aml = value; } }
        private Timing timing { get; set; }
        public static List<string> types { get; set; }
        private Dictionary<string, object> _parameters { get; set; }
        public Dictionary<string, object> Parameters { get { return _parameters; } }
        public string Process
        {
            get { return (string)_parameters["process"]; }
            set { _parameters["process"] = value; }
        }
        private List<string> implementedProcesses { get; set; }
        public string type
        {
            get { return (string)_parameters["type"]; }
            set { _parameters["type"] = value; }
        }
        public bool Help { get; set; }
        public string Input
        {
            get
            {

                if (_parameters.ContainsKey("input"))
                    return (string)_parameters["input"];
                else
                    return null;
            }
            set { _parameters["input"] = value; }
        }
        //public FileInfo inputFile
        //{
        //    get
        //    {
        //        if (_parameters.ContainsKey("inputFile"))
        //            return (FileInfo)_parameters["inputFile"];
        //        return null;
        //    }
        //    set { 
        //        _parameters["inputFile"] = value;
        //        _parameters["input"] = value;
        //        _parameters["inputDirectory"] = null;
        //    }
        //}
        //public DirectoryInfo inputDirectory
        //{
        //    get
        //    {
        //        if (_parameters.ContainsKey("inputDirectory"))
        //            return (DirectoryInfo)_parameters["inputDirectory"];
        //        return null;
        //    }
        //    set { _parameters["inputDirectory"] = value; }
        //}
        public string Output
        {
            get { return (string)_parameters["output"]; }
            set { _parameters["output"] = value; }
        }
        public string Extension
        {
            get { return (string)_parameters["extension"]; }
            set { _parameters["extension"] = value; }
        }
        public bool recursiveSearch
        {
            get { return (bool)_parameters["recursive"]; }
            set { _parameters["recursive"] = value; }
        }
        public string Language
        {
            get { return (string)_parameters["lang"]; }
            set { _parameters["lang"] = value; }
        }
        public string RecordSeparator
        {
            get { return (string)_parameters["recordSeparator"]; }
            set { _parameters["recordSeparator"] = value; }
        }
        public string FieldSeparator
        {
            get { return (string)_parameters["fieldSeparator"]; }
            set { _parameters["fieldSeparator"] = value; }
        }
        public string QuotationText
        {
            get { return (string)_parameters["quotationChars"]; }
            set { _parameters["quotationChars"] = value; }
        }
        public string Encoding
        {
            get { return (string)_parameters["encoding"]; }
            set { _parameters["encoding"] = value; }
        }
        public string id
        {
            get { return (string)_parameters["id"]; }
            set { _parameters["id"] = value; }
        }
        public DirectoryInfo documents
        {
            get { return (DirectoryInfo)_parameters["doc"]; }
            set { _parameters["doc"] = value; }
        }
        private CustomOptions structuredOptions
        {
            get
            {
                return (CustomOptions)Get("CustomOptions");
            }
            set
            {
                _parameters["CustomOptions"] = value;
            }
        }
        public Dictionary<string, string[]> FieldFormats
        {
            get { return (Dictionary<string, string[]>)structuredOptions.formats; }
        }
        public Dictionary<string, string[]> ProcessesToAppend
        {
            get { return (Dictionary<string, string[]>)structuredOptions.processes; }
        }
        public int ScanDepth
        {
            get { return (int)_parameters["scanDepth"]; }
            set { _parameters["scanDepth"] = value; }
        }
        private string[] _files { get; set; }
        public string[] Files
        {
            get
            {
                if (_files == null)
                {
                    if (File.Exists(Input))
                    {
                        _files = new string[] { Input };
                        timing.archiveSize += new FileInfo(_files[0]).Length;
                    }
                    else
                    {
                        Console.WriteLine("Scanning folder {0} for files", Input);
                        _files = Directory.GetFiles(
                            Input,
                            "*." + Extension,
                            recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                            );

                        for (int i = 0; i < _files.Length; i++)
                            timing.archiveSize += new FileInfo(_files[i]).Length;
                    }
                }

                return _files;
            }
        }
        public long archivePosts { get; set; }

        public Project()
        {
            timing = new Timing();
            Addml = new addml();
            archivePosts = 0;
            types = new List<string>() { "delimited" };
            implementedProcesses = new List<string>() { "convertDescription", "help" };
            _parameters = new Dictionary<string, object>();
        }

        public Project(List<string> args) : this()
        {
            timing = new Timing();
            Addml = new addml();
            archivePosts = 0;
            Create(args);
        }

        public object Get(string key)
        {
            if (_parameters.ContainsKey(key))
                return _parameters[key];
            else
                return null;
        }

        public string GetString(string key)
        {
            if (_parameters.ContainsKey(key))
                return (string)_parameters[key];
            else
                return null;
        }

        public void Start()
        {
            timing.Start();
        }

        public void Stop(long recordsRead)
        {
            timing.Stop();
            timing.archivePosts = recordsRead;
            Console.WriteLine("Time taken: {0}", timing.Elapsed());
            Console.WriteLine("{0}", timing.RecordsPerSecond());
            Console.WriteLine("{0}", timing.SizePerSecond());
        }

        private void Create(List<string> arguments)
        {
            // Check for keywords

            // Type of run
            if (arguments.Count > 0)
            {
                Process = arguments[0];
            }
            else
            {
                Process = null;
                return;
            }

            // Type of run
            int indexOfKeyword = arguments.IndexOf("-t") >= 0 ? arguments.IndexOf("-t") : arguments.IndexOf("--type");
            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                type = arguments[indexOfKeyword + 1];
            }
            else
            {
                type = null;
            }

            // Help?
            indexOfKeyword = arguments.IndexOf("-h") >= 0 ? arguments.IndexOf("-h") : arguments.IndexOf("--help");
            if (indexOfKeyword >= 0)
            {
                Help = true;
            }

            // Input-folder or file
            indexOfKeyword = arguments.IndexOf("-i") >= 0 ? arguments.IndexOf("-i") : arguments.IndexOf("--input");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                if (File.Exists(arguments[indexOfKeyword + 1]) ||
                    Directory.Exists(arguments[indexOfKeyword + 1]))
                {
                    Input = arguments[indexOfKeyword + 1].Replace("\\", "/");
                }
                else
                {
                    Console.WriteLine($"Error on 'input': No such file or directory.");
                }
            }

            // Output
            indexOfKeyword = arguments.IndexOf("-o") >= 0 ? arguments.IndexOf("-o") : arguments.IndexOf("--output");
            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                Output = arguments[indexOfKeyword + 1].Replace("\\", "/");
            }

            // Addml project-file
            indexOfKeyword = arguments.IndexOf("-a") >= 0 ? arguments.IndexOf("-a") : arguments.IndexOf("--addml");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                if (File.Exists(arguments[indexOfKeyword + 1]))
                {
                    Parameters["Addml"] = arguments[indexOfKeyword + 1].Replace("\\", "/");
                }
                else
                {
                    Console.WriteLine($"Error on 'addml project-file': No such file.");
                }
            }


            // Recursive search
            indexOfKeyword = arguments.IndexOf("-r") >= 0 ? arguments.IndexOf("-r") : arguments.IndexOf("--recursive");

            recursiveSearch = indexOfKeyword >= 0;

            // Extension to search for
            indexOfKeyword = arguments.IndexOf("-x") >= 0 ? arguments.IndexOf("-x") : arguments.IndexOf("--extension");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                Extension = arguments[indexOfKeyword + 1];
            }

            // Extension to search for
            indexOfKeyword = arguments.IndexOf("-l") >= 0 ? arguments.IndexOf("-l") : arguments.IndexOf("--lang");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                Language = arguments[indexOfKeyword + 1];
            }
            else
            {
                Language = Thread.CurrentThread.CurrentCulture.Name;
            }

            // Record-separator for files
            indexOfKeyword = arguments.IndexOf("-rs") >= 0 ? arguments.IndexOf("-rs") : arguments.IndexOf("--recordSeparator");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                RecordSeparator = arguments[indexOfKeyword + 1];
                RecordSeparator = TextUtils.toSeparator(RecordSeparator);
            }
            else
            {
                // System-spesific newline
                RecordSeparator = TextUtils.toSeparator(Environment.NewLine);
            }

            // Field-separator for files
            indexOfKeyword = arguments.IndexOf("-fs") >= 0 ? arguments.IndexOf("-fs") : arguments.IndexOf("--fieldSeparator");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                FieldSeparator = arguments[indexOfKeyword + 1];
                FieldSeparator = TextUtils.toSeparator(FieldSeparator);
            }

            // Quotation-characters for files
            indexOfKeyword = arguments.IndexOf("-qc") >= 0 ? arguments.IndexOf("-qc") : arguments.IndexOf("--quotationChars");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                QuotationText = arguments[indexOfKeyword + 1];
                QuotationText = TextUtils.toSeparator(QuotationText);
            }
            else
            {
                QuotationText = null;
            }

            // Encoding for files
            indexOfKeyword = arguments.IndexOf("-e") >= 0 ? arguments.IndexOf("-e") : arguments.IndexOf("--encoding");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                Encoding = arguments[indexOfKeyword + 1];
            }

            // Number of records per file to scan
            indexOfKeyword = arguments.IndexOf("-sd") >= 0 ? arguments.IndexOf("-sd") : arguments.IndexOf("--scandepth");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                ScanDepth = int.Parse(arguments[indexOfKeyword + 1]);
            }
            else
            {
                ScanDepth = -1;
            }

            // Type of system to output
            indexOfKeyword = arguments.IndexOf("-at") >= 0 ? arguments.IndexOf("-at") : arguments.IndexOf("--archivetype");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                _parameters["archivetype"] = arguments[indexOfKeyword + 1];
            }

            // Custom field-formats
            indexOfKeyword = arguments.IndexOf("-co") >= 0 ? arguments.IndexOf("-co") : arguments.IndexOf("--customoptions");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                structuredOptions =
                    arguments[indexOfKeyword + 1].EndsWith(".json") ?
                    decodeFieldFormatsFromFile(arguments[indexOfKeyword + 1]) :
                    decodeFieldFormatsFromString(arguments[indexOfKeyword + 1]);
            }
            else
            {
                // Deliver standard field-formats
                structuredOptions = decodeFieldFormatsFromString("{\"formats\":{},\"processes\": {}}");
            }

            // Folder with documents
            indexOfKeyword = arguments.IndexOf("-d") >= 0 ? arguments.IndexOf("-d") : arguments.IndexOf("--documents");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                if (File.Exists(Input))
                {
                    documents = new DirectoryInfo(new FileInfo(Input).Directory.FullName + Path.PathSeparator + arguments[indexOfKeyword + 1]);
                }
                else
                {
                    documents = new DirectoryInfo(new DirectoryInfo(Input).FullName + Path.PathSeparator + arguments[indexOfKeyword + 1]);
                }

                if (!documents.Exists)
                {
                    System.Diagnostics.Debug.WriteLine($"Error on Project.documents: {documents.FullName} doesnt exist.");
                }
            }

            // Folder with documents
            indexOfKeyword = arguments.IndexOf("-mf") >= 0 ? arguments.IndexOf("-mf") : arguments.IndexOf("--metsfile");

            if (indexOfKeyword >= 0 && arguments.Count > indexOfKeyword + 1)
            {
                if (File.Exists(arguments[indexOfKeyword + 1]))
                {
                    _parameters["metsfile"] = arguments[indexOfKeyword + 1];
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Error on Project.metsfile: {arguments[indexOfKeyword + 1]} doesnt exist.");
                }
            }
        }

        private static CustomOptions decodeFieldFormatsFromFile(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                CustomOptions result = JsonSerializer.Deserialize<CustomOptions>(r.ReadToEnd());

                if (!result.formats.ContainsKey("float"))
                    result.formats.Add("float", new string[] { "," });

                return result;
            }

        }

        private static CustomOptions decodeFieldFormatsFromString(string text)
        {
            CustomOptions result = JsonSerializer.Deserialize<CustomOptions>(text);

            if (!result.formats.ContainsKey("float"))
                result.formats.Add("float", new string[] { "," });

            return result;
        }

        private class CustomOptions
        {
            public Dictionary<string, string[]> formats { get; set; }
            public Dictionary<string, string[]> processes { get; set; }
        }

        public List<string> GetMissingArguments()
        {
            List<string> list = new List<string>();

            if (type == null)
                list.Add("'-t' or '--type'");
            else
            {
                switch (type)
                {
                    case "delimited":
                        if (Input == null)
                            list.Add("'-i' or '--input'");
                        if (Output == null)
                            list.Add("'-o' or '--output'");
                        if (Extension == null)
                            list.Add("'-x' or '--extension'");
                        if (RecordSeparator == null)
                            list.Add("'-rs' or '--recordSeparator'");
                        if (FieldSeparator == null)
                            list.Add("'-fs' or '--fieldSeparator'");
                        if (Encoding == null)
                            list.Add("'-e' or '--encoding'");
                        break;
                }
            }

            return list;
        }

        public void printHelp()
        {
            Console.WriteLine("List of implemented processes");
            foreach (string processName in implementedProcesses)
            {
                switch (processName)
                {
                    default:
                        Console.WriteLine("TBD!");
                        break;
                }
            }
        }

        public void printHelp(string process)
        {

        }
    }
}
