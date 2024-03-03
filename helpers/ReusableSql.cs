using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;

        public ReusableSql(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        public bool UpsertUser(UserComplete user)
        {
            string sql = @"EXEC TutorialAppSchema.spUser_Upsert
            @FirstName = @FirstNameParameter,
            @LastName = @LastNameParameter,
            @Email = @EmailParameter,
            @Gender = @GenderParameter,
            @Active = @ActiveParameter,
            @JobTitle = @JobTitleParameter,
            @Department = @DepartmentParameter,
            @Salary = @SalaryParameter,
            @UserId = @UserIdParameter
            ";

            DynamicParameters sqlParams = new();

            sqlParams.Add("@FirstNameParameter", user.FirstName, DbType.String);
            sqlParams.Add("@LastNameParameter", user.LastName, DbType.String);
            sqlParams.Add("@EmailParameter", user.Email, DbType.String);
            sqlParams.Add("@GenderParameter", user.Gender, DbType.String);
            sqlParams.Add("@ActiveParameter", user.Active, DbType.String);
            sqlParams.Add("@JobTitleParameter", user.JobTitle, DbType.String);
            sqlParams.Add("@DepartmentParameter", user.Department, DbType.String);
            sqlParams.Add("@SalaryParameter", user.Salary, DbType.String);
            sqlParams.Add("@UserIdParameter", user.UserId, DbType.Int32);

            return _dapper.ExecuteSqlWithDynamicParameters(sql, sqlParams);
        }
    }
}