using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Infrastructure.Persistence;

namespace TestSupport
{
    public class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<WerehouseDbContext> _options;
        private readonly List<WerehouseDbContext> _contexts = new();
        private bool _disposed;

        public WerehouseDbContext DbContext { get; }

        public SqliteTestDatabase()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _options = new DbContextOptionsBuilder<WerehouseDbContext>()
                .UseSqlite(_connection)
                .Options;

            try
            {
                _connection.Open();
                DbContext = CreateNewContext();
                DbContext.Database.EnsureCreated();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        // The new context uses the same database but has its own ChangeTracker.
        public WerehouseDbContext CreateNewContext()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var context = new WerehouseDbContext(_options);
            _contexts.Add(context);
            return context;
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var context in _contexts)
            {
                context.Dispose();
            }
            _contexts.Clear();
            _connection.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
        public void ConfigureOptions(DbContextOptionsBuilder options)
        {
            options.UseSqlite(_connection);
        }
    }
}
