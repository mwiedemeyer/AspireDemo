var builder = DistributedApplication.CreateBuilder(args);

var nosecret = builder.AddParameter("NoSecret");
var apiKey = builder.AddParameter("OpenAIApiKey", secret: true);

var cosmos = builder.AddAzureCosmosDB("cosmos")
                    .RunAsPreviewEmulator(p => p.WithDataExplorer().WithDataVolume().WithLifetime(ContainerLifetime.Persistent))
                    .AddCosmosDatabase("db")
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
    .WithEnvironment("OpenAIApiKey", apiKey);

builder.Build().Run();
