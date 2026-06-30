using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmsTestConsole.Database;
using SmsTestLibrary.Models;

namespace SmsTestConsole.Services
{
	public class DatabaseService : IDatabaseService
	{
		private readonly AppDbContext _context;
		private readonly ILogger<DatabaseService> _logger;

		public DatabaseService(AppDbContext context, ILogger<DatabaseService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task SaveDishesAsync(List<Dish> dishes)
		{
			_logger.LogInformation($"Сохранение {dishes.Count} блюд в БД...");

			foreach (var dish in dishes)
			{
				var entity = new DishEntity
				{
					Id = dish.Id,
					Article = dish.Article,
					Name = dish.Name,
					Price = (decimal)dish.Price,
					IsWeighted = dish.IsWeighted,
					FullPath = dish.FullPath,
					Barcodes = string.Join(";", dish.Barcodes),
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				var existing = await _context.Dishes.FindAsync(entity.Id);
				if (existing != null)
				{
					existing.Article = entity.Article;
					existing.Name = entity.Name;
					existing.Price = entity.Price;
					existing.IsWeighted = entity.IsWeighted;
					existing.FullPath = entity.FullPath;
					existing.Barcodes = entity.Barcodes;
					existing.UpdatedAt = DateTime.UtcNow;
				}
				else
				{
					await _context.Dishes.AddAsync(entity);
				}
			}

			await _context.SaveChangesAsync();
			_logger.LogInformation("Блюда успешно сохранены");
		}

		public async Task<List<Dish>> GetDishesAsync()
		{
			_logger.LogInformation("Чтение блюд из БД...");

			var entities = await _context.Dishes.ToListAsync();

			var dishes = entities.Select(e => new Dish
			{
				Id = e.Id,
				Article = e.Article,
				Name = e.Name,
				Price = (double)e.Price,
				IsWeighted = e.IsWeighted,
				FullPath = e.FullPath,
				Barcodes = e.Barcodes.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
			}).ToList();

			_logger.LogInformation($"Прочитано {dishes.Count} блюд");
			return dishes;
		}
		
		public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

				if (!canConnect)
				{
					_logger.LogWarning("База данных не существует. Создание...");
					await _context.Database.EnsureCreatedAsync(cancellationToken);
					_logger.LogInformation("База данных создана");
				}
				else
				{
					_logger.LogInformation("База данных существует");

					var tableExists = await _context.Database
						.ExecuteSqlRawAsync("SELECT 1 FROM information_schema.tables WHERE table_name = 'Dishes'", cancellationToken);

					if (tableExists == 0)
					{
						_logger.LogWarning("Таблица Dishes не существует. Создание через миграции...");
						await _context.Database.MigrateAsync(cancellationToken);
						_logger.LogInformation("Таблица Dishes создана");
					}
					else
					{
						_logger.LogInformation("Таблица Dishes существует");
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Операция создания БД была отменена");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Ошибка при проверке БД: {ex.Message}");
				throw;
			}
		}
	}
}