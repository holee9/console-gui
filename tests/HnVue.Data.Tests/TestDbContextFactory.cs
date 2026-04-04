using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests;

/// <summary>
/// Creates isolated in-memory <see cref="HnVueDbContext"/> instances for unit testing.
/// Each call returns a context backed by a unique in-memory database to prevent test interference.
/// </summary>
internal static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new <see cref="HnVueDbContext"/> backed by an in-memory database.
    /// The database schema is automatically created.
    /// </summary>
    internal static HnVueDbContext Create()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new HnVueDbContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
