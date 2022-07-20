using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BizzSummitAPI.Services
{
    public class ResourcesService : IResourcesService
    {
        private Container _container;

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public ResourcesService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<Resource>> GetResourcesAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Resource>(new QueryDefinition(queryString));
            List<Resource> results = new List<Resource>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            return results;
        }

        public async Task AddResourceAsync(Resource resource)
        {
            await this._container.CreateItemAsync<Resource>(resource, new PartitionKey(resource.Id));
        }

        public async Task UpdateResourceAsync(Resource resource)
        {
            if (await GetResourceDataFromId(resource.Id))
            {
                await this._container.ReplaceItemAsync<Resource>(resource, resource.Id, new PartitionKey(resource.Id));
            }
        }

        public async Task DeleteResourceAsync(string id)
        {
            if (await GetResourceDataFromId(id))
            {
                await this._container.DeleteItemAsync<Resource>(id, new PartitionKey($"{id}"));
            }
        }       
      
        private async Task<bool> GetResourceDataFromId(string id)
        {            
            string query = $"select * from c where c.id=@ResourceId";
            QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@ResourceId", id);

            List<Resource> ResourceResults = new List<Resource>();            
            
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
                        List<Resource> ResourceResult = streamResponse.Documents.ToObject<List<Resource>>();
                        ResourceResults.AddRange(ResourceResult);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (ResourceResults != null && ResourceResults.Count > 0)
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
