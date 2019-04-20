using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BinsController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        public BinsController(AuthenticationContext context)=> _context = context;

        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var data = _context.Bins.ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Route("getfullbins")]
        public ActionResult GetFullBins()
        {
            try
            {
                var data = _context.Bins.Where(q=>q.Distance >= 75).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Route("gethalfbins")]
        public ActionResult GetHalfBins()
        {
            try
            {
                var data = _context.Bins.Where(q => q.Distance >= 50 && q.Distance < 75).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Route("getemptybins")]
        public ActionResult GetEmptyBins()
        {
            try
            {
                var data = _context.Bins.Where(q => q.Distance <= 49).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Post(Bin bin)
        {
            try
            {
                _context.Bins.Add(bin);
                _context.SaveChanges();
                return Ok(bin.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public ActionResult Put(Bin bin)
        {
            try
            {
                var theBin = _context.Bins.Find(bin.Id);
                if (theBin == null) return BadRequest("I can't update this Bin because I can't find it.");

                theBin.Lat = bin.Lat;
                theBin.Lng = bin.Lng;
                theBin.Name = bin.Name;
                theBin.Location = bin.Location;

                _context.Bins.Update(theBin);
                _context.SaveChanges();
                return Ok(theBin.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public ActionResult Delete(long id)
        {
            try
            {
                var bin = _context.Bins.Find(id);
                _context.Bins.Remove(bin);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}