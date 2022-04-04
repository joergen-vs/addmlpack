using AddmlPack.Objects.Datatypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects
{
    public class Field
    {
        public string Name { get; }
        public string Value { get; }

        public List<BaseType> DataTypes { get; set; }

        public Field(string name, string value)
        {
            Name = name;
            Value = value;
            DataTypes = new List<BaseType>()
            {
                new IntegerType(),
                new BooleanType(),
                new FloatType(),
                new DateType(),
                new URIType(),
                new ClobType(),
                new StringType(),
                new UnknownType()
            };
        }
    }
}
