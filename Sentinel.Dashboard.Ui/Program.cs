using Sentinel.Dashboard.Ui.Model.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLamar((context, registry) =>
{
    registry.Scan(x =>
    {
        x.AssemblyContainingType<Program>();
        x.WithDefaultConventions();
        x.LookForRegistries();
    });

    registry.For<IIssuesRepository>().Use<FakeIssueRepository>();
    registry.For<IPrometheusRepository>().Use<FakePrometheusRepository>();
});

builder.WebHost.ConfigureKestrel(x => x.ListenAnyIP(8090))
    .ConfigureServices(c =>
    {
        c.AddMemoryCache();
        c.AddHealthChecks();
        c.AddRazorPages(options =>
        {
            options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
        });
    })
    .ConfigureAppConfiguration((_, config) =>
    {
        var builtConfig = config.Build();
        var secretClient = new SecretClient(new Uri(builtConfig["KeyVault"]), new DefaultAzureCredential());
        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    })
    .ConfigureLogging((context, config) =>
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(new JsonFormatter(renderMessage:true))
            .CreateLogger();

        config.ClearProviders();
        config.AddSerilog(logger);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapMetrics();
app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();
