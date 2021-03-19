## AddmlPack
This is a toolbox under development, for quick and simple production of archival descriptions which follows the archival standard ADDML. 

[Releases](https://github.com/joergen-vs/addmlpack/releases)

## Modules

### AddmlPack.Standard
Contains the addml-classes, with various schemas (xml-schema and document-type definition).
- [x] class-objects
    - [x] v8.3
- [x] schemas
    - [x] v7.3 (DTD)
    - [x] v8.2 (XML-Schema)
    - [x] v8.3 (XML-Schema)

### AddmlPack.Utils
Grouped support-functions, for manipulation of common objects like files, addml and projects.

### AddmlPack.Spreadsheet
Supports the conversion of addml to and from Excel 2007+ (.xlsx), using the ClosedXML library from [github.com](https://github.com/ClosedXML/ClosedXML).
- [ ] transforming
    - [ ] context - currently working on
    - [ ] content - currently working on
    - [x] flatFile
    - [x] flatFileDefinition
    - [x] recordDefinition
    - [x] keys
    - [x] fieldDefinition
    - [x] flatFileType
    - [x] recordType
    - [x] fieldType
    - [x] flatFileProcesses
    - [x] recordProcesses
    - [x] fieldProcesses
    - [ ] processes
    - [ ] dataObject - currently working on
- [ ] prosess
    - [ ] checksums
    - [ ] processes
          - [x] flat files
          - [ ] data objects

### AddmlPack.API
Common interface for all tools in AddmlPack.

### AddmlPack.CLI
Fully portable command-line interface.

## Processes
- generate: generate addml- or excel-file from template
- convert: transforms addml to and from excel
- appendProcesses: Append a given or standard set of processes to file
- appendMetsInfo: Append agents from at DIAS-METS-file to file
- help: shows documentation of and lists all implemented processes

All processes will show documentation with keyword -h or --help.

### generate
```markdown
Usage:
dotnet Addml.CLI.dll generate
    (-t | --type) (addml | excel)
    (-o | --output) (<output-file>)
    [(-at | --archivetype) (<"Noark-3" | "Noark 5" | "Fagsystem">)]
    [(-l | --lang) (<Language>)]
dotnet Addml.CLI.dll generate (-h |--help)
```
Language is using the language tag listed on [docs.microsoft.com](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c). Currently, English and Norwegian (Bokmal) is implemented. If no language argument is given, the system-language will apply.

### convert
```markdown
Usage:
dotnet Addml.CLI.dll convert
    (-t | --type) (addml2excel | excel2addml)
    (-i | --input) (<input-file>)
    (-o | --output) (<output-file>)
    [(-l | --lang) (<Language>)]
dotnet Addml.CLI.dll convert (-h |--help)
```

### appendMetsInfo
```markdown
Usage:
dotnet Addml.CLI.dll appendMetsInfo
    (-t | --type) (addml | excel)
    (-i | --input) (<input-file>)
    (-o | --output) (<output-file>)
    (-mf | --metsfile) (<file-path>)
    [(-l | --lang) (<Language>)]
dotnet Addml.CLI.dll appendMetsInfo (-h |--help)
```

### appendProcesses
```markdown
Usage:
dotnet Addml.CLI.dll appendProcesses
    (-t | --type) (addml | excel)
    (-i | --input) (<input-file>)
    (-o | --output) (<output-file>)
    [(-co | --customoptions) (<json-string> | <file-path>)]
    [(-l | --lang) (<Language>)]
dotnet Addml.CLI.dll appendProcesses (-h |--help)
```
The keyword customoptions takes a string or filepath of a json-structure, to apply some text-heavy options. If omitted, a standard set of processes is added. Currently uses following format:

```json
{
  "processes": {
      "file": [
          "Control_NumberOfRecords"
      ]
  }
}
```
Addml supports processes on four levels, with work done on three; file, record and field. See below for complete list.

| Level | Name | Description | Part of standard set |
| ----------- | ----------- | ----------- | ----------- |
| file | Analyse_CountRecords | "Description" | Yes |
| file | Analyse_CountChars | "Description" | No |
| file | Control_AllFixedLength | "Description" | No |
| file | Control_NumberOfRecords | "Description" | Yes |
| record | Analyse_FindExtremeRecords | "Description" | No |
| record | Analyse_CountRecordDefinitionOccurences | "Description" | Yes |
| record | Analyse_AllFrequenceList | "Description" | No |
| record | Analyse_CrossTable | "Description" | No |
| record | Control_FixedLength | "Description" | Yes |
| record | Control_NotUsedRecordDef | "Description" | Yes |
| record | Control_Key | "Description" | Yes |
| record | Control_ForeignKey | "Description" | Yes |
| field | Analyse_CountNULL | "Description" | Yes |
| field | Analyse_FindExtremeValues | "Description" | Yes |
| field | Analyse_FindMinMaxValue | "Description" | Yes |
| field | Analyse_FrequenceList | "Description" | Yes |
| field | Control_MinLength | "Description" | Yes |
| field | Control_MaxLength | "Description" | Yes |
| field | Control_DataFormat | "Description" | Yes |
| field | Control_NotNull | "Description" | Yes |
| field | Control_Uniqueness | "Description" | Yes |
| field | Control_Codes | "Description" | Yes |

