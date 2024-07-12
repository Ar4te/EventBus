using Infrastructure;

namespace TimedTask.Base;

public sealed partial class TimedTaskScheduler
{
    private OperateResult StopTask(TimedTaskDetail timedTask)
    {
        try
        {
            if (_runningTasks.TryRemove(new(timedTask.Name, timedTask)))
            {
                timedTask.Stop();
                _timedTaskGroupInfos[timedTask.Group].Remove(timedTask.Name);
                return Success();
            }
            return Fail($"停止[{timedTask.Name}]发生异常：" + "从运行列表中删除任务失败");
        }
        catch (Exception ex)
        {
            return Fail($"停止[{timedTask.Name}]发生异常：" + ex.Message);
        }
    }

    private OperateResult StartTask(TimedTaskDetail timedTask)
    {
        try
        {
            _taskFactory.StartNew(async () => await timedTask.Start());
            _runningTasks.AddOrUpdate(timedTask.Name, timedTask, (_, old) => timedTask);
            return Success();
        }
        catch (Exception ex)
        {
            return Fail($"启动[{timedTask.Name}]发生异常：" + ex.Message);
        }
    }
}