# BizzSummit2022 - Introducción
Fusion Teams Workshop for BizzSummit 2022

Pre-requisitos:

  1. Suscripción a Azure: [Cuenta gratuïta de Azure](https://azure.microsoft.com/es-es/free/)
  2. Acceso a un entorno de Power Platform: [Plan de desarrollo de Power Apps](https://docs.microsoft.com/es-es/power-apps/maker/developer-plan)
  3. Acceso a Visual Studio o Visual Studio Code


En este taller proponemos el siguiente caso de uso:

![image](https://user-images.githubusercontent.com/18615795/181196622-dfe5f539-5cfe-4b48-9eda-0adb1384891c.png)

Queremos permitir que los "Citizen Developers" puedan crear un conector personalizado que les permita utilizar una Web API que expone algunos servicios internos de la compañía. En este caso, queremos crear un conector personalizado que accede a una API que permite gestionar el seguimiento y gestión del tiempo en los proyectos por parte del personal de la compañía.

Los componentes que se van a usar en el Back-End son los siguientes: 

   1. [APIM](https://azure.microsoft.com/es-es/services/api-management/): Permite implementar puertas de enlace de API en paralelo con las API hospedadas en Azure, en otras nubes y en el entorno local para optimizar el flujo de tráfico de las API. Satisfaze los requisitos de seguridad y cumplimiento normativo, con una experiencia de administración unificada y una observación completa de todas las API internas y externas.
   2. [App Service](https://azure.microsoft.com/es-es/services/app-service/): Compile, implemente y escale aplicaciones web y API de forma automática según sus condiciones. En nuestro caso, crearemos, publicaremos y alojaremos una API web con .NET Core.
   3. [Cosmos DB](https://azure.microsoft.com/es-es/services/cosmos-db/): Azure Cosmos DB es una base de datos NoSQL sin servidor totalmente administrada para aplicaciones de alto rendimiento de cualquier tamaño o escala.

Los componentes que vamos a usar en el Front-End son los siguientes:

1. [Conector Personalizado](https://docs.microsoft.com/es-es/connectors/custom-connectors/): Permite utilizar la API desde cualquier aplicación (Power App) o flujo (Power Automate) en el entorno donde esté instalado.
2. [Power App](https://powerapps.microsoft.com/es-es/): Aplicación para poder gestionar el seguimiento de actividades y tiempo dedicado a los proyectos por parte de los empleados y empleadas de la compañía.
3. [Power Automate](): Flujos que se utilizan para implementar las distintas reglas de negocio.



