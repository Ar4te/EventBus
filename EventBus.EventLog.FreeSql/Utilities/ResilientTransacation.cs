using Microsoft.EntityFrameworkCore;

namespace EventBus.EventLog.FreeSql.Utilities;

public class ResilientTransacation
{
    private readonly DbContext _dbContext;

    private ResilientTransacation(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public static ResilientTransacation New(DbContext dbContext) => new(dbContext);

    public async Task ExecuteAsync(Func<Task> func)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await func();
            await transaction.CommitAsync();
        });
    }
}