using System.Security.Claims;
using IdentityModel;
using Kiss;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

namespace Kiss
{
    public static class Policies
    {
        public const string RedactiePolicy = "RedactiePolicy";
    }

    public static class UserExtensions
    {
        private const string ObjectIdentitifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public static string? GetId(this ClaimsPrincipal? user) => user?.FindFirstValue(ObjectIdentitifier) ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        public static string? GetEmail(this ClaimsPrincipal? user) => user?.FindFirstValue(JwtClaimTypes.Email) ?? user?.FindFirstValue(JwtClaimTypes.PreferredUserName);
        public static string? GetLastName(this ClaimsPrincipal? user) => user?.FindFirstValue(JwtClaimTypes.FamilyName) ?? user?.FindFirstValue(JwtClaimTypes.Name) ?? user?.Identity?.Name;
        public static string? GetFirstName(this ClaimsPrincipal? user) => user?.FindFirstValue(JwtClaimTypes.GivenName);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public delegate bool IsRedacteur(ClaimsPrincipal? user);

    public static class AuthenticationSetupExtensions
    {
        private const string SignOutCallback = "/signout-callback-oidc";
        private const string CookieSchemeName = "cookieScheme";
        private const string ChallengeSchemeName = "challengeScheme";

        public static IServiceCollection AddKissAuth(this IServiceCollection services, string authority, string clientId, string clientSecret, string klantcontactmedewerkerRole, string redacteurRole)
        {
            klantcontactmedewerkerRole ??= "Klantcontactmedewerker";
            redacteurRole ??= "Redacteur";

            services.AddSingleton<IsRedacteur>(user => user?.IsInRole(redacteurRole) ?? false);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieSchemeName;
                options.DefaultChallengeScheme = ChallengeSchemeName;
            }).AddCookie(CookieSchemeName, options =>
            {
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                // TODO: make configurable?
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                //options.Events.OnSigningOut = (e) => e.HttpContext.RevokeRefreshTokenAsync();
                options.Events.OnRedirectToAccessDenied = HandleLoggedOut;
                options.Events.OnRedirectToLogin = HandleLoggedOut;
            })
            .AddOpenIdConnect(ChallengeSchemeName, options =>
            {
                options.NonceCookie.HttpOnly = true;
                options.NonceCookie.IsEssential = true;
                options.NonceCookie.SameSite = SameSiteMode.None;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Authority = authority;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.SignedOutRedirectUri = SignOutCallback;
                options.ResponseType = OidcConstants.ResponseTypes.Code;
                options.UsePkce = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Clear();
                options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                options.Scope.Add(OidcConstants.StandardScopes.Profile);
                //options.Scope.Add(OidcConstants.StandardScopes.OfflineAccess);
                //options.SaveTokens = true;

                options.Events.OnRemoteFailure = RedirectToRoot;
                options.Events.OnSignedOutCallbackRedirect = RedirectToRoot;
                options.Events.OnRedirectToIdentityProvider = (ctx) =>
                {
                    if (ctx.Request.Headers.ContainsKey("is-api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.Headers.Location = ctx.ProtocolMessage.CreateAuthenticationRequestUrl();
                        ctx.HandleResponse();
                    }
                    return Task.CompletedTask;
                };
            });

            services.AddDistributedMemoryCache();
            services.AddOpenIdConnectAccessTokenManagement();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireRole(klantcontactmedewerkerRole)
                    .Build();

                options.AddPolicy(Policies.RedactiePolicy,
                    new AuthorizationPolicyBuilder()
                        .RequireRole(redacteurRole)
                        .Build());
            });

            return services;
        }

        public static IApplicationBuilder UseKissAuthMiddlewares(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.Use((context, next) =>
            {
                if (context.Request.Headers["x-forwarded-proto"] == "https")
                {
                    context.Request.Scheme = "https";
                }
                return next();
            });

            app.Use(StrictSameSiteExternalAuthenticationMiddleware);

            return app;
        }

        public static IEndpointRouteBuilder MapKissAuthEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/logoff", LogoffAsync).AllowAnonymous();
            endpoints.MapGet("api/me", GetMe).AllowAnonymous();
            endpoints.MapGet("api/challenge", ChallengeAsync).AllowAnonymous();

            return endpoints;
        }

        private static Task RedirectToRoot<TOptions>(HandleRequestContext<TOptions> context) where TOptions : AuthenticationSchemeOptions
        {
            context.Response.Redirect("/");
            context.HandleResponse();

            return Task.CompletedTask;
        }

        public static Task HandleLoggedOut<TOptions>(RedirectContext<TOptions> ctx) where TOptions : AuthenticationSchemeOptions
        {
            if (ctx.Request.Headers.ContainsKey("is-api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.Headers.Location = ctx.RedirectUri;
            }
            return Task.CompletedTask;
        }

        private static async Task LogoffAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieSchemeName);
            await httpContext.SignOutAsync(ChallengeSchemeName);
        }

        private static KissUser GetMe(HttpContext httpContext)
        {
            var isLoggedIn = httpContext.User.Identity?.IsAuthenticated ?? false;
            var email = httpContext.User.GetEmail();
            var isRedacteur = httpContext.RequestServices.GetService<IsRedacteur>()?.Invoke(httpContext.User) ?? false;
            
            var organisatieIds = httpContext.RequestServices
                .GetService<IConfiguration>()
                ?["ORGANISATIE_IDS"]
                ?.Split('/')
                ?? Array.Empty<string>();

            return new KissUser(email, isLoggedIn, isRedacteur, organisatieIds);
        }

        private readonly record struct KissUser(string? Email, bool IsLoggedIn, bool IsRedacteur, IReadOnlyList<string> OrganisatieIds);


        private static Task ChallengeAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var returnUrl = (request.Query["returnUrl"].FirstOrDefault() ?? string.Empty)
                .AsSpan()
                .TrimStart('/');

            var fullReturnUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/{returnUrl}";

            if (httpContext.User.Identity?.IsAuthenticated ?? false)
            {
                httpContext.Response.Redirect(fullReturnUrl);
                return Task.CompletedTask;
            }

            return httpContext.ChallengeAsync(new AuthenticationProperties
            {
                RedirectUri = fullReturnUrl,
            });
        }

        private static async Task StrictSameSiteExternalAuthenticationMiddleware(HttpContext ctx, RequestDelegate next)
        {
            var schemes = ctx.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var handlers = ctx.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
            {
                if (await handlers.GetHandlerAsync(ctx, scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                {
                    // start same-site cookie special handling
                    string? location = null;
                    if (ctx.Response.StatusCode == 302)
                    {
                        location = ctx.Response.Headers["location"];
                    }
                    else if (ctx.Request.Method == "GET" && !ctx.Request.Query["skip"].Any())
                    {
                        location = ctx.Request.Path + ctx.Request.QueryString + "&skip=1";
                    }

                    if (location != null)
                    {
                        ctx.Response.ContentType = "text/html";
                        ctx.Response.StatusCode = 200;
                        var html = $@"
                        <html><head>
                            <meta http-equiv='refresh' content='0;url={location}' />
                        </head></html>";
                        await ctx.Response.WriteAsync(html);
                    }
                    // end same-site cookie special handling

                    return;
                }
            }

            await next(ctx);
        }
    }
}
