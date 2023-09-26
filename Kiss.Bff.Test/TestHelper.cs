global using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Kiss.Bff.Test
{
    public class TestHelper
    {
        // This method initializes the in-memory database with a unique database name using Guid.NewGuid().ToString().
        // The Initialize method now calls InitializeInMemoryDatabase to set up the database before each test.
        // this ensures that each test has its own isolated database, preventing conflicts between tests.
        protected DbContextOptions<BeheerDbContext> _dbContextOptions;

        public void InitializeDatabase()
        {
            _dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        public static bool IsUserAuthorized(string policyName, ClaimsPrincipal user)
        {
            var serviceProvider = new ServiceCollection()
                .AddAuthorization(options =>
                {
                    options.AddPolicy(policyName, policy => policy.RequireAssertion(context =>
                    {
                        return context.User.HasClaim(c => c.Type == ClaimTypes.Name) && context.User.HasClaim(c => c.Type == ClaimTypes.Email);
                    }));
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
            var authorizationResult = authorizationService.AuthorizeAsync(httpContext.User, null, policyName).GetAwaiter().GetResult();

            return authorizationResult.Succeeded;
        }
    }
}



