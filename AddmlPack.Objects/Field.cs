using AddmlPack.Objects.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddmlPack.Objects
{
    public class Field
    {
        public string Name { get; }
        public string Value { get; }
        public bool Unique { get { return values != null; } }
        public bool NotNull { get; set; }

        public string DataType { get { return DataTypes.First(x => x.isValidated).Datatype; } }
        public string Description { get { return DataTypes.First(x => x.isValidated).Description; } }
        private BaseType unknown, boolsk;
        public List<BaseType> DataTypes { get; set; }
        public string[] nullValues { get; set; }
        private HashSet<string> values = new HashSet<string>();

        public HashSet<string> Codes { get; set; }

        public Field(string name, string value, Dictionary<string, string[]> formats)
        {
            Name = name;
            Value = value;
            DataTypes = new List<BaseType>();

            DataTypes.Add(boolsk = new BooleanType());
            DataTypes.Add(unknown = new UnknownType());
            DataTypes.Add(new IntegerType());

            DataTypes.Add(new DateType(formats.ContainsKey("date") ? formats["date"] : null));
            if (formats.ContainsKey("float"))
                DataTypes.Add(new FloatType(formats["float"][0]));
            else
                DataTypes.Add(new FloatType("."));
            DataTypes.Add(new URIType());
            DataTypes.Add(new ClobType());
            DataTypes.Add(new StringType());

            Codes = new HashSet<string>() { Value };
            nullValues = formats.ContainsKey("nullValues") ? formats["nullValues"] : new string[] {""};
            NotNull = true;
        }

        public int validate()
        {
            int counter = 0;

            if (values != null)
                if (values.Contains(Value))
                {
                    values = null;
                }
                else
                    values.Add(Value);

            if (Codes != null)
                addCode(Value);

            if (string.Empty == Value)
            {
                if (values != null)
                {
                    values = null;
                }
                NotNull = false;
                if (boolsk.isValidated)
                    boolsk.isValidated = false;
                return 0;
            }

            foreach (BaseType type in DataTypes)
            {
                if (type.isValidated)
                    if (type.isValidated = type.validate(Value))
                        counter++;
            }

            return counter;
        }

        public int validate(string text)
        {
            int counter = 0;

            if (values != null)
                if (values.Contains(text))
                {
                    values = null;
                }
                else
                    values.Add(text);

            if (Codes != null)
                addCode(text);

            if (string.Empty == text)
            {
                if (values != null) { 
                values = null;
                }
                NotNull = false;
                if (boolsk.isValidated)
                    boolsk.isValidated = false;
                return 0;
            }

            foreach (BaseType type in DataTypes)
            {
                if (type.isValidated)
                    if (type.isValidated = type.validate(text))
                        counter++;
            }

            return counter;
        }

        private void addCode(string text)
        {
            if (Codes != null && Codes.Count < 50)
                Codes.Add(text);
            else
            {
                Codes = null;
            }
        }

        public override string ToString()
        {
            string _ = "";
            foreach (BaseType type in DataTypes)
                if (type.isValidated)
                    _ += "," + type.Description;
            return $"'{Name}={Description}':[{_.Substring(1)}],Unique={Unique},NotNull={NotNull}";
        }
    }
}
