// Models/EnvironmentVariableModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmsTestWpf.Models
{
	public partial class EnvironmentVariableModel : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _value = string.Empty;

		[ObservableProperty]
		private string _originalValue = string.Empty;

		[ObservableProperty]
		private EnvironmentVariableTarget _originalTarget = EnvironmentVariableTarget.User;

		[ObservableProperty]
		private string _source = "User";
		[ObservableProperty]
		private bool _isChanged;

		partial void OnValueChanged(string value)
		{
			IsChanged = value != OriginalValue;
		}

		public void ResetToOriginal()
		{
			Value = OriginalValue;
			IsChanged = false;
		}

		public void AcceptChanges()
		{
			OriginalValue = Value;
			IsChanged = false;
		}

		public void SetInitialValue(string value)
		{
			Value = value;
			OriginalValue = value;
			IsChanged = false;
		}

		public void SetInitialValue(string value, EnvironmentVariableTarget target)
		{
			Value = value;
			OriginalValue = value;
			OriginalTarget = target;
			Source = target.ToString();
			IsChanged = false;
		}
	}
}