using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Config
{
    public class CloudMessageConfig
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var firebaseAdminSDK = new Dictionary<string, string>
        {
            { "type", configuration["CLOUDMESSAGE_TYPE"] },
            { "project_id", configuration["CLOUDMESSAGE_PROJECT_ID"] },
            { "private_key_id", configuration["CLOUDMESSAGE_PRIVATE_KEY_ID"] },
            { "private_key", configuration["CLOUDMESSAGE_PRIVATE_KEY"].Replace("\\n", "\n") },
            { "client_email", configuration["CLOUDMESSAGE_CLIENT_EMAIL"] },
            { "client_id", configuration["CLOUDMESSAGE_CLIENT_ID"] },
            { "auth_uri", configuration["CLOUDMESSAGE_AUTH_URI"] },
            { "token_uri", configuration["CLOUDMESSAGE_TOKEN_URI"] },
            { "auth_provider_x509_cert_url", configuration["CLOUDMESSAGE_AUTH_PROVIDER_X509_CERT_URL"] },
            { "client_x509_cert_url", configuration["CLOUDMESSAGE_CLIENT_X509_CERT_URL"] },
            { "universe_domain", configuration["CLOUDMESSAGE_UNIVERSE_DOMAIN"] }
        };

            var firebaseAdminSDKJson = JsonConvert.SerializeObject(firebaseAdminSDK);
            var googleCredential = GoogleCredential.FromJson(firebaseAdminSDKJson);

            FirebaseApp.Create(new AppOptions
            {
                Credential = googleCredential,
                ProjectId = configuration["CLOUDMESSAGE_PROJECT_ID"]
            });
        }
    }
}
