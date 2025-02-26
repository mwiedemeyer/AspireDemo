using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AspireDemo
{
    public class Function1(ILogger<Function1> logger, AppDbContext dbContext)
    {
        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var user = await dbContext.Users.FindAsync("1");

            if (user == null)
            {
                await dbContext.Users.AddAsync(new User { Id = "1", Name = "Azure Functions" });
                await dbContext.SaveChangesAsync();

                user = await dbContext.Users.FindAsync("1");
            }

            return new OkObjectResult($"Welcome {user?.Name}!");
        }
    }
}
