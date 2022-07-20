using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BizzSummitAPI.Services
{
    public class ProjectsService : IProjectsService
    {
        private Container _container;

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public ProjectsService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Project>(new QueryDefinition(queryString));
            List<Project> results = new List<Project>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            return results;
        }

        public async Task AddProjectAsync(Project project)
        {
            await this._container.CreateItemAsync<Project>(project, new PartitionKey(project.Id));
        }

        public async Task UpdateProjectAsync(Project project)
        {
            if (await GetProjectDataFromId(project.Id))
            {
                await this._container.ReplaceItemAsync<Project>(project, project.Id, new PartitionKey(project.Id));
            }
        }

        public async Task DeleteProjectAsync(string id)
        {
            if (await GetProjectDataFromId(id))
            {
                await this._container.DeleteItemAsync<Project>(id, new PartitionKey($"{id}"));
            }
        }       
      
        private async Task<bool> GetProjectDataFromId(string id)
        {            
            string query = $"select * from c where c.id=@ProjectId";
            QueryDefinition queryDefinition = new QueryDefinition(query).WithParameter("@ProjectId", id);

            List<Project> ProjectResults = new List<Project>();            
            
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
                        List<Project> ProjectResult = streamResponse.Documents.ToObject<List<Project>>();
                        ProjectResults.AddRange(ProjectResult);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (ProjectResults != null && ProjectResults.Count > 0)
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
