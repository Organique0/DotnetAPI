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
        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="sql">Raw sql query</param>
        /// <param name="param">Optinal object of parameters to be inserted in the query</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is 
        /// queried then the data from the first column is assumed, otherwise an instance is created per row, 
        /// and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public IEnumerable<T> LoadData<T>(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql, param);
        }
        /// <summary>
        /// Executes a single-row query, returning the data typed as T. 
        /// </summary>
        /// <param name="sql">Raw sql query</param>
        /// <param name="param">Optinal object of parameters to be inserted in the query</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) 
        /// is queried then the data from the first column is assumed, otherwise an instance
        /// is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public T LoadDataSingle<T>(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql, param);
        }
        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">Raw sql query</param>
        /// <param name="param">Optinal object of parameters to be inserted in the query</param>
        /// <returns>
        ///  Returns True if the number of rows affected is > 0 
        /// </returns>
        public bool ExecuteSql(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, param) > 0;
        }
        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">Raw sql query</param>
        /// <param name="param">Optinal object of parameters to be inserted in the query</param>
        /// <returns>
        ///  Returns the number of rows affected
        /// </returns>
        public int ExecuteSqlWithRowCount(string sql, object? param = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, param);
        }
        /// <summary>
        /// Execute SQL with all parameters in the sqlParams List included
        /// </summary>
        /// <param name="sql">Raw sql query</param>
        /// <param name="sqlParams">A List containing objects with parameters</param>
        /// <returns>
        ///  Returns True of the number of rows affected is greater that 0
        /// </returns>
        public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> sqlParams)
        {
            SqlCommand commandWithParams = new(sql);

            foreach (SqlParameter parameter in sqlParams)
            {
                commandWithParams.Parameters.Add(parameter);
            }
            using SqlConnection dbConnection = new(_config.GetConnectionString("DefaultConnection"));
            dbConnection.Open();

            commandWithParams.Connection = dbConnection;

            int rowsAffected = commandWithParams.ExecuteNonQuery();

            dbConnection.Close();

            return rowsAffected > 0;
        }

    }
}