using BlogLap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLap.Repository
{
    public interface IPhotoRespository 
    {
        public Task<Photo> InsertAsync(PhotoCreate photoCreate, int applicationUserId);

        public Task<Photo> GetAsync(int photoId);

        public Task<List<Photo>> GetAllByUserAsync(int applicationUserId);


        public Task<int> DeleteAsync(int photoId);

    }
}
