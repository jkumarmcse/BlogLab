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
    public class PhotoRepository : IPhotoRespository
    {

        private readonly IConfiguration _config;

        public PhotoRepository(IConfiguration config)
        {
            _config = config;

        }
        public async Task<int> DeleteAsync(int photoId)
        {
            int affectedRows = 0;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                affectedRows = await connection.ExecuteAsync("Photo_Delete", new { PhotoId = photoId },
                commandType: CommandType.StoredProcedure
                );


            }
            return affectedRows;
        }

        public async Task<List<Photo>> GetAllByUserAsync(int applicationUserId)
        {

            IEnumerable<Photo> photos;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                photos = await connection.QueryAsync<Photo>("Photo_GetUserId", new { ApplicationUserId  = applicationUserId },
                commandType: CommandType.StoredProcedure
                );

            }

            return photos.ToList();
        }

        public async Task<Photo> GetAsync(int photoId)
        {
             Photo  photo;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                photo = await connection.QueryFirstOrDefaultAsync<Photo>("Photo_Get", new { PhotoId = photoId },
                commandType: CommandType.StoredProcedure
                );

            }

            return photo;
        }

        public async Task<Photo> InsertAsync(PhotoCreate photoCreate, int applicationUserId)
        {
            var dataTable = new DataTable();
     
            dataTable.Columns.Add("PublicId", typeof(string));
            dataTable.Columns.Add("ImageUrl", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));

            dataTable.Rows.Add(
    
                photoCreate.PublicId,
                photoCreate.ImageUrl,
                photoCreate.Description
                );

            int newPhotoID;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                newPhotoID = await connection.ExecuteScalarAsync<int>("Photo_Insert", 
                    new { Photo = dataTable.AsTableValuedParameter("dbo.PhotoType"),
                       ApplicationUserID = applicationUserId
                    },
                commandType: CommandType.StoredProcedure
                );

            }

            Photo photo = await GetAsync(newPhotoID);

            return photo;


        }
    }
}
