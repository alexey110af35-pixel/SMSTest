// Services/DatabaseService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmsTestConsole.Database;
using SmsTestLibrary.Models;

namespace SmsTestConsole.Services
{
	public interface IDatabaseService
	{
		Task InitializeDatabaseAsync();
		Task SaveDishesAsync(List<Dish> dishes);
		Task<List<Dish>> GetDishesAsync();
	}

	public class DatabaseService : IDatabaseService
	{
		private readonly AppDbContext _context;
		private readonly ILogger<DatabaseService> _logger;

		public DatabaseService(AppDbContext context, ILogger<DatabaseService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task InitializeDatabaseAsync()
		{
			try
			{
				// Применяем миграции (создает БД и таблицы)
				await _context.Database.MigrateAsync();
				_logger.LogInformation("База данных успешно инициализирована с помощью миграций");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при инициализации базы данных");
				throw;
			}
		}

		public async Task SaveDishesAsync(List<Dish> dishes)
		{
			try
			{
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
				_logger.LogInformation($"Сохранено {dishes.Count} блюд в базу данных");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении блюд в БД");
				throw;
			}
		}

		public async Task<List<Dish>> GetDishesAsync()
		{
			try
			{
				var entities = await _context.Dishes.ToListAsync();
				return entities.Select(e => new Dish
				{
					Id = e.Id,
					Article = e.Article,
					Name = e.Name,
					Price = (double)e.Price,
					IsWeighted = e.IsWeighted,
					FullPath = e.FullPath,
					Barcodes = e.Barcodes.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
				}).ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении блюд из БД");
				throw;
			}
		}
	}
}