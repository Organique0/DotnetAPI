using DotnetAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using System.Buffers;
using Dapper;
using System.Data;

namespace DotnetAPI.Controllers
{
    //require authorization to access this controller
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostCompleteController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostCompleteController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("Posts/{userId}/{searchValue}/{postId}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchValue = "None")
        {
            string PostsSql = "EXEC TutorialAppSchema.spPosts_Get";
            string parameters = "";

            DynamicParameters sqlParams = new();

            if (postId != 0)
            {
                parameters += ", @PostId = @PostIdParameter";
                sqlParams.Add("@PostIdParameter", postId, DbType.Int32);
            }

            if (userId != 0)
            {
                parameters += ", @UserId = @UserIdParameter";
                sqlParams.Add("@UserIdParameter", userId, DbType.Int32);
            }

            if (!searchValue.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            {
                parameters += ", @SearchValue = @SearchValueParameters";
                sqlParams.Add("@SearchValueParameters", searchValue, DbType.String);
            }
            if (parameters.Length > 0)
            {
                PostsSql += parameters[1..];
            }
            return _dapper.LoadDataDynamic<Post>(PostsSql, sqlParams);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string PostsSql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParameter";

            DynamicParameters sqlParams = new();
            sqlParams.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            return _dapper.LoadDataDynamic<Post>(PostsSql, sqlParams);

        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string upsertPostSql = @"EXEC TutorialAppSchema.spPosts_Upsert
            @UserId = @UserIdParameter,
            @PostTitle = @PostTitleParameter,
            @PostContent = @PostsContentParameter
            ";

            DynamicParameters sqlParams = new();

            if (postToUpsert.PostId > 0)
            {
                upsertPostSql += ", @PostId = @PostIdParameter";
                sqlParams.Add("@PostIdParameter", postToUpsert.PostId);
            }

            sqlParams.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@PostTitleParameter", postToUpsert.PostTitle, DbType.String);
            sqlParams.Add("@PostContentParameter", postToUpsert.PostContent, DbType.String);


            if (_dapper.ExecuteSqlWithDynamicParameters(upsertPostSql, sqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert a post");
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string deletePostSql = @"EXEC TutorialAppSchema.spPost_Delete 
            @PostId = @PostIdParameter, 
            @UserId = @UserIdParameter";

            DynamicParameters sqlParams = new();

            sqlParams.Add("@PostIdParameter", postId, DbType.Int32);
            sqlParams.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            if (_dapper.ExecuteSqlWithDynamicParameters(deletePostSql, sqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to delete the post");
        }
    }
}