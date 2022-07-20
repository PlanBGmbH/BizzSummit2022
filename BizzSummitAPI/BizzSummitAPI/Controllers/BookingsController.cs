using BizzSummitAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BizzSummitAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ILogger<BookingsController> _logger;

        private readonly IBookingsService _bookingsService;

        public BookingsController(IBookingsService bookingsService, ILogger<BookingsController> logger)
        {
            _bookingsService = bookingsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            try
            {
                var family = await _bookingsService.GetBookingsAsync("SELECT * FROM c");
                return Ok(family);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while retreiving data");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddBooking(Booking booking)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _bookingsService.AddBookingAsync(booking);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while inserting data");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteBooking(string id)
        {
            try
            {
                await _bookingsService.DeleteBookingAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while deleting data");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateBooking(Booking booking)
        {
            try
            {
                await _bookingsService.UpdateBookingAsync(booking);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while updating data");
            }
        }
    }
}
