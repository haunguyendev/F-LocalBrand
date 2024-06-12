using F_LocalBrand.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using SWD.F_LocalBrand.API.Attributes;
using SWD.F_LocalBrand.API.Common.Payloads.Requests;
using SWD.F_LocalBrand.API.Hubs;
using SWD.F_LocalBrand.API.Middlewares;
using SWD.F_LocalBrand.API.Settings;
using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.Helpers;
using SWD.F_LocalBrand.Business.Mapper;
using SWD.F_LocalBrand.Business.Services;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.DataAccess;
using SWD.F_LocalBrand.Data.Repositories;
using SWD.F_LocalBrand.Data.UnitOfWorks;
using System.Text;

namespace SWD.F_LocalBrand.API.Extentions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<ExceptionMiddleware>();
            services.AddControllers();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSignalR();



            var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured.");
            }
            var jwtSettings = new JwtSettings
            {
                Key = secretKey
            };
            services.Configure<JwtSettings>(val =>
            {
                val.Key = jwtSettings.Key;
            });

            //services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

            //services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));

            services.AddAuthorization();


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });
            services.ConfigureDbContext(configuration);

            //Get config mail form environment
            services.Configure<MailSettings>(options =>
            {
                options.Server = Environment.GetEnvironmentVariable("MailSettings__Server");
                options.Port = int.Parse(Environment.GetEnvironmentVariable("MailSettings__Port") ?? "0");
                options.SenderName = Environment.GetEnvironmentVariable("MailSettings__SenderName");
                options.SenderEmail = Environment.GetEnvironmentVariable("MailSettings__SenderEmail");
                options.UserName = Environment.GetEnvironmentVariable("MailSettings__UserName");
                options.Password = Environment.GetEnvironmentVariable("MailSettings__Password");
            });
            var redisConnection = new RedisConnection();
            configuration.GetSection("RedisConnection").Bind(redisConnection);

            // Register RedisConfiguration as a singleton
            services.AddSingleton(redisConnection);

            // Configure Redis connection
            services.AddSingleton<IConnectionMultiplexer>(option =>
               ConnectionMultiplexer.Connect(new ConfigurationOptions
               {

                   EndPoints = { $"{redisConnection.Host}:{redisConnection.Port}" },
                   //Ssl = redisConnection.IsSSL,
                   //Password = redisConnection.Password


               }));
            services.AddSingleton<MessageHub>();

            // Add StackExchangeRedisCache as the IDistributedCache implementation
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{redisConnection.Host}:{redisConnection.Port}";
            });

            

            services.AddInfrastructureServices();
            // Add Mapper Services to Container injection
            services.AddAutoMapper(typeof(ApplicationMapper));
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "FLocalBrand API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
            });


            services.AddCors(option =>
                option.AddPolicy("CORS", builder =>
                    builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((host) => true)));


            return services;
        }
        private static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbTrustServerCertificate = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE");
            var dbMultipleActiveResultSets = Environment.GetEnvironmentVariable("DB_MULTIPLE_ACTIVE_RESULT_SETS");

            var connectionString = $"Data Source={dbServer};Initial Catalog={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate={dbTrustServerCertificate};MultipleActiveResultSets={dbMultipleActiveResultSets}";

            services.AddDbContext<SwdFlocalBrandContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });

            return services;
        }

        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            return services.AddTransient(typeof(IRepositoryBaseAsync<>), typeof(RepositoryBaseAsync<>))
                .AddTransient<IUserRepository, UserRepository>()
                .AddTransient<ICustomerRepository, CustomerRepository>()
                .AddTransient<IUnitOfWork, UnitOfWork>()
                .AddScoped<IdentityService>()
                .AddScoped<UserService>()
                .AddScoped<JwtSettings>()
                .AddScoped<EmailService>()
                .AddScoped<CustomerService>()

                // Register ResponseCacheService
                .AddSingleton<IResponseCacheService, ResponseCacheService>()

                //Add Validation

                .AddScoped<IValidator<LoginRequest>, LoginValidation>()
                .AddScoped<IValidator<SignupRequest>, SignupValidation>();




        }
    }
}
