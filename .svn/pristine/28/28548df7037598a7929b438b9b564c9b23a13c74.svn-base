using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LosslessAudioConverter.Helpers
{
    class ValidDirectoryRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string path = (value ?? "").ToString();

            if (string.IsNullOrWhiteSpace(path)) return new ValidationResult(false, "Path must not be empty.");
            if (!Directory.Exists(path)) return new ValidationResult(false, "The path does not exist");
            if (!Path.IsPathRooted(path)) return new ValidationResult(false, "Relative paths are not allowed");

            try { Path.GetDirectoryName(path); }
            catch (Exception e) { return new ValidationResult(false, e); }

            return ValidationResult.ValidResult;
        }
    }
}
