using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace WebAPI.Models
{
    public class UnitOfWork : IDisposable
    {
        public UnitOfWork(string connectionString) =>
            SqlConnection = new SqlConnection(connectionString);

        public UnitOfWork(string connectionString, int accountId) : this(connectionString) =>
            AccountId = accountId;

        public DbConnection SqlConnection { get; set; } = null!;

        public DbTransaction? SqlTransaction { get; set; }

        public int? AccountId { get; set; }

        public async Task BeginTransactionAsync()
        {
            await SqlConnection.OpenAsync();
            SqlTransaction = await SqlConnection.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (SqlTransaction != null)
                await SqlTransaction.CommitAsync();
        }

        public async void Dispose()
        {
            if (SqlTransaction != null)
                await SqlTransaction.DisposeAsync();
            if (SqlConnection != null)
                await SqlConnection.DisposeAsync();
        }
    }
}
