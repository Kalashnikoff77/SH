using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;

namespace WebAPI.Models
{
    public class UnitOfWork : IAsyncDisposable
    {
        public UnitOfWork(string connectionString, IMapper mapper, IMemoryCache cache)
        {
            SqlConnection = new SqlConnection(connectionString);
            Mapper = mapper;
            Cache = cache;
        }

        public DbConnection SqlConnection { get; set; } = null!;
        public DbTransaction? SqlTransaction { get; set; }

        public IMapper Mapper { get; set; }
        public IMemoryCache Cache { get; set; }

        public int? AccountId { get; set; }
        public Guid? AccountGuid { get; set; }

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

        public async ValueTask DisposeAsync()
        {
            if (SqlTransaction != null)
                await SqlTransaction.DisposeAsync();
            if (SqlConnection != null)
                await SqlConnection.DisposeAsync();
        }
    }
}
