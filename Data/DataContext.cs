using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace nett.Data
{
    public class DataContext(IConfiguration config) : DbContext
    {
        readonly IConfiguration _config = config;

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }

        public bool ExecuteSqlWithParamenter(string sql, List<SqlParameter> parameters)
        {
            SqlCommand commandWithParams = new(sql);

            foreach (SqlParameter parameter in parameters)
            {
                commandWithParams.Parameters.Add(parameter);
            }
            SqlConnection dbConnection = new(_config.GetConnectionString("DefaultConnection"));

            dbConnection.Open();
            commandWithParams.Connection = dbConnection;

            int rowsEffected = commandWithParams.ExecuteNonQuery();

            dbConnection.Close();

            return rowsEffected > 0;
        }
    }
}