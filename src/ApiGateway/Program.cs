using Yarp.ReverseProxy;
var builder = WebApplication.CreateBuilder(args);
// Đọc cấu hình reverse proxy từ appsettings
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
// // Add services to the container.
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware kiểm tra host được phép gọi
app.Use(async (context, next) =>
{
    var allowedHost = builder.Configuration["AllowedHost"];
    var origin = context.Request.Headers["Origin"].FirstOrDefault()
                 ?? context.Request.Headers["Host"].FirstOrDefault();

    if (origin != null && origin.Contains(allowedHost))
    {
        await next();
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Forbidden: Invalid Host");
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
// Forward request nếu qua được middleware
app.MapReverseProxy();

app.Run();
