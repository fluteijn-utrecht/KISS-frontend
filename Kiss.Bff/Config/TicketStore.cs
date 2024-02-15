using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Config
{
    public static class TicketExtensions
    {
        public static void AddTicketStore(this ModelBuilder builder) => builder.Entity<TicketEntity>().HasIndex(x => x.Expires);

        public static void AddTicketStore<TDbContext>(this IServiceCollection services, string cookieSchemeName, TimeSpan? cleanUpInterval = null) where TDbContext : DbContext
        {
            services.AddOptions<CookieAuthenticationOptions>(cookieSchemeName)
                .Configure<IHttpContextAccessor, IDataProtectionProvider>((options, accessor, provider) => options.SessionStore = new TicketStore(
                    () => accessor.HttpContext!.RequestServices.GetRequiredService<TDbContext>(),
                    provider
                ));

            services.AddHostedService(s => new TicketStoreHostedService<TDbContext>(
                s.GetRequiredService<IServiceScopeFactory>(), 
                cleanUpInterval
            ));
        }
    }

    public class TicketEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }

    public class TicketStore : ITicketStore
    {
        private readonly Func<DbContext> _getDbContext;
        private readonly IDataProtector _protector;

        public TicketStore(Func<DbContext> getDbContext, IDataProtectionProvider provider)
        {
            _getDbContext = getDbContext;
            _protector = provider.CreateProtector(nameof(TicketStore));
        }

        public async Task RemoveAsync(string key)
        {
            var id = Guid.Parse(key);
            var dbContext = _getDbContext();
            var entity = await dbContext.Set<TicketEntity>()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                dbContext.Remove(entity);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var id = Guid.Parse(key);
            var dbContext = _getDbContext();
            var entity = await dbContext.Set<TicketEntity>()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new() { Id = id };
                await dbContext.AddAsync(entity);
            }
            entity.Expires = ticket.Properties.ExpiresUtc;
            var bytes = TicketSerializer.Default.Serialize(ticket);
            var encrypted = _protector.Protect(bytes);
            entity.Bytes = encrypted;
            await dbContext.SaveChangesAsync();
        }

        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            try
            {
                var id = Guid.Parse(key);
                var dbContext = _getDbContext();
                var entity = await dbContext.Set<TicketEntity>()
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
                if (entity == null) return null;
                var decrypted = _protector.Unprotect(entity.Bytes);
                return TicketSerializer.Default.Deserialize(decrypted);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var bytes = TicketSerializer.Default.Serialize(ticket);
            var encrypted = _protector.Protect(bytes);
            var entity = new TicketEntity { Id = Guid.NewGuid(), Bytes = encrypted, Expires = ticket.Properties.ExpiresUtc };
            var dbContext = _getDbContext();
            await dbContext.AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return entity.Id.ToString();
        }
    }

    public class TicketStoreHostedService<TDbContext> : BackgroundService where TDbContext : DbContext
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval;

        public TicketStoreHostedService(IServiceScopeFactory scopeFactory, TimeSpan? interval)
        {
            _scopeFactory = scopeFactory;
            _interval = interval ?? TimeSpan.FromMinutes(10);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using (var scope = _scopeFactory.CreateAsyncScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                    await foreach (var item in dbContext.Set<TicketEntity>().Where(x => x.Expires < DateTimeOffset.UtcNow).AsAsyncEnumerable())
                    {
                        dbContext.Remove(item);
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
