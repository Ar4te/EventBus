var builder = DistributedApplication.CreateBuilder(args);

//var redis = builder.AddRedis("redis");
//var rabbitmq = builder.AddRabbitMQ("eventbus");
builder.AddProject<Projects.WebApplication1>("webapplication1");
    //.WithReference(redis)
    //.WithReference(rabbitmq);

builder.Build().Run();
