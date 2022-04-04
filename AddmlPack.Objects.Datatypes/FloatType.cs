using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddmlPack.Objects.Datatypes
{
    public class FloatType : BaseType
    {
        public static string datatype = "float";
        private Regex pattern;

        public FloatType(string separator)
        {
            this.Description = "FLOAT";
            this.Datatype = FloatType.datatype;
            this.FieldFormat = separator;
            this.pattern = separator.Equals(".") ? new Regex("\\d+\\.\\d+") : new Regex("\\d+" + this.FieldFormat + "\\d+");
            this.isValidated = true;
        }

        public override string fieldformat() => string.Format("nn{0}nn", (object)this.FieldFormat);

        public override bool validate(string text)
        {
            bool success = this.pattern.Match(text).Success;
            return this.pattern.Match(text).Success;
        }
    }
}
