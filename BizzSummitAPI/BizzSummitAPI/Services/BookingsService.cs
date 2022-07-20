using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BizzSummitAPI.Services
{
    public class BookingsService: IBookingsService
    {
        private Container _container;

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public BookingsService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Booking>(new QueryDefinition(queryString));
            List<Booking> results = new List<Booking>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            return results;
        }

        public async Task AddBookingAsync(Booking booking)
        {
            await this._container.CreateItemAsync<Booking>(booking, new PartitionKey(booking.Id));
        }

        public async Task UpdateBookingAsync(Booking Booking)
        {
            if (await GetBookingFromId(Booking.Id))
            {
                await this._container.ReplaceItemAsync<Booking>(Booking, Booking.Id, new PartitionKey(Booking.Id));
            }
        }

        public async Task DeleteBookingAsync(string id)
        {
            if (await GetBookingFromId(id))
            {
                await this._container.DeleteItemAsync<Booking>(id, new PartitionKey($"{id}"));
            }
        }      
       
        private async Task<bool> GetBookingFromId(string id)
        {
            
            string query = $"select * from c where c.id=@BookingId";
            QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@BookingId", id);

            List<Booking> BookingResults = new List<Booking>();            
            
            FeedIterator streamResultSet = _container.GetItemQueryStreamIterator(
             queryDefinition,
             requestOptions: new QueryRequestOptions()
             {
                 PartitionKey = new PartitionKey(id),
                 MaxItemCount = 10,
                 MaxConcurrency = 1
             });

            while (streamResultSet.HasMoreResults)
            {
                using (ResponseMessage responseMessage = await streamResultSet.ReadNextAsync())
                {

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        dynamic streamResponse = FromStream<dynamic>(responseMessage.Content);
                        List<Booking> BookingResult = streamResponse.Documents.ToObject<List<Booking>>();
                        BookingResults.AddRange(BookingResult);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (BookingResults != null && BookingResults.Count > 0)
            {
                return true;
            }
            return false;
        }

        private static T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
                    {
                        return Serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }
    }
}
