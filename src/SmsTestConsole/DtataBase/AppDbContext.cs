using Microsoft.EntityFrameworkCore;

namespace SmsTestConsole.Database
{
	public class AppDbContext : DbContext
	{
		public DbSet<DishEntity> Dishes { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<DishEntity>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Article)
				.IsRequired()
				.HasMaxLength(50);

				entity.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(200);

				entity.Property(e => e.Price)
				.HasPrecision(18, 2);

				entity.Property(e => e.FullPath)
				.HasMaxLength(500);

				entity.Property(e => e.Barcodes)
				.HasMaxLength(1000);

				entity.HasIndex(e => e.Article)
				.IsUnique();

				entity.HasIndex(e => e.Name);
			});
		}
	}
}