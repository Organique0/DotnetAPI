using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _config;
        public DataContextDapper(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IEnumerable<T> LoadData<T>(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql, param);
        }

        public T LoadDataSingle<T>(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql, param);
        }

        public bool ExecuteSql(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, param) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, param);
        }
    }
}