using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects.Datatypes
{
    public abstract class BaseType
    {
        public string Description { get; set; }

        public string Datatype { get; set; }

        public string FieldFormat { get; set; }

        public bool isValidated { get; set; }

        public abstract bool validate(string text);

        public abstract string fieldformat();
    }
}
