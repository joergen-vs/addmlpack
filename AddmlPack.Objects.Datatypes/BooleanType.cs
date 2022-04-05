using System;
using System.Collections.Generic;

namespace AddmlPack.Objects.Datatypes
{
    public class BooleanType : BaseType
    {
        public static string datatype = "bool";
        private HashSet<string> trueOrFalse;

        public BooleanType()
        {
            this.Description = "BOOLEAN";
            this.Datatype = BooleanType.datatype;
            this.isValidated = true;
            this.trueOrFalse = new HashSet<string>();
        }

        public override string fieldformat() => (string)null;

        public override bool validate(string text)
        {
            this.trueOrFalse.Add(text);
            return this.trueOrFalse.Count < 3;
        }
    }
}
