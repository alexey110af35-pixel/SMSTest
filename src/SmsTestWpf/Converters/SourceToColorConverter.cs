using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmsTestWpf.Converters
{
	public class SourceToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string source)
			{
				return source switch
				{
					"User" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")),    // Зеленый
					"Machine" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0")), // Синий
					"Not found" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E")), // Серый
					_ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E"))
				};
			}

			return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}