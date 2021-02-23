using AddmlPack.Standard.v8_3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AddmlPack.Utils
{
    public class FileUtils
    {
        public static addml AddmlFromFile(string path)
        {
            if (File.Exists(path))
                return AddmlUtils.ToAddml(File.ReadAllText(path));
            else
                Console.WriteLine($"File {path} doesnt exist.");
            return null;
        }
        public static addml AddmlFromFile(FileInfo file)
        {
            if (file.Exists)
                AddmlUtils.ToAddml(File.ReadAllText(file.FullName));
            else
                Console.WriteLine($"File {file.FullName} doesnt exist.");
            return null;
        }

        public static string AddmlToFile(addml _aml, string path)
        {
            string objectData = AddmlUtils.FromAddml(_aml);
            if (path == null)
            {
                Console.WriteLine(objectData);
                return objectData;
            }
            else
            {
                File.WriteAllText(path, objectData, Encoding.UTF8);
                return objectData;
            }
        }

        public static string GenerateChecksum(string pathToFile)
        {
            HashAlgorithm h = (HashAlgorithm)CryptoConfig.CreateFromName("SHA256");

            byte[] bytes;
            using (FileStream fs = new FileInfo(pathToFile).OpenRead())
            {
                bytes = h.ComputeHash(fs);
            }
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string GenerateChecksum(string pathToFile, string algorithm)
        {
            HashAlgorithm h = (HashAlgorithm)CryptoConfig.CreateFromName(algorithm);

            byte[] bytes;
            using (FileStream fs = new FileInfo(pathToFile).OpenRead())
            {
                bytes = h.ComputeHash(fs);
            }
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string GetEncoding(string _name)
        {
            try
            {
                return Encoding.GetEncoding(_name).BodyName;
            }
            catch (ArgumentException)
            {
                return _name
                    ;
            }
        }
    }
}
