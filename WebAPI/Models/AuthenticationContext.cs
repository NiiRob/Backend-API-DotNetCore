using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class AuthenticationContext: IdentityDbContext<ApplicationUser>
    {
        public DbSet<Bin> Bins { get; set; }
        public DbSet<Order> Orders { get; set; }

        public AuthenticationContext(DbContextOptions options): base(options)
        {
            
        }

        //public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}
