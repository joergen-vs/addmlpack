using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AddmlPack.Objects.Datatypes
{
    public class DateType : BaseType
    {
        public static string datatype = "date";
        private List<string> _DateFieldFormats;
        private bool[] _ValidatedFieldFormats;

        public DateType(string[] customFieldFormats)
        {
            this.Description = "DATE";
            this.Datatype = DateType.datatype;
            this.isValidated = true;
            this._DateFieldFormats = new List<string>();
            this._DateFieldFormats.Add("dd.MM.yyyyTHH:mm:sszzz");
            this._DateFieldFormats.Add("dd.MM.yyyy");
            this._DateFieldFormats.Add("ddMMyyyy");
            this._DateFieldFormats.Add("yyyyMMdd");
            if (customFieldFormats != null)
            {
                foreach (string customFieldFormat in customFieldFormats)
                    this._DateFieldFormats.Add(customFieldFormat);
            }
            this._ValidatedFieldFormats = new bool[this._DateFieldFormats.Count];
            for (int index = 0; index < this._ValidatedFieldFormats.Length; ++index)
                this._ValidatedFieldFormats[index] = true;
        }

        public override string fieldformat()
        {
            for (int index = 0; index < this._DateFieldFormats.Count; ++index)
            {
                if (this._ValidatedFieldFormats[index])
                    return this._DateFieldFormats[index];
            }
            return (string)null;
        }

        public override bool validate(string text)
        {
            for (int index = 0; index < this._ValidatedFieldFormats.Length; ++index)
            {
                if (this._ValidatedFieldFormats[index])
                    this._ValidatedFieldFormats[index] = this.validate(text, this._DateFieldFormats[index]);
            }
            return ((IEnumerable<bool>)this._ValidatedFieldFormats).Any<bool>((Func<bool, bool>)(v => v));
        }

        private bool validate(string text, string _dateTimeFormat) => DateTimeOffset.TryParseExact(text, _dateTimeFormat, (IFormatProvider)CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset _);
    }
}
