 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddmlPack.Objects.Datatypes
{
    public class ClobType : BaseType
    {
        public static string datatype = "string";
        public static string description = "CLOB";
        private char[] validCharacters;

        public ClobType()
        {
            this.Description = ClobType.description;
            this.Datatype = ClobType.datatype;
            this.isValidated = true;
            this.validCharacters = "0123456789ABCDEF".ToCharArray();
        }

        public override string fieldformat() => (string)null;

        public override bool validate(string text)
        {
            if ((uint)(text.Length % 2) > 0U)
                return false;
            for (int index = 0; index < text.Length; ++index)
            {
                if (!((IEnumerable<char>)this.validCharacters).Contains<char>(text[index]))
                    return false;
            }
            return true;
        }
    }
}
