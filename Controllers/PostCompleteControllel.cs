using DotnetAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using System.Buffers;

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

            parameters += postId != 0 ? ", @PostId=" + postId.ToString() : "";
            parameters += userId != 0 ? ", @UserId=" + userId.ToString() : "";
            parameters += searchValue.ToLower() != "none" ? ", @SearchValue=" + searchValue : "";

            if (parameters.Length > 0)
            {
                PostsSql += parameters[1..];
            }
            return _dapper.LoadData<Post>(PostsSql);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string PostsSql = @"EXEC TutorialAppSchema.spPosts_Get @UserId";

            var PostsSqlParams = new
            {
                Userid = this.User.FindFirst("userId")?.Value
            };

            return _dapper.LoadData<Post>(PostsSql, PostsSqlParams);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string upsertPostSql = @"EXEC TutorialAppSchema.spPosts_Upsert
            @UserId,
            @PostTitle,
            @PostContent
            ";

            upsertPostSql += postToUpsert.PostId > 0 ? $", @PostId={postToUpsert.PostId}" : "";
            var addPostSqlParams = new
            {
                UserId = this.User.FindFirst("userId")?.Value,
                postToUpsert.PostTitle,
                postToUpsert.PostContent,
            };

            if (_dapper.ExecuteSql(upsertPostSql, addPostSqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert a post");
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string editPostSql = @"EXEC TutorialAppSchema.spPost_Delete @PostId, @UserId";

            var DeletePostSqlParams = new
            {
                PostId = postId,
                UserId = this.User.FindFirst("userId")?.Value
            };

            if (_dapper.ExecuteSql(editPostSql, DeletePostSqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to delete the post");
        }
    }
}