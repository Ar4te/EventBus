//using System.Collections.Concurrent;

//namespace TimedTask;

//public interface IJob
//{
//    void Execute(IDictionary<string, object> jobDataMap);
//}

//public class Scheduler
//{
//    private readonly List<Timer> _timers = new();
//    private readonly ConcurrentDictionary<string, JobDetail> _jobDetails = new();

//    public void ScheduleJob(JobDetail jobDetail, TimeSpan interval)
//    {
//        _jobDetails.TryAdd(jobDetail.Name, jobDetail);
//        var timer = new Timer(_ =>
//        {
//            if (jobDetail.IsPause) return;
//            jobDetail.JobInstance?.Execute(jobDetail.JobDataMap);

//        }, null, TimeSpan.Zero, interval);

//        _timers.Add(timer);
//    }

//    public void PauseJob(string jobName)
//    {
//        if (_jobDetails.TryGetValue(jobName, out var jobDetail) && jobDetail is { IsPause: false })
//        {
//            jobDetail.Pause();
//        }
//    }

//    public void ResumeJob(string jobName)
//    {
//        if (_jobDetails.TryGetValue(jobName, out var jobDetail) && jobDetail is { IsPause: true })
//        {
//            jobDetail.Resume();
//        }
//    }
//}

//public class JobDetail
//{
//    public Type JobType { get; }
//    public string Name { get; }
//    public string Group { get; }
//    public IDictionary<string, object> JobDataMap { get; }
//    public bool IsPause { get; private set; }

//    private IJob? _jobInstance = null;

//    public IJob? JobInstance
//    {
//        get
//        {
//            _jobInstance ??= Activator.CreateInstance(JobType) as IJob;
//            return _jobInstance;
//        }
//    }

//    public JobDetail(Type jobType, string name, string group, IDictionary<string, object> jobDataMap = null)
//    {
//        JobType = jobType ?? throw new ArgumentNullException(nameof(jobType));
//        Name = name ?? throw new ArgumentNullException(nameof(name));
//        Group = group ?? throw new ArgumentNullException(nameof(group));
//        JobDataMap = jobDataMap ?? new Dictionary<string, object>();
//    }

//    public void Pause()
//    {
//        IsPause = true;
//    }

//    public void Resume()
//    {
//        IsPause = false;
//    }
//}
