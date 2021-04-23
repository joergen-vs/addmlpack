using AddmlPack.Standards.Addml.Classes.v8_3;
using AddmlPack.Utils.Spreadsheet.Languages;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace AddmlPack.Utils.SpreadsheetUtils
{
    public class SpreadsheetUtils
    {
        private static string[] horizontal = new string[] { ", " };
        private static string[] vertical = new string[] { "\r\n" };
        private static string[] Horizontal { get { return horizontal; } }
        private static string[] Vertical { get { return vertical; } }

        public static addml FromSpreadsheet(string pathOfExcelFile)
        {
            var workbook = new XLWorkbook(pathOfExcelFile);
            return ToAddml(workbook, pathOfExcelFile);
        }

        public static object ToSpreadsheet(addml aml, string path, string lang)
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

            Dictionary<string, int> indexes = new Dictionary<string, int>();
            Dictionary<string, string> NameToId = new Dictionary<string, string>();

            XLWorkbook wb = new XLWorkbook();
            wb.Properties.Comments = $"{{\"lang\":\"{Thread.CurrentThread.CurrentCulture.Name}\"," +
                $"\"addml-version\":\"8.3\"}}";

            // Dataset
            var ws = wb.AddWorksheet(Excel.Sheet_Dataset);
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
            row += 1;

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
                    if (agent.hasElement("contact"))
                    {
                        foreach(additionalElement e in agent.getElements("contact"))
                        {
                            AddRow(ws, row, column, new string[] {
                            agent.getElement("name")?.value,
                            agent.getProperty("type")?.value,
                            agent.getProperty("role")?.value,
                            e.value,
                            e.getProperty("type")?.value,
                        });
                            row += 1;
                        }
                    }else { 
                        AddRow(ws, row, column, new string[] {
                            agent.getElement("name").value,
                            agent.getProperty("type")?.value,
                            agent.getProperty("role")?.value
                        });
                        row += 1;
                    }
                }
            }
            row += 2;
            AddSection(ws, row, column, new string[]
            {
                Excel.Reference_System_Name,
                Excel.Reference_System_Type,
                Excel.Reference_Archive,
                Excel.Reference_ArchivalPeriod_Start,
                Excel.Reference_ArchivalPeriod_End,
            }, new string[]
            {
                aml.dataset?[0].reference?.context?.additionalElements?
                    .getElement("systemName")?.value,
                aml.dataset?[0].reference?.context?.additionalElements?
                .getElement("systemType")?.value,
                aml.dataset?[0].reference?.context?.additionalElements?
                .getElement("archive")?.value,
                aml.dataset?[0].reference?.content?.additionalElements?
                .getElement("archivalPeriod")?.getProperty("startDate")?.value,
                aml.dataset?[0].reference?.content?.additionalElements?
                .getElement("archivalPeriod")?.getProperty("endDate")?.value
            });
            row += 1;

            ws.Columns().AdjustToContents();

            flatFiles files = aml.dataset[0].flatFiles;

            // Flat Files
            if (files != null)
            {

                ws = wb.AddWorksheet(Excel.Sheet_FlatFiles);
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
                    Excel.File_ChecksumValue,
                    Excel.File_Process
                });
                row += 1;

                indexes["Flat Files/Files"] = row;

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

                ws.Columns().AdjustToContents();

                // Definitions
                ws = wb.AddWorksheet(Excel.Sheet_Definitions);
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
                    Excel.Record_Definition_Process
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
                                Console.WriteLine($"_recordDefinition.keys.Length={_recordDefinition.keys.Length}");
                                foreach (key _key in _recordDefinition.keys)
                                {
                                    Console.WriteLine(_key.Item.GetType().Name);
                                    if (_key.Item.GetType().IsEquivalentTo(typeof(primaryKey)))
                                    {
                                        allPrimaryKeys.Add(new string[]
                                        {
                                            _key.name,
                                            _flatFileDefinition.name,
                                            _recordDefinition.name,
                                            Excel.Keys_PrimaryKey,
                                            string.Join(Horizontal[0], _key.fieldDefinitionReferences.Select(
                                                c => c.name).ToList())
                                        });
                                    }
                                    else if (_key.Item.GetType().IsEquivalentTo(typeof(foreignKey)))
                                    {
                                        foreignKey _foreignKey = (foreignKey)_key.Item;
                                        allForeignKeys.Add(new string[]
                                        {
                                            _key.name,
                                            _flatFileDefinition.name,
                                            _recordDefinition.name,
                                            string.Join(Horizontal[0], _key.fieldDefinitionReferences.Select(
                                                c => c.name).ToList()),
                                            _foreignKey.relationType,
                                            _foreignKey.flatFileDefinitionReference.name,
                                            _foreignKey.flatFileDefinitionReference.recordDefinitionReferences[0].name,
                                            string.Join(Horizontal[0], _foreignKey.flatFileDefinitionReference
                                                .recordDefinitionReferences[0].fieldDefinitionReferences.Select(
                                                c => c.name).ToList())
                                        });
                                    }
                                    else if (_key.Item.GetType().IsEquivalentTo(typeof(alternateKey)))
                                    {
                                        allPrimaryKeys.Add(new string[]
                                        {
                                            _key.name,
                                            _flatFileDefinition.name,
                                            _recordDefinition.name,
                                            Excel.Keys_AlternateKey,
                                            string.Join(Horizontal[0], _key.fieldDefinitionReferences.Select(
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
                Excel.Field_Definition_Process
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
                ws = wb.AddWorksheet(Excel.Sheet_Types);
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
                            recordSeparator = TextUtils.fromSeparator(((fixedFileFormat)fileformat).recordSeparator);
                        }
                        else
                        {
                            recordSeparator = TextUtils.fromSeparator(((delimFileFormat)fileformat).recordSeparator);
                            fieldSeparator = TextUtils.fromSeparator(((delimFileFormat)fileformat).fieldSeparatingChar);
                            quotingChar = TextUtils.fromSeparator(((delimFileFormat)fileformat).quotingChar);
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
                                    if (_recordDefinition.typeReference != null &&
                                        _recordDefinition.typeReference.Equals(_recordType.name))
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
                            string.Join(Horizontal[0], _fieldType.nullValues)

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
                ws = wb.AddWorksheet(Excel.Sheet_Keys);
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
                Excel.File_Definition_Reference,
                Excel.Record_Definition_Reference,
                Excel.Key_PrimaryOrAlternate,
                Excel.Field_Definition_Reference
            });
                row += 1;

                string[] tmp = new string[4];

                foreach (string[] tuple in allPrimaryKeys)
                {
                    AddRow(ws, row, column, tuple);
                    AddLink(ws, row, column + 1, Excel.Sheet_Definitions, indexes[$"Definitions/Files/{tuple[1]}"], column);
                    AddLink(ws, row, column + 2, Excel.Sheet_Definitions, indexes[$"Definitions/Records/{tuple[1]}.{tuple[2]}"], column);

                    row += 1;
                }
                row += 1;

                ws.Cell(row, column).Value = Excel.Section_ForeignKeys;
                row += 1;

                AddRow(ws, row, column, new string[] {
                Excel.Name,
                Excel.File_Definition_Reference,
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
                    AddRow(ws, row, column, tuple);
                    AddLink(ws, row, column + 1, Excel.Sheet_Definitions, indexes[$"Definitions/Files/{tuple[1]}"], column);
                    AddLink(ws, row, column + 2, Excel.Sheet_Definitions, indexes[$"Definitions/Records/{tuple[1]}.{tuple[2]}"], column);
                    AddLink(ws, row, column + 5, Excel.Sheet_Definitions, indexes[$"Definitions/Files/{tuple[5]}"], column);
                    AddLink(ws, row, column + 6, Excel.Sheet_Definitions, indexes[$"Definitions/Records/{tuple[5]}.{tuple[6]}"], column);

                    row += 1;
                }

                ws.Columns().AdjustToContents();

                // Flat File Other Options
                ws = wb.AddWorksheet(Excel.Sheet_FlatFiles_OtherOptions);
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

            }

            // Object-structure and root-object [only if 'archive'-type]

            ws = wb.AddWorksheet(Excel.Sheet_Objects_Structure);
            row = 3;
            column = 1;

            List<InternalDataObject> dataObjects = aml.dataset[0].dataObjects?.dataObject != null ?
                GetDataObjects(aml.dataset[0].dataObjects, "") :
                new List<InternalDataObject>();

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

            ws.Cell(row, column).Value = Excel.Section_Object_Structure;
            row += 1;

            AddRow(ws, row, column, new string[]{
                Excel.Name, Excel.Object_Parent, Excel.Object_Type
            });
            row += 1;

            Dictionary<string, int> types = new Dictionary<string, int>();

            foreach (string[] tuple in ListDataObjects(dataObjects))
            {
                AddRow(ws, row, column, tuple);
                row += 1;
                if (types.ContainsKey(tuple[2]))
                    types[tuple[2]] += 1;
                else
                    types[tuple[2]] = 1;
            }
            row += 1;

            if (types.ContainsKey("archive"))
            {
                foreach (InternalDataObject _dataObject in dataObjects)
                {
                    if (_dataObject.Type == "archive")
                    {


                        ws.Cell(row, column).Value = Excel.Section_Object_Archive;
                        row += 1;

                        AddSection(ws, row, column, new string[]{
                            Excel.Name,
                            Excel.Object_Archive_Type,
                            Excel.Object_Archive_TypeVersion,
                            Excel.Object_Archive_Period_IngoingSeparation,
                            Excel.Object_Archive_Period_OutgoingSeparation,
                            Excel.Object_Archive_ContainsRestrictedEntries,
                            Excel.Object_Archive_IncludeDisposedDocuments,
                            Excel.Object_Archive_ContainsDisposalResolutionsForDocuments,
                            Excel.Object_Archive_ContainsCompanySpecificMetadata,
                            Excel.Object_Archive_NumberOfDocuments
                        }, new string[]{
                            _dataObject.Name,
                            _dataObject.DataObject.getProperty("info")?.getProperty("type")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("type")?
                            .getProperty("version")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("periode")?.getProperty("inngaaendeSkille")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("periode")?.getProperty("utgaaendeSkille")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("inneholderSkjermetInformasjon")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("omfatterDokumenterSomErKassert")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("inneholderDokumenterSomSkalKasseres")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("inneholderVirksomhetsspesifikkeMetadata")?.value,
                            _dataObject.DataObject.getProperty("info")?.getProperty("additionalInfo")?
                            .getProperty("antallDokumentfiler")?.value
                        });

                        row += 1;
                        break;
                    }
                }
            }

            ws.Columns().AdjustToContents();

            // Objects - XML

            if (types.ContainsKey("xml file"))
            {
                ws = wb.AddWorksheet(Excel.Sheet_Objects_XML);
                row = 3;
                column = 1;

                ws.Cell(row, column).Value = string.Format(
                    Excel.InDataset,
                    Excel.DataObjects);
                row += 1;

                ws.Cell(row, column).Value = string.Format(
                    Excel.NumberInUse,
                    Excel.DataObjects.ToLower(),
                    "" + types["xml file"]
                );
                row += 2;

                ws.Cell(row, column).Value = Excel.Section_Object_Xml_File;
                row += 1;

                AddRow(ws, row, column, TypeRepresentationHeader("file"));
                row += 1;

                foreach (InternalDataObject _dataObject in dataObjects)
                {
                    foreach (property _property in _dataObject.DataObject.getProperties("file", true))
                    {
                        AddRow(ws, row, column, TypeRepresentation(_dataObject.Name, _property));
                        row += 1;
                    }
                }
                row += 1;

                ws.Cell(row, column).Value = Excel.Section_Object_Xml_Schema;
                row += 1;

                AddRow(ws, row, column, TypeRepresentationHeader("schema"));
                row += 1;

                foreach (InternalDataObject _dataObject in dataObjects)
                {
                    foreach (property _property in _dataObject.DataObject.getProperties("schema"))
                    {
                        AddRow(ws, row, column, TypeRepresentation(_dataObject.Name, _property));
                        row += 1;
                    }
                }
                row += 1;

                ws.Cell(row, column).Value = Excel.Section_Object_Xml_NumberOfOccurrences;
                row += 1;

                AddRow(ws, row, column, TypeRepresentationHeader("numberOfOccurrences"));
                row += 1;

                foreach (InternalDataObject _dataObject in dataObjects)
                {
                    foreach (property _property in _dataObject.DataObject.getProperties("numberOfOccurrences", true))
                    {
                        AddRow(ws, row, column, TypeRepresentation(_dataObject.Name, _property));
                        row += 1;
                    }
                }
                row += 1;

                ws.Columns().AdjustToContents();
            }

            // Processes

            ws = wb.AddWorksheet(Excel.Sheet_Processes);
            row = 3;
            column = 1;

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

                    if (_flatFileProcesses.recordProcesses != null)
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
            indexes["Processes/File"] = row;
            row += 1;

            foreach (string[] tuple in fileProcesses)
            {
                try
                {
                    wb.Worksheet(Excel.Sheet_FlatFiles).Cell(
                        indexes[$"Flat Files/Files/{tuple[1]}"],
                        column + 6).Value = true;
                    AddLink(
                        wb.Worksheet(Excel.Sheet_FlatFiles),
                        indexes[$"Flat Files/Files/{tuple[1]}"],
                        column + 6, Excel.Sheet_Processes,
                        indexes["Processes/File"], column);

                    AddRow(ws, row, column, tuple);

                    row += 1;
                }
                catch (Exception) { }
            }

            row += 2;

            ws.Cell(row, column).Value = Excel.Record_Processes;
            row += 1;

            AddRow(ws, row, column, new string[] {
                    Excel.Name,
                    Excel.File_Reference,
                    Excel.Record_Definition_Reference
                });
            indexes["Processes/Record"] = row;
            row += 1;

            foreach (string[] tuple in recordProcesses)
            {
                AddLink(
                    wb.Worksheet(Excel.Sheet_Definitions),
                    indexes[$"Definitions/Records/{tuple[1]}.{tuple[2]}"],
                    column + 9, Excel.Sheet_Definitions,
                    indexes["Processes/Record"], column
                );

                AddRow(ws, row, column, tuple);

                row += 1;
            }

            row += 2;

            ws.Cell(row, column).Value = Excel.Field_Processes;
            indexes["Processes/Field"] = row;
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
                AddLink(
                    wb.Worksheet(Excel.Sheet_Definitions),
                    indexes[$"Definitions/Fields/{tuple[1]}.{tuple[2]}.{tuple[3]}"],
                    column + 3, Excel.Sheet_Types, indexes["Processes/Field"], column);
                AddRow(ws, row, column, tuple);

                row += 1;
            }

            ws.Columns().AdjustToContents();


            wb.SaveAs(path);

            return wb;
        }

        public static addml ToAddml(XLWorkbook wb, string source)
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
            Console.WriteLine($"Worksheet({Excel.Sheet_Dataset})");
            ws = wb.Worksheet(Excel.Sheet_Dataset);

            dataset _dataset = aml.addDataset(GeneratorUtils.NewGUID(), "Fagsystem");
            _dataset.reference = new reference();

            Console.WriteLine($"Worksheet({Excel.Sheet_FlatFiles})");
            ws = wb.Worksheet(Excel.Sheet_FlatFiles);
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
                Console.WriteLine($"Worksheet({Excel.Sheet_Definitions})");
                ws = wb.Worksheet(Excel.Sheet_Definitions);
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
                                        _fieldDefinition.codes = getCodes(
                                            wb.Worksheet(Excel.Sheet_FlatFiles_OtherOptions),
                                            column, _recordDefinition.name, _fieldDefinition.name);
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
                Console.WriteLine($"Worksheet({Excel.Sheet_Types})");
                ws = wb.Worksheet(Excel.Sheet_Types);

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
                        Split(Horizontal, StringSplitOptions.None);

                    fieldTypeIndex += 1;
                }


                // Flat Files Keys
                {
                    //Console.WriteLine($"Worksheet({Excel.Sheet_Keys})");
                    ws = wb.Worksheet(Excel.Sheet_Keys);

                    int keyIndex = searchSheet(ws, Excel.Section_CandidateKeys, column, 1) + 2;
                    while (keyIndex > 1 && !ws.Cell(keyIndex, column).IsEmpty())
                    {
                        foreach (flatFileDefinition flatFileDefinition_ in _flatFiles.flatFileDefinitions)
                        {
                            if (flatFileDefinition_.name.Equals(getCellValue(ws, keyIndex, column + 1)))
                            {
                                foreach (recordDefinition recordDefinition_ in flatFileDefinition_.recordDefinitions)
                                {
                                    if (recordDefinition_.name.Equals(getCellValue(ws, keyIndex, column + 2)))
                                    {
                                        key key_ = recordDefinition_.addKey(getCellValue(ws, keyIndex, column));
                                        if (getCellValue(ws, keyIndex, column + 3).Equals(Excel.Keys_PrimaryKey))
                                        {
                                            key_.Item = new primaryKey();
                                        }
                                        else
                                        {
                                            key_.Item = new alternateKey();
                                        }

                                        foreach (string fieldReference in getCellValue(ws, keyIndex, column + 4).
                                            Split(Horizontal, StringSplitOptions.None))
                                        {
                                            key_.addFieldDefinitionReference(fieldReference);
                                        }
                                    }
                                }
                            }
                        }

                        keyIndex += 1;
                    }

                    keyIndex = searchSheet(ws, Excel.Section_ForeignKeys, column, 1) + 2;
                    while (keyIndex > 1 && !ws.Cell(keyIndex, column).IsEmpty())
                    {
                        foreach (flatFileDefinition flatFileDefinition_ in _flatFiles.flatFileDefinitions)
                        {
                            if (flatFileDefinition_.name.Equals(getCellValue(ws, keyIndex, column + 1)))
                            {
                                foreach (recordDefinition recordDefinition_ in flatFileDefinition_.recordDefinitions)
                                {
                                    if (recordDefinition_.name.Equals(getCellValue(ws, keyIndex, column + 2)))
                                    {
                                        key key_ = recordDefinition_.addKey(getCellValue(ws, keyIndex, column));

                                        foreignKey foreignKey_ = new foreignKey();
                                        foreignKey_.relationType = getCellValue(ws, keyIndex, column + 4);
                                        foreignKey_.flatFileDefinitionReference = new flatFileDefinitionReference();
                                        foreignKey_.flatFileDefinitionReference.name = getCellValue(ws, keyIndex, column + 5);

                                        recordDefinitionReference recordDefinitionReference_ = new recordDefinitionReference();
                                        recordDefinitionReference_.name = getCellValue(ws, keyIndex, column + 6);
                                        foreach (string fieldReference in getCellValue(ws, keyIndex, column + 7).
                                            Split(Horizontal, StringSplitOptions.None))
                                        {
                                            recordDefinitionReference_.addFieldDefinitionReference(fieldReference);
                                        }

                                        foreignKey_.flatFileDefinitionReference.recordDefinitionReferences =
                                            new recordDefinitionReference[] { recordDefinitionReference_ };

                                        key_.Item = foreignKey_;


                                        foreach (string fieldReference in getCellValue(ws, keyIndex, column + 3).
                                            Split(Horizontal, StringSplitOptions.None))
                                        {
                                            key_.addFieldDefinitionReference(fieldReference);
                                        }
                                    }
                                }
                            }
                        }

                        keyIndex += 1;
                    }
                }

                // Flat File Options
            }

            // Objects
            //Console.WriteLine($"Worksheet({Excel.Sheet_Objects_Structure})");
            ws = wb.Worksheet(Excel.Sheet_Objects_Structure);
            int objectIndex = searchSheet(ws, Excel.Section_Object_Structure, column, 1);


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
                //Console.WriteLine($"Worksheet({Excel.Sheet_Processes})");
                ws = wb.Worksheet(Excel.Sheet_Processes);

                flatFiles files = aml.dataset[0]?.flatFiles;
                if (files != null)
                {
                    //Console.WriteLine($"searchSheet(ws, {Excel.File_Processes}, {column}, 1) + 2 ={searchSheet(ws, Excel.File_Processes, column, 1) + 2}");
                    int fileProcessesIndex = searchSheet(ws, Excel.File_Processes, column, 1) + 2;
                    flatFileProcesses _flatFileProcesses;
                    while (!ws.Cell(fileProcessesIndex, column).IsEmpty())
                    {
                        _flatFileProcesses = files.addFlatFileProcesses(getCellValue(ws, fileProcessesIndex, column + 1));
                        _flatFileProcesses.addProcess(getCellValue(ws, fileProcessesIndex, column));

                        fileProcessesIndex += 1;
                    }

                    //Console.WriteLine($"searchSheet(ws, {Excel.Record_Processes}, {column}, 1) + 2 ={searchSheet(ws, Excel.Record_Processes, column, 1) + 2}");
                    int recordProcessesIndex = searchSheet(ws, Excel.Record_Processes, column, 1) + 2;
                    recordProcesses _recordProcesses;
                    while (!ws.Cell(recordProcessesIndex, column).IsEmpty())
                    {
                        _flatFileProcesses = files.addFlatFileProcesses(getCellValue(ws, recordProcessesIndex, column + 1));
                        _recordProcesses = _flatFileProcesses.addRecordProcesses(getCellValue(ws, recordProcessesIndex, column + 2));
                        _recordProcesses.addProcess(getCellValue(ws, recordProcessesIndex, column));

                        recordProcessesIndex += 1;
                    }

                    //Console.WriteLine($"searchSheet(ws, {Excel.Field_Processes}, {column}, 1) + 2 ={searchSheet(ws, Excel.Field_Processes, column, 1) + 2}");
                    int fieldProcessesIndex = searchSheet(ws, Excel.Field_Processes, column, 1) + 2;
                    fieldProcesses _fieldProcesses;
                    while (!ws.Cell(fieldProcessesIndex, column).IsEmpty())
                    {
                        _flatFileProcesses = files.addFlatFileProcesses(getCellValue(ws, fieldProcessesIndex, column + 1));
                        _recordProcesses = _flatFileProcesses.addRecordProcesses(getCellValue(ws, fieldProcessesIndex, column + 2));
                        _fieldProcesses = _recordProcesses.addFieldProcesses(getCellValue(ws, fieldProcessesIndex, column + 3));
                        _fieldProcesses.addProcess(getCellValue(ws, fieldProcessesIndex, column));

                        //Console.WriteLine($"Added field-process {getCellValue(ws, fieldProcessesIndex, column)} to" +
                        //    $"{getCellValue(ws, fieldProcessesIndex, column + 1)}/" +
                        //    $"{getCellValue(ws, fieldProcessesIndex, column + 2)}/" +
                        //    $"{getCellValue(ws, fieldProcessesIndex, column + 3)}");

                        fieldProcessesIndex += 1;
                    }
                }
            }

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
                    string.Join(Vertical[0], agentElement.getElements("contact").Select(
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
                $"'{target}'!{columnIndex[targetColumn - 1]}{targetRow}"
            );
        }

        private static void AddSection(IXLWorksheet ws, int r, int c, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    ws.Cell(r + i, c).Value = values[i];
                }
            }
        }

        private static void AddSection(IXLWorksheet ws, int r, int c, string[] keys, string[] values)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != null)
                {
                    ws.Cell(r + i, c).Value = keys[i];
                }
            }
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    ws.Cell(r + i, c + 1).Value = values[i];
                }
            }
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

        private static List<InternalDataObject> GetDataObjects(dataObjects _dataObjects, string parent)
        {
            List<InternalDataObject> list = new List<InternalDataObject>();
            foreach (dataObject _dataObject in _dataObjects.dataObject)
            {
                list.Add(new InternalDataObject(_dataObject, parent));

                if (_dataObject.dataObjects != null)
                    list.AddRange(GetDataObjects(_dataObject.dataObjects, list.Last().Name));
            }
            return list;
        }

        private class InternalDataObject
        {
            public InternalDataObject(dataObject dataObject, string parent)
            {
                DataObject = dataObject;
                Parent = parent;
            }

            public dataObject DataObject { get; set; }
            public string Parent { get; set; }
            public string Name { get { return DataObject.name; } }
            public string Type
            {
                get
                {
                    if (DataObject.getProperty("type")?.value != null)
                    {
                        return DataObject.getProperty("type")?.value;
                    }
                    if (DataObject.getProperty("file") != null)
                    {
                        if (DataObject.getProperty("file")?.getProperty("format")?.value != null)
                            return DataObject.getProperty("file")?.getProperty("format")?.value.ToLower() + " file";
                    }

                    return "archive";
                }
            }
        }

        private static string[] TypeRepresentation(string dataObject, property _property)
        {
            switch (_property.name)
            {
                case "file":
                    return new string[]
                    {
                        dataObject,
                        _property.getProperty("name").value,
                        _property.getProperty("format")?.value,
                        _property.getProperty("format")?.getProperty("version")?.value,
                        _property.getProperty("checksum")?.getProperty("algorithm")?.value,
                        _property.getProperty("checksum")?.getProperty("value")?.value,
                    };
                case "schema":
                    return new string[]
                    {
                        dataObject,
                        _property.value != null ? _property.value : null,
                        _property.getProperty("file")?.getProperty("name")?.value,
                        _property.getProperty("type")?.value,
                        _property.getProperty("type")?.getProperty("version")?.value
                    };
                case "numberOfOccurrences":
                    return new string[]
                    {
                        dataObject,
                        _property.value,
                        _property.properties[0].name,
                        _property.properties[0].value,
                        _property.properties[1].value
                    };
                default:
                    return new string[] { };
            }
        }

        private static string[] TypeRepresentationHeader(string _property)
        {
            switch (_property)
            {
                case "file":
                    return new string[]
                    {
                        "dataObject",
                        "name",
                        "format",
                        "format-version",
                        "checksum-algorithm",
                        "checksum-value",
                    };
                case "schema":
                    return new string[]
                    {
                        "dataObject",
                        "value",
                        "name",
                        "type",
                        "type-version"
                    };
                case "numberOfOccurrences":
                    return new string[]
                    {
                        "dataObject",
                        "value",
                        "filter",
                        "filter-value",
                        "Count"
                    };
                default:
                    return new string[] { };
            }
        }

        private static List<string[]> ListDataObjects(List<InternalDataObject> objects)
        {
            List<string[]> list = new List<string[]>();
            foreach (InternalDataObject _object in objects)
            {
                list.Add(new string[]
                {
                    _object.Name, _object.Parent, _object.Type
                });
            }
            return list;
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
    }
}
