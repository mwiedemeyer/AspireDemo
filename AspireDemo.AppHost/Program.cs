using AspireDemo.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var nosecret = builder.AddParameter("NoSecret");
var apiKey = builder.AddParameter("OpenAIApiKey", secret: true);

var cosmos = builder.AddAzureCosmosDB("cosmos")
                    .RunAsPreviewEmulator(p => p.WithDataExplorer().WithDataVolume().WithLifetime(ContainerLifetime.Persistent));

cosmos.AddCosmosDatabase("db")
        .AddContainer("users", "/id");

var sb = builder.AddAzureServiceBus("sb")
                    .RunAsEmulator(p => p.WithLifetime(ContainerLifetime.Persistent));

var sbUserTopic = sb.AddServiceBusTopic("user-topic");
sbUserTopic.AddServiceBusSubscription("sub-processing-func", "sub-processing-func")
            .WithPeekMessagesCommand();

builder.AddAzureFunctionsProject<Projects.AspireDemo>("func-user")
    .WithReference(cosmos)
    .WaitFor(cosmos)
    .WithReference(sb)
    .WaitFor(sb)
    .WithEnvironment("NoSecret", nosecret)
    .WithEnvironment("OpenAIApiKey", apiKey)
    // workaround for https://github.com/dotnet/aspire/issues/7785
    .WithEnvironment(context =>
    {
        context.EnvironmentVariables["ConnectionStrings:cosmos"] = cosmos.Resource.ConnectionStringExpression;
        context.EnvironmentVariables["ConnectionStrings:sb"] = sb.Resource.ConnectionStringExpression;
    });

builder.AddProject<Projects.SetupDatabase>("setupdatabase")
    .WithReference(cosmos)
    .WaitFor(cosmos)
    .WithExplicitStart();

builder.AddAzureFunctionsProject<Projects.ProcessingFunction>("func-processing")
    .WithReference(sb)
    .WaitFor(sb)
    // workaround for https://github.com/dotnet/aspire/issues/7785
    .WithEnvironment(context =>
    {
        context.EnvironmentVariables["ConnectionStrings:sb"] = sb.Resource.ConnectionStringExpression;
    });

builder.Build().Run();
