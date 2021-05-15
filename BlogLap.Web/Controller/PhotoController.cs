using BlogLap.Models;
using BlogLap.Repository;
using BlogLap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLap.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoRespository _photoRespository;
        private readonly IBlogRepository  _blogRepository;
        private readonly IPhotoService  _photoService;

        public PhotoController(IPhotoRespository photoRespository,
            IBlogRepository blogRepository,
            IPhotoService photoService

            )
        {
            _photoRespository = photoRespository;
            _blogRepository = blogRepository;
            _photoService = photoService;

        }

        //http://localhost:5000/api/photo
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> UploadPhoto(IFormFile file)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

            var photoCreate = new PhotoCreate
            {
                PublicId = uploadResult.PublicId,
                ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                Description = file.FileName
            };

            var photo = await _photoRespository.InsertAsync(photoCreate, applicationUserId);

            return Ok(photo);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Photo>>> GetApplicationUserId()
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var photos = await _photoRespository.GetAllByUserAsync(applicationUserId);

            return Ok(photos);
        }

  
        [HttpGet("{photoId}")]
        public async Task<ActionResult<Photo>> Get(int photoId)
        {
            //int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var photo = await _photoRespository.GetAsync(photoId);

            return Ok(photo);
        }


        [Authorize]
        [HttpDelete("{photoId}")]
        public async Task<ActionResult<int>> Delete(int photoId)
        {
             int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var foundPhoto = await _photoRespository.GetAsync(photoId);

            if (foundPhoto != null)
            {
                if (foundPhoto.ApplicationUserId == applicationUserId)
                {
                    var blogs = await _blogRepository.GetAllByUserAsync(applicationUserId);
                    var usedInBlog = blogs.Any(b => b.PhotoId == photoId);
                    if (usedInBlog) return BadRequest("Cannot remove photo as it is being used in published blog(s).");
                    var deleteResult = await _photoService.DeletePhotoAsync(foundPhoto.PublicId);

                    if (deleteResult.Error != null) return BadRequest(deleteResult.Error.Message);

                    var affectedRows = await _photoRespository.DeleteAsync(foundPhoto.PhotoId);
                    return Ok(affectedRows);
                }
                else
                {
                    return BadRequest("Photo was not uploaded by current user");
                }
            }

            return BadRequest(" Photo Does not exists");
        }

    }
}
