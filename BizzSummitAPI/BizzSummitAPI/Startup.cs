using BizzSummitAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;

namespace BizzSummitAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen();
            var CosmosDBEndpoint = Configuration["CosmosDB:Endpoint"];
            var CosmosDBKey = Configuration["CosmosDB:Key"];
            var CosmosDBDatabaseName = Configuration["CosmosDB:DatabaseName"];
            var CosmosDBBookingsContainer = Configuration["CosmosDB:BookingsContainer"];
            var CosmosDBProjectsContainer = Configuration["CosmosDB:ProjectsContainer"];
            var CosmosDBResourcesContainer = Configuration["CosmosDB:ResourcesContainer"];
            
            services.AddSingleton<IBookingsService>(InitializeCosmosBookingsClientInstanceAsync(CosmosDBEndpoint, CosmosDBKey, CosmosDBDatabaseName, CosmosDBBookingsContainer).GetAwaiter().GetResult());
            services.AddSingleton<IProjectsService>(InitializeCosmosProjectsClientInstanceAsync(CosmosDBEndpoint, CosmosDBKey, CosmosDBDatabaseName, CosmosDBProjectsContainer).GetAwaiter().GetResult());
            services.AddSingleton<IResourcesService>(InitializeCosmosResourcesClientInstanceAsync(CosmosDBEndpoint, CosmosDBKey, CosmosDBDatabaseName, CosmosDBResourcesContainer).GetAwaiter().GetResult());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BizzSummitAPI V1");
                c.RoutePrefix = string.Empty;
            });
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Creates, if not existing, the CosmosDB Database and Bookings Container
        /// </summary>
        /// <returns></returns>
        private static async Task<BookingsService> InitializeCosmosBookingsClientInstanceAsync(string CosmosDBEndpoint, string CosmosDBKey, string CosmosDBDatabaseName, string CosmosDBBookingsContainer)
        {                               
            CosmosClient client = new CosmosClient(CosmosDBEndpoint, CosmosDBKey);
            BookingsService bookingsService = new BookingsService(client, CosmosDBDatabaseName, CosmosDBBookingsContainer);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(CosmosDBDatabaseName);            
            await database.Database.CreateContainerIfNotExistsAsync(CosmosDBBookingsContainer, "/id");            
            return bookingsService;
        }

        // <summary>
        /// Creates, if not existing, the CosmosDB Database and Projects Container
        /// </summary>
        /// <returns></returns>
        private static async Task<ProjectsService> InitializeCosmosProjectsClientInstanceAsync(string CosmosDBEndpoint, string CosmosDBKey, string CosmosDBDatabaseName, string CosmosDBProjectsContainer)
        {           
            CosmosClient client = new CosmosClient(CosmosDBEndpoint, CosmosDBKey);            
            ProjectsService projectsService = new ProjectsService(client, CosmosDBDatabaseName, CosmosDBProjectsContainer);            
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(CosmosDBDatabaseName);            
            await database.Database.CreateContainerIfNotExistsAsync(CosmosDBProjectsContainer, "/id");            
            return projectsService;
        }

        // <summary>
        /// Creates, if not existing, the CosmosDB Database and Resources Container
        /// </summary>
        /// <returns></returns>
        private static async Task<ResourcesService> InitializeCosmosResourcesClientInstanceAsync(string CosmosDBEndpoint, string CosmosDBKey, string CosmosDBDatabaseName, string CosmosDBResourcesContainer)
        {           
            CosmosClient client = new CosmosClient(CosmosDBEndpoint, CosmosDBKey);
            ResourcesService resourcesService = new ResourcesService(client, CosmosDBDatabaseName, CosmosDBResourcesContainer);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(CosmosDBDatabaseName);
            await database.Database.CreateContainerIfNotExistsAsync(CosmosDBResourcesContainer, "/id");
            return resourcesService;
        }
    }
}
