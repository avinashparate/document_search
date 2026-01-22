Distributed Multi-Tenant Document Search Service (Prototype)

Overview
This project is a prototype implementation of a distributed, multi-tenant document search service designed to demonstrate enterprise-grade architectural patterns such as scalability, fault tolerance, and tenant isolation.
The system supports full-text search with relevance ranking and is capable of handling millions of documents with sub-second query latency using Elasticsearch.

Architecture Overview
Clients interact with a stateless ASP.NET Core API, which enforces tenant isolation and query validation.
Elasticsearch acts as the primary search and storage engine, while Redis provides caching and rate limiting.

For full architectural details, refer to:
Distributed_Document_Search_Architecture.docx

Technology Stack
Layer					Technology
Language				C# (.NET 8)
API Framework			ASP.NET Core
Search Engine			Elasticsearch 8.x
Cache / Rate Limiting	Redis
Containerization		Docker, Docker Compose
API Docs		Swagger (OpenAPI)

Getting Started
Prerequisites
-Docker Desktop
-Git
-.NET 8 SDK (optional, for debugging)

Run Locally
- git clone https://github.com/avinashparate/document_search.git
- Navigate to Folder document_search\DocumentSearch\DocumentSearch.Api in Powershell
- Docker Desktop should be up and running
- execute command  "docker-compose up --build"
This will launch Service
- export file DocumentSearchService.postman_collection in Postman which has collection of command
- can execute POST GET command for different endpoints for indexing, getting by id, by keyword etc

Access Services
-API (Swagger UI): http://localhost:5000/swagger
-Elasticsearch: http://localhost:9200

API Examples
Please use DocumentSearchService.postman_collection and export in Postman else can use below curl commands 
Index a Document
curl -X POST http://localhost:5000/documents \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: tenantA" \
  -d '{
    "title": "Distributed Systems",
    "content": "Elasticsearch enables fast full-text search."
  }'

Search Documents
curl -X GET "http://localhost:5000/search?q=elasticsearch" \
  -H "X-Tenant-Id: tenantA"

Get Document by ID
curl -X GET http://localhost:5000/documents/doc1 \
  -H "X-Tenant-Id: tenantA"

Delete a Document
curl -X DELETE http://localhost:5000/documents/doc1 \
  -H "X-Tenant-Id: tenantA"

Health Check
curl http://localhost:5000/health



