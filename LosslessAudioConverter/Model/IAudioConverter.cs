using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosslessAudioConverter.Model
{
    internal interface IAudioConverter : IDisposable
    {
        void ConvertAudioFile_Synchronous(string audioFilePath, string outputLocation);
    }
}
