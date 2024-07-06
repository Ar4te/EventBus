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
                return OperateResultExtension.Success();
            }
            return OperateResultExtension.Fail($"停止[{timedTask.Name}]发生异常：" + "从运行列表中删除任务失败");
        }
        catch (Exception ex)
        {
            return OperateResultExtension.Fail($"停止[{timedTask.Name}]发生异常：" + ex.Message);
        }
    }

    private OperateResult StartTask(TimedTaskDetail timedTask)
    {
        try
        {
            timedTask.Start();
            _runningTasks.AddOrUpdate(timedTask.Name, timedTask, (_, old) => timedTask);
            return OperateResultExtension.Success();
        }
        catch (Exception ex)
        {
            return OperateResultExtension.Fail($"启动[{timedTask.Name}]发生异常：" + ex.Message);
        }
    }
}