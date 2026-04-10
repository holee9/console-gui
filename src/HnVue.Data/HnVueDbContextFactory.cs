using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HnVue.Data;

/// <summary>
/// Design-time factory for <see cref="HnVueDbContext"/>.
/// Used by EF Core CLI tools (dotnet ef migrations) to create DbContext instances.
/// </summary>
public sealed class HnVueDbContextFactory : IDesignTimeDbContextFactory<HnVueDbContext>
{
    /// <inheritdoc/>
    public HnVueDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HnVueDbContext>();
        optionsBuilder.UseSqlite("Data Source=hnvue_design.db");

        return new HnVueDbContext(optionsBuilder.Options);
    }
}
