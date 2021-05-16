using BlogLap.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLap.Repository
{
    public class BlogCommentRepository : IBlogCommentRepository
    {
        private readonly IConfiguration _config;

        public BlogCommentRepository(IConfiguration config)
        {
            _config = config;

        }
        public async Task<int> DeleteAsync(int blogCommentId)
        {
            int affectedRows = 0;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                affectedRows = await connection.ExecuteAsync("BlogComment_Delete", new { BlogCommentID = blogCommentId },
                commandType: CommandType.StoredProcedure
                );


            }
            return affectedRows;
        }

        public async Task<List<BlogComment>> GetAllAsync(int blogId)
        {
            IEnumerable<BlogComment> blogComment;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                blogComment = await connection.QueryAsync<BlogComment>("BlogComment_GetAll", new { BlogId = blogId },
                commandType: CommandType.StoredProcedure
                );

            }

            return blogComment.ToList();
        }

        public async Task<BlogComment> GetAsync(int blogCommentId)
        {
            BlogComment blogComment;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                blogComment = await connection.QueryFirstOrDefaultAsync<BlogComment>("BlogComment_Get", new { BlogCommentID = blogCommentId },
                commandType: CommandType.StoredProcedure
                );

            }

            return blogComment;
        }

        public async Task<BlogComment> UpsertAsync(BlogCommentCreate blogCommentCreate, int applicationUserId)
        {
           
            var dataTable = new DataTable();
            dataTable.Columns.Add("BlogCommentID", typeof(int));
            dataTable.Columns.Add("ParentBlogCommnetId", typeof(int));
            dataTable.Columns.Add("BlogId", typeof(int));
            dataTable.Columns.Add("Content", typeof(string));


            dataTable.Rows.Add(
                blogCommentCreate.BlogCommentId,
                blogCommentCreate.ParentBlogCommentId,
                blogCommentCreate.BlogId,
                blogCommentCreate.Content
                );

            int? newblogCommentID;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                newblogCommentID = await connection.ExecuteScalarAsync<int>("BlogComment_Upsert",
                    new { BlogComment = dataTable.AsTableValuedParameter("dbo.BlogCommentType"),
                        ApplicationUserId = applicationUserId
                    },
                commandType: CommandType.StoredProcedure
                );

            }

            newblogCommentID = newblogCommentID ?? blogCommentCreate.BlogCommentId;

            BlogComment blogComment = await GetAsync(newblogCommentID.Value);

            return blogComment;
        }
    }
}
