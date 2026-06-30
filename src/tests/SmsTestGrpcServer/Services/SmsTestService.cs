using Grpc.Core;
using Sms.Test;

namespace SmsTestGrpcServer.Services
{
	public class SmsTestService : Sms.Test.SmsTestService.SmsTestServiceBase
	{
		private readonly ILogger<SmsTestService> _logger;

		public SmsTestService(ILogger<SmsTestService> logger)
		{
			_logger = logger;
		}

		public override async Task<GetMenuResponse> GetMenu(
			Google.Protobuf.WellKnownTypes.BoolValue request,
			ServerCallContext context)
		{
			_logger.LogInformation("GetMenu вызван с WithPrice={WithPrice}", request.Value);

			var response = new GetMenuResponse
			{
				Success = true,
				ErrorMessage = ""
			};

			response.MenuItems.Add(new MenuItem
			{
				Id = "5979224",
				Article = "A1004292",
				Name = "Каша гречневая",
				Price = 50,
				IsWeighted = false,
				FullPath = "ПРОИЗВОДСТВО\\Гарниры"
			});
			response.MenuItems[0].Barcodes.Add("57890975627974236429");

			response.MenuItems.Add(new MenuItem
			{
				Id = "9084246",
				Article = "A1004293",
				Name = "Конфеты Коровка",
				Price = 300,
				IsWeighted = true,
				FullPath = "ДЕСЕРТЫ\\Развес"
			});

			return await Task.FromResult(response);
		}

		public override async Task<SendOrderResponse> SendOrder(
			Order request,
			ServerCallContext context)
		{
			_logger.LogInformation("SendOrder вызван с OrderId={OrderId}", request.Id);
			_logger.LogInformation("Количество позиций: {Count}", request.OrderItems.Count);

			foreach (var item in request.OrderItems)
			{
				_logger.LogInformation("  Item: Id={Id}, Quantity={Quantity}", item.Id, item.Quantity);
			}

			return await Task.FromResult(new SendOrderResponse
			{
				Success = true,
				ErrorMessage = ""
			});
		}
	}
}