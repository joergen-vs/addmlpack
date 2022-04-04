using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects.Datatypes
{
    public class UnknownType : BaseType
    {
        public UnknownType()
        {
            this.Description = "No data in column";
            this.Datatype = "string";
            this.isValidated = true;
        }

        public override string fieldformat() => (string)null;

        public override bool validate(string text) => text.Equals(string.Empty);
    }
}
