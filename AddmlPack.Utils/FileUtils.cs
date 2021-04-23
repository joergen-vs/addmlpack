using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AddmlPack.Utils
{
    public class FileUtils
    {
        public static string FromFile(string path)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                Console.WriteLine($"File {path} doesnt exist.");
            return null;
        }
        public static string FromFile(FileInfo file)
        {
            if (file.Exists)
                File.ReadAllText(file.FullName);
            else
                Console.WriteLine($"File {file.FullName} doesnt exist.");
            return null;
        }

        public static string ToFile(string _aml, string path)
        {
            if (path == null)
            {
                Console.WriteLine(_aml);
            }
            else
            {
                File.WriteAllText(path, _aml, Encoding.UTF8);
            }
            return _aml;
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
                return TextUtils.GetEncoding(_name).BodyName;
            }
            catch (ArgumentException)
            {
                return _name
                    ;
            }
        }
    }
}
