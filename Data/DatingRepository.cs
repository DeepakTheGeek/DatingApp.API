﻿using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(u => u.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);
            if (userParams.Likees)
            {
                var userLikeeIds = await GetUserLikes(userParams.UserId, true);
                users = users.Where(u => userLikeeIds.Contains(u.Id));
            }
            if (userParams.Likers)
            {
                var userLikerIds = await GetUserLikes(userParams.UserId, false);
                users = users.Where(u => userLikerIds.Contains(u.Id));
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 90)
            {
                DateTime minDOB = DateTime.Now.AddYears(-userParams.MaxAge - 1);
                DateTime maxDOB = DateTime.Now.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDOB && u.DateOfBirth <= maxDOB);
            }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likees)
        {
            var currentUser = await _context.Users.Include(u => u.Likees).Include(u => u.Likers).FirstOrDefaultAsync(u => u.Id == id);

            if (likees)
            {
                return currentUser.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
            }
            else
            {
                return currentUser.Likers.Where(u => u.LikeeId == id).Select(u => u.LikerId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
