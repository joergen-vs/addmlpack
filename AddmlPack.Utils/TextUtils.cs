using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Utils
{
    public class TextUtils
    {
        public static string toSeparator(string text)
        {
            if (text == null)
                return text;
            if (text.Contains("CRLF"))
                text = text.Replace("CRLF", "\r\n");
            if (text.Contains("CR"))
                text = text.Replace("CR", "\r");
            if (text.Contains("LF"))
                text = text.Replace("LF", "\n");
            if (text.Contains("TAB"))
                text = text.Replace("TAB", "\t");

            return text;
        }

        public static string fromSeparator(string text)
        {
            if (text == null)
                return text;
            if (text.Contains("\r\n"))
                text = text.Replace("\r\n", "CRLF");
            if (text.Contains("\r"))
                text = text.Replace("\r", "CR");
            if (text.Contains("\n"))
                text = text.Replace("\n", "LF");
            if (text.Contains("\t"))
                text = text.Replace("\t", "\\t");

            return text;
        }
    }
}
