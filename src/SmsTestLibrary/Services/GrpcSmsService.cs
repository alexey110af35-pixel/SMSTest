using Grpc.Net.Client;
using Sms.Test;
using SmsTestLibrary.Models;

namespace SmsTestLibrary.Services
{
	public class GrpcSmsService : IDisposable
	{
		private readonly GrpcChannel _channel;
		private readonly SmsTestService.SmsTestServiceClient _client;

		public GrpcSmsService(string serverAddress)
		{
			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

			_channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
			{
				HttpHandler = handler
			});

			_client = new SmsTestService.SmsTestServiceClient(_channel);
		}

		public async Task<List<Dish>> GetMenuAsync(bool withPrice = true)
		{
			try
			{
				var request = new Google.Protobuf.WellKnownTypes.BoolValue
				{
					Value = withPrice
				};

				var response = await _client.GetMenuAsync(request);

				if (!response.Success)
				{
					throw new Exception(response.ErrorMessage ?? "Неизвестная ошибка сервера");
				}

				return response.MenuItems.Select(item => new Dish
				{
					Id = item.Id,
					Article = item.Article,
					Name = item.Name,
					Price = item.Price,
					IsWeighted = item.IsWeighted,
					FullPath = item.FullPath,
					Barcodes = item.Barcodes.ToList()
				}).ToList();
			}
			catch (Grpc.Core.RpcException ex)
			{
				throw new Exception($"gRPC ошибка: {ex.Message}", ex);
			}
		}
				
		public async Task<bool> SendOrderAsync(Models.Order order)
		{
			try
			{
				if (order == null)
					throw new ArgumentNullException(nameof(order));
								
				var grpcOrder = new Sms.Test.Order
				{
					Id = order.Id ?? Guid.NewGuid().ToString()
				};

				foreach (var item in order.OrderItems)
				{
					grpcOrder.OrderItems.Add(new Sms.Test.OrderItem
					{
						Id = item.Id,
						Quantity = item.Quantity
					});
				}

				var response = await _client.SendOrderAsync(grpcOrder);

				if (!response.Success)
				{
					throw new Exception(response.ErrorMessage ?? "Неизвестная ошибка сервера");
				}

				return true;
			}
			catch (Grpc.Core.RpcException ex)
			{
				throw new Exception($"gRPC ошибка: {ex.Message}", ex);
			}
		}

		public void Dispose()
		{
			_channel?.Dispose();
		}
	}
}