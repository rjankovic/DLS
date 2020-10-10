using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using System.IdentityModel.Tokens;
using System.Configuration;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Jwt;
using CD.DLS.ExcelAddinO365Web.App_Start;

namespace CD.DLS.ExcelAddinO365Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // TODO3: Configure the validation settings
            var tvps = new TokenValidationParameters
            {
                ValidAudience = ConfigurationManager.AppSettings["ida:Audience"],
                ValidIssuer = ConfigurationManager.AppSettings["ida:Issuer"],
                SaveSigninToken = true
            };
            // TODO4: Specify the type of authorization and the discovery endpoint
            // of the secure token service.
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(tvps, new OpenIdConnectCachingSecurityTokenProvider("https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration"))
            });
        }
    }
}