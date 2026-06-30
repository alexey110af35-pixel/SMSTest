// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SmsTestWpf.Models;
using SmsTestWpf.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmsTestWpf.ViewModels
{
	public partial class MainViewModel : ObservableObject
	{
		private readonly IEnvironmentService _environmentService;
		private readonly ILogger<MainViewModel> _logger;
		private readonly List<string> _variableNames;

		[ObservableProperty]
		private ObservableCollection<EnvironmentVariableModel> _variables = new();

		[ObservableProperty]
		private bool _isLoading;

		[ObservableProperty]
		private string _statusMessage = "Готов к работе";

		[ObservableProperty]
		private bool _hasChanges;

		[ObservableProperty]
		private int _changedCount;

		[ObservableProperty]
		private bool _isProcessing;

		[ObservableProperty]
		private string _progressMessage = string.Empty;

		[ObservableProperty]
		private int _progressValue;

		[ObservableProperty]
		private bool _showProgress;

		public ICommand SaveCommand { get; }
		public ICommand ResetCommand { get; }
		public ICommand RefreshCommand { get; }

		public MainViewModel(
			IEnvironmentService environmentService,
			ILogger<MainViewModel> logger,
			List<string> variableNames)
		{
			_environmentService = environmentService;
			_logger = logger;
			_variableNames = variableNames;

			SaveCommand = new RelayCommand(SaveVariablesAsync, CanSave);
			ResetCommand = new RelayCommand(ResetVariablesAsync, CanReset);
			RefreshCommand = new RelayCommand(LoadVariables);

			LoadVariables();
		}

		private void LoadVariables()
		{
			IsLoading = true;
			StatusMessage = "Загрузка переменных...";

			try
			{
				var loaded = _environmentService.LoadVariables(_variableNames);
				Variables.Clear();
				foreach (var item in loaded)
				{
					item.PropertyChanged += (s, e) =>
					{
						if (e.PropertyName == nameof(EnvironmentVariableModel.IsChanged))
						{
							UpdateChangeStatus();
						}
					};
					Variables.Add(item);
				}

				UpdateChangeStatus();
				StatusMessage = $"Загружено {Variables.Count} переменных";
				_logger.LogInformation($"Загружено {Variables.Count} переменных");
			}
			catch (Exception ex)
			{
				StatusMessage = $"Ошибка загрузки: {ex.Message}";
				_logger.LogError(ex, "Ошибка загрузки переменных");
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void UpdateChangeStatus()
		{
			var changed = _environmentService.GetChangedVariables(Variables.ToList());
			HasChanges = changed.Count > 0;
			ChangedCount = changed.Count;

			StatusMessage = HasChanges
				? $"📝 Есть {ChangedCount} измененных переменных"
				: "✅ Нет изменений";
		}

		private bool CanSave() => !IsLoading && !IsProcessing && HasChanges;

		private async void SaveVariablesAsync()
		{
			if (!CanSave()) return;

			IsProcessing = true;
			ShowProgress = true;
			ProgressValue = 0;
			StatusMessage = "Сохранение...";

			try
			{
				// Создаем Progress для отслеживания прогресса
				var progress = new Progress<string>(message =>
				{
					ProgressMessage = message;
					// Обновляем статус из сообщения
					if (message.Contains("✅"))
					{
						StatusMessage = message;
					}
				});

				await _environmentService.SaveVariablesAsync(Variables.ToList(), progress);

				UpdateChangeStatus();
				StatusMessage = "✅ Сохранение завершено";
				_logger.LogInformation("Сохранение завершено");
			}
			catch (Exception ex)
			{
				StatusMessage = $"❌ Ошибка сохранения: {ex.Message}";
				_logger.LogError(ex, "Ошибка сохранения переменных");
			}
			finally
			{
				IsProcessing = false;
				ShowProgress = false;
				ProgressMessage = string.Empty;
			}
		}

		private bool CanReset() => !IsLoading && !IsProcessing && HasChanges;

		private async void ResetVariablesAsync()
		{
			if (!CanReset()) return;

			IsProcessing = true;
			ShowProgress = true;
			ProgressValue = 0;
			StatusMessage = "Сброс...";

			try
			{
				var progress = new Progress<string>(message =>
				{
					ProgressMessage = message;
					if (message.Contains("🔄"))
					{
						StatusMessage = message;
					}
				});

				await _environmentService.ResetVariablesAsync(Variables.ToList(), progress);

				UpdateChangeStatus();
				StatusMessage = "🔄 Сброс завершен";
				_logger.LogInformation("Сброс завершен");
			}
			catch (Exception ex)
			{
				StatusMessage = $"❌ Ошибка сброса: {ex.Message}";
				_logger.LogError(ex, "Ошибка сброса переменных");
			}
			finally
			{
				IsProcessing = false;
				ShowProgress = false;
				ProgressMessage = string.Empty;
			}
		}

		public string StatusColor => HasChanges ? "#FF6B35" : "#4CAF50";
	}
}