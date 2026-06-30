using SmsTestWpf.Models;

namespace SmsTestWpf.Services
{
	public interface IEnvironmentService
	{
		List<EnvironmentVariableModel> LoadVariables(List<string> variableNames);
		Task SaveVariablesAsync(List<EnvironmentVariableModel> variables, IProgress<string>? progress = null);
		Task ResetVariablesAsync(List<EnvironmentVariableModel> variables, IProgress<string>? progress = null);
		bool HasChanges(List<EnvironmentVariableModel> variables);
		List<EnvironmentVariableModel> GetChangedVariables(List<EnvironmentVariableModel> variables);
	}
}