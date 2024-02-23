using DotnetAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Models;
using DotnetAPI.Dtos;

namespace DotnetAPI.Controllers
{
    //require authorization to access this controller
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string PostsSql = @"
                SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
                FROM TutorialAppSchema.Posts;
            ";

            return _dapper.LoadData<Post>(PostsSql);
        }

        [HttpGet("PostSingle/{postId}")]
        public Post GetPostSingle(int postId)
        {
            string PostsSql = @"
                SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
                FROM TutorialAppSchema.Posts
                WHERE PostId = @postId
            ";

            var PostsSqlParams = new
            {
                postId
            };

            return _dapper.LoadDataSingle<Post>(PostsSql, PostsSqlParams);
        }

        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string PostsSql = @"
                SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
                FROM TutorialAppSchema.Posts
                WHERE UserId = @userId
            ";

            var PostsSqlParams = new
            {
                userId
            };

            return _dapper.LoadData<Post>(PostsSql, PostsSqlParams);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string PostsSql = @"
                SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
                FROM TutorialAppSchema.Posts
                WHERE UserId = @userId
            ";

            var PostsSqlParams = new
            {
                userid = this.User.FindFirst("userId")?.Value
            };

            return _dapper.LoadData<Post>(PostsSql, PostsSqlParams);
        }

        [HttpGet("PostBySearch/{searchParam}")]
        public IEnumerable<Post> PostBySearch(string searchParam)
        {
            string PostsSearchSql = @"
                SELECT *
                FROM TutorialAppSchema.Posts
                WHERE PostTitle LIKE @SearchParam
                OR PostContent LIKE @SearchParam
            ";

            //INFO:make sure that you put wildcards in the parameters and not SQL
            var PostsSqlParams = new
            {
                SearchParam = "%" + searchParam + "%"
            };

            return _dapper.LoadData<Post>(PostsSearchSql, PostsSqlParams);
        }

        [HttpPost("AddPost")]
        public IActionResult AddPost(PostToAddDTO postToAdd)
        {
            string addPostSql = @"
                INSERT into TutorialAppSchema.Posts
                    (
                        [UserId],
                        [PostTitle],
                        [PostContent],
                        [PostCreated],
                        [PostUpdated]
                    )
                VALUES
                    (
                        @UserId,
                        @PostTitle,
                        @PostContent,
                        GETDATE(),
                        GETDATE()
                )
            ";

            var addPostSqlParams = new
            {
                UserId = this.User.FindFirst("userId")?.Value,
                postToAdd.PostTitle,
                postToAdd.PostContent,
            };

            if (_dapper.ExecuteSql(addPostSql, addPostSqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to create a new post");
        }

        [HttpPut("UpdatePost")]
        public IActionResult EditPost(PostToEditDTO postToEdit)
        {
            string editPostSql = @"
                UPDATE TutorialAppSchema.Posts
                SET 
                    PostTitle = @PostTitle, 
                    PostContent = @PostContent,
                    PostUpdated = GETDATE()
                WHERE PostId = @PostId
                AND UserId = @userId
            ";

            var EditPostSqlParams = new
            {
                postToEdit.PostTitle,
                postToEdit.PostContent,
                postToEdit.PostId,
                userId = this.User.FindFirst("userId")?.Value,
            };

            if (_dapper.ExecuteSql(editPostSql, EditPostSqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to update the post");
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string editPostSql = @"
                DELETE FROM TutorialAppSchema.Posts
                WHERE PostId = @postId
                AND UserId = @userId
            ";

            var DeletePostSqlParams = new
            {
                postId,
                userId = this.User.FindFirst("userId")?.Value
            };

            if (_dapper.ExecuteSql(editPostSql, DeletePostSqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to delete the post");
        }
    }
}