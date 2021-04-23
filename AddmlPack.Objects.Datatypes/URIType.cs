using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects.Datatypes
{
    class URIType : BaseType
    {
        public URIType()
        {
            this.Description = "URI";
            this.Datatype = "anyURI";
            this.isValidated = true;
        }

        public override string fieldformat() => (string)null;

        public override bool validate(string text) => false;
    }
}
