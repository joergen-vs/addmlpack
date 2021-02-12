## AddmlPack
This is a toolbox under development, for quick and simple production of archival descriptions which follows the archival standard ADDML. 

## Modules

### Addml.Standard
Contains the addml-classes, with various schemas (xml-schema and document-type definition).

### Addml.Utils
Grouped support-functions, for manipulation of common objects like files, addml and projects.

### Addml.Spreadsheet
Supports the conversion of addml to and from Excel 2007+ (.xlsx, .xlsm), using the ClosedXML library from [github.com](https://github.com/ClosedXML/ClosedXML).
Elements from addml:
- [ ] context
- [ ] content
- [x] flatFile
- [x] flatFileDefinition
- [x] recordDefinition
- [ ] keys - currently working on
- [x] fieldDefinition
- [x] flatFileType
- [x] recordType
- [x] fieldType
- [ ] flatFileProcesses
- [ ] recordProcesses
- [ ] fieldProcesses
- [ ] processes
- [ ] dataObject

### Addml.API
Common interface for all tools in AddmlPack.

### Addml.CLI
Fully portable command-line interface.

## Processes
- generate: generate addml- or excel-file from template
- convert: transforms addml to and from excel
- help: shows documentation of and lists all implemented processes

All processes will show documentation with keyword -h or --help.

### generate
```markdown
Usage:
dotnet Addml.CLI.dll generate
    (-t | --type) (addml | excel)
    (-o | --output) (<output-file>)
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

### appendProcesses
```markdown
Usage:
dotnet Addml.CLI.dll appendProcesses
    (-t | --type) (addml | excel)
    (-i | --input) (<input-file>)
    (-o | --output) (<output-file>)
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

### file
- Analyse_CountRecords
- Analyse_CountChars
- Control_AllFixedLength
- Control_NumberOfRecords

### record
- Analyse_FindExtremeRecords
- Analyse_CountRecordDefinitionOccurences
- Analyse_AllFrequenceList
- Analyse_CrossTable
- Control_FixedLength
- Control_NotUsedRecordDef
- Control_Key
- Control_ForeignKey

### field
- Analyse_CountNULL
- Analyse_FindExtremeValues
- Analyse_FindMinMaxValue
- Analyse_FrequenceList
- Control_MinLength
- Control_MaxLength
- Control_DataFormat
- Control_NotNull
- Control_Uniqueness
- Control_Codes
