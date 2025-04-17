using System.Net;
using IssueStatsService.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StackExchange.Redis;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Define the retry policy for Redis
var redisRetryPolicy = Policy
    .Handle<RedisConnectionException>()
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(5));

// Execute your Redis initialization with the retry policy
redisRetryPolicy.ExecuteAndCapture(() =>
{
    // Create the Redis connection
    var redisConfiguration = $"{builder.Configuration.GetValue("Redis:Host", "localhost")},password={builder.Configuration.GetValue("Redis:Password", "redis_password")}";
    var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration);

    builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<IssueCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("issue-stats", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest")!);
            h.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest")!);
        });

        cfg.ConfigureEndpoints(context);

        cfg.UseMessageRetry(r =>
        {
            r.Interval(retryCount: 5, interval: TimeSpan.FromSeconds(5));
        });
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
