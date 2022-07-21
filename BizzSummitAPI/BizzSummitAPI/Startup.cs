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
            services.AddSingleton<IBookingsService>(InitializeCosmosBookingsClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton<IProjectsService>(InitializeCosmosProjectsClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton<IResourcesService>(InitializeCosmosResourcesClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
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
        private static async Task<BookingsService> InitializeCosmosBookingsClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string bookingsContainer = configurationSection.GetSection("BookingsContainer").Value;            
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClient client = new CosmosClient(account, key);
            BookingsService bookingsService = new BookingsService(client, databaseName, bookingsContainer);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);            
            await database.Database.CreateContainerIfNotExistsAsync(bookingsContainer, "/id");            
            return bookingsService;
        }

        // <summary>
        /// Creates, if not existing, the CosmosDB Database and Projects Container
        /// </summary>
        /// <returns></returns>
        private static async Task<ProjectsService> InitializeCosmosProjectsClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;            
            string projectsContainer = configurationSection.GetSection("ProjectsContainer").Value;            
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClient client = new CosmosClient(account, key);            
            ProjectsService projectsService = new ProjectsService(client, databaseName, projectsContainer);            
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);            
            await database.Database.CreateContainerIfNotExistsAsync(projectsContainer, "/id");            
            return projectsService;
        }

        // <summary>
        /// Creates, if not existing, the CosmosDB Database and Resources Container
        /// </summary>
        /// <returns></returns>
        private static async Task<ResourcesService> InitializeCosmosResourcesClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string resourcesContainer = configurationSection.GetSection("ResourcesContainer").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClient client = new CosmosClient(account, key);
            ResourcesService resourcesService = new ResourcesService(client, databaseName, resourcesContainer);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(resourcesContainer, "/id");
            return resourcesService;
        }
    }
}
