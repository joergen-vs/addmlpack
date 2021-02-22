using AddmlPack.Spreadsheet.Languages;
using AddmlPack.Standard.v8_3;
using AddmlPack.Utils;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace AddmlPack.Spreadsheet
{
    public class SpreadsheetUtils
    {
        private static string Horizontal { get { return ", "; } }
        private static string Vertical { get { return "\r\n"; } }

        public static void Addml2Excel(string pathOfAddmlFile, string pathOfExcelFile)
        {
            addml aml = FileUtils.AddmlFromFile(pathOfAddmlFile);
            ToExcel(aml, pathOfExcelFile, Thread.CurrentThread.CurrentCulture.Name);
        }

        public static addml Excel2Addml(string pathOfExcelFile, string pathOfAddmlFile)
        {
            var workbook = new XLWorkbook(pathOfExcelFile);
            return ToAddml(workbook, pathOfAddmlFile, pathOfExcelFile);
        }


        public static void ToExcel(addml aml, string path, string lang)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);

            int row;
            int column;

            List<string[]> allCodes = new List<string[]>();
            List<string[]> allProcesses = new List<string[]>();
            List<string[]> characterDefinitions = new List<string[]>();
            List<string[]> allPrimaryKeys = new List<string[]>();
            List<string[]> allForeignKeys = new List<string[]>();
            List<string[]> allAlternateKeys = new List<string[]>();


            string[] sheetNames = new string[]
            {
                Excel.Sheet_Dataset,
                Excel.Sheet_FlatFiles,
                Excel.Sheet_Definitions,
                Excel.Sheet_Types,
                Excel.Sheet_Keys,
                Excel.Sheet_FlatFiles_OtherOptions,
                Excel.Sheet_Objects,
                Excel.Sheet_Processes,
            };

            Dictionary<string, int> indexes = new Dictionary<string, int>();
            Dictionary<string, string> NameToId = new Dictionary<string, string>();

            XLWorkbook wb = new XLWorkbook();
            wb.Properties.Comments = $"{{\"lang\":\"{Thread.CurrentThread.CurrentCulture.Name}\"," +
                $"\"addml-version\":\"8.3\"}}";

            // Dataset
            var ws = wb.AddWorksheet(sheetNames[0]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = Excel.Section_Agents;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Agent_Type,
                Excel.Agent_Role,
                Excel.Agent_Contact,
                Excel.Agent_ContactType
            });

            if (aml.dataset?[0].reference?.context?.additionalElements.getElement("recordCreators") != null)
            {
                foreach (additionalElement agent in aml.dataset?[0].reference.context?.additionalElements.getElement("recordCreators")?.additionalElements?.additionalElement)
                {
                    if (agent.name.Equals("recordCreator") && agent.value != null)
                    {
                        AddRow(ws, row, column, new string[] {
                            agent.value,
                            "organization",
                            "recordCreator",
                        });

                        row += 1;
                    }
                }
            }
            else if (aml.dataset?[0].reference?.context?.additionalElements.getElement("recordCreator") != null)
            {
                additionalElement agent = aml.dataset?[0].reference.context?.additionalElements.getElement("recordCreator");
                {
                    if (agent.value != null)
                    {
                        AddRow(ws, row, column, new string[] {
                            agent.value,
                            "organization",
                            "recordCreator",
                        });

                        row += 1;
                    }
                }
            }
            else if (aml.dataset?[0].reference?.context?.additionalElements?.getElement("agents")?
                    .additionalElements?.getElements("agent").Count > 0)
            {
                foreach (additionalElement agent in
                    aml.dataset?[0].reference?.context?.additionalElements?.getElement("agents")?
                    .additionalElements?.getElements("agent"))
                {
                    AddRow(ws, row, column, new string[] {
                            agent.value,
                            agent.getProperty("type")?.value,
                            agent.getProperty("role")?.value,
                            "recordCreator",
                        });

                    row += 1;
                }
            }

            ws.Columns().AdjustToContents();

            // Flat Files
            ws = wb.AddWorksheet(sheetNames[1]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = Excel.Section_File;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.File_Relative_Path,
                Excel.File_Definition_Reference,
                Excel.File_NumberOfRecords,
                Excel.File_ChecksumAlgorithm,
                Excel.File_ChecksumValue
            });
            row += 1;

            indexes["Flat Files/Files"] = row;

            flatFiles files = aml.dataset[0].flatFiles;
            if (files != null)
            {

                for (int i = 0; i < files.flatFile.Length; i++)
                {
                    string fileName = files.flatFile[i].
                        getProperty("fileName")?.value;

                    string checksumAlgorithm = files.flatFile[i].
                        getProperty("checksum")?.
                        getProperty("akgorithm")?.value;

                    string checksumValue = files.flatFile[i].
                        getProperty("checksum")?.
                        getProperty("value")?.value;

                    string numberOfRecords = files.flatFile[i].
                        getProperty("numberOfRecords")?.value;

                    AddRow(ws, row + i, column, new string[] {
                    files.flatFile[i].name,
                    fileName,
                    files.flatFile[i].definitionReference,
                    numberOfRecords,
                    checksumAlgorithm,
                    checksumValue
                });

                    indexes["Flat Files/Files/" + files.flatFile[i].name] = row + i;
                    indexes["Flat Files/Files/" + files.flatFile[i].name + "/Definition/" + files.flatFile[i].definitionReference] = row + i;
                }
                row += 1;
            }

            ws.Columns().AdjustToContents();

            // Definitions
            ws = wb.AddWorksheet(sheetNames[2]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = Excel.Section_File_Definitions;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Description,
                Excel.File_Reference,
                Excel.Type_Reference,
            });
            row += 1;

            indexes["Definitions/Files"] = row;
            if (files != null)
            {
                foreach (flatFileDefinition _flatFileDefinition in files.flatFileDefinitions)
                {
                    string description = _flatFileDefinition.description;

                    string flatFileName;
                    for (int j = 0; j < files.flatFile.Length; j++)
                        if (files.flatFile[j].definitionReference.Equals(_flatFileDefinition.name))
                        {
                            flatFileName = files.flatFile[j].name;

                            AddRow(ws, row, column, new string[] {
                            _flatFileDefinition.name,
                            description,
                            flatFileName,
                            _flatFileDefinition.typeReference,
                        });

                            AddLink(ws, row, column + 2,
                                Excel.Sheet_FlatFiles, indexes["Flat Files/Files/" + flatFileName + "/Definition/" + _flatFileDefinition.name], 1);

                            indexes["Definitions/Files/" + _flatFileDefinition.name] = row;
                            indexes["Definitions/Files/" + _flatFileDefinition.name + "/Type/" + _flatFileDefinition.typeReference] = row;

                            for (int k = indexes["Flat Files/Files"]; !wb.Worksheet(Excel.Sheet_FlatFiles).Cell(k, 1).Value.Equals(string.Empty); k++)
                            {
                                if (wb.Worksheet(Excel.Sheet_FlatFiles).Cell(k, column + 2).Value.Equals(_flatFileDefinition.name))
                                    AddLink(wb.Worksheet(Excel.Sheet_FlatFiles), k, column + 2,
                                        Excel.Sheet_Definitions, row, 1);
                            }

                            row += 1;
                        }
                }

            }
            row += +2;


            ws.Cell(row, column).Value = Excel.Section_Record_Definitions;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Description,
                Excel.File_Definition_Reference,
                Excel.Type_Reference,
                Excel.Record_Definition_FieldValue,
                Excel.Record_Definition_Incomplete,
                Excel.Record_Definition_FixedLength,
                Excel.Record_Definition_RepeatingGroups,
                Excel.Record_Definition_Keys,
            });
            row += 1;

            indexes["Definitions/Records"] = row;
            if (files != null)
            {
                foreach (flatFileDefinition _flatFileDefinition in files.flatFileDefinitions)
                {
                    foreach (recordDefinition _recordDefinition in _flatFileDefinition.recordDefinitions)
                    {
                        AddRow(ws, row, column, new string[] {
                        _recordDefinition.name,
                        _recordDefinition.description,
                        _flatFileDefinition.name,
                        _recordDefinition.typeReference,
                    });

                        if (_recordDefinition.keys != null)
                        {
                            foreach (key _key in _recordDefinition.keys)
                            {
                                if (_key.Item.GetType().IsEquivalentTo(typeof(primaryKey)))
                                {
                                    allPrimaryKeys.Add(new string[]
                                    {
                                    _flatFileDefinition.name,
                                    _key.name,
                                    _recordDefinition.name,
                                    Excel.Keys_PrimaryKey,
                                    string.Join(Horizontal, _key.fieldDefinitionReferences.Select(
                                        c => c.name).ToList())
                                    });
                                }
                                else if (_key.Item.GetType().IsEquivalentTo(typeof(foreignKey)))
                                {
                                    foreignKey _foreignKey = (foreignKey)_key.Item;
                                    allForeignKeys.Add(new string[]
                                    {
                                    _flatFileDefinition.name,
                                    _key.name,
                                    _recordDefinition.name,
                                    string.Join(Horizontal, _key.fieldDefinitionReferences.Select(
                                        c => c.name).ToList()),
                                    _foreignKey.relationType,
                                    _foreignKey.flatFileDefinitionReference.name,
                                    _foreignKey.flatFileDefinitionReference.recordDefinitionReferences[0].name,
                                    string.Join(Horizontal, _foreignKey.flatFileDefinitionReference
                                        .recordDefinitionReferences[0].fieldDefinitionReferences.Select(
                                        c => c.name).ToList())

                                    });
                                }
                                else if (_key.Item.GetType().IsEquivalentTo(typeof(alternateKey)))
                                {
                                    allPrimaryKeys.Add(new string[]
                                    {
                                    _flatFileDefinition.name,
                                    _key.name,
                                    _recordDefinition.name,
                                    Excel.Keys_AlternateKey,
                                    string.Join(Horizontal, _key.fieldDefinitionReferences.Select(
                                        c => c.name).ToList())
                                    });
                                }
                            }
                        }

                        AddLink(ws, row, 3, Excel.Sheet_Definitions, indexes["Definitions/Files/" +
                            _flatFileDefinition.name], 1);

                        indexes["Definitions/Records/" +
                            _flatFileDefinition.name + "." +
                            _recordDefinition.name] = row;
                        row += 1;
                    }
                }
            }
            row += +2;

            ws.Cell(row, column).Value = Excel.Section_Field_Definitions;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Description,
                Excel.Record_Definition_Reference,
                Excel.Type_Reference,
                Excel.Field_Definition_StartPos,
                Excel.Field_Definition_EndPos,
                Excel.Field_Definition_FixedLength,
                Excel.Field_Definition_MinLength,
                Excel.Field_Definition_MaxLength,
                Excel.Field_Definition_Unique,
                Excel.Field_Definition_NotNull,
                Excel.Field_Definition_Fieldparts,
                Excel.Field_Definition_Codes,
            });
            row += 1;

            indexes["Definitions/Fields"] = row;
            if (files != null)
            {
                foreach (flatFileDefinition _flatFileDefinition in files.flatFileDefinitions)
                {
                    foreach (recordDefinition _recordDefinition in _flatFileDefinition.recordDefinitions)
                    {
                        foreach (fieldDefinition _fieldDefinition in _recordDefinition.fieldDefinitions)
                        {
                            // Check for codes
                            if (_fieldDefinition.codes != null)
                            {
                                foreach (code _code in _fieldDefinition.codes)
                                {
                                    allCodes.Add(new string[]
                                    {
                                    _flatFileDefinition.name+"."+
                                    _recordDefinition.name + "." +
                                    _fieldDefinition.name,
                                    _recordDefinition.name,
                                    _fieldDefinition.name,
                                    _code.codeValue,
                                    _code.explan,
                                    });
                                }
                            }

                            AddRow(ws, row, column, new string[] {
                            _fieldDefinition.name,
                            _fieldDefinition.description,
                            _recordDefinition.name,
                            _fieldDefinition.typeReference,
                            _fieldDefinition.startPos,
                            _fieldDefinition.endPos,
                            _fieldDefinition.fixedLength,
                            _fieldDefinition.minLength,
                            _fieldDefinition.maxLength,
                            $"{_fieldDefinition.unique != null}".ToUpper(),
                            $"{_fieldDefinition.notNull != null}".ToUpper(),
                            $"{_fieldDefinition.fieldParts != null}".ToUpper(),
                            $"{_fieldDefinition.codes != null}".ToUpper(),
                        });

                            AddLink(ws, row, 3, Excel.Sheet_Definitions, indexes["Definitions/Records/" +
                                _flatFileDefinition.name + "." +
                                _recordDefinition.name], 1);

                            indexes["Definitions/Fields/" +
                                _flatFileDefinition.name + "." +
                                _recordDefinition.name + "." +
                                _fieldDefinition.name] = row;
                            row += 1;
                        }
                    }
                }
            }

            ws.Columns().AdjustToContents();

            // Types
            ws = wb.AddWorksheet(sheetNames[3]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = Excel.Section_File_Types;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Description,
                Excel.File_Type_Charset,
                Excel.File_Type_CharacterDefinitions,
                Excel.File_Type_FixedOrDelimited,
                Excel.File_Type_RecordSeparator,
                Excel.File_Type_FieldSeparator,
                Excel.File_Type_QuotationSeparator,
            });
            row += 1;

            if (files != null)
            {
                foreach (flatFileType _flatFileType in files.structureTypes.flatFileTypes)
                {
                    string description = _flatFileType.description;
                    description = description == null ? "" : description;

                    string recordSeparator = "";
                    string fieldSeparator = "";
                    string quotingChar = "";
                    var fileformat = _flatFileType.Item;
                    if (fileformat.GetType().Equals(typeof(fixedFileFormat)))
                    {
                        recordSeparator = AddmlUtils.fromSeparator(((fixedFileFormat)fileformat).recordSeparator);
                    }
                    else
                    {
                        recordSeparator = AddmlUtils.fromSeparator(((delimFileFormat)fileformat).recordSeparator);
                        fieldSeparator = AddmlUtils.fromSeparator(((delimFileFormat)fileformat).fieldSeparatingChar);
                        quotingChar = AddmlUtils.fromSeparator(((delimFileFormat)fileformat).quotingChar);
                    }

                    if (_flatFileType.charDefinitions != null)
                    {
                        characterDefinitions.Add(new string[]
                        {
                        _flatFileType.name,
                        _flatFileType.charDefinitions[0].fromChar,
                        _flatFileType.charDefinitions[0].toChar,
                        });
                    }

                    AddRow(ws, row, column, new string[] {
                            _flatFileType.name,
                            _flatFileType.description,
                            _flatFileType.charset,
                            $"{_flatFileType.charDefinitions != null}".ToUpper(),
                            fileformat.GetType().Equals(typeof(fixedFileFormat)) ?
                            Excel.File_Type_FixedOrDelimited_Fixed :
                            Excel.File_Type_FixedOrDelimited_Delimited,
                            recordSeparator,
                            fieldSeparator,
                            quotingChar

                        });

                    indexes["Types/Files/" + _flatFileType.name] = row;

                    for (int k = indexes["Definitions/Files"]; !wb.Worksheet(Excel.Sheet_Definitions).Cell(k, 1).Value.Equals(string.Empty); k++)
                    {
                        if (wb.Worksheet(Excel.Sheet_Definitions).Cell(k, column + 3).Value.Equals(_flatFileType.name))
                            AddLink(wb.Worksheet(Excel.Sheet_Definitions), k, column + 3,
                                Excel.Sheet_Types, row, 1);
                    }

                    row += 1;
                }
            }

            if (files != null)
            {
                if (files.structureTypes.recordTypes != null)
                {
                    row += 2;

                    ws.Cell(row, column).Value = Excel.Section_Record_Types;
                    row += 1;

                    AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Description,
                Excel.Record_Type_Trimming,
            });
                    row += 1;

                    foreach (recordType _recordType in files.structureTypes.recordTypes)
                    {
                        foreach (flatFileDefinition _flatFileDefinition in files.flatFileDefinitions)
                        {
                            foreach (recordDefinition _recordDefinition in _flatFileDefinition.recordDefinitions)
                            {
                                if (_recordDefinition.typeReference.Equals(_recordType.name))
                                {
                                    AddRow(ws, row, column, new string[] {
                                    _recordType.name,
                                    _recordType.description,
                                    $"{_recordType.trimmed != null}"
                                });

                                    // Link recordDefinition to recordType
                                    AddLink(
                                        wb.Worksheet(Excel.Sheet_Definitions),
                                        indexes[
                                            "Definitions/Records/" +
                                            _flatFileDefinition.name + "." +
                                            _recordDefinition.name
                                        ],
                                        column + 2, Excel.Sheet_Definitions, row, 1
                                    );

                                    indexes["Types/Records/" + _recordType.name] = row;

                                    row += 1;
                                }
                            }
                        }
                    }
                }
            }


            if (files != null)
            {
                if (files.structureTypes.fieldTypes != null)
                {
                    row += 1;

                    ws.Cell(row, column).Value = Excel.Section_Field_Types;
                    row += 1;

                    AddRow(ws, row, column, new string[] {
                    Excel.Name,
                    Excel.Description,
                    Excel.Field_Type_Datatype,
                    Excel.Field_Type_FieldFormat,
                    Excel.Field_Type_Alignment,
                    Excel.Field_Type_Padding,
                    Excel.Field_Type_Packing,
                    Excel.Field_Type_NullValues,
                });
                    row += 1;


                    foreach (fieldType _fieldType in files.structureTypes.fieldTypes)
                    {
                        string description = _fieldType.description;

                        AddRow(ws, row, column, new string[] {
                        _fieldType.name,
                        _fieldType.description,
                        _fieldType.dataType,
                        _fieldType.fieldFormat,
                        _fieldType.alignment,
                        _fieldType.padChar,
                        _fieldType.packType,
                        _fieldType.nullValues == null ? "" :
                            string.Join(Horizontal, _fieldType.nullValues)

                    });

                        for (int k = indexes["Definitions/Fields"]; !wb.Worksheet(Excel.Sheet_Definitions).Cell(k, 1).Value.Equals(string.Empty); k++)
                        {
                            if (wb.Worksheet(Excel.Sheet_Definitions).Cell(k, column + 3).Value.Equals(_fieldType.name))
                                AddLink(wb.Worksheet(Excel.Sheet_Definitions), k, column + 3,
                                    Excel.Sheet_Types, row, 1);
                        }

                        indexes["Types/Fields/" + _fieldType.name] = row;

                        row += 1;
                    }
                }
            }

            ws.Columns().AdjustToContents();

            // Keys
            ws = wb.AddWorksheet(sheetNames[4]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = Excel.Keys;
            row += 1;
            ws.Cell(row, column).Value = string.Format(
                Excel.NumberInUse,
                Excel.Section_CandidateKeys.ToLower(),
                allPrimaryKeys.Count);
            row += 1;
            ws.Cell(row, column).Value = string.Format(
                Excel.NumberInUse,
                Excel.Section_ForeignKeys.ToLower(),
                allForeignKeys.Count);
            row += 2;

            ws.Cell(row, column).Value = Excel.Section_CandidateKeys;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Record_Definition_Reference,
                Excel.Key_PrimaryOrAlternate,
                Excel.Field_Definition_Reference
            });
            row += 1;

            string[] tmp = new string[4];

            foreach (string[] tuple in allPrimaryKeys)
            {
                AddRow(ws, row, column, new string[]{
                    tuple[1],
                    tuple[2],
                    tuple[3],
                    tuple[4]
                });
                AddLink(ws, row, column + 1, Excel.Sheet_Definitions, indexes[$"Definitions/Records/{tuple[0]}.{tuple[2]}"], column);

                row += 1;
            }
            row += 1;

            ws.Cell(row, column).Value = Excel.Section_ForeignKeys;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.Record_Definition_Reference,
                Excel.Field_Definition_Reference,
                Excel.Keys_Relation,
                Excel.File_Definition_Reference,
                Excel.Record_Definition_Reference,
                Excel.Field_Definition_Reference
            });
            row += 1;

            foreach (string[] tuple in allForeignKeys)
            {
                AddRow(ws, row, column, new string[]{
                    tuple[1],
                    tuple[2],
                    tuple[3],
                    tuple[4],
                    tuple[5],
                    tuple[6],
                    tuple[7]
                });
                AddLink(ws, row, column + 1, Excel.Sheet_Definitions, indexes[$"Definitions/Records/{tuple[0]}.{tuple[2]}"], column);
                AddLink(ws, row, column + 5, Excel.Sheet_Definitions, indexes[$"Definitions/Records/{tuple[5]}.{tuple[6]}"], column);

                row += 1;
            }

            ws.Columns().AdjustToContents();

            // Flat File Other Options
            ws = wb.AddWorksheet(sheetNames[5]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = string.Format(
                Excel.InDataset,
                Excel.Section_Codes);
            row += 1;
            ws.Cell(row, column).Value = string.Format(
                Excel.NumberInUse,
                Excel.Section_Codes.ToLower(),
                allCodes.Count);
            row += 2;

            ws.Cell(row, column).Value = Excel.Section_Codes;
            row += 1;

            AddRow(ws, row, column, new string[] {
                Excel.Record_Definition_Reference,
                Excel.Field_Definition_Reference,
                Excel.Code_Value,
                Excel.Code_Explanation
            });
            row += 1;

            foreach (string[] tuple in allCodes)
            {
                AddRow(ws, row, column, new string[] {
                    tuple[1],
                    tuple[2],
                    tuple[3],
                    tuple[4],
                });
                AddLink(ws, row, column + 1, Excel.Sheet_Definitions, indexes[$"Definitions/Fields/{tuple[0]}"], 1);

                row += 1;
            }

            row += 2;

            // Character-definitions
            ws.Cell(row, column).Value = string.Format(
                Excel.InDataset,
                Excel.Section_CharacterDefinitions);
            row += 1;
            ws.Cell(row, column).Value = string.Format(
                Excel.NumberInUse,
                Excel.Section_CharacterDefinitions.ToLower(),
                characterDefinitions.Count);

            row += 2;

            ws.Cell(row, column).Value = Excel.Section_CharacterDefinitions;
            row += 1;

            AddRow(ws, row, column, new string[] {
                    Excel.File_Type_Reference,
                    Excel.CharacterDefinitions_FromCharacter,
                    Excel.CharacterDefinitions_ToCharacter
                });
            row += 1;

            foreach (string[] tuple in characterDefinitions)
            {
                AddRow(ws, row, column, tuple);

                row += 1;
            }

            ws.Columns().AdjustToContents();

            // Objects
            ws = wb.AddWorksheet(sheetNames[6]);
            row = 3;
            column = 1;

            List<DataObjectTypes> dataObjects = aml.dataset[0].dataObjects != null ?
                GetDataObjects(aml.dataset[0].dataObjects, "") :
                new List<DataObjectTypes>();

            ws.Cell(row, column).Value = string.Format(
                Excel.InDataset,
                Excel.DataObjects);
            row += 1;

            ws.Cell(row, column).Value = string.Format(
                Excel.NumberInUse,
                Excel.DataObjects.ToLower(),
                dataObjects.Count
            );
            row += 2;

            ws.Cell(row, column).Value = Excel.Section_Objects;
            row += 1;

            foreach (DataObjectTypes obj in dataObjects)
            {
                foreach (string[] tuple in obj.getTypes())
                {
                    AddRow(ws, row, column, tuple);
                    row += 1;
                }
                row += 1;
            }

            ws.Columns().AdjustToContents();

            // Processes

            List<string[]> fileProcesses = new List<string[]>();
            List<string[]> recordProcesses = new List<string[]>();
            List<string[]> fieldProcesses = new List<string[]>();

            if (files != null && files.flatFileProcesses != null)
            {
                foreach (flatFileProcesses _flatFileProcesses in files.flatFileProcesses)
                {
                    foreach (process _process in _flatFileProcesses.processes)
                    {
                        fileProcesses.Add(new string[]
                        {
                            _process.name,
                            _flatFileProcesses.flatFileReference,
                        });
                    }

                    if(_flatFileProcesses.recordProcesses != null)
                    foreach (recordProcesses _recordProcesses in _flatFileProcesses.recordProcesses)
                    {
                        if (_recordProcesses.processes != null)
                            foreach (process _process in _recordProcesses.processes)
                            {
                                recordProcesses.Add(new string[]
                                {
                                    _process.name,
                                    _flatFileProcesses.flatFileReference,
                                    _recordProcesses.definitionReference,
                                });
                            }

                            if (_recordProcesses.fieldProcesses != null)
                                foreach (fieldProcesses _fieldProcesses in _recordProcesses.fieldProcesses)
                        {
                            if (_fieldProcesses.processes != null)
                                foreach (process _process in _fieldProcesses.processes)
                                {
                                    fieldProcesses.Add(new string[]
                                    {
                                         _process.name,
                                        _flatFileProcesses.flatFileReference,
                                        _recordProcesses.definitionReference,
                                        _fieldProcesses.definitionReference,
                                    });
                                }
                        }
                    }
                }
            }

            ws = wb.AddWorksheet(sheetNames[7]);
            row = 3;
            column = 1;

            ws.Cell(row, column).Value = string.Format(
                Excel.InDataset,
                Excel.Section_Processes);
            row += 1;
            ws.Cell(row, column).Value = string.Format(
                Excel.NumberInUse,
                Excel.Section_Processes.ToLower(),
                fileProcesses.Count +
                recordProcesses.Count +
                fieldProcesses.Count);
            row += 2;

            ws.Cell(row, column).Value = Excel.File_Processes;
            row += 1;

            AddRow(ws, row, column, new string[] {
                    Excel.Name,
                    Excel.File_Reference
                });
            row += 1;

            foreach (string[] tuple in fileProcesses)
            {
                AddRow(ws, row, column, tuple);

                row += 1;
            }

            row += 2;

            ws.Cell(row, column).Value = Excel.Record_Processes;
            row += 1;

            AddRow(ws, row, column, new string[] {
                    Excel.Name,
                    Excel.File_Reference,
                    Excel.Record_Definition_Reference
                });
            row += 1;

            foreach (string[] tuple in recordProcesses)
            {
                AddRow(ws, row, column, tuple);

                row += 1;
            }

            row += 2;

            ws.Cell(row, column).Value = Excel.Field_Processes;
            row += 1;

            AddRow(ws, row, column, new string[] {
                    Excel.Name,
                    Excel.File_Reference,
                    Excel.Record_Definition_Reference,
                    Excel.Field_Definition_Reference
                });
            row += 1;

            foreach (string[] tuple in fieldProcesses)
            {
                AddRow(ws, row, column, tuple);

                row += 1;
            }

            ws.Columns().AdjustToContents();


            wb.SaveAs(path);
        }

        public static addml ToAddml(XLWorkbook wb, string path, string source)
        {
            IXLWorksheet ws;
            addml aml = new addml();
            int column = 1;

            DirectoryInfo workingDirectory = new DirectoryInfo(source.Substring(0, source.LastIndexOf("/") + 1));

            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(wb.Properties.Comments);
            //$"{{\"lang\":\"{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}\"," +
            //    $"\"addml-version\":\"8.3\"}}";

            Thread.CurrentThread.CurrentCulture = new CultureInfo(values["lang"]);

            // Reference-information
            ws = wb.Worksheet(1);

            dataset _dataset = aml.addDataset(GeneratorUtils.NewGUID(), "Fagsystem");
            _dataset.reference = new reference();

            ws = wb.Worksheet(2);
            int flatFileIndex = searchSheet(ws, Excel.Section_File, column, 1) + 2;

            if (!ws.Cell(flatFileIndex, column).IsEmpty())
            {
                flatFiles _flatFiles = (aml.dataset[0].flatFiles = new flatFiles());

                // Flat files
                while (!ws.Cell(flatFileIndex, column).IsEmpty())
                {
                    flatFile _flatfile = _flatFiles.addFlatFile(
                        ws.Cell(flatFileIndex, column).Value.ToString(),
                        ws.Cell(flatFileIndex, column + 2).Value.ToString()
                    );

                    _flatfile.addProperty("fileName").value = ws.Cell(flatFileIndex, 2).Value.ToString();

                    string checksumAlgorithm = getCellValue(ws, flatFileIndex, column + 4);
                    string checksumValue = getCellValue(ws, flatFileIndex, column + 5);

                    if (checksumAlgorithm != null)

                        if (!checksumAlgorithm.Equals(""))
                        {
                            _flatfile.addProperty("checksum").addProperty("algorithm").value =
                                checksumAlgorithm;
                        }

                    if (checksumValue != null)
                        if (checksumValue.Equals("<generate>"))
                        {
                            _flatfile.addProperty("checksum").addProperty("value").value =
                                FileUtils.GenerateChecksum(
                                    workingDirectory.FullName + Path.AltDirectorySeparatorChar + _flatfile.addProperty("fileName").value
                                );
                            _flatfile.addProperty("checksum").addProperty("algorithm").value = checksumAlgorithm;
                        }
                        else
                        {
                            _flatfile.addProperty("checksum").addProperty("value").value = checksumValue;
                        }

                    flatFileIndex += 1;
                }

                // Flat File Definitions
                ws = wb.Worksheet(3);
                string TRUE;

                int fileDefinitionsIndex = searchSheet(ws, Excel.Section_File_Definitions, column, 1) + 2;

                while (!ws.Cell(fileDefinitionsIndex, column).IsEmpty())
                {
                    flatFileDefinition _flatFileDefinition = _flatFiles.addFlatFileDefinition(
                        ws.Cell(fileDefinitionsIndex, column).Value.ToString(),
                        ws.Cell(fileDefinitionsIndex, column + 3).Value.ToString()
                        );

                    _flatFileDefinition.description = getCellValue(ws, fileDefinitionsIndex, column + 1);
                    TRUE = getCellValue(ws, fileDefinitionsIndex, column + 4);
                    _flatFileDefinition.external = TRUE == null ? null : TRUE.Equals("True") ? new external() : null;
                    _flatFileDefinition.recordDefinitionFieldIdentifier = getCellValue(ws, fileDefinitionsIndex, column + 5);

                    int recordDefinitionsIndex = searchSheet(ws, Excel.Section_Record_Definitions, column, fileDefinitionsIndex) + 2;

                    while (!ws.Cell(recordDefinitionsIndex, column).IsEmpty())
                    {
                        if (ws.Cell(recordDefinitionsIndex, column + 2).Value.Equals(
                            _flatFileDefinition.name))
                        {
                            recordDefinition _recordDefinition = _flatFileDefinition.addRecordDefinition(
                                ws.Cell(recordDefinitionsIndex, column).Value.ToString(),
                                ws.Cell(recordDefinitionsIndex, column + 3).IsEmpty() ?
                                    null : ws.Cell(recordDefinitionsIndex, column + 3).Value.ToString()
                                );

                            _recordDefinition.description = getCellValue(ws, recordDefinitionsIndex, column + 1);
                            _recordDefinition.recordDefinitionFieldValue = getCellValue(ws, recordDefinitionsIndex, column + 4);
                            TRUE = getCellValue(ws, recordDefinitionsIndex, column + 5);
                            _recordDefinition.incomplete = TRUE == null ? null : TRUE.Equals("True") ? new incomplete() : null;
                            _recordDefinition.fixedLength = getCellValue(ws, recordDefinitionsIndex, column + 6);
                            _recordDefinition.repeatingGroups = null;

                            int fieldDefinitionsIndex = searchSheet(ws, Excel.Section_Field_Definitions, column, recordDefinitionsIndex) + 2;

                            while (!ws.Cell(fieldDefinitionsIndex, column).IsEmpty())
                            {
                                if (ws.Cell(fieldDefinitionsIndex, column + 2).Value.Equals(
                                    _recordDefinition.name))
                                {
                                    fieldDefinition _fieldDefinition = _recordDefinition.addFieldDefinition(
                                        ws.Cell(fieldDefinitionsIndex, column).Value.ToString(),
                                        getCellValue(ws, fieldDefinitionsIndex, column + 3)
                                        );

                                    _fieldDefinition.description = getCellValue(ws, fieldDefinitionsIndex, column + 1);
                                    _fieldDefinition.startPos = getCellValue(ws, fieldDefinitionsIndex, column + 4);
                                    _fieldDefinition.endPos = getCellValue(ws, fieldDefinitionsIndex, column + 5);
                                    _fieldDefinition.fixedLength = getCellValue(ws, fieldDefinitionsIndex, column + 6);
                                    _fieldDefinition.minLength = getCellValue(ws, fieldDefinitionsIndex, column + 7);
                                    _fieldDefinition.maxLength = getCellValue(ws, fieldDefinitionsIndex, column + 8);

                                    TRUE = getCellValue(ws, fieldDefinitionsIndex, column + 9);
                                    _fieldDefinition.unique = TRUE == null ? null : TRUE.Equals("True") ? new unique() : null;
                                    TRUE = getCellValue(ws, fieldDefinitionsIndex, column + 10);
                                    _fieldDefinition.notNull = TRUE == null ? null : TRUE.Equals("True") ? new notNull() : null;
                                    TRUE = getCellValue(ws, fieldDefinitionsIndex, column + 11);
                                    _fieldDefinition.fieldParts = TRUE == null ? null : TRUE.Equals("True") ? new fieldParts() : null;
                                    TRUE = getCellValue(ws, fieldDefinitionsIndex, column + 12);
                                    if (TRUE != null && TRUE.Equals("True"))
                                    {
                                        _fieldDefinition.codes = getCodes(wb.Worksheet(6), column, _recordDefinition.name, _fieldDefinition.name);
                                    }
                                    else
                                    {
                                        _fieldDefinition.codes = null;
                                    }
                                }

                                fieldDefinitionsIndex += 1;
                            }
                        }

                        recordDefinitionsIndex += 1;
                    }

                    fileDefinitionsIndex += 1;
                }

                // Flat File Types
                ws = wb.Worksheet(4);

                int fileTypeIndex = searchSheet(ws, Excel.Section_File_Types, column, 1) + 2;
                while (!ws.Cell(fileTypeIndex, column).IsEmpty())
                {
                    flatFileType _flatFileType = _flatFiles.structureTypes.addFlatFileType(
                        ws.Cell(fileTypeIndex, column).Value.ToString()
                    );

                    _flatFileType.description = getCellValue(ws, fileTypeIndex, column + 1);

                    _flatFileType.charset = getCellValue(ws, fileTypeIndex, column + 2);

                    if (!ws.Cell(fileTypeIndex, column + 3).IsEmpty())
                    {
                        if (ws.Cell(fileTypeIndex, column + 4).Value.Equals("Fixed-length"))
                        {
                            fixedFileFormat item = new fixedFileFormat();
                            item.recordSeparator = getCellValue(ws, fileTypeIndex, column + 5);
                            _flatFileType.Item = item;
                        }
                        else
                        {
                            delimFileFormat item = new delimFileFormat();
                            item.recordSeparator = getCellValue(ws, fileTypeIndex, column + 5);
                            if (item.recordSeparator == null)
                                item.recordSeparator = Environment.NewLine;
                            item.fieldSeparatingChar = getCellValue(ws, fileTypeIndex, column + 6);
                            item.quotingChar = getCellValue(ws, fileTypeIndex, column + 7);
                            _flatFileType.Item = item;
                        }
                    }


                    fileTypeIndex += 1;
                }

                int recordTypeIndex = searchSheet(ws, Excel.Section_Record_Types, column, fileTypeIndex) + 2;
                while (!ws.Cell(recordTypeIndex, column).IsEmpty())
                {
                    recordType _recordType = _flatFiles.structureTypes.addRecordType(
                        ws.Cell(recordTypeIndex, column).Value.ToString()
                    );
                    recordTypeIndex += 1;
                }

                int fieldTypeIndex = searchSheet(ws, Excel.Section_Field_Types, column, recordTypeIndex) + 2;
                while (!ws.Cell(fieldTypeIndex, column).IsEmpty())
                {
                    fieldType _fieldType = _flatFiles.structureTypes.addFieldType(
                        ws.Cell(fieldTypeIndex, column).Value.ToString()
                    );

                    _fieldType.description = getCellValue(ws, fieldTypeIndex, column + 1);
                    _fieldType.dataType = getCellValue(ws, fieldTypeIndex, column + 2);
                    _fieldType.fieldFormat = getCellValue(ws, fieldTypeIndex, column + 3);
                    _fieldType.alignment = getCellValue(ws, fieldTypeIndex, column + 4);
                    _fieldType.padChar = getCellValue(ws, fieldTypeIndex, column + 5);
                    _fieldType.packType = getCellValue(ws, fieldTypeIndex, column + 6);
                    _fieldType.nullValues = getCellValue(ws, fieldTypeIndex, column + 7) == null ? null :
                        getCellValue(ws, fieldTypeIndex, column + 7).
                        Split(new string[] { Horizontal }, StringSplitOptions.None);

                    fieldTypeIndex += 1;
                }


                // Flat Files Keys
                {
                    ws = wb.Worksheet(4);


                    int candidateKeyIndex = searchSheet(ws, Excel.Section_CandidateKeys, column, fileTypeIndex) + 2;
                    while (!ws.Cell(candidateKeyIndex, column).IsEmpty())
                    {
                        recordType _recordType = _flatFiles.structureTypes.addRecordType(
                            ws.Cell(recordTypeIndex, column).Value.ToString()
                        );
                        recordTypeIndex += 1;
                    }
                }

                // Flat File Options
            }

            // Objects
            ws = wb.Worksheet(7);
            int objectIndex = searchSheet(ws, "Objects", column, 1);


            if (objectIndex > 1)
            {
                objectIndex += 2;
            }
            else
            {
                _dataset.dataObjects = null;
            }

            // Processes
            {
                ws = wb.Worksheet(8);

                flatFiles files = aml.dataset[0]?.flatFiles;
                if(files != null)
                {
                    Console.WriteLine($"searchSheet(ws, {Excel.File_Processes}, {column}, 1) + 2 ={searchSheet(ws, Excel.File_Processes, column, 1) + 2}");
                    int fileProcessesIndex = searchSheet(ws, Excel.File_Processes, column, 1) + 2;
                    flatFileProcesses _flatFileProcesses;
                    while (!ws.Cell(fileProcessesIndex, column).IsEmpty())
                    {
                        _flatFileProcesses = files.addFlatFileProcesses(getCellValue(ws, fileProcessesIndex, column + 1));
                        _flatFileProcesses.addProcess(getCellValue(ws, fileProcessesIndex, column));

                        fileProcessesIndex += 1;
                    }

                    Console.WriteLine($"searchSheet(ws, {Excel.Record_Processes}, {column}, 1) + 2 ={searchSheet(ws, Excel.Record_Processes, column, 1) + 2}");
                    int recordProcessesIndex = searchSheet(ws, Excel.Record_Processes, column, 1) + 2;
                    recordProcesses _recordProcesses;
                    while (!ws.Cell(recordProcessesIndex, column).IsEmpty())
                    {
                        _flatFileProcesses = files.addFlatFileProcesses(getCellValue(ws, recordProcessesIndex, column + 1));
                        _recordProcesses = _flatFileProcesses.addRecordProcesses(getCellValue(ws, recordProcessesIndex, column + 2));
                        _recordProcesses.addProcess(getCellValue(ws, recordProcessesIndex, column));

                        recordProcessesIndex += 1;
                    }

                    Console.WriteLine($"searchSheet(ws, {Excel.Field_Processes}, {column}, 1) + 2 ={searchSheet(ws, Excel.Field_Processes, column, 1) + 2}");
                    int fieldProcessesIndex = searchSheet(ws, Excel.Field_Processes, column, 1) + 2;
                    fieldProcesses _fieldProcesses;
                    while (!ws.Cell(fieldProcessesIndex, column).IsEmpty())
                    {
                        _flatFileProcesses = files.addFlatFileProcesses(getCellValue(ws, fieldProcessesIndex, column + 1));
                        _recordProcesses = _flatFileProcesses.addRecordProcesses(getCellValue(ws, fieldProcessesIndex, column + 2));
                        _fieldProcesses = _recordProcesses.addFieldProcesses(getCellValue(ws, fieldProcessesIndex, column + 3));
                        _fieldProcesses.addProcess(getCellValue(ws, fieldProcessesIndex, column));

                        Console.WriteLine($"Added field-process {getCellValue(ws, fieldProcessesIndex, column)} to" +
                            $"{getCellValue(ws, fieldProcessesIndex, column + 1)}/" +
                            $"{getCellValue(ws, fieldProcessesIndex, column + 2)}/" +
                            $"{getCellValue(ws, fieldProcessesIndex, column + 3)}");

                        fieldProcessesIndex += 1;
                    }
                }
            }


            // Write to file
            if (path != null)
                FileUtils.AddmlToFile(aml, path);
            return aml;
        }

        // Excel 2 Addml

        private static List<string[]> getAgents(additionalElement agentsElement)
        {
            List<string[]> agents = new List<string[]>();

            foreach (additionalElement agentElement in agentsElement.additionalElements.additionalElement)
            {
                agents.Add(new string[] {
                    agentElement.getElement("name")?.value,
                    agentElement.getProperty("type")?.value,
                    agentElement.getProperty("role")?.value,
                    string.Join(Vertical, agentElement.getElements("contact").Select(
                        c => c.getProperty("type").value + ": " + c.value
                    ).ToList())
                });

                if (agentElement.hasElement("agent"))
                {
                    foreach (additionalElement subagent in agentElement.getElements("agents"))
                        agents.AddRange(getAgents(agentElement));
                }
            }

            return agents;
        }

        private static List<char> columnIndex = new List<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());

        private static void AddLink(IXLWorksheet source, int sourceRow, int sourceColumn, string target, int targetRow, int targetColumn)
        {
            source.Cell(sourceRow, sourceColumn).Hyperlink = new XLHyperlink(
                $"'{target}'!{columnIndex[targetColumn]}{targetRow}"
            );
        }

        private static void AddRow(IXLWorksheet ws, int[] rc, string[] values)
        {
            AddRow(ws, rc[0], rc[1], values);
        }

        private static void AddRow(IXLWorksheet ws, int r, int c, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    ws.Cell(r, c + i).Value = values[i];
                }
            }
        }

        private static List<DataObjectTypes> GetDataObjects(dataObjects _dataObjects, string parent)
        {
            List<DataObjectTypes> objects = new List<DataObjectTypes>();

            if (_dataObjects.dataObject != null)
                foreach (dataObject _dataObject in _dataObjects.dataObject)
                {
                    objects.Add(new DataObjectTypes(_dataObject.name));

                    foreach (property type in _dataObject.properties)
                    {
                        List<List<string>> _object = GetProperties(type.properties, "");

                        if (type.value != null)
                        {
                            _object[0].Insert(0, $"type.value");
                            _object[1].Add($"{type.value}");
                        }

                        _object[0].Insert(0, "type");
                        _object[1].Insert(0, type.name);

                        _object[0].Insert(0, "parent-object");
                        _object[1].Insert(0, parent);

                        _object[0].Insert(0, "object-name");
                        _object[1].Insert(0, _dataObject.name);

                        objects.Last().addType(_object);
                    }

                    if (_dataObject.dataObjects != null)
                        objects.AddRange(GetDataObjects(_dataObject.dataObjects, _dataObject.name));
                }

            return objects;
        }

        private static List<List<string>> GetProperties(property[] _properties, string parent)
        {
            List<List<string>> objects = new List<List<string>>{
                new List<string>(),
                new List<string>(),
            };
            List<List<string>> subs;

            foreach (property _property in _properties)
            {
                if (_property.value != null)
                {
                    objects[0].Add($"{parent}/{_property.name}");
                    objects[1].Add(_property.value);
                }

                if (_property.properties != null)
                {
                    subs = GetProperties(_property.properties, $"{parent}/{_property.name}");
                    objects[0].AddRange(subs[0]);
                    objects[1].AddRange(subs[1]);
                }
            }

            return objects;
        }

        private class DataObjectTypes
        {
            string name { get; set; }
            List<List<List<string>>> properties { get; set; }
            public DataObjectTypes(string n)
            {
                name = n;
                properties = new List<List<List<string>>>();
            }
            public void addType(List<List<string>> _properties)
            {
                properties.Add(
                    _properties
                );
            }
            public List<string[]> getTypes()
            {
                List<string[]> types = new List<string[]>();
                foreach (List<List<string>> type in properties)
                {
                    types.Add(type[0].ToArray());
                    types.Add(type[1].ToArray());
                }
                return types;
            }
        }

        // Addml 2 Excel

        private static code[] getCodes(IXLWorksheet ws, int column, string recordDefinitionName, string fieldDefinitionName)
        {
            List<code> codes = new List<code>();
            code _code;

            int codesIndex = searchSheet(ws, Excel.Section_Codes, column, 1) + 2;

            while (!ws.Cell(codesIndex, column).IsEmpty())
            {
                if (ws.Cell(codesIndex, column).
                    Value.Equals(recordDefinitionName) &&
                    ws.Cell(codesIndex, column + 1).
                    Value.Equals(fieldDefinitionName)
                    )
                {
                    _code = new code();
                    _code.codeValue = getCellValue(ws, codesIndex, column + 2);
                    _code.explan = getCellValue(ws, codesIndex, column + 3);
                    codes.Add(_code);
                }
                codesIndex += 1;
            }

            return codes.ToArray();
        }

        private static string getCellValue(IXLWorksheet ws, int row, int column)
        {
            return
                ws.Cell(row, column).IsEmpty() ?
                null :
                ws.Cell(row, column).Value.ToString();
        }

        private static int searchSheet(IXLWorksheet ws, string fieldValue, int column, int start)
        {
            int endOfSheet = 0;
            int row = start;

            while (endOfSheet < 5)
            {
                if (ws.Cell(row, column).IsEmpty())
                {
                    endOfSheet += 1;
                }
                else
                {
                    endOfSheet = 0;
                    if (ws.Cell(row, column).Value.Equals(fieldValue))
                        return row;
                }

                row += 1;
            }

            return -1;
        }

        public static string getLanguage(string pathToExcelFile)
        {
            IXLWorkbook wb = new XLWorkbook(pathToExcelFile);

            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(wb.Properties.Comments);

            return values["lang"];
        }

        // Generator for templates

        public static void GenerateExcelTemplate(string pathOfExcelFile, string lang)
        {
            GenerateExcelTemplate("Noark-3", pathOfExcelFile, lang);
        }

        public static void GenerateExcelTemplate(string template, string pathOfExcelFile, string lang)
        {
            addml aml = AddmlUtils.ToAddml(GeneratorUtils.GetTemplate(template));
            ToExcel(aml, pathOfExcelFile, lang);
        }
    }
}
