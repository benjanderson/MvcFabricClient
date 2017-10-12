using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(MvcFabricClient.Startup))]
namespace MvcFabricClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                SlidingExpiration = true
            });


            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = "oidc",
                SignInAsAuthenticationType = "Cookies",
                Authority = ConfigurationManager.AppSettings["IdentityEndpoint"],
                ClientId = ConfigurationManager.AppSettings["FabricClientId"],
                //ClientSecret = ConfigurationManager.AppSettings["FabricClientSecret"],
                ResponseType = "id_token token",
                RedirectUri = ConfigurationManager.AppSettings["ApplicationEndpoint"],
                //Scope = "openid profile fabric.profile patientapi fabric/authorization.read fabric/authorization.write fabric/authorization.manageclients",
                //AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                UseTokenLifetime = false,
            });
        }
    }
}