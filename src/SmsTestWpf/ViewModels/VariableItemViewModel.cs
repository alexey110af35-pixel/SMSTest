using SmsTestWpf.Helpers;
using SmsTestWpf.Models;

namespace SmsTestWpf.ViewModels
{
	public class VariableItemViewModel : ViewModelBase
	{
		private readonly EnvVariableModel _model;
		private bool _isModified;
		private bool _isSelected; // Новое свойство для удержания стрелочки
		private string _currentValue = string.Empty;
		private string _comment = string.Empty;

		public VariableItemViewModel(EnvVariableModel model)
		{
			_model = model;
			_currentValue = model.Value;
			_comment = model.Comment;
		}

		public string Field => _model.Name;

		public string Value
		{
			get => _currentValue;
			set
			{
				if (_currentValue != value)
				{
					_currentValue = value;
					OnPropertyChanged();
					IsModified = _currentValue != _model.Value;
				}
			}
		}

		public string Comment
		{
			get => _comment;
			set
			{
				if (_comment != value)
				{
					_comment = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsModified
		{
			get => _isModified;
			set
			{
				if (_isModified != value)
				{
					_isModified = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					OnPropertyChanged();
				}
			}
		}

		public void ApplyChanges()
		{
			_model.Value = _currentValue;
			_model.Comment = _comment;
			IsModified = false;
		}

		public void ResetChanges()
		{
			_currentValue = _model.Value;
			_comment = _model.Comment;
			IsModified = false;
			OnPropertyChanged(nameof(Value));
			OnPropertyChanged(nameof(Comment));
		}
	}
}
