using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmsTestWpf.Helpers;
using SmsTestWpf.Models;

namespace SmsTestWpf.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private const string DefaultValue = "NOT_SET";
		private bool _isSaving;
		public ObservableCollection<VariableItemViewModel> Variables { get; } = new();

		public ICommand MinimizeCommand { get; }
		public ICommand CloseCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand ResetCommand { get; }

		public bool IsSaving
		{
			get => _isSaving;
			set { _isSaving = value; OnPropertyChanged(); }
		}

		public MainViewModel()
		{
			MinimizeCommand = new RelayCommand(p => ((Window?)p)!.WindowState = WindowState.Minimized);
			CloseCommand = new RelayCommand(p => { Log.CloseAndFlush(); ((Window?)p)?.Close(); });

			SaveCommand = new RelayCommand(async p =>
				await SaveAllChangesAsync(), p => Variables.Any(v => v.IsModified) && !IsSaving);
			ResetCommand = new RelayCommand(p =>
				ResetAllChanges(), p => Variables.Any(v => v.IsModified) && !IsSaving);

			ConfigureLogging();
			LoadEnvironmentVariables();
		}

		private void ConfigureLogging()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.File(
					path: "logs/test-sms-wpf-app-.log",
					rollingInterval: RollingInterval.Day,
					outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}")
				.CreateLogger();
		}

		private void LoadEnvironmentVariables()
		{
			Variables.Clear();
			try
			{
				var config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.Build();

				var varNames = config.GetSection("EnvironmentVariables").Get<List<string>>();
				if (varNames == null) return;

				foreach (var name in varNames)
				{
					string? osValue = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
					bool isDefaultUsed = false;

					if (string.IsNullOrEmpty(osValue))
					{
						osValue = DefaultValue;
						isDefaultUsed = true;
					}

					var model = new EnvVariableModel
					{
						Name = name,
						Value = osValue,
						Comment = isDefaultUsed ? "Значение по умолчанию" : "Прочитано из ОС"
					};

					Variables.Add(new VariableItemViewModel(model));
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Ошибка загрузки переменных.");
			}
		}

		private async Task SaveAllChangesAsync()
		{
			var modifiedItems = Variables.Where(v => v.IsModified).ToList();
			if (!modifiedItems.Any()) return;

			IsSaving = true;
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				await Task.Run(() =>
				{
					foreach (var item in modifiedItems)
					{
						string oldValue =
							Environment.GetEnvironmentVariable(item.Field, EnvironmentVariableTarget.User) ?? DefaultValue;

						Environment.SetEnvironmentVariable(item.Field, item.Value, EnvironmentVariableTarget.User);

						Log.Information("Переменная '{VarName}' изменена. Старое: '{Old}', Новое: '{New}'",
							item.Field, oldValue, item.Value);
					}
				});

				foreach (var item in modifiedItems)
				{
					item.Comment = $"Сохранено в ОС: {DateTime.Now:HH:mm:ss}";
					item.ApplyChanges();
				}

				Mouse.OverrideCursor = null;
				MessageBox.Show(
					"Все изменения успешно записаны в систему в фоновом режиме!",
					"Успех",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Mouse.OverrideCursor = null;
				Log.Error(ex, "Ошибка сохранения.");
				MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				IsSaving = false;
			}
		}

		private void ResetAllChanges()
		{
			if (MessageBox.Show("Отменить не сохраненные правки?", "Отмена", MessageBoxButton.YesNo)
				== MessageBoxResult.Yes)
			{
				foreach (var item in Variables) item.ResetChanges();
			}
		}

		public void AddNewVariable()
		{
			var model =
				new EnvVariableModel
				{
					Name = "NEW_VAR_" + (Variables.Count + 1),
					Value = "VALUE",
					Comment = "Новая запись"
				};

			Variables.Add(new VariableItemViewModel(model) { IsModified = true });
		}
	}
}
