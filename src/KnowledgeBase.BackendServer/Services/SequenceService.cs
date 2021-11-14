using System.Data;
using System.Threading.Tasks;
using Dapper;
using KnowledgeBase.BackendServer.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KnowledgeBase.BackendServer.Services
{
    public class SequenceService : ISequenceService
    {
        private readonly IConfiguration _configuration;

        public SequenceService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<int> GetKnowledgeNewId()
        {
            await using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            if (conn.State == ConnectionState.Closed)
            {
                await conn.OpenAsync();
            }

            var result = await conn.ExecuteScalarAsync<int>(@"SELECT (NEXT VALUE FOR KnowledgeSequence)", null, null, 120, CommandType.Text);
            return result;
        }
    }
}