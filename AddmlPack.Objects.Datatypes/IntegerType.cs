using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AddmlPack.Objects.Datatypes
{
    public class IntegerType : BaseType
    {
        public static string datatype = "integer";

        public IntegerType()
        {
            this.Description = "Integer";
            this.Datatype = IntegerType.datatype;
            this.isValidated = true;
        }

        public override string fieldformat() => (string)null;

        public override bool validate(string text)
        {
            try { int.Parse(text); }
            catch(Exception e) { return false; }
            return true;
        }
    }
}
