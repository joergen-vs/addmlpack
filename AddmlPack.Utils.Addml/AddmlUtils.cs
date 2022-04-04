using AddmlPack.Standards.Addml;
using AddmlPack.Standards.Addml.Classes.v7_3;
using AddmlPack.Standards.Addml.Classes.v8_3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AddmlPack.Utils.Addml
{
    public class AddmlUtils
    {
        public static string FromAddml(addml aml)
        {
            var serializer = new XmlSerializer(typeof(addml));

            using (CustomStringWriter sw = new CustomStringWriter())
            {
                using (XmlTextWriter xw = new XmlTextWriter(sw))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "http://www.arkivverket.no/standarder/addml");

                    xw.Formatting = Formatting.Indented;
                    xw.IndentChar = '\t';

                    serializer.Serialize(xw, aml, ns);

                    return PrettyPrintXML(sw.ToString());
                }
            }
        }

        public static string FromAddml(addml aml, string version)
        {
            var serializer = new XmlSerializer(typeof(addml));

            using (CustomStringWriter sw = new CustomStringWriter())
            {
                using (XmlTextWriter xw = new XmlTextWriter(sw))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "http://www.arkivverket.no/standarder/addml");

                    xw.Formatting = Formatting.Indented;
                    xw.IndentChar = '\t';

                    serializer.Serialize(xw, aml, ns);

                    return PrettyPrintXML(sw.ToString());
                }
            }
        }

        public static addml ToAddml(string objectData)
        {
            return ToAddml(objectData, AddmlVersionsImplemented.Latest);
        }

        public static addml ToAddml(string objectData, string version)
        {
            Type amlType = null;
            if (!AddmlVersionsImplemented.Instance.Versions.Contains(version))
            {
                return null;
            }
            if (version == AddmlVersionsImplemented.V8_3 ||
                version == AddmlVersionsImplemented.Latest)
                amlType = typeof(Standards.Addml.Classes.v8_3.addml);

            var serializer = new XmlSerializer(amlType);
            addml aml = null;

            TextReader reader = null;
            try
            {
                reader = new StringReader(objectData);
                aml = (addml)serializer.Deserialize(reader);
            }
            catch (InvalidOperationException e)
            {
                string error = e.Message + ": " + e.InnerException?.Message;
                Console.WriteLine(error);
            }
            finally
            {
                reader?.Close();
            }

            return aml;
        }

        public static string PrettyPrintXML(string xml)
        {
            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                mStream.Close();
                writer.Close();

                return formattedXml;
            }
            catch (XmlException)
            {
                mStream.Close();
                writer.Close();

                return null;
            }
        }

        public static void AppendProcesses(addml aml)
        {
            Dictionary<string, List<string>> D = new Dictionary<string, List<string>>();

            D.Add("file", new List<string>
            {
                "Analyse_CountRecords",
                "Control_NumberOfRecords",
            });
            D.Add("record", new List<string>
            {
                "Analyse_CountRecordDefinitionOccurences",
                "Control_FixedLength",
                "Control_NotUsedRecordDef",
                "Control_Key",
                "Control_ForeignKey",
            });
            D.Add("field", new List<string>
            {
                "Analyse_CountNULL",
                "Analyse_FindExtremeValues",
                "Analyse_FindMinMaxValues",
                "Analyse_FrequenceList",
                "Control_MinLength",
                "Control_MaxLength",
                "Control_DataFormat",
                "Control_NotNull",
                "Control_Uniqueness",
                "Control_Codes",
            });

            AppendProcesses(aml, D);
        }

        public static void AppendProcesses(addml aml, Dictionary<string, string[]> processesToAppend)
        {
            Dictionary<string, List<string>> D = new Dictionary<string, List<string>>();
            foreach (string key in processesToAppend.Keys)
                D.Add(key, new List<string>(processesToAppend[key]));

            AppendProcesses(aml, D);
        }

        public static void AppendProcesses(addml aml, Dictionary<string, List<string>> processesToAppend)
        {
            Console.WriteLine("AppendProcesses");
            flatFiles _flatFiles = aml.dataset[0].flatFiles;
            if (_flatFiles == null)
                return;

            foreach (flatFile _flatFile in _flatFiles.flatFile)
            {
                flatFileProcesses _flatFileProcess = null;

                if (_flatFiles.flatFileProcesses != null)
                {
                    foreach (flatFileProcesses flatFileProcess in _flatFiles.flatFileProcesses)
                    {
                        if (flatFileProcess.flatFileReference == _flatFile.definitionReference)
                        {
                            _flatFileProcess = flatFileProcess;
                            break;
                        }
                    }
                }

                if (_flatFileProcess == null)
                    _flatFileProcess = _flatFiles.addFlatFileProcesses(_flatFile.definitionReference);

                foreach (flatFileDefinition _flatFileDefinition in _flatFiles.flatFileDefinitions)
                {
                    if (_flatFileDefinition.name.Equals(_flatFile.definitionReference))
                    {
                        foreach (recordDefinition _recordDefinition in _flatFileDefinition.recordDefinitions)
                        {
                            recordProcesses _recordProcess = null;

                            if (_flatFileProcess.recordProcesses != null)
                                foreach (recordProcesses recordProcess in _flatFileProcess.recordProcesses)
                                {
                                    if (recordProcess.definitionReference == _recordDefinition.name)
                                    {
                                        _recordProcess = recordProcess;
                                        break;
                                    }
                                }

                            if (_recordProcess == null)
                                _recordProcess = _flatFileProcess.addRecordProcesses(_recordDefinition.name);

                            foreach (fieldDefinition _fieldDefinition in _recordDefinition.fieldDefinitions)
                            {
                                fieldProcesses _fieldProcess = null;

                                if (_recordProcess.fieldProcesses != null)
                                    foreach (fieldProcesses fieldProcess in _recordProcess.fieldProcesses)
                                    {
                                        if (fieldProcess.definitionReference == _fieldDefinition.name)
                                        {
                                            _fieldProcess = fieldProcess;
                                            break;
                                        }
                                    }

                                if (_fieldProcess == null)
                                    _fieldProcess = _recordProcess.addFieldProcesses(_fieldDefinition.name);

                                // Add field-processes
                                //Analyse_CountNULL
                                if (processesToAppend["field"].Contains("Analyse_CountNULL"))
                                    if (_fieldDefinition.notNull == null)
                                        _fieldProcess.addProcess("Analyse_CountNULL");

                                //Analyse_FindExtremeValues
                                if (processesToAppend["field"].Contains("Analyse_FindExtremeValues"))
                                    _fieldProcess.addProcess("Analyse_FindExtremeValues");

                                //Analyse_FindMinMaxValues
                                if (processesToAppend["field"].Contains("Analyse_FindMinMaxValues"))
                                    _fieldProcess.addProcess("Analyse_FindMinMaxValues");

                                //Analyse_FrequenceList
                                if (processesToAppend["field"].Contains("Analyse_FrequenceList"))
                                    if (_fieldDefinition.codes != null)
                                        _fieldProcess.addProcess("Analyse_FrequenceList");

                                //Control_MinLength
                                if (processesToAppend["field"].Contains("Control_MinLength"))
                                    if (_fieldDefinition.minLength != null)
                                        _fieldProcess.addProcess("Control_MinLength");

                                //Control_MaxLength
                                if (processesToAppend["field"].Contains("Control_MaxLength"))
                                    if (_fieldDefinition.maxLength != null)
                                        _fieldProcess.addProcess("Control_MaxLength");

                                //Control_DataFormat
                                if (processesToAppend["field"].Contains("Control_DataFormat"))
                                {
                                    fieldType _fieldType = _flatFiles.structureTypes?.getFieldType(_fieldDefinition.typeReference);
                                    if (_fieldType.dataType != null)
                                        _fieldProcess.addProcess("Control_DataFormat");
                                }

                                //Control_NotNull
                                if (processesToAppend["field"].Contains("Control_NotNull"))
                                    if (_fieldDefinition.notNull != null)
                                        _fieldProcess.addProcess("Control_NotNull");

                                //Control_Uniqueness
                                if (processesToAppend["field"].Contains("Control_Uniqueness"))
                                    if (_fieldDefinition.unique != null)
                                        _fieldProcess.addProcess("Control_Uniqueness");

                                //Control_Codes
                                if (processesToAppend["field"].Contains("Control_Codes"))
                                    if (_fieldDefinition.codes != null)
                                        _fieldProcess.addProcess("Control_Codes");

                                if (_fieldProcess.processes != null &&
                                    _fieldProcess.processes.Length == 0)
                                    _fieldProcess.processes = null;

                                if (_fieldProcess.processes == null)
                                    _recordProcess.remFieldProcesses(_fieldDefinition.name);
                            }

                            if (_recordProcess.fieldProcesses != null &&
                                _recordProcess.fieldProcesses.Length == 0)
                                _recordProcess.fieldProcesses = null;

                            // Add record-processes

                            //Analyse_FindExtremeRecords
                            if (processesToAppend["record"].Contains("Analyse_FindExtremeRecords"))
                                _recordProcess.addProcess("Analyse_FindExtremeRecords");

                            //Analyse_CountRecordDefinitionOccurences
                            if (processesToAppend["record"].Contains("Analyse_CountRecordDefinitionOccurences"))
                                if (_recordDefinition.recordDefinitionFieldValue != null)
                                    _recordProcess.addProcess("Analyse_CountRecordDefinitionOccurences");

                            //Analyse_AllFrequenceList
                            // Not necessary. Already added Control_Codes for each field.
                            if (processesToAppend["record"].Contains("Analyse_AllFrequenceList"))
                                _recordProcess.addProcess("Analyse_AllFrequenceList");

                            //Analyse_CrossTable
                            if (processesToAppend["record"].Contains("Analyse_CrossTable"))
                                _recordProcess.addProcess("Analyse_CrossTable");

                            // Control_FixedLength
                            if (processesToAppend["record"].Contains("Control_FixedLength"))
                                if (_recordDefinition.fixedLength != null)
                                    _recordProcess.addProcess("Control_FixedLength");

                            // Control_NotUsedRecordDef
                            if (processesToAppend["record"].Contains("Control_NotUsedRecordDef"))
                                if (_recordDefinition.recordDefinitionFieldValue != null)
                                    _recordProcess.addProcess("Control_NotUsedRecordDef");

                            // Control_Key
                            if (processesToAppend["record"].Contains("Control_Key"))
                                if (_recordDefinition.keys != null)
                                    _recordProcess.addProcess("Control_Key");

                            // Control_ForeignKey
                            if (processesToAppend["record"].Contains("Control_ForeignKey"))
                                if (_recordDefinition.keys != null)
                                {
                                    foreach (key _key in _recordDefinition.keys)
                                    {
                                        if (_key.Item.GetType().IsEquivalentTo(typeof(foreignKey)))
                                        {
                                            _recordProcess.addProcess("Control_ForeignKey");
                                        }
                                    }
                                }


                            if (_recordProcess.processes == null &&
                             _recordProcess.fieldProcesses == null)
                                _flatFileProcess.remRecordProcesses(_recordDefinition.name);
                        }
                    }
                }

                if (_flatFileProcess.recordProcesses != null &&
                    _flatFileProcess.recordProcesses.Length == 0)
                    _flatFileProcess.recordProcesses = null;

                // Add flatFile-processes

                //Analyse_CountRecords
                if (processesToAppend["file"].Contains("Analyse_CountRecords"))
                    _flatFileProcess.addProcess("Analyse_CountRecords");

                //Analyse_CountChars
                if (processesToAppend["file"].Contains("Analyse_CountChars"))
                    _flatFileProcess.addProcess("Analyse_CountChars");

                //Control_AllFixedLength
                // Not necessary. Already added Control_FixedLength for each record.
                if (processesToAppend["file"].Contains("Control_AllFixedLength"))
                    _flatFileProcess.addProcess("Control_AllFixedLength");

                //Control_NumberOfRecords
                if (processesToAppend["file"].Contains("Control_NumberOfRecords"))
                    if (_flatFile.getProperty("numberOfRecords")?.value != null)
                        _flatFileProcess.addProcess("Control_NumberOfRecords");


                if (_flatFileProcess.processes == null &&
                    _flatFileProcess.recordProcesses == null)
                    _flatFiles.remFlatFileProcesses(_flatFile.name);
            }

            if (_flatFiles.flatFileProcesses != null &&
                _flatFiles.flatFileProcesses.Length == 0)
                _flatFiles.flatFileProcesses = null;
        }

        public static void AppendProcess(addml aml, string flatFileName, string processName)
        {
            flatFiles _flatFiles = aml.dataset[0].flatFiles;
            if (_flatFiles == null)
                return;

            foreach (flatFile _flatFile in _flatFiles.flatFile)
            {
                if (_flatFile.name.Equals(flatFileName))
                {
                    flatFileProcesses _flatFileProcess = _flatFiles.addFlatFileProcesses(_flatFile.name);
                    _flatFileProcess.addProcess(processName);
                }
            }
        }

        public static void AppendProcess(addml aml, string flatFileName, string recordDefinitionName, string processName)
        {
            flatFiles _flatFiles = aml.dataset[0].flatFiles;
            if (_flatFiles == null)
                return;

            foreach (flatFile _flatFile in _flatFiles.flatFile)
            {
                if (_flatFile.name.Equals(flatFileName))
                {
                    flatFileProcesses _flatFileProcess = _flatFiles.addFlatFileProcesses(_flatFile.name);

                    foreach (flatFileDefinition _flatFileDefinition in _flatFiles.flatFileDefinitions)
                    {
                        if (_flatFileDefinition.Equals(_flatFile.definitionReference))
                        {
                            foreach (recordDefinition _recordDefinition in _flatFileDefinition.recordDefinitions)
                            {
                                if (_recordDefinition.name.Equals(recordDefinitionName))
                                {
                                    recordProcesses _recordProcess = _flatFileProcess.addRecordProcesses(recordDefinitionName);
                                    _recordProcess.addProcess(processName);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AppendProcess(addml aml, string flatFileName, string recordDefinitionName, string fieldDefinitionName, string processName)
        {
            flatFiles _flatFiles = aml.dataset[0].flatFiles;
            if (_flatFiles == null)
                return;

            foreach (flatFile _flatFile in _flatFiles.flatFile)
            {
                if (_flatFile.name.Equals(flatFileName))
                {
                    flatFileProcesses _flatFileProcess = _flatFiles.addFlatFileProcesses(_flatFile.name);

                    foreach (flatFileDefinition _flatFileDefinition in _flatFiles.flatFileDefinitions)
                    {
                        if (_flatFileDefinition.Equals(_flatFile.definitionReference))
                        {
                            foreach (recordDefinition _recordDefinition in _flatFileDefinition.recordDefinitions)
                            {
                                if (_recordDefinition.name.Equals(recordDefinitionName))
                                {
                                    recordProcesses _recordProcess = _flatFileProcess.addRecordProcesses(recordDefinitionName);

                                    foreach (fieldDefinition _fieldDefinition in _recordDefinition.fieldDefinitions)
                                    {
                                        if (_fieldDefinition.name.Equals(fieldDefinitionName))
                                        {
                                            fieldProcesses _fieldProcess = _recordProcess.addFieldProcesses(fieldDefinitionName);
                                            _fieldProcess.addProcess(processName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public addml upgrade(addmml aml)
        {
            return null;
        }
    }
}
