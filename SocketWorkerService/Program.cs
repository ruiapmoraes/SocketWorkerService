using SocketWorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<SocketServerWorker>();
    })
    .Build();

await host.RunAsync();
