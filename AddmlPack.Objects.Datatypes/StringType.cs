using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AddmlPack.Objects.Datatypes
{
    class StringType : BaseType
    {
        public static string _description = "STRING";
        public static string _datatype = "string";

        public StringType()
        {
            this.Description = StringType._description;
            this.Datatype = StringType._datatype;
            this.isValidated = true;
        }

        public override string fieldformat() => (string)null;

        public override bool validate(string text) => true;
    }
}
