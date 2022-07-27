# BizzSummit2022 - Backend

Fusion Teams Workshop for BizzSummit 2022

Prerequisites:

  1. Azure subscription:

In this workshop, we propose the following business scenario:

![image](https://user-images.githubusercontent.com/18615795/181196622-dfe5f539-5cfe-4b48-9eda-0adb1384891c.png)

We want to enable Citizen Developers, as Professional Developers, the possibility to create a custom connector that allows the usage of a Web API that exposes internal company services. In this case, we want to expose for Power Platform usage an API that allows to manage project time trackings, so employees can track their working times within projects.

The involved components are: 

   1. [APIM](https://azure.microsoft.com/en-us/services/api-management/): Azure API Management: Enables API gateways deployments side-by-side with the APIs hosted in Azure, other clouds, and on-premises, optimizing API traffic flow. Meet security and compliance requirements while enjoying a unified management experience and full observability across all internal and external APIs.

   2. [App Service](https://azure.microsoft.com/en-us/services/app-service/): Enables to build, deploy, and scale web apps and APIs on your terms. In our case, we will build, host and deploy a .NET Core Web API.

   3. [Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/): Fully managed, serverless NoSQL database for high-performance applications of any size or scale, with fast writes and reads anywhere in the world with multi-region writes and data replication.

Let's dive into the actual Backend development and release process. We start with creating a VS solution

