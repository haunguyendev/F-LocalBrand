using DotNetEnv;
using Microsoft.Extensions.Configuration;
using SWD.F_LocalBrand.Business.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Config
{
    public class ConfigEnv
    {
        private readonly IConfiguration _configuration;

        public ConfigEnv()
        {
            // Load the .env file into environment variables
            Env.Load();
        }

        public JwtSettings LoadJwtSettings()
        {
            var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured.");
            }
            var jwtSettings = new JwtSettings
            {
                Key = secretKey
            };
            return jwtSettings;
        }

        public static IConfiguration LoadEnvConfiguration()
        {
            // Load the .env file into environment variables
            Env.Load();

            // Build a configuration from the environment variables
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            // Return the built configuration
            return builder.Build();
        }

        public static JwtSettings LoadJwtSettings(IConfiguration configuration)
        {
            // Load the JWT settings from the environment variables
            var jwtSettings = new JwtSettings
            {
                Key = configuration["SECRET_KEY"]
            };

            if (string.IsNullOrEmpty(jwtSettings.Key))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured.");
            }

            return jwtSettings;
        }

    }
}
