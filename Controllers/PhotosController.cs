﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(_cloudinaryConfig.Value.CloudName, _cloudinaryConfig.Value.ApiKey, _cloudinaryConfig.Value.ApiSecret);
            cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photoForReturn = _mapper.Map<PhotoForReturnDTO>(photoFromRepo);
            return Ok(photoForReturn);
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoForUser(int UserId, [FromForm] PhotoForCreationDTO photoForCreationDTO)
        {
            if (UserId != Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(UserId);

            var file = photoForCreationDTO.File;
            var imageUploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var imageUploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    imageUploadResult = cloudinary.Upload(imageUploadParams);
                }
            }

            var photo = _mapper.Map<Photo>(photoForCreationDTO);
            photo.URL = imageUploadResult.Url.ToString();
            photo.PublicId = imageUploadResult.PublicId;

            if (!userFromRepo.Photos.Any(p => p.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                var photoForReturn = _mapper.Map<PhotoForReturnDTO>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = UserId, id = photo.Id }, photoForReturn);
            }

            return BadRequest("Photo could not be saved.");
        }

        [HttpPost("{id}/SetMain")]
        public async Task<IActionResult> SetMainPhoto(int userid, int id)
        {
            if (userid != Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userid);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo.");

            var mainPhoto = userFromRepo.Photos.FirstOrDefault(p => p.IsMain);
            mainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("This photo could not be set as main.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("You can not delete your main photo.");

            if (photoFromRepo.PublicId == null)
            {
                userFromRepo.Photos.Remove(photoFromRepo);
            }
            else
            {
                var deleteResponse = cloudinary.Destroy(new DeletionParams(photoFromRepo.PublicId));
             
                if (deleteResponse.Result == "ok")
                    userFromRepo.Photos.Remove(photoFromRepo);
                else
                    return BadRequest("Something went wrong while deleting photo from cloudinary.");
            }

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Photo could not be deleted.");
        }
    }
}
