using ConnetToRedisSample.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;
RedisConnection redisConnection = new();
config.GetSection("RedisConnection").Bind(redisConnection);

builder.Services.AddSingleton<IConnectionMultiplexer>(option =>
   ConnectionMultiplexer.Connect(new ConfigurationOptions
   {
       EndPoints = { $"{redisConnection.Host}:{redisConnection.Port}" },
       AbortOnConnectFail = false,
       Ssl = redisConnection.IsSSL,
       Password = redisConnection.Password
   }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", async (IConnectionMultiplexer redis) =>
{
    var result = await redis.GetDatabase().PingAsync();
    try
    {
        return result.CompareTo(TimeSpan.Zero) > 0 ? $"PONG: {result}" : null;
    }
    catch (RedisTimeoutException e)
    {
        throw;
    }
});

app.Run();

