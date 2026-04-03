# RAG Solution (Without Vector Database)

## 📋 Project Overview

A **Retrieval-Augmented Generation (RAG)** system built with .NET 9 that enables intelligent document ingestion and question-answering capabilities **without using vector databases**. Instead of embeddings and vector similarity search, this solution uses **lexical overlap and relevance scoring** to retrieve relevant document chunks.

### Key Features
- 📄 Document ingestion with chunking and metadata support
- 🔍 Question answering with context retrieval
- 🎯 Lexical-based relevance scoring (no vector embeddings required)
- 🤖 OpenAI GPT integration for answer generation
- 💾 SQL Server for document persistence
- 🚀 FastEndpoints for high-performance API endpoints
- ⚡ React 19 + Vite frontend UI
- 🏥 Health check endpoints for monitoring
- 📚 API versioning (v1, v2)
- 📖 Swagger/OpenAPI documentation
- 🔐 Environment variable configuration with .env support

---

## 🏗️ Architecture & Infrastructure

### System Architecture
```
┌─────────────────┐
│   React UI      │  (Vite + React 19 + TypeScript)
│  (Port 51136)   │
└────────┬────────┘
         │ HTTP/REST
         ↓
┌─────────────────────────────────────────────┐
│    ASP.NET Core Web API (.NET 9)            │
│    • HTTP: localhost:5228                   │
│    • HTTPS: localhost:7256                  │
│  ┌─────────────────────────────────────┐   │
│  │      FastEndpoints (v8.0.1)         │   │
│  │  • /api/v1/ingest                   │   │
│  │  • /api/v1/ask                      │   │
│  │  • /api/v1/health                   │   │
│  └─────────────────────────────────────┘   │
│  ┌─────────────────────────────────────┐   │
│  │    RAG Processing Pipeline          │   │
│  │  • TextProcessor (chunking)         │   │
│  │  • RelevanceScorer (lexical match)  │   │
│  │  • IngestDocumentService            │   │
│  │  • QuestionAnsweringService         │   │
│  └─────────────────────────────────────┘   │
└───────────┬─────────────────────────────────┘
            │
            ↓
┌───────────────────────┐        ┌──────────────────┐
│   SQL Server          │        │   OpenAI API     │
│  (RAGWithoutVectorDB) │        │   (GPT-4o-mini)  │
│  • RagDocuments Table │        │                  │
└───────────────────────┘        └──────────────────┘
```

### Data Flow

**Document Ingestion:**
1. User submits document via `/api/v1/ingest` endpoint
2. `TextProcessor` chunks content into sentence groups (10 sentences per chunk)
3. Each chunk is tokenized for lexical search
4. Document stored in SQL Server `RagDocuments` table
5. In-memory cache updated for fast retrieval

**Question Answering:**
1. User submits question via `/api/v1/ask` endpoint
2. Question is tokenized into search terms
3. `RelevanceScorer` computes lexical overlap with all document chunks
4. Top-K most relevant chunks are retrieved (default: 5, max: 10)
5. Context assembled from top chunks
6. OpenAI GPT generates answer based on context
7. Response includes answer + source snippets with relevance scores

---

## 🛠️ Technology Stack

### Backend (Web.API)
- **Framework:** ASP.NET Core 9.0 (.NET 9)
- **API Pattern:** FastEndpoints 8.0.1
- **Database:** SQL Server (with ADO.NET)
- **AI Integration:** 
  - Azure.AI.OpenAI 2.1.0
  - OpenAI 2.9.1
  - OpenAI-DotNet 8.8.8
- **Documentation:** Swashbuckle.AspNetCore 7.2.0
- **Validation:** FluentValidation (via FastEndpoints)
- **Database Client:** Microsoft.Data.SqlClient 6.1.1
- **Environment Variables:** DotNetEnv 3.1.1

### Frontend (Web.UI)
- **Framework:** React 19.2.0
- **Build Tool:** Vite 7.3.1
- **Language:** TypeScript 5.9.3
- **Package Manager:** npm/pnpm
- **Development Port:** 51136

### Database Schema
```sql
CREATE TABLE dbo.RagDocuments
(
    Id NVARCHAR(200) NOT NULL PRIMARY KEY,
    Title NVARCHAR(300) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    MetadataJson NVARCHAR(MAX) NULL,
    CreatedUtc DATETIME2 NOT NULL,
    UpdatedUtc DATETIME2 NOT NULL
)
```

---

## 📁 Project Structure

```
RAGSolution/
├── Web.API/                              # ASP.NET Core Web API
│   ├── Features/                         # Feature-oriented architecture
│   │   ├── IngestDocument/
│   │   │   └── Post/
│   │   │       ├── EndPoint/
│   │   │       │   └── EndPoint.cs       # FastEndpoint for ingestion
│   │   │       ├── Model/
│   │   │       │   └── Models.cs         # Request/Response DTOs
│   │   │       └── Data/
│   │   │           ├── IIngestDocumentService.cs
│   │   │           └── IngestDocumentService.cs
│   │   ├── AskQuestion/
│   │   │   └── Post/
│   │   │       ├── EndPoint/
│   │   │       │   └── EndPoint.cs       # FastEndpoint for Q&A
│   │   │       ├── Model/
│   │   │       │   └── Models.cs         # Request/Response DTOs
│   │   │       ├── Data/
│   │   │       │   ├── IQuestionAnsweringService.cs
│   │   │       │   └── QuestionAnsweringService.cs
│   │   │       └── Validator.cs          # FluentValidation rules
│   │   └── CheckHealth/
│   │       └── EndPoint.cs               # Health check endpoint
│   ├── Database/
│   │   ├── IRagDocumentStore.cs          # Repository interface
│   │   └── SqlServerRagDocumentStore.cs  # SQL Server implementation
│   ├── Shared/
│   │   ├── TextProcessor.cs              # Chunking & tokenization
│   │   ├── RelevanceScorer.cs            # Lexical scoring algorithm
│   │   └── InternalModels.cs             # Shared domain models
│   ├── Program.cs                        # Application entry point
│   ├── Web.API.csproj                    # Project file
│   ├── .env                              # Environment variables (API keys)
│   ├── appsettings.json                  # Configuration
│   └── appsettings.Development.json      # Dev configuration
│
└── Web.UI/                               # React Frontend
    ├── src/
    ├── package.json
    └── vite.config.ts
```

### Feature-Oriented Architecture
The API follows **Vertical Slice Architecture** using FastEndpoints:
- Each feature is self-contained in its own folder
- Clear separation: EndPoint → Model → Data → Validator
- Minimal coupling between features
- Easy to test and maintain
- **Note:** This project uses FastEndpoints exclusively (no traditional MVC Controllers)

---

## 🔧 Configuration

### Environment Variables (.env file)

Create a `.env` file in the `Web.API` directory:

```env
# OpenAI Configuration
OpenAI__ApiKey=sk-proj-YOUR-OPENAI-API-KEY-HERE

# Azure OpenAI Configuration (if using Azure)
AzureOpenAI__Endpoint=https://YOUR-RESOURCE-NAME.openai.azure.com/
AzureOpenAI__ApiKey=YOUR-AZURE-OPENAI-API-KEY
AzureOpenAI__DeploymentName=gpt-4o-mini
```

**Important:** The `.env` file is loaded at application startup using `DotNetEnv.Env.Load()` in `Program.cs`.

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "RagSqlServer": "Server=SERVER\\INSTANCE;Database=RAGWithoutVectorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "AI": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-4o-mini"
  },
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  },
  "Rag": {
    "SeedDocuments": []
  }
}
```

**Configuration Priority:**
1. Environment variables from `.env` file (highest priority)
2. `appsettings.json`
3. `appsettings.Development.json` (in Development environment)

**AI Provider Options:**
- `"OpenAI"` - Uses OpenAI API directly
- `"AzureOpenAI"` or `"Azure"` - Uses Azure OpenAI Service
- `"InMemory"` - Mock provider for testing

---

## 🚀 Getting Started

### Prerequisites
- **.NET 9 SDK**
- **SQL Server** (LocalDB, Express, or full version)
- **Node.js** 18+ (for frontend)
- **OpenAI API Key** (or Azure OpenAI credentials)

### Backend Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/tuanmn-kms/ragsolution-withoutvectordb
   cd RAGSolution
   ```

2. **Create .env file:**
   ```bash
   cd Web.API
   # Create .env file with your OpenAI API key
   echo "OpenAI__ApiKey=sk-proj-YOUR-KEY-HERE" > .env
   ```

3. **Configure the database:**
   - Update connection string in `Web.API/appsettings.json`
   - Database and table will be auto-created on first run

4. **Install dependencies:**
   ```bash
   dotnet restore
   ```

5. **Run the API:**
   ```bash
   dotnet run
   ```

6. **Access the API:**
   - HTTP: `http://localhost:5228`
   - HTTPS: `https://localhost:7256`
   - Swagger UI: `https://localhost:7256/swagger`

### Frontend Setup

1. **Install dependencies:**
   ```bash
   cd Web.UI
   npm install
   ```

2. **Run development server:**
   ```bash
   npm run dev
   ```

3. **Access the UI:**
   - Navigate to `http://localhost:51136`

---

## 📡 API Endpoints

### Base URL
- **HTTP:** `http://localhost:5228`
- **HTTPS:** `https://localhost:7256`

### Version 1 (v1)

#### **POST** `/api/v1/ingest`
Ingest a document into the RAG system.

**Request:**
```json
{
  "id": "doc-001",
  "title": "Document Title",
  "content": "Full document content...",
  "metadata": {
    "author": "John Doe",
    "source": "manual"
  }
}
```

**Response:**
```json
{
  "message": "Document 'doc-001' ingested successfully.",
  "documentId": "doc-001",
  "chunkCount": 15
}
```

#### **POST** `/api/v1/ask`
Ask a question and get an AI-generated answer with sources.

**Request:**
```json
{
  "question": "What is the RAG architecture?",
  "topK": 5,
  "model": "gpt-4o-mini",
  "maxAnswerSentences": 3
}
```

**Response:**
```json
{
  "question": "What is the RAG architecture?",
  "answer": "The RAG architecture uses ASP.NET Core...",
  "sources": [
    {
      "documentId": "seed-architecture-overview",
      "title": "RAG Architecture Overview",
      "snippet": "This solution uses ASP.NET Core Web API...",
      "relevanceScore": 0.8542
    }
  ]
}
```

**Request Parameters:**
- `question` (required): The question to ask
- `topK` (optional): Number of document chunks to retrieve (1-10, default: 5)
- `model` (optional): OpenAI model to use (default: from config)
- `maxAnswerSentences` (optional): Maximum sentences in answer (default: 3)

#### **GET** `/api/v1/health`
Check API and database health status.

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0",
  "service": "RAG API",
  "databaseStatus": "Healthy",
  "databaseMessage": "Connected to server: DESKTOP-XXX, database: RAGWithoutVectorDB"
}
```

---

## 🧠 RAG Algorithm (Without Vector DB)

### Text Processing Pipeline

1. **Chunking** (`TextProcessor.ChunkContent`)
   - Splits content into sentence groups (10 sentences/chunk)
   - Uses regex pattern `(?<=[.!?])\s+` to detect sentence boundaries
   - Preserves semantic context within chunks
   - Pre-computes tokens for each chunk

2. **Tokenization** (`TextProcessor.Tokenize`)
   - Extracts alphanumeric terms using regex `[a-zA-Z0-9]+`
   - Minimum token length: 2 characters
   - Converts to lowercase for case-insensitive matching
   - Uses compiled regex for performance

3. **Relevance Scoring** (`RelevanceScorer.ComputeRelevance`)
   - **Lexical Score:** `overlap / questionTerms.Count`
   - **Density Bonus:** `overlap / chunkTerms.Count`
   - **Final Score:** `lexicalScore + densityBonus`
   - Ranks chunks by combined score

4. **Context Assembly**
   - Retrieves top-K ranked chunks (1-10, default: 5)
   - Filters chunks with score > 0
   - Orders by score descending, then by document ID
   - Concatenates chunk text with separators
   - Passes context to OpenAI for answer generation

### Why No Vector Database?

- ✅ **Simpler infrastructure** - No Pinecone, Weaviate, or Chroma needed
- ✅ **Faster prototyping** - No embedding generation overhead
- ✅ **Lower costs** - No additional services or API calls for embeddings
- ✅ **Good for small-medium datasets** - Lexical search works well at scale
- ✅ **Transparent scoring** - Easy to debug and understand relevance
- ⚠️ **Trade-off:** May miss semantic similarity (e.g., "car" vs "automobile")

---

## 🏥 Health Monitoring

The API includes comprehensive health checks accessible at `/api/v1/health`:

- **API Status:** Always returns service availability
- **Database Connectivity:** Tests SQL Server connection
- **Response Time:** Timestamp for monitoring latency
- **Degraded State:** Detects partial failures

**Health Check Responses:**
- `Healthy` - All systems operational
- `Degraded` - Service running but database unavailable
- Includes server name and database name in response

**Use Cases:**
- Kubernetes liveness/readiness probes
- Azure App Service health checks
- Monitoring dashboards (Azure Monitor, Grafana, etc.)
- Load balancer health checks

---

## 🔒 Security Considerations

### Current Implementation
- ⚠️ **Anonymous access** enabled for all endpoints (development mode)
- 🔐 API keys stored in `.env` file (excluded from source control)
- ✅ **Parameterized SQL queries** to prevent SQL injection
- ✅ **CORS policy** restricts access to localhost:51136

### Production Recommendations

1. **Authentication & Authorization:**
   - Enable JWT bearer token authentication
   - Add role-based access control (RBAC)
   - Consider Azure AD B2C for enterprise scenarios

2. **Secrets Management:**
   - Move secrets to **Azure Key Vault**
   - Use managed identities for Azure resources
   - Never commit `.env` file to source control

3. **API Security:**
   - Add rate limiting (consider AspNetCoreRateLimit)
   - Implement request size limits
   - Add input validation and sanitization
   - Enable HTTPS redirect in production

4. **CORS Configuration:**
   - Replace localhost origins with production URLs
   - Use specific allowed headers/methods
   - Consider using Azure API Management

5. **Monitoring & Logging:**
   - Add Application Insights telemetry
   - Log security events (failed auth, suspicious activity)
   - Monitor API usage patterns

6. **Database Security:**
   - Use Azure SQL Database with firewall rules
   - Enable SQL Server auditing
   - Use separate read/write connection strings
   - Implement connection pooling limits

---

## 🧪 Testing Strategy

### Recommended Test Approach

1. **Unit Tests:**
   - `TextProcessor.Tokenize()` - various input formats
   - `TextProcessor.ChunkContent()` - edge cases
   - `RelevanceScorer.ComputeRelevance()` - scoring accuracy
   - `TextProcessor.BuildSnippet()` - truncation logic

2. **Integration Tests:**
   - FastEndpoints with in-memory database
   - Document ingestion flow
   - Question answering with mocked OpenAI
   - Health check with database connectivity

3. **End-to-End Tests:**
   - Full ingestion → query flow
   - Multiple document ingestion
   - Complex question scenarios
   - Error handling (no documents, invalid input)

4. **Performance Tests:**
   - Chunk retrieval latency with 1K, 10K, 100K chunks
   - Concurrent request handling
   - Memory usage under load
   - Database query performance

### Example Test Areas
- ✅ Document ingestion with various content sizes
- ✅ Relevance scoring accuracy and consistency
- ✅ Question answering with no documents ingested
- ✅ Chunking edge cases (empty content, single sentence, very long content)
- ✅ Database connection failures and recovery
- ✅ Invalid API key scenarios
- ✅ CORS policy validation
- ✅ Concurrent document updates

---

## 📊 Performance Characteristics

### Expected Performance
- **Document Ingestion:** ~50-200ms per document (depending on size and chunk count)
- **Question Answering:** ~1-3 seconds (includes ~1-2s OpenAI API latency)
- **Chunk Retrieval:** <50ms for datasets up to 10,000 chunks
- **Database Operations:** <100ms for typical CRUD operations
- **Health Check:** <50ms (with database connection)

### Scaling Considerations

**Current Architecture:**
- In-memory document cache trades memory for speed
- Single-instance deployment suitable for small-medium workloads
- SQL Server can handle millions of documents with proper indexing

**Horizontal Scaling Options:**
- Use **Redis** or **Azure Cache for Redis** for distributed caching
- Replace in-memory dictionary with distributed cache
- Add load balancer (Azure Load Balancer, Application Gateway)
- Consider Azure Container Apps for auto-scaling

**Bottlenecks:**
- OpenAI API is the primary latency bottleneck (~1-2s per request)
- In-memory cache doesn't scale horizontally (lost on restart)
- Lexical search complexity grows linearly with chunk count

**Optimization Ideas:**
- Implement result caching for frequent questions
- Add connection pooling for SQL Server
- Use async/await throughout (already implemented)
- Consider pagination for large result sets
- Add database indexes on Title and Id columns

---

## 🛣️ Roadmap & Future Enhancements

### Potential Improvements

**Search & Retrieval:**
- [ ] Add hybrid search (lexical + semantic embeddings)
- [ ] Implement BM25 ranking algorithm
- [ ] Support for boolean queries (AND, OR, NOT)
- [ ] Add document filtering by metadata

**Caching & Performance:**
- [ ] Implement distributed caching (Redis)
- [ ] Add result caching for frequent questions
- [ ] Optimize chunk retrieval with database indexes
- [ ] Add response compression

**Features:**
- [ ] Document update/delete endpoints
- [ ] Support multiple document formats (PDF, DOCX, TXT, HTML)
- [ ] Add conversation history tracking
- [ ] Implement streaming responses for real-time answers
- [ ] Add document versioning
- [ ] Support for document collections/namespaces

**AI Integration:**
- [ ] Support multiple AI providers (Anthropic Claude, Cohere, local LLMs)
- [ ] Add custom prompt templates
- [ ] Implement function calling/tool use
- [ ] Add answer citation verification

**Security & Operations:**
- [ ] User authentication and authorization
- [ ] Multi-tenancy support
- [ ] API key management
- [ ] Usage quotas and rate limiting
- [ ] Audit logging

**DevOps:**
- [ ] Add Application Insights telemetry
- [ ] Containerize with Docker and Kubernetes support
- [ ] CI/CD pipeline with GitHub Actions
- [ ] Infrastructure as Code (Bicep/Terraform)
- [ ] Automated testing in pipeline

**Documentation:**
- [ ] OpenAPI/Swagger enhancements
- [ ] API usage examples in multiple languages
- [ ] Video tutorials
- [ ] Performance benchmarks

---

## 📝 Development Notes

### Design Decisions

**Architecture:**
- **FastEndpoints over Controllers:** 40% faster than MVC Controllers, less boilerplate, better for vertical slice architecture
- **Vertical Slice Architecture:** Each feature is self-contained, reducing coupling and improving maintainability
- **ADO.NET over EF Core:** More control, better performance for simple CRUD operations, smaller memory footprint
- **Lexical search over embeddings:** Simplifies infrastructure, reduces costs, sufficient for many use cases

**Data Management:**
- **In-memory caching:** Fast retrieval at the cost of memory, suitable for small-medium datasets
- **SQL Server:** Reliable, mature, good tooling, supports both Windows and Linux
- **Singleton services:** Document store and services are singletons for performance (loaded once)

**Environment Variables:**
- **DotNetEnv library:** Simple .env file support, familiar to developers from other ecosystems
- **Configuration hierarchy:** .env overrides appsettings.json, following 12-factor app principles

### Known Limitations

**Functional:**
- No semantic similarity detection (can't match "car" with "automobile")
- No support for multi-language documents
- No conversation context preservation between requests
- Document updates require full re-ingestion

**Scalability:**
- In-memory cache doesn't scale horizontally across multiple instances
- No distributed locking for concurrent document updates
- Linear time complexity for chunk retrieval (O(n) where n = total chunks)

**External Dependencies:**
- OpenAI API rate limits apply (varies by tier)
- Network latency to OpenAI impacts response time
- SQL Server must be available (no offline mode)

**Security:**
- No built-in authentication (anonymous access)
- API keys in configuration files (not encrypted at rest)
- No request throttling or abuse prevention

---

## 📄 License

No License

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch:** `git checkout -b feature/amazing-feature`
3. **Make your changes** and ensure code quality
4. **Write/update tests** for your changes
5. **Commit your changes:** `git commit -m 'Add amazing feature'`
6. **Push to the branch:** `git push origin feature/amazing-feature`
7. **Open a Pull Request**

### Code Standards
- Follow C# coding conventions
- Use meaningful variable/method names
- Add XML documentation comments for public APIs
- Keep methods small and focused (< 50 lines preferred)
- Write unit tests for new functionality

---

## 📧 Contact & Support

- **Repository:** https://github.com/tuanmn-kms/ragsolution-withoutvectordb
- **Issues:** https://github.com/tuanmn-kms/ragsolution-withoutvectordb/issues

---

## 🙏 Acknowledgments

- **FastEndpoints:** High-performance endpoint library by @dj-nitehawk
- **OpenAI:** GPT models for answer generation
- **Microsoft:** .NET 9 and ASP.NET Core framework
- **React Team:** React 19 and ecosystem
- **DotNetEnv:** Simple .env file support by @tonerdo

---

**Built with ❤️ using .NET 9 and Modern Web Technologies**