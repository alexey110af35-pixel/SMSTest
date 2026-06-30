using SmsTestLibrary.Models;

namespace SmsTestLibrary.Services
{
	public class SmsService : IDisposable
	{
		private readonly HttpSmsService? _httpService;
		private readonly GrpcSmsService? _grpcService;
		private readonly bool _useGrpc;
		private bool _disposed;

		public SmsService(string baseUrl, string username, string password)
		{
			if (string.IsNullOrWhiteSpace(baseUrl))
				throw new ArgumentException("BaseUrl не может быть пустым", nameof(baseUrl));

			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username не может быть пустым", nameof(username));

			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("Password не может быть пустым", nameof(password));

			_useGrpc = false;
			_httpService = new HttpSmsService(baseUrl, username, password);
			_grpcService = null;
		}

		public SmsService(string grpcAddress, bool useGrpc = true)
		{
			if (string.IsNullOrWhiteSpace(grpcAddress))
				throw new ArgumentException("GrpcAddress не может быть пустым", nameof(grpcAddress));

			if (!useGrpc)
				throw new ArgumentException("Для gRPC конструктора параметр useGrpc должен быть true", nameof(useGrpc));

			_useGrpc = true;
			_grpcService = new GrpcSmsService(grpcAddress);
			_httpService = null;
		}

		public async Task<List<Dish>> GetMenuAsync()
		{
			if (_useGrpc)
			{
				if (_grpcService == null)
					throw new InvalidOperationException("gRPC сервис не инициализирован");

				return await _grpcService.GetMenuAsync();
			}
			else
			{
				if (_httpService == null)
					throw new InvalidOperationException("HTTP сервис не инициализирован");

				return await _httpService.GetMenuAsync();
			}
		}
		
		public async Task<bool> SendOrderAsync(Order order)
		{
			if (order == null)
				throw new ArgumentNullException(nameof(order), "Заказ не может быть null");

			if (order.OrderItems == null || order.OrderItems.Count == 0)
				throw new ArgumentException("Заказ должен содержать хотя бы одну позицию", nameof(order));

			if (_useGrpc)
			{
				if (_grpcService == null)
					throw new InvalidOperationException("gRPC сервис не инициализирован");

				return await _grpcService.SendOrderAsync(order);
			}
			else
			{
				if (_httpService == null)
					throw new InvalidOperationException("HTTP сервис не инициализирован");

				return await _httpService.SendOrderAsync(order);
			}
		}

		public bool IsGrpcMode => _useGrpc;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_useGrpc)
				{
					_grpcService?.Dispose();
				}
				else
				{
					_httpService?.Dispose();
				}
			}

			_disposed = true;
		}
	}
}