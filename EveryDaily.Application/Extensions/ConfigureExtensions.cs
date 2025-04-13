using EveryDaily.Application.Consumers;
using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Services.Badge;
using EveryDaily.Application.Services.Email;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Prefix.RabbitMQ;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EveryDaily.Application.Extensions;

public static class ConfigureExtensions
{
   /// <summary>
   /// Redis baglantisinin yapilandirilmasi
   /// </summary>
   /// <param name="services"></param>
   /// <param name="configuration"></param>
   public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
   {
      services.AddMemoryCache();
      services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));
      services.AddSingleton<IRedisService>(sp =>
      {
         var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;

         var redis = new RedisService(redisSettings.Host, redisSettings.Port);

         var result = redis.Connect();
         return redis;
      });
   }
   
   public static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
   {
      services.AddMassTransit(x =>
      {
         x.AddConsumersFromNamespaceContaining<EmailSendingConsumer>();
         x.UsingRabbitMq((context, cfg) =>
         {
            cfg.Host(configuration["ConnectionStrings:EventBusConnection"]);
            cfg.UseMessageRetry(r => r.Interval(10, TimeSpan.FromSeconds(5)));
            cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10)));
        
            cfg.ReceiveEndpoint(RabbitmqConstacts.EMAIL_SENDING_QUEUE, c =>
            {
               c.ConfigureConsumer<EmailSendingConsumer>(context);
            });

            cfg.ReceiveEndpoint(RabbitmqConstacts.RANK_ACTIVITY_QUEUE, c =>
            {
                c.ConfigureConsumer<RankActivityConsumer>(context);
            });
         });
      });
   }
   
   /// <summary>
   /// Cors politikalarinin yapilandirilmasi
   /// </summary>
   /// <param name="services"></param>
   public static void ConfigureCors(this IServiceCollection services,string corsName)
   {
      services.AddCors(options =>
      {
         options.AddPolicy(corsName, builder =>
         {
            builder.WithOrigins("http://localhost:3000", "https://dailyngo.com","https://dailyngo.com")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
         });
      });
   }

   /// <summary>
   /// Database baglantisinin yapilandirilmasi
   /// </summary>
   /// <param name="services"></param>
   /// <param name="configuration"></param>
   public static void ConfigureNpgsql(this IServiceCollection services, IConfiguration configuration)
   {
      services.AddDbContext<AppDbContext>(options =>
         options.UseNpgsql(configuration.GetConnectionString("NpgsqlConnection"),
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
   }
   
   public static void ConfigureMongoDbRepositories(this IServiceCollection services, IConfiguration configuration)
   {
      services.Configure<MongoDbSettings>(configuration.GetSection("MongoDBConnection"));
      services.AddScoped<MongoDocContext>(opt => new MongoDocContext(opt.GetRequiredService<IOptions<MongoDbSettings>>()));
    }
   
   public static void ConfigureServices(this IServiceCollection services)
   {
      services.AddTransient<IUserService, UserService>();
      services.AddTransient<IEmailService, EmailService>();
      services.AddScoped<IRankService, RankService>();
    }
}