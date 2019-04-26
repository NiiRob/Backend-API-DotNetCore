using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool OnDuty { get; set; }
        public UserType Type { get; set; }
    }

    public enum UserType
    {
        Admin,
        Borlaman
    }

    public class Bin
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }
        public int Distance { get; set; }
        public string Location { get; set; }
        public List<Order> Orders { get; set; }
        public string ChannelId { get; set; }
        public string ApiKey { get; set; }
    }

    public class Order
    {
        public long Id { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime? DateCompleted { get; set; }
        public virtual Bin Bin { get; set; }
        public long BinId { get; set; }
        public bool Completed { get; set; }
    }
}
