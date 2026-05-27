using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DesktopApp.Converters
{
    public class ApiImageToUrlConverter : IValueConverter
    {
        private const string BaseUrl = "http://localhost:3000";

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? path = value switch
            {
                IEnumerable<string> list => list.FirstOrDefault(),
                string s => s,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(path))
                return null;

            var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? path
                : $"{BaseUrl}{(path.StartsWith("/") ? "" : "/")}{path}";

            try
            {
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                bitmap.EndInit();

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IMAGE ERROR: {ex.Message}");
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }


    public class StatusToInVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Solo mostramos el botón Check-in si la reserva está confirmada
            return value?.ToString() == "confirmada" ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StatusToOutVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Solo mostramos el botón Check-out si el cliente está "inHotel"
            return value?.ToString() == "inHotel" ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
