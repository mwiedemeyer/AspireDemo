using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AspireDemo
{
    public class UserFunctions(ILogger<UserFunctions> logger, AppDbContext dbContext, ServiceBusClient serviceBusClient)
    {
        [Function("AddUser")]
        public async Task<IActionResult> AddUser([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            logger.LogInformation("AddUser executing...");

            var user = await dbContext.Users.AddAsync(new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Azure Functions",
                LastUpdate = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            logger.LogInformation("User {Id} created", user?.Entity?.Id);
            // for demonstration of json log viewer in Aspire Dashboard
            logger.LogWarning(JsonSerializer.Serialize(new {Message = $"User {user?.Entity?.Id} created" }));

            await serviceBusClient.CreateSender("user-topic").SendMessageAsync(new ServiceBusMessage(new BinaryData(user.Entity)));

            return new OkObjectResult(new { Message = $"User {user?.Entity?.Id} created" });
        }

        [Function("GetUser")]
        public async Task<IActionResult> GetUser([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetUser/{userId}")] HttpRequest req, string userId)
        {
            logger.LogInformation("GetUser for {UserId} executing...", userId);

            var user = await dbContext.Users.FindAsync(userId);

            if (user is null)
            {
                logger.LogInformation("User with ID {Id} not found", userId);
                return new OkObjectResult($"User with ID {userId} not found");
            }

            logger.LogInformation("User with ID {Id} found", user?.Id);

            dbContext.Entry(user).Property(u => u.LastUpdate).CurrentValue = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            logger.LogInformation("User with ID {Id} updated", user?.Id);

            return new OkObjectResult(user);
        }
    }
}
