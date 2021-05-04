using System;
using Windows.UI.Xaml.Data;

namespace ContosoNotes.UI
{
    public class LastSyncDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return "Edited " + dateTime.ToString("MMMM dd, yyyy, h:mmtt");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
