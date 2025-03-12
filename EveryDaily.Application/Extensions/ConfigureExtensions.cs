using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Core.Settings;
using EveryDaily.Persistence;
using EveryDaily.Persistence.BaseRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

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
            builder.WithOrigins("http://localhost:3000", "https://dailyngo.com", "http://dailyngo.com")
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
      // asagidaki ornekteki gibi repositoryler eklenebilir.
      services.AddScoped<MongoDbRepository<TestModel,ObjectId>,TestRepository>();
   }
   
   public static void ConfigureServices(this IServiceCollection services)
   {
      services.AddTransient<IUserService, UserService>();
   }
}