using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Templates
{
    public class Template
    {
        public static string GetTemplate(string template)
        {
            if (template == null)
                return Files.Addml_for_N3;

            switch (template)
            {
                case "Noark-3":
                    return Files.Addml_for_N3;
                case "Noark 5":
                    return Files.Addml_for_N5;
                case "Fagsystem":
                    return Files.Addml_for_FF;
                default:
                    return Files.Addml_for_N3;
            }
        }
    }
}
