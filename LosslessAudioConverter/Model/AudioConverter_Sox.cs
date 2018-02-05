using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LosslessAudioConverter.Model
{
    /// <summary>
    /// Uses the open-source SOX Sound eXchange binaries to convert audio.
    /// Note: This launches a sox.exe process for each audio file that gets converted.
    /// This makes it thread-safe, but a fairly heavy operation.  Be careful, ya' dingus.
    /// 
    /// It may be possible to access it through some API and use fewer system resources, but that's out-of-scope for release 1
    /// </summary>
    internal class AudioConverter_Sox : IAudioConverter
    {
        private readonly Process _process;

        private static readonly string SoxLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\lib\sox\sox.exe");

        /*
         * TODO:
         *  Determine format support by checking for appropriate .dlls (user must provide lame encoder)
         *  Creating a ffmpeg-based converter would allow conversion to/from opus, mp4, mp3, most anything else (if we make the user provide non-free binaries)
         *  
         */

        /// <summary>
        /// TODO: Create overload with OutputOptions argument.
        /// This would allow defining quality settings, downmixing options, output format, etc
        /// </summary>
        public AudioConverter_Sox()
        {
            _process = new Process();

            _process.StartInfo.FileName = SoxLocation;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
        }

        public void ConvertAudioFile_Synchronous(string audioFilePath, string outputLocation)
        {
            /*
             * NOTE: add "channels 2" for stereo, "channels 1" for mono, or exlude altogether to keep as-is.
             * NICE!!
             * What remains to be seen if if "channels 2" does any extra processing for inputs already in stereo
             * when would I EVER want a lossless, 5.1 song converted to a lossy 5.1 song?  PLEX streaming, maybe?
             * as it stands... this is trivially easy to support, at least in the code-behind.  Sox is nice.
            */
            _process.StartInfo.Arguments = String.Format("\"{0}\" -C6 \"{1}\"", audioFilePath, outputLocation);
            _process.Start();
            _process.PriorityClass = ProcessPriorityClass.BelowNormal; //Long-running operations like this should be low priority.
            _process.WaitForExit();
        }

        public void Dispose()
        {
            _process.Close();
            _process.Dispose();
        }
    }
}
