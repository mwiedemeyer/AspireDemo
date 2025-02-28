using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AspireDemo;

public class ExceptionFunction
{
    [Function("ExceptionFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        throw new ApplicationException("This is an exception");
    }
}
