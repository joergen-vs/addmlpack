using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddmlPack.Standards.Addml
{
    public sealed class AddmlVersionsImplemented
    {
        private static AddmlVersionsImplemented instance = null;
        private static readonly object padlock = new object();

        private string v8_3 { get; set; }
        public static string V8_3 { get { return instance.v8_3; } }
        public static string Latest { get { return Instance.versions.Last(); } }
        private List<string> versions { get; set; }
        public List<string> Versions { get { return Instance.versions; } }

        AddmlVersionsImplemented()
        {
             v8_3 = "v8_3";
            versions = new List<string>() {
                v8_3
            };
        }

        public static AddmlVersionsImplemented Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AddmlVersionsImplemented();
                    }
                    return instance;
                }
            }
        }
    }
}
