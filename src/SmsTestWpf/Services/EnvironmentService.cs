using Microsoft.Extensions.Logging;
using SmsTestWpf.Models;
using System.Security;

namespace SmsTestWpf.Services
{
	public class EnvironmentService : IEnvironmentService
	{
		private readonly ILogger<EnvironmentService> _logger;

		public EnvironmentService(ILogger<EnvironmentService> logger)
		{
			_logger = logger;
		}

		public List<EnvironmentVariableModel> LoadVariables(List<string> variableNames)
		{
			var variables = new List<EnvironmentVariableModel>();

			foreach (var varName in variableNames)
			{
				var model = new EnvironmentVariableModel { Name = varName };

				// Проверяем User
				var userValue = Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.User);
				var machineValue = Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.Machine);

				if (!string.IsNullOrEmpty(userValue))
				{
					model.SetInitialValue(userValue, EnvironmentVariableTarget.User);
				}
				else if (!string.IsNullOrEmpty(machineValue))
				{
					model.SetInitialValue(machineValue, EnvironmentVariableTarget.Machine);
				}
				else
				{
					model.SetInitialValue(string.Empty, EnvironmentVariableTarget.User);
					model.Source = "Not found";
				}

				variables.Add(model);

				_logger.LogInformation($"Загружена переменная {varName} = {(string.IsNullOrEmpty(model.Value) ? "(пусто)" : model.Value)} (источник: {model.Source})");
			}

			return variables;
		}

		public async Task SaveVariablesAsync(List<EnvironmentVariableModel> variables, IProgress<string>? progress = null)
		{
			var changedVariables = GetChangedVariables(variables);

			if (changedVariables.Count == 0)
			{
				progress?.Report("Нет измененных переменных для сохранения");
				return;
			}

			progress?.Report($"Начинаем сохранение {changedVariables.Count} переменных...");

			foreach (var item in changedVariables)
			{
				await Task.Run(() =>
				{
					try
					{
						var target = item.OriginalTarget;

						var valueToSave = string.IsNullOrEmpty(item.Value) ? string.Empty : item.Value;

						Environment.SetEnvironmentVariable(item.Name, valueToSave, target);
						_logger.LogInformation($"Переменная {item.Name} сохранена со значением '{valueToSave}' на уровне {target}");

						item.AcceptChanges();
					}
					catch (SecurityException ex)
					{
						var message = $"Недостаточно прав для сохранения переменной {item.Name}. " +
									  $"Запустите приложение от имени администратора.\n\n{ex.Message}";

						_logger.LogError(ex, message);
						throw new Exception(message, ex);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, $"Ошибка при сохранении {item.Name}");
						throw;
					}
				});
			}

			progress?.Report($"Сохранено {changedVariables.Count} переменных");
		}

		public async Task ResetVariablesAsync(List<EnvironmentVariableModel> variables, IProgress<string>? progress = null)
		{
			var changedVariables = GetChangedVariables(variables);

			if (changedVariables.Count == 0)
			{
				progress?.Report("Нет измененных переменных для сброса");
				_logger.LogInformation("Нет измененных переменных для сброса");
				return;
			}

			progress?.Report($"Начинаем сброс {changedVariables.Count} переменных...");
			_logger.LogInformation($"Начинаем сброс {changedVariables.Count} переменных...");

			var total = changedVariables.Count;
			var current = 0;

			foreach (var item in changedVariables)
			{
				current++;
				progress?.Report($"Сброс {item.Name} ({current}/{total})...");

				// Выполняем операцию в отдельном потоке
				await Task.Run(() =>
				{
					item.ResetToOriginal();
					_logger.LogInformation($"Переменная {item.Name} сброшена к исходному значению");
				});

				progress?.Report($"↩️ {item.Name} сброшена ({current}/{total})");
			}

			progress?.Report($"Сброшено {changedVariables.Count} переменных");
			_logger.LogInformation($"Сброшено {changedVariables.Count} переменных");
		}

		public bool HasChanges(List<EnvironmentVariableModel> variables)
		{
			return variables.Any(v => v.IsChanged);
		}

		public List<EnvironmentVariableModel> GetChangedVariables(List<EnvironmentVariableModel> variables)
		{
			return variables.Where(v => v.IsChanged).ToList();
		}
	}
}