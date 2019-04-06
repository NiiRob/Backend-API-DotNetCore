using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private AuthenticationContext _context;
        public UserProfileController(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        //GET : /api/UserProfile
        public async Task<Object> UserProfile() {
            string userId = User.Claims.First(c => c.Type == "UserId").Value;
            var user = await _userManager.FindByIdAsync(userId);

            return new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.UserName,
                user.PhoneNumber
            };
        }

        [HttpGet]
        public IEnumerable<object> RegisteredUsers()
        {
            return _context.Users.Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.UserName,
                x.Email,
                x.PhoneNumber,
                Type = x.Type.ToString()
            }).ToList();
        }

        
    }
}