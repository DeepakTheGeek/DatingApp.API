using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDTO>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()))
                .ForMember(dest => dest.PhotoURL, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).URL));
                
            CreateMap<User, UserForDetailedDTO>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()))
                .ForMember(dest => dest.PhotoURL, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).URL));  

            CreateMap<Photo, PhotosForDetailedDTO>();

            CreateMap<UserForUpdateDTO, User>();

            CreateMap<PhotoForCreationDTO, Photo>();
            CreateMap<Photo, PhotoForReturnDTO>();

            CreateMap<UserForRegisterDTO, User>();

            CreateMap<MessageForCreationDTO, Message>().ReverseMap();
        }
    }
}
