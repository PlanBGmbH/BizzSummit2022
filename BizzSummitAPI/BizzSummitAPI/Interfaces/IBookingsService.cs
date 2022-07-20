using System.Collections.Generic;
using System.Threading.Tasks;

namespace BizzSummitAPI.Services
{
    public interface IBookingsService
    {
        Task<IEnumerable<Booking>> GetBookingsAsync(string query);

        Task AddBookingAsync(Booking booking);

        Task DeleteBookingAsync(string id);

        Task UpdateBookingAsync(Booking booking);
    }
}
