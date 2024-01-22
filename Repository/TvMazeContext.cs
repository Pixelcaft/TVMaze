using Microsoft.EntityFrameworkCore;

namespace TVMaze.Repository
{
    public class TvMazeContext : DbContext
    {
        public DbSet<Show> Shows { get; set; }
        public DbSet<Actor> Actors { get; set; }

        public TvMazeContext(DbContextOptions<TvMazeContext> options) : base(options)
        {
            // Voer database migraties uit bij het starten van de applicatie
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Hier kun je eventuele extra configuraties voor je modellen toevoegen
            // bijvoorbeeld: modelBuilder.Entity<Show>().Property(s => s.SomeProperty).IsRequired();
        }
    }

    public class Show
    {
        public int ID { get; set; }
        public int ShowId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class Actor
    {
        public int ID { get; set; }
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public int ShowID { get; set; }

        // Navigatie-eigenschap voor het Show-model
        public Show Show { get; set; }
    }
}
