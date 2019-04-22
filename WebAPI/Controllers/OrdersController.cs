using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        public OrdersController(AuthenticationContext context) => _context = context;

        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var data = _context.Orders.Select(x=> new
                {
                    x.Id,
                    x.BinId,
                    x.Completed,
                    x.DateCompleted,
                    x.DateRequested,
                    x.Bin.Location,
                    x.Bin.Name
                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}