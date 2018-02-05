using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosslessAudioConverter.Model
{
    public struct ProgressUpdate
    {
        public readonly int Progress;
        public readonly int Total;

        internal ProgressUpdate(int progress, int total)
        {
            Progress = progress;
            Total = total;
        }

        public static ProgressUpdate Create(int progress, int total)
        {
            return new ProgressUpdate(progress, total);
        }

        public override bool Equals(object obj)
        {
            ProgressUpdate other = (ProgressUpdate)obj;
            return Object.ReferenceEquals(this, other) ||
                (this.Progress == other.Progress &&
                this.Total == other.Total);
        }

        public override int GetHashCode()
        {
            return Progress.GetHashCode() ^ Total.GetHashCode(); //xor, right?  That's a basic way to combine hashcodes?
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Progress, Total);
        }
    }
}
