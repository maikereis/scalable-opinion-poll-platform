using Microsoft.EntityFrameworkCore;
using Parrhesia.Infrastructure.Persistence;

namespace Parrhesia.Infrastructure.Tests;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly ParrhesiaDbContext Context;

    protected IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<ParrhesiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ParrhesiaDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
