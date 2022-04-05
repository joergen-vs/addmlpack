using AddmlPack.Objects;
using AddmlPack.Scan.Delimited;
using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Scan
{
    class Scan
    {
        public Scan(Project P)
        {
            string[] filePaths = new string[] { };
            if (System.IO.Directory.Exists(P.Input))
            {
                System.IO.Directory.GetFiles(
                    P.Input,
                    P.Extension,
                    System.IO.SearchOption.AllDirectories
                );
            }
            else
            {
                if (P.Input.ToLower().EndsWith(P.Extension.ToLower()))
                    filePaths = new string[] { P.Input };
            }

            foreach(string filePath in filePaths)
            {

                switch (P.GetString("fileformat"))
                {
                    case "delimited":
                        {
                            new DelimitedScan(filePath, P);
                        }
                }
            }

        }
    }
}
