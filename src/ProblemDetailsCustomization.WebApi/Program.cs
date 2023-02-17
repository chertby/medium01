using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsCustomization.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// For more information, see https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-7.0#problem-details-service
builder.Services.AddProblemDetails();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var exceptionHandlerOptions = new ExceptionHandlerOptions
{
    ExceptionHandler = async (context) =>
    {
        if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            await ExceptionHandler();
        }

        async Task ExceptionHandler()
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

            var statusCode = exceptionHandlerFeature!.Error switch
            {
                ApplicationSpecificException => StatusCodes.Status418ImATeapot,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Title = "I'm a teapot",
                Detail = exceptionHandlerFeature.Error.Message,
                Status = statusCode
            };

            if (app.Environment.IsDevelopment())
            {
                problemDetails.Title = exceptionHandlerFeature.Error.GetType().ToString();
                problemDetails.Extensions["exception"] = new
                {
                    Details = exceptionHandlerFeature.Error.ToString(),
                    context.Request.Headers,
                    Path = context.Request.Path.ToString(),
                    Endpoint = exceptionHandlerFeature.Endpoint?.ToString(),
                    exceptionHandlerFeature.RouteValues,
                };
            }

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                AdditionalMetadata = exceptionHandlerFeature.Endpoint?.Metadata,
                ProblemDetails = problemDetails
            });
        }
    }
};
app.UseExceptionHandler(exceptionHandlerOptions);
app.UseStatusCodePages();

app.MapGet("coffee", () =>
{
    throw new ApplicationSpecificException("You won't get coffee.");
});

app.Run();