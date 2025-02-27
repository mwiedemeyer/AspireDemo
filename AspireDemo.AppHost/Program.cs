var builder = DistributedApplication.CreateBuilder(args);

var nosecret = builder.AddParameter("NoSecret");
var apiKey = builder.AddParameter("OpenAIApiKey", secret: true);

var cosmos = builder.AddAzureCosmosDB("cosmos")
                    .RunAsPreviewEmulator(p => p.WithDataExplorer().WithDataVolume().WithLifetime(ContainerLifetime.Persistent));

cosmos.AddCosmosDatabase("db")
        .AddContainer("users", "/id");

var sb = builder.AddAzureServiceBus("sb")
                    .RunAsEmulator(p => p.WithLifetime(ContainerLifetime.Persistent))
                    .AddServiceBusTopic("mytopic")
                    .AddServiceBusSubscription("sub1", "sub1");

builder.AddAzureFunctionsProject<Projects.AspireDemo>("func")
    .WithReference(cosmos)
    .WaitFor(cosmos)
    .WithReference(sb)
    .WaitFor(sb)
    .WithEnvironment("NoSecret", nosecret)
    .WithEnvironment("OpenAIApiKey", apiKey)
    // workaround for https://github.com/dotnet/aspire/issues/7785
    .WithEnvironment(context =>
    {
        context.EnvironmentVariables["Aspire__Microsoft__EntityFrameworkCore__Cosmos__AppDbContext__ConnectionString"] = cosmos.Resource.ConnectionStringExpression;
    });

builder.AddProject<Projects.SetupDatabase>("setupdatabase")
    .WithReference(cosmos)
    .WaitFor(cosmos)
    .WithExplicitStart();

builder.Build().Run();
