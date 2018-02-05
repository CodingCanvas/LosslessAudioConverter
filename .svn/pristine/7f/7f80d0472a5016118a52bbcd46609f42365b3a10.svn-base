using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace LosslessAudioConverter.Helpers
{
    public enum ApplicationState
    {
        Starting = 0,
        Converting,
        Copying,
        Cancelling,
        CompleteSuccess,
        CompleteCancelled
    }

    public class ApplicationStateToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ApplicationState)value;

            switch (state)
            {
                case ApplicationState.Starting:
                    return 0;
                case ApplicationState.Converting:
                case ApplicationState.Copying:
                case ApplicationState.Cancelling:
                    return 1;
                case ApplicationState.CompleteSuccess:
                case ApplicationState.CompleteCancelled:
                    return 2;
                default:
                    return -1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
