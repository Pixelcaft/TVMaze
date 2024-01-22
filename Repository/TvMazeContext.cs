using Microsoft.EntityFrameworkCore;

namespace TVMaze.Repository
{
    public class TvMazeContext : DbContext
    {
        public TvMazeContext(DbContextOptions<TvMazeContext> options): base(options) { }


        public DbSet<Show> Shows { get; set; }
        public DbSet<Actor> Actors { get; set; }

    }
}
