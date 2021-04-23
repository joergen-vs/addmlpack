using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Utils
{
    // TODO: There has to be a better way...
    public sealed class Logging
    {
        private static Logging instance = null;
        private static readonly object padlock = new object();

        public enum Level
        {
            Out, Error
        }

        Logging() { }

        public void Write(string message, Level level)
        {
            Console.Out.WriteLine(message);

            if (level == Level.Error)
                Console.Error.WriteLine(message);
        }

        public static Logging Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logging();
                    }
                    return instance;
                }
            }
        }
    }
}
