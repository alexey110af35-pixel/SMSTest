var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/menu", async (HttpContext context) =>
{
	using var reader = new StreamReader(context.Request.Body);
	var body = await reader.ReadToEndAsync();

	Console.WriteLine($"Получен запрос: {body}");
	
	var authHeader = context.Request.Headers.Authorization.ToString();
	if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
	{
		context.Response.StatusCode = 401;
		return Results.Json(new { error = "Unauthorized" });
	}

	var response = new
	{
		Command = "GetMenu",
		Success = true,
		ErrorMessage = "",
		Data = new
		{
			MenuItems = new[]
			{
				new
				{
					Id = "5979224",
					Article = "A1004292",
					Name = "Каша гречневая",
					Price = 50,
					IsWeighted = false,
					FullPath = "ПРОИЗВОДСТВО\\Гарниры",
					Barcodes = new[] { "57890975627974236429" }
				},
				new
				{
					Id = "9084246",
					Article = "A1004293",
					Name = "Конфеты Коровка",
					Price = 300,
					IsWeighted = true,
					FullPath = "ДЕСЕРТЫ\\Развес",
					Barcodes = Array.Empty<string>()
				},
				new
				{
					Id = "1234567",
					Article = "A1004294",
					Name = "Салат Цезарь",
					Price = 250,
					IsWeighted = false,
					FullPath = "САЛАТЫ\\Горячие",
					Barcodes = new[] { "12345678901234567890" }
				}
			}
		}
	};

	return Results.Json(response);
});

app.MapPost("/api/order", async (HttpContext context) =>
{
	using var reader = new StreamReader(context.Request.Body);
	var body = await reader.ReadToEndAsync();

	Console.WriteLine($"Получен заказ: {body}");
	
	var authHeader = context.Request.Headers.Authorization.ToString();
	if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
	{
		context.Response.StatusCode = 401;
		return Results.Json(new { error = "Unauthorized" });
	}

	var response = new
	{
		Command = "SendOrder",
		Success = true,
		ErrorMessage = ""
	};

	return Results.Json(response);
});

//app.Run("http://localhost:5000");
app.Run();