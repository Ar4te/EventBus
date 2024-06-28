using EventBus.EventLog.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.TimedTasks;

public class TestCustomTimedTaskService
{
    private readonly TestDbContext _testDbContext;

    public TestCustomTimedTaskService(TestDbContext testDbContext)
    {
        _testDbContext = testDbContext;
    }
    public async Task<string> TestCustomTimedTask()
    {
        var count = await _testDbContext.Set<IntegrationEventLogEntry>().ToListAsync();

        return "TestCustomTimedTask" + DateTime.Now + $";IntegrationLog count = {count.Count}";
    }
}
