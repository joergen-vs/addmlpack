using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Utils
{
    public class GeneratorUtils
    {
        public static string NewGUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}