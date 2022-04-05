using AddmlPack.Objects;
using AddmlPack.Standards.Addml.Classes.v8_3;
using AddmlPack.Standards.Mets;
using AddmlPack.Templates;
using AddmlPack.Utils;
using AddmlPack.Utils.Addml;
using AddmlPack.Utils.Mets;
using AddmlPack.Utils.SpreadsheetUtils;
using System;
using System.Collections.Generic;

namespace AddmlPack.API
{
    public class API
    {
        private API(Project P)
        {
            if (P.Process == null)
            {
                Console.WriteLine(Messages.NoProcessFound);
                return;
            }
            else
                switch (P.Process)
                {
                    case "generate":
                        if (P.Help)
                        {
                            help("generate");
                        }
                        else
                        {
                            switch (P.type)
                            {
                                case "addml":
                                    {
                                        string _aml = P.GetString("archivetype");
                                        _aml = Template.GetTemplate(_aml);

                                        addml aml = AddmlUtils.ToAddml(_aml);
                                        _aml = AddmlUtils.FromAddml(aml);

                                        FileUtils.ToFile(_aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        string _aml = P.GetString("archivetype");
                                        _aml = Template.GetTemplate(_aml);

                                        addml aml = AddmlUtils.ToAddml(_aml);

                                        SpreadsheetUtils.ToSpreadsheet(aml, P.Output, P.Language);
                                    }
                                    break;
                                default:
                                    {
                                        Console.WriteLine(Messages.InvalidUseOfProcess);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "convert":
                        if (P.Help)
                        {
                            help("convert");
                        }
                        else
                        {
                            switch (P.type)
                            {
                                case "addml2excel":
                                    {
                                        string _aml = FileUtils.FromFile(P.Input);

                                        addml aml = AddmlUtils.ToAddml(_aml);

                                        SpreadsheetUtils.ToSpreadsheet(aml, P.Output, P.Language);
                                    }
                                    break;
                                case "excel2addml":
                                    {
                                        addml aml = SpreadsheetUtils.FromSpreadsheet(P.Input);

                                        string _aml = AddmlUtils.FromAddml(aml);

                                        FileUtils.ToFile(_aml, P.Output);
                                    }
                                    break;
                                default:
                                    {
                                        Console.WriteLine(Messages.InvalidUseOfProcess);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "appendProcesses":
                        if (P.Help)
                        {
                            help("appendProcesses");
                        }
                        else
                        {
                            switch (P.type)
                            {
                                case "addml":
                                    {
                                        string _aml = FileUtils.FromFile(P.Input);

                                        addml aml = AddmlUtils.ToAddml(_aml);

                                        AddmlUtils.AppendProcesses(aml);

                                        _aml = AddmlUtils.FromAddml(aml);

                                        FileUtils.ToFile(_aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        addml aml = SpreadsheetUtils.FromSpreadsheet(P.Input);

                                        AddmlUtils.AppendProcesses(aml);

                                        P.Language = SpreadsheetUtils.getLanguage(P.Input);

                                        SpreadsheetUtils.ToSpreadsheet(aml, P.Output, P.Language);
                                    }
                                    break;
                                default:
                                    {
                                        Console.WriteLine(Messages.InvalidUseOfProcess);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "appendMetsInfo":
                        if (P.Help)
                        {
                            help("appendMetsInfo");
                        }
                        else
                        {
                            switch (P.type)
                            {
                                case "addml":
                                    {
                                        string _aml = FileUtils.FromFile(P.Input);
                                        addml aml = AddmlUtils.ToAddml(_aml);

                                        var metsHdr = MetsUtils.getAgents((string)P.Parameters["metsfile"]);
                                        MetsUtils.AppendMetsInfo(aml, metsHdr);

                                        _aml = AddmlUtils.FromAddml(aml);
                                        FileUtils.ToFile(_aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        addml aml = SpreadsheetUtils.FromSpreadsheet(P.Input);

                                        var metsHdr = MetsUtils.getAgents((string)P.Parameters["metsfile"]);
                                        MetsUtils.AppendMetsInfo(aml, metsHdr);

                                        P.Language = SpreadsheetUtils.getLanguage(P.Input);
                                        SpreadsheetUtils.ToSpreadsheet(aml, P.Output, P.Language);
                                    }
                                    break;
                                default:
                                    {
                                        Console.WriteLine(Messages.InvalidUseOfProcess);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "scan":
                        if (P.Help)
                        {
                            help("scan");
                        }
                        else
                        {
                            Console.WriteLine("It's not done yet!");
                            return;
                            switch (P.type)
                            {
                                case "addml":
                                    {
                                        string _aml = P.GetString("archivetype");
                                        _aml = Template.GetTemplate(_aml);

                                        addml aml = AddmlUtils.ToAddml(_aml);

                                        // Add or append files

                                        _aml = AddmlUtils.FromAddml(aml);

                                        FileUtils.ToFile(_aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        string _aml = P.GetString("archivetype");
                                        _aml = Template.GetTemplate(_aml);

                                        addml aml = AddmlUtils.ToAddml(_aml);

                                        SpreadsheetUtils.ToSpreadsheet(aml, P.Output, P.Language);
                                    }
                                    break;
                                default:
                                    {
                                        Console.WriteLine(Messages.InvalidUseOfProcess);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "update":
                        if (P.Help)
                        {
                            help("update");
                        }
                        else
                        {
                            Console.WriteLine("It's not done yet!");
                            return;
                            switch (P.type)
                            {
                                case "addml":
                                    {
                                        string _aml = P.GetString("archivetype");
                                        _aml = Template.GetTemplate(_aml);

                                        addml aml = AddmlUtils.ToAddml(_aml);
                                        _aml = AddmlUtils.FromAddml(aml);

                                        FileUtils.ToFile(_aml, P.Output);
                                    }
                                    break;
                                default:
                                    {
                                        Console.WriteLine(Messages.InvalidUseOfProcess);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "help":
                        help();
                        break;
                    default:
                        Console.WriteLine(Messages.InvalidUseOfProcess);
                        return;
                }
        }

        public void help()
        {
            Console.WriteLine("Usage: dotnet AddmlPack.dll <process-name> -t <type> <args>");
            Console.WriteLine("Options for process/type:");
            Console.WriteLine(Messages.Process_Type_Description_Format, "generate", "addml", Messages.Generate_Addml_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "generate", "excel", Messages.Generate_Excel_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "generate", "help", Messages.Generate_Help_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "convert", "addml2excel", Messages.Convert_A2E_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "convert", "excel2addml", Messages.Convert_E2A_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "convert", "help", Messages.Convert_Help_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "appendProcesses", "addml", Messages.AppendProcesses_Addml_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "appendProcesses", "excel", Messages.AppendProcesses_Excel_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "appendProcesses", "help", Messages.AppendProcesses_Help_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "appendMetsInfo", "addml", Messages.AppendMetsInfo_Addml_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "appendMetsInfo", "excel", Messages.AppendMetsInfo_Excel_Description);
        }

        public void help(string process)
        {
            switch (process)
            {
                case "generate":
                    {
                        Console.WriteLine("Usage: dotnet AddmlPack.CLI.dll generate -t <TYPE> -o <Output> [-l <Language>]");
                        Console.WriteLine("");
                        Console.WriteLine("Otpions:");
                        Console.WriteLine(Messages.ArgShort_ArgLong_Description,
                            Messages.Arg_Type.Split(' ')[0],
                            Messages.Arg_Type.Split(' ')[1],
                            Messages.Arg_Type_Description);
                    }
                    break;
                case "convert":
                    {
                        Console.WriteLine("Usage: dotnet AddmlPack.CLI.dll convert -t <TYPE> -i <Input> -o <Output> [-l <Language>]");
                    }
                    break;
                case "appendProcesses":
                    {
                        Console.WriteLine("Usage: dotnet AddmlPack.CLI.dll appendProcesses -t <TYPE> -i <Input> -o <Output> [-l <Language>]");
                    }
                    break;
                case "appendMetsInfo":
                    {
                        Console.WriteLine("Usage: dotnet AddmlPack.CLI.dll appendMetsInfo -t <TYPE> -i <Input> -mf <METS-file> -o <Output> [-l <Language>]");
                    }
                    break;
                default:
                    {
                        Console.WriteLine($"Usage: dotnet AddmlPack.CLI.dll {process} TBD");
                    }
                    break;
            }
        }

        public static void Run(string[] args)
        {
            // Generate parameters
            Project project = new Project(new List<string>(args));

            if (project.Process == null)
            {
                // Return, invalid process
                Console.WriteLine("Invalid process, use help to list implemented processes.");
                return;
            }

            List<string> missing = project.GetMissingArguments();
            if (missing.Count > 0)
            {
                Console.WriteLine("Missing arguments:");
                foreach (string arg in missing)
                {
                    Console.WriteLine(" * " + arg);
                }
                return;
            }

            new API(project);
        }
    }
}
