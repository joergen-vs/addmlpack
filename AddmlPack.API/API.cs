﻿using AddmlPack.Spreadsheet;
using AddmlPack.Standard.v8_3;
using AddmlPack.Utils;
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
                                        addml aml = AddmlUtils.ToAddml(
                                            GeneratorUtils.GetTemplate(P.GetString("archivetype"))
                                        );
                                        FileUtils.AddmlToFile(aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        addml aml = AddmlUtils.ToAddml(
                                            GeneratorUtils.GetTemplate(P.GetString("archivetype"))
                                        );
                                        SpreadsheetUtils.ToExcel(aml, P.Output, P.Language);
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
                                        addml aml = FileUtils.AddmlFromFile(P.Input);
                                        SpreadsheetUtils.ToExcel(aml, P.Output, P.Language);
                                    }
                                    break;
                                case "excel2addml":
                                    {
                                        SpreadsheetUtils.Excel2Addml(P.Input, P.Output);
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
                                        Console.WriteLine("FileUtils.AddmlFromFile");
                                        addml aml = FileUtils.AddmlFromFile(P.Input);
                                        AddmlUtils.AppendProcesses(aml);

                                        Console.WriteLine("FileUtils.AddmlToFile");
                                        FileUtils.AddmlToFile(aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        Console.WriteLine("SpreadsheetUtils.Excel2Addml");
                                        addml aml = SpreadsheetUtils.Excel2Addml(P.Input, null);

                                        Console.WriteLine("AddmlUtils.AppendProcesses"); 
                                        AddmlUtils.AppendProcesses(aml);

                                        P.Language = SpreadsheetUtils.getLanguage(P.Input);

                                        Console.WriteLine("SpreadsheetUtils.ToExcel"); 
                                        SpreadsheetUtils.ToExcel(aml, P.Output, P.Language);
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
                                        addml aml = FileUtils.AddmlFromFile(P.Input);
                                        
                                        MetsHdr metsHdr = MetsUtils.getAgents((string)P.Parameters["metsfile"]);
                                        AddmlUtils.AppendMetsInfo(aml, metsHdr);
                                        
                                        FileUtils.AddmlToFile(aml, P.Output);
                                    }
                                    break;
                                case "excel":
                                    {
                                        addml aml = SpreadsheetUtils.Excel2Addml(P.Input, null);
                                        
                                        MetsHdr metsHdr = MetsUtils.getAgents((string)P.Parameters["metsfile"]);
                                        AddmlUtils.AppendMetsInfo(aml, metsHdr);

                                        P.Language = SpreadsheetUtils.getLanguage(P.Input);
                                        SpreadsheetUtils.ToExcel(aml, P.Output, P.Language);
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
            Console.WriteLine(Messages.Process_Type_Description_Format, "generate", "addml", Messages.AppendProcesses_Addml_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "generate", "excel", Messages.AppendProcesses_Excel_Description);
            Console.WriteLine(Messages.Process_Type_Description_Format, "generate", "help", Messages.AppendProcesses_Help_Description);
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
            }

            new API(project);
        }
    }
}
