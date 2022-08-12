# BizzSummit2022 - Backend
Fusion Teams Workshop para BizzSummit 2022

Los componentes que se van a usar en el Back-End son los siguientes: 

   1. [APIM](https://azure.microsoft.com/es-ES/services/api-management/): Azure API Management: Implementa puertas de enlace de API en paralelo con las API hospedadas en Azure, en otras nubes y en el entorno local para optimizar el flujo de tráfico de las API, satisfaciendo los requisitos de seguridad y cumplimiento normativo, proveyendo una experiencia de administración unificada y una observación completa de todas las API internas y externas.
   2. [App Service](https://azure.microsoft.com/es-ES/services/app-service/): Permite compilar, implementar y escalar aplicaciones web y API de forma automática. En nuestro caso implementaremos, i alojaremos una aplicación .NET Core Web API.
   3. [Cosmos DB](https://azure.microsoft.com/es-ES/services/cosmos-db/): Es una base de datos NoSQL sin servidor totalmente administrada para aplicaciones de alto rendimiento de cualquier tamaño o escala.

Empezamos con la creación de la solución que conformará el Backend. Para ello abrimos Visual Studio y creamos un nuevo proyecto, de tipo ASP.NET Core Web Application:

![image](https://user-images.githubusercontent.com/18615795/182643880-1dfaab8b-9952-4548-a0ca-505c90af3430.png)

A continuación le damos un nombre y una ruta donde guardar la solución:

![image](https://user-images.githubusercontent.com/18615795/182644475-a8434bce-a96d-4fec-b93e-c49c69f5e2d7.png)

Por último, seleccionamos el template del proyecto (API) y la versión de .NET Core a utilizar (3.1):

![image](https://user-images.githubusercontent.com/18615795/182644731-da7b5d79-02bb-4d92-8579-d19d7bff2484.png)

Una vez creada la solución, instalamos los Nuget Packages necesarios: 

![image](https://user-images.githubusercontent.com/18615795/182648595-3d8f15bc-b600-47fa-bfe9-5cf3dea2cf23.png)

- Microsoft.Azure.CosmosDB: Librería que nos permite conectarnos y trabajar con Azure Cosmos DB usando la API de SQL.
- Newtonsoft.Json: Nos permite serializar y deserializar objetos, utilizados para la comunicación con Cosmos DB.
- Swashbuckle.AspNetCore (versión 5.6.3, que es compatible con proyectos ASP.NET Core 3.1: Nos permite utilizar Swagger para documentar nuestras APIs.

Empezamos creando los tres modelos de datos que serán persistidos en la base de datos. Creamos una carpeta en la solución de nombre Models (Modelos), donde creamos tres clases:

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

        [JsonProperty(PropertyName = "fullName")]
        public string FullName { get; set; }

        [JsonProperty(PropertyName = "mail")]
        public string Mail { get; set; }
    }
  ```

A continuación, creamos las tres interficies de lo que serán nuestros servicios, que para un primer ejemplo pueden contener simplemente las operaciones CRUD (Create, Read, Update y Delete) de cada una de las entidades. Creamos una carpeta con nombre Interficies (Interfaces), y las añadimos. Por ejemplo, para Reservas (Bookings), creamos la IReservasSevicio.cs (IBookingsService.cs):

  ```cs
    public interface IBookingsService
    {
        Task<IEnumerable<Booking>> GetBookingsAsync(string query);

        Task AddBookingAsync(Booking booking);

        Task DeleteBookingAsync(string id);

        Task UpdateBookingAsync(Booking booking);
    }
  ```
El siguiente paso es crear los servicios que implementan las interficies. Creamos la carpeta Servicios (Services), y creamos los tres servicios. Por ejemplo, para Reservas (Bookings), creamos la clase BookingsService.cs:

  ```cs
    public class BookingsService: IBookingsService  <-- Implementa la interficie
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
       
       //... Implementar el resto de métodos definidos en la interficie. La implementación completa está en la carpeta BizzSummitAPI del directorio raíz de este repo.
  ```
Una vez tenemos los servicios, vamos a implementar los controladores (Controllers), usando la carpeta ya existente del mismo nombre. Creamos un fichero para cada controller. Por ejemplo, creamos el ReservasControlador.cs (BookingsController.cs):

  ```cs
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
        //... Implementar el resto de acciones para completar el CRUD. La implementación completa está en la carpeta BizzSummitAPI del directorio raíz de este repo.
  ```

Configuramos los servicios que dependen del proyecto.

![image](https://user-images.githubusercontent.com/18615795/182646899-76ca6af4-fd2e-470e-8116-6b970a5f6c04.png)

Para ello, editamos el método Configure Services de la clase Startup, añadiendo como servicios (AddSingleton) mediante inyección de dependencias la inicialización única de la Cosmos DB y las colecciones:

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
 
 En cada uno de los métodos InitializeCosmosXXX, nos aseguramos que la Cosmos DB y el container existe (sino lo creamos automáticamente) y creamos la instancia única del servicio:
 
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
 
 En nuestra subscripción de Azure, creamos una Cosmos DB, preferiblemente dentro de un grupo de recursos propio al proyecto. Necesitamos definir los secretos de usuario, que contienen todos los valores necesarios para acceder a la Cosmos DB. Para ello editamos los secretos de usuario:
 
 ![image](https://user-images.githubusercontent.com/18615795/184005037-08a80bff-1f0f-4207-8732-1fa22867b33c.png)
 
   ```cs
   {
    "CosmosDB:Endpoint": "Endpoint de la Cosmos DB. Ej: https://XXXXcosmosdb.documents.azure.com:443/",
    "CosmosDB:Key": "Key de la Cosmos DB.",
    "CosmosDB:DatabaseName": "Nombre de la base de datos en la Comsos DB. Ej: BizzSummit",
    "CosmosDB:BookingsContainer": "Bookings",
    "CosmosDB:ProjectsContainer": "Projects",
    "CosmosDB:ResourcesContainer": "Resources"
   }
   ```

 El endpoint lo podemos encontrar directamente en la overview del componente Cosmos DB:
 
 ![image](https://user-images.githubusercontent.com/18615795/184008365-3698b34e-0f8e-4f88-897b-9eb4846fc420.png)

 También se encuentra dentro de Keys, donde tenéis que usar la Primary Key como Key en la solución:

 ![image](https://user-images.githubusercontent.com/18615795/184008601-bead7144-6fe2-4c8e-98d2-e5f50aec04bc.png)

 Creamos una base de datos para el proyecto dentro de la Cosmos DB y configuramos, en la solución, la key DatabaseName.

 Para comprobar que Swagger esté bien configurado, en la clase Startup.cs:
 - El método ConfigureServices tiene que tener la línea services.AddSwaggerGen();
 - El método Configure tiene que tener la app configurada para usar Swagger:
 ```cs
 app.UseSwagger(c =>
 {
   c.SerializeAsV2 = true;
 });
 app.UseSwaggerUI(c =>
 {
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "BizzSummitAPI V1");
   c.RoutePrefix = string.Empty;
 });
 ```

 Probamos que la aplicación funcione. La lanzamos en local (IIS Express) y comprobamos que Swagger esté bien configurado:
 
 ![image](https://user-images.githubusercontent.com/18615795/184009203-73ef3885-71e3-4df3-941f-257e42aee9ab.png)

 El siguiente paso es publicar la aplicación en Azure. Para ello creamos, dentro de nuestra subscripción, un App Service:
 
 ![image](https://user-images.githubusercontent.com/18615795/184011002-d9a466a9-4f93-424b-b3d9-70edf3b391b6.png)

  En nuestra solución, botón derecho -> Publicar:
 
 ![image](https://user-images.githubusercontent.com/18615795/184011351-ce55688f-04da-4f17-8dd2-5d4b8117afe7.png)

 Configuramos un nuevo destino y seguimos el Wizard, seleccionando el componente Web App creado en Azure:
 
 ![image](https://user-images.githubusercontent.com/18615795/184013691-78afd1a9-60e3-4366-b08d-10c0a84f7c0d.png)
 
 Publicamos la solución. Una vez completado, se abrirá la URL de la webapp en Azure con la solución desplegada:
 
 ![image](https://user-images.githubusercontent.com/18615795/184013829-f046e732-8156-4166-b06d-349d7e877fdd.png)
 
 Por último, creamos también en Azure un nuevo componente, API Management
 
 ![image](https://user-images.githubusercontent.com/18615795/184012127-8139c744-6400-40e9-9916-0975fc554e7d.png)

 Copiamos, desde la aplicación web, el link a la especificación OpenAPI:
 
 ![image](https://user-images.githubusercontent.com/18615795/184014085-fda0f48f-e3b5-4788-a764-abdd395b3c1e.png)

 En el componente API Management, creamos una nueva API a partir de la definición OpenAPI:
 
 ![image](https://user-images.githubusercontent.com/18615795/184014278-1e477069-1bf9-4565-a770-c29be4f99a28.png)

 Introducimos el link:
 
 ![image](https://user-images.githubusercontent.com/18615795/184014488-319a088b-117c-4c0e-bcf4-eb607c302d51.png)

 Comprobamos que se ha importado correctamente:
 
 ![image](https://user-images.githubusercontent.com/18615795/184014661-24a61d78-efdb-4132-b2c9-2a6a4d325f7a.png)

 Con esto, tenemos el backend listo y podemos pasar al siguiente bloque, que es la creación del Custom Conector y su uso en Power Platform.
 
 No hace falta decir que estamos obviando, por motivos de tiempo y facilidad de uso dentro del Workshop, pasos absolutamente necesarios para poder poner la aplicación en un entorno de producción: Autenticación / Autorización, Logging, etc. Estamos encantados, si estáis interesados, en que nos contactéis en cualquier momento para profundizar más en cualquier duda que tengáis acerca de ello.
