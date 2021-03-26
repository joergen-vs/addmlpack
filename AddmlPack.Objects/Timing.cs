using System;
using System.Collections.Generic;
using System.Text;

namespace AddmlPack.Objects
{
    public class Timing
    {
        private System.Diagnostics.Stopwatch watch;
        public long archiveSize { get; set; }
        public long archiveRead { get; set; }
        public long archivePosts { get; set; }

        public Timing()
        {
            watch = new System.Diagnostics.Stopwatch();
            archiveRead = 0;
        }

        public void Start()
        {
            watch.Restart();
            archivePosts = 0;
            archiveRead = 0; archiveSize = 0;
            watch.Start();
        }

        public void Stop()
        {
            watch.Stop();
        }

        public string Elapsed()
        {
            TimeSpan elapsed = watch.Elapsed;
            if (elapsed.Hours > 0)
                return elapsed.ToString(@"h\:mm\:ss\.fff");
            else
                return elapsed.ToString(@"m\:ss\.fff");
        }

        public string RecordsPerSecond()
        {
            return String.Format("{0} records/second", archivePosts / watch.Elapsed.TotalSeconds);

        }

        public string SizePerSecond()
        {
            return String.Format("{0} byte/second", archiveSize / watch.Elapsed.TotalSeconds);

        }
    }
}
