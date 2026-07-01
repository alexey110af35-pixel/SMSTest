using System.Windows;
using SmsTestWpf.ViewModels;

namespace SmsTestWpf.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();			
			DataContext = new MainViewModel();
			
			MouseLeftButtonDown += (s, e) =>
			{
				if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove();
			};
		}
	}
}
