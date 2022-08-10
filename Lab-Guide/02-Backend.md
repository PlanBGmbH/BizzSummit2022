# BizzSummit2022 - Backend
Fusion Teams Workshop for BizzSummit 2022

Los componentes que se van a usar en el Back-End son los siguientes: 

   1. [APIM](https://azure.microsoft.com/en-us/services/api-management/): Azure API Management: Enables API gateways deployments side-by-side with the APIs hosted in Azure, other clouds, and on-premises, optimizing API traffic flow. Meet security and compliance requirements while enjoying a unified management experience and full observability across all internal and external APIs.
   2. [App Service](https://azure.microsoft.com/en-us/services/app-service/): Enables to build, deploy, and scale web apps and APIs on your terms. In our case, we will build, host and deploy a .NET Core Web API.
   3. [Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/): Fully managed, serverless NoSQL database for high-performance applications of any size or scale, with fast writes and reads anywhere in the world with multi-region writes and data replication.

Empezamos con la creación de la solución que conformará el Backend. Para ello abrimos Visual Studio y creamos un nuevo proyecto, de tipo ASP.NET Core Web Application:

![image](https://user-images.githubusercontent.com/18615795/182643880-1dfaab8b-9952-4548-a0ca-505c90af3430.png)

A continuación le damos un nombre y una ruta donde guardar la solución:

![image](https://user-images.githubusercontent.com/18615795/182644475-a8434bce-a96d-4fec-b93e-c49c69f5e2d7.png)

Por último, seleccionamos el template del proyecto (API) y la versión de .NET Core a utilizar (3.1):

![image](https://user-images.githubusercontent.com/18615795/182644731-da7b5d79-02bb-4d92-8579-d19d7bff2484.png)

Una vez creada la solución, instalamos los Nuget Packages necesarios: 

![image](https://user-images.githubusercontent.com/18615795/182648595-3d8f15bc-b600-47fa-bfe9-5cf3dea2cf23.png)

- Microsoft.Azure.CosmosDB: Librería que nos permite conectarnos y trabajar con Azure Cosmos DB usando la API de SQL.
- Newtonsoft.Json: Nos permite serializar y deserializar objetos, usados para la comunicación con Cosmos DB.
- Swashbuckle.AspNetCore (versión 5.6.3, que es compatible con proyectos ASP.NET Core 3.1: Nos permite usar Swagger para documentar nuestras APIs.

Empezamos creando los tres modelos de datos que seran persistidos en la base de datos. Creamos una carpeta en la solucion de nombre Models (Modelos), donde creamos tres clases:

1) Reserva (Booking)
  ```cs
    public class Booking
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "projectId")]
        public string ProjectId { get; set; }

        [JsonProperty(PropertyName = "resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "hours")]
        public double Hours { get; set; }
    }
  ```
2) Proyecto (Project)
  ```cs
    public class Project
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public string Customer { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }        
    }
  ```
 3) Recurso (Resource)
  ```cs
    public class Resource
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }
    }
  ```

A continuación, creamos las tres interfícies de lo que seran nuestros servicios, que para un primer ejemplo pueden contener simplemente las operaciones CRUD (Create, Read, Update y Delete) de cada una de las entidades. Creamos una carpeta con nombre Interficies (Interfaces), y las añadimos. Por ejemplo, para Reservas (Bookings), creamos la IReservasSevicio.cs (IBookingsService.cs):

  ```cs
    public interface IBookingsService
    {
        Task<IEnumerable<Booking>> GetBookingsAsync(string query);

        Task AddBookingAsync(Booking booking);

        Task DeleteBookingAsync(string id);

        Task UpdateBookingAsync(Booking booking);
    }
  ```
El siguiente paso es crear los servícios que implementan las interfícies. Creamos la carpeta Servicios (Services), y creamos los tres servícios. Por ejemplo, para Reservas (Bookings), creamos la clase BookingsService.cs:

  ```cs
    public class BookingsService: IBookingsService  <-- Implementa la interfície
    {
        private Container _container; <-- Container de Cosmos DB

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public BookingsService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName); <- El constructor del servicio recibe el cliente de Cosmos DB y obtiene el container.
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string queryString) <- Obtiene un conjunto de reservas basadas en la querystring recibida.
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

        public async Task AddBookingAsync(Booking booking) <- Añade una nueva reserva
        {
            await this._container.CreateItemAsync<Booking>(booking, new PartitionKey(booking.Id));
        }
       
       //... Implementar el resto de métodos definidos en la Interfície
  ```


Configuraremos los servicios que dependen del proyecto.

![image](https://user-images.githubusercontent.com/18615795/182646899-76ca6af4-fd2e-470e-8116-6b970a5f6c04.png)

Para ello, editamos el método Configure Services de la clase Startup, añadiendo como servicios la inicialización única de la cosmos db y las colecciones:

```cs
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
 ```
 
 En cada uno de los métdos InitializeCosmosXXX, nos aseguramos que la Cosmos DB y el container existe, y si no lo creamos automáticamente:
 
 ```cs
        private static async Task<BookingsService> InitializeCosmosBookingsClientInstanceAsync(string CosmosDBEndpoint, string CosmosDBKey, string CosmosDBDatabaseName, string CosmosDBBookingsContainer)
        {                               
            CosmosClient client = new CosmosClient(CosmosDBEndpoint, CosmosDBKey);
            BookingsService bookingsService = new BookingsService(client, CosmosDBDatabaseName, CosmosDBBookingsContainer);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(CosmosDBDatabaseName);            
            await database.Database.CreateContainerIfNotExistsAsync(CosmosDBBookingsContainer, "/id");            
            return bookingsService;
        }
 ```
