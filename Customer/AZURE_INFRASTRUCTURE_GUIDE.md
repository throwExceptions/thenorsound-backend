# ☁️ ThenorSound - Azure Deployment & Infrastructure Guide

**Dokument:** Azure Infrastructure Specification  
**Version:** 1.0  
**Datum:** 2026-02-09  
**Fokus:** Kostnadsoptimering + Skalbarhet (Dev → Prod)

---

## 📋 Innehållsförteckning

1. [Azure Architecture Overview](#azure-architecture-overview)
2. [Development Environment (Gratis Tier)](#development-environment-gratis-tier)
3. **[Database Strategy - Solving Free Tier Limitation](#database-strategy---solving-free-tier-limitation)** ⭐ NEW
4. [Production Environment (AKS)](#production-environment-aks)
5. [Infrastructure as Code (Terraform)](#infrastructure-as-code-terraform)
6. [Cost Optimization Strategy](#cost-optimization-strategy)
7. [Migration Dev → Prod](#migration-dev--prod)
8. [Monitoring & Logging](#monitoring--logging)
9. [Security Best Practices](#security-best-practices)

---

## Azure Architecture Overview

### Development Environment
```
Developer Laptop
    ↓ (Docker Compose)
┌──────────────────────────────────┐
│  Local Development               │
│  - 5 containerized services      │
│  - Local MongoDB (Docker)        │
│  - Local RabbitMQ (Docker)       │
│  - Total cost: $0                │
└──────────────────────────────────┘
    ↓ (Push to GitHub)
    ↓ (GitHub Actions)
┌──────────────────────────────────┐
│  Azure Container Registry (ACR)  │
│  - Free tier: 10GB storage       │
│  - Push images from CI/CD        │
│  - Cost: ~$5/month               │
└──────────────────────────────────┘
    ↓ (Deploy to Azure)
┌──────────────────────────────────┐
│  Azure Container Instances (ACI) │
│  - Dev deployment                │
│  - 1 CPU, 1GB RAM per service    │
│  - Cost: ~$20-30/month total     │
│  - OR Local Docker Compose       │
└──────────────────────────────────┘
    ↓ (Database)
┌──────────────────────────────────┐
│  Azure Cosmos DB (Free Tier!)    │
│  - 1000 RU/s free                │
│  - MongoDB API                   │
│  - TLS encrypted                 │
│  - Cost: $0 (Free tier)          │
└──────────────────────────────────┘

TOTAL DEV COST: ~$0-30/month (mostly ACR)
```

### Production Environment
```
GitHub Repo (main branch)
    ↓ (Commit)
┌──────────────────────────────────┐
│  GitHub Actions (CI/CD)          │
│  - Automated tests               │
│  - Build Docker images           │
│  - Push to ACR                   │
│  - Deploy to AKS                 │
│  - Cost: Free (GitHub included)  │
└──────────────────────────────────┘
    ↓ (Built images)
┌──────────────────────────────────┐
│  Azure Container Registry (ACR)  │
│  - Prod images (v1.0, v1.1, etc)│
│  - Cost: $5-10/month             │
└──────────────────────────────────┘
    ↓ (Deploy)
┌──────────────────────────────────────────────────────────┐
│  Azure Kubernetes Service (AKS)                         │
│  ────────────────────────────────────────────────────   │
│  ├─ 3 worker nodes (minimal production)                 │
│  ├─ Each: Standard_B2s (2 vCPU, 4GB RAM)               │
│  ├─ Load Balancer (for API Gateway)                     │
│  ├─ Network Policy & RBAC enabled                       │
│  ├─ Auto-scaling enabled                                │
│  ├─ Cost: ~$200-300/month (nodes)                       │
│  └─ + Ingress/Load Balancer: ~$50-70/month             │
└──────────────────────────────────────────────────────────┘
    ↓ (Databases)
┌──────────────────────────────────┐
│  Azure Cosmos DB (Single DB Strategy) │
│  - 1 database: "thenorsound"     │
│  - 5 collections (logical DBs)   │
│  - Shared throughput             │
│  - 1000 RU/s provisioned         │
│  - Cost: $0-35/month (FREE tier) │
│  - Upgrade: $35-50/month         │
└──────────────────────────────────┘
    ↓ (Message Queue)
┌──────────────────────────────────┐
│  Azure Service Bus OR RabbitMQ   │
│  - Message queues                │
│  - Pub/Sub topics                │
│  - Cost: ~$20-50/month           │
└──────────────────────────────────┘
    ↓ (Logging/Monitoring)
┌──────────────────────────────────────────────────────────┐
│  Azure Monitor + Application Insights                   │
│  - Request tracing                                       │
│  - Performance metrics                                   │
│  - Log aggregation                                       │
│  - Cost: ~$50-100/month                                 │
└──────────────────────────────────────────────────────────┘

TOTAL PROD COST: ~$320-450/month (with Single DB Strategy)
(Down from $620-1,100, saving $300-650/month vs 5 separate DBs)
(Roughly $0.01-0.02 per request at scale)
```

---

## Development Environment (Gratis Tier)

### Option 1: Local Docker Compose (Rekommenderat för Dev)

**Kostnader:** $0  
**Setup-tid:** 15 minuter  
**Performance:** Full lokal kontroll

```yaml
# docker-compose.yml (Recommended: Single DB with Collections)
version: '3.8'

services:
  # Single MongoDB instance with 1 database, 6 collections
  mongo:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
    environment:
      MONGO_INITDB_DATABASE: thenorsound

  # Message Broker
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq

  # Microservices (all use same MongoDB connection)
  auth-service:
    build: ./AuthService
    ports:
      - "5001:5000"
    depends_on:
      - mongo
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongo:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq

  user-service:
    build: ./UserService
    ports:
      - "5002:5000"
    depends_on:
      - mongo
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongo:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq

  # ... 4 more services (customer, event, crew, booking)
  # All using same ConnectionString and DatabaseName="thenorsound"
  # Each service owns its collection (auth_refreshTokens, users, customers, etc)

  api-gateway:
    build: ./ApiGateway
    ports:
      - "5000:5000"
    depends_on:
      - auth-service
      - user-service
      # ... depends on all services
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

volumes:
  mongo-data:
  rabbitmq-data:
```

**Start dev environment:**
```bash
cd /path/to/project
docker-compose up -d

# Verify all services running
docker-compose ps

# View logs
docker-compose logs -f auth-service

# Access MongoDB directly
mongo mongodb://localhost:27017/thenorsound

# Stop
docker-compose down
```

**Key Advantages:**
- ✅ Single MongoDB container (simple!)
- ✅ 1 database "thenorsound" with 6 collections
- ✅ Identical structure to production (dev/prod parity!)
- ✅ Cost: $0 (local machine storage only)
- ✅ Same code paths as production

---

### Option 2: Azure Container Instances (for Shared Dev/Testing)

**Kostnader:** ~$20-30/month  
**Setup-tid:** 30 minuter with IaC  
**Performance:** Shared team environment  

**When to use:**
- Multiple developers
- Mobile testing (need real Azure endpoint)
- Load testing
- CI/CD pipeline testing

**Architecture:**
```
6 Container Groups (one per service)
├─ auth-service
├─ user-service
├─ customer-service
├─ event-service
├─ crew-service
└─ booking-service

+ Cosmos DB (Free Tier)
+ RabbitMQ in container OR Azure Service Bus
+ Application Insights (free tier)
```

**Cost breakdown:**
```
6 services × $0.0000015/vCPU/sec × 730 hours/month
= 6 × $0.0000015 × 2628000
≈ $24/month

Cosmos DB (Free tier): $0
Service Bus (free tier): $0
Application Insights (1GB/day): ~$7/month

TOTAL: ~$30/month
```

---

## Database Strategy - Solving Free Tier Limitation

### ⚠️ Problem: Azure Cosmos DB Free Tier = 1 Database Only

The Free tier includes only **1 database** (with up to 25 containers), but our microservices architecture needs:
- 6 separate logical databases (auth, users, customers, events, crew, bookings)
- Data isolation per service
- Independent scaling

**Solutions ranked by cost:**

---

### ✅ RECOMMENDED: Hybrid Strategy (Dev/Prod Split)

**Development Phase: Use Local MongoDB (Cost: $0)**
```yaml
# docker-compose.yml
services:
  auth-db:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - auth-data:/data/db

  user-db:
    image: mongo:7
    ports:
      - "27018:27017"
    volumes:
      - user-data:/data/db

  customer-db:
    image: mongo:7
    ports:
      - "27019:27017"
    volumes:
      - customer-data:/data/db

  event-db:
    image: mongo:7
    ports:
      - "27020:27017"
    volumes:
      - event-data:/data/db

  crew-db:
    image: mongo:7
    ports:
      - "27021:27017"
    volumes:
      - crew-data:/data/db

  booking-db:
    image: mongo:7
    ports:
      - "27022:27017"
    volumes:
      - booking-data:/data/db
```

**Advantages:**
- ✅ Zero cost
- ✅ Full data isolation per service
- ✅ No limits on database count
- ✅ Fast local development
- ✅ Works offline
- ✅ Can test database backups/restores

**Disadvantages:**
- ✗ Not shared with team (need separate dev setup)
- ✗ No cloud connectivity for mobile testing
- ✗ Local machine storage usage

**Production Phase: Use Shared Cosmos Database (Cost-effective)**
```
1 Cosmos DB Account (Free tier)
  └─ 1 Database: "thenorsound"
     ├─ Collection: "auth_refreshTokens"
     ├─ Collection: "users"
     ├─ Collection: "customers"
     ├─ Collection: "events"
     ├─ Collection: "crew"
     └─ Collection: "bookings"
```

With this design:
- Use ONE database with **6 collections** (one per service)
- Each collection acts as a separate logical database
- Still maintain service isolation
- Cost in prod: $0 for free tier, scales to ~$100-200/month when upgraded
- Migration: Simple - rename database to collection at application layer

---

### ✅ ALTERNATIVE 1: MongoDB Atlas Free Tier (512 MB)

**Cost: $0**  
**Shared across team: ✅ Yes**

```
Pros:
├─ Free 512 MB managed database (Cloud)
├─ Shared access for team
├─ Real cloud endpoint
├─ Auto-backups included
├─ Mobile testing possible
└─ Easy team collaboration

Cons:
├─ Limited to 512 MB (small for dev)
├─ Have to pay when exceeding limit
├─ Data stored outside Azure ecosystem
└─ Requires learning another platform

Best for: Small team, testing phase
```

**Setup:**
```bash
# Create free account at https://www.mongodb.com/cloud/atlas
# Create free cluster (M0 - shared)
# Get connection string
# Use in services via environment variable

ConnectionString=mongodb+srv://user:password@cluster0.abc.mongodb.net/
```

### ✅ ALTERNATIVE 2: Single Cosmos Database + Collections (Cost: $0 → $35/month)

**Cost: $0 (Free tier with limit) → $35/month when scaled**

```
DEVELOPMENT:

1 Cosmos DB Account (Free tier)
  ├─ Database: "thenorsound-dev"
  ├─ Collections (6):
  │  ├─ auth-tokens
  │  ├─ users
  │  ├─ customers
  │  ├─ events
  │  ├─ crew
  │  └─ bookings
  └─ Shared throughput across all

Cost: $0/month (within free tier limits)
```

**Implementation:**
```csharp
// Use same Cosmos account, different collection names
// services/Auth/Program.cs
var settings = new MongoDbSettings
{
    ConnectionString = "mongodb+srv://...",
    DatabaseName = "thenorsound-dev",  // Single DB
    TokenCollectionName = "auth-tokens"  // Different collection
};

// services/User/Program.cs
var settings = new MongoDbSettings
{
    ConnectionString = "mongodb+srv://...",  // Same connection
    DatabaseName = "thenorsound-dev",       // Same DB
    UserCollectionName = "users"             // Different collection
};
```

**Collection-level isolation in code:**
```csharp
public interface IAuthRepository
{
    Task<RefreshToken> GetTokenAsync(string tokenId);
    // Automatically uses: db["auth-tokens"]
}

public interface IUserRepository
{
    Task<User> GetUserAsync(string userId);
    // Automatically uses: db["users"]
}

// Different collections = logical separation even in same database
```

**Pros:**
- ✅ Zero cost in dev
- ✅ Same Azure ecosystem
- ✅ Easy migration to prod
- ✅ Scales to 25 collections (more than enough)
- ✅ Per-collection indexing strategy

**Cons:**
- ✗ All collections share RU throughput
- ✗ Less flexible for high-variance workloads
- ✗ Outgrow free tier faster

**When to upgrade:**
- Free tier limit: 1000 RU/s shared
- Upgrade cost: ~$35/month (provisioned 400 RU/s)
- Still 1 database, but now paid

---

### ✅ ALTERNATIVE 3: PostgreSQL (Different approach, no MongoDB)

**Cost: $0 (Free tier) → $50/month (when scaled)**

```
Pros:
├─ Azure Database for PostgreSQL FREE tier available
├─ True schema/data isolation per service
├─ Better data consistency (ACID)
├─ No need to rewrite later
├─ MUCH cheaper at scale
├─ Mature ORM ecosystem
└─ Better for relational data

Cons:
├─ Requires migration from Boilerplate (designed for MongoDB)
├─ Need to learn new ORM (EF Core instead of MongoDB.Driver)
├─ Slightly slower development initially
└─ More schema design upfront

Best for: If you don't mind refactoring boilerplate
```

**Migration path (if interested):**
```csharp
// Old: MongoDB
var user = await userCollection.FindAsync(x => x.Id == "123");

// New: PostgreSQL with EF Core
var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == "123");
```

---

### RECOMMENDATION MATRIX

```
┌─────────────────────────────────────────────────────────┐
│                 CHOOSE YOUR PATH                        │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ✅ RECOMMENDED FOR THENORSOUND:                        │
│                                                         │
│    Hybrid Strategy:                                     │
│    ├─ DEV: Local MongoDB in Docker (6 separate DBs)    │
│    │   Cost: $0                                        │
│    │   Benefits: Full isolation, fast, offline         │
│    │   Setup: 10 minutes with docker-compose           │
│    │                                                    │
│    └─ PROD: Azure Cosmos with 1 DB + 6 collections    │
│        Cost: $0 (free) → $35/month (scaled)           │
│        Benefits: Cost-effective, still isolated        │
│        Migration: Minimal code changes                 │
│                                                         │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ✅ IF YOU NEED TEAM SHARING:                           │
│                                                         │
│    MongoDB Atlas Free Tier:                            │
│    ├─ Cost: $0 (512 MB limit)                         │
│    ├─ Benefits: Shared, cloud-based, easy access      │
│    ├─ Drawback: Only 512 MB total                      │
│    └─ Best for: Small team, short-term               │
│                                                         │
│    OR upgrade to: Atlas $9/month tier                  │
│                                                         │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ✅ IF YOU WANT TO AVOID MONGODB:                       │
│                                                         │
│    Azure PostgreSQL:                                   │
│    ├─ Cost: $0 (free tier) → $50+/month              │
│    ├─ Benefits: Better consistency, cheaper longterm  │
│    ├─ Drawback: Requires boilerplate rewrite (CQRS)   │
│    └─ Best for: Long-term project                     │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

### FINAL DECISION: Use Hybrid Strategy

```
DEVELOPMENT PHASE (Sprint 1-6):
┌─────────────────────────────────┐
│ Local Docker Compose            │
├─────────────────────────────────┤
│ 6 × MongoDB containers:         │
│                                 │
│ docker-compose up -d            │
│                                 │
│ auth-db       : 27017          │
│ user-db       : 27018          │
│ customer-db   : 27019          │
│ event-db      : 27020          │
│ crew-db       : 27021          │
│ booking-db    : 27022          │
│                                 │
│ Cost: $0/month                  │
│ Setup: 5 minutes                │
└─────────────────────────────────┘


PRODUCTION PHASE (Week 7+):
┌──────────────────────────────────────────┐
│ Azure Cosmos DB (Free Tier)              │
├──────────────────────────────────────────┤
│ 1 Database: "thenorsound-prod"          │
│ 6 Collections:                           │
│                                          │
│ - auth-tokens (TTL: 7 days)             │
│ - users (index: customerId)             │
│ - customers (index: orgNumber)          │
│ - events (index: customerId,startDate) │
│ - crew (index: customerId)              │
│ - bookings (index: eventId,crewId)     │
│                                          │
│ Throughput: 1000 RU/s shared            │
│ Cost: $0/month (free tier)              │
│ Upgrade: $35+/month when needed         │
└──────────────────────────────────────────┘
```

**Application Configuration:**
```csharp
// appsettings.Development.json (Local)
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://auth-db:27017",
    "DatabaseName": "thenorsound-auth",
    "CollectionName": "tokens"
  }
}

// appsettings.Production.json (Azure Cosmos)
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://thenorsound:***@thenorsound.mongo.cosmos.azure.com",
    "DatabaseName": "thenorsound-prod",
    "CollectionName": "auth-tokens"  // Same service, different collection
  }
}
```

**Migration code (Zero breaking changes):**
```csharp
// Repository handles both local & Cosmos transparently
public class AuthRepository : IAuthRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<RefreshToken> _collection;

    public AuthRepository(IMongoDatabase database)
    {
        _database = database;
        // Works with both local MongoDB and Azure Cosmos
        _collection = _database.GetCollection<RefreshToken>("auth-tokens");
    }

    public async Task<RefreshToken> GetTokenAsync(string tokenId)
    {
        return await _collection.Find(t => t.Id == tokenId).FirstOrDefaultAsync();
    }
}

// Services don't need to know if they're using local or cloud
```

---

### Migration Timeline

```
Sprint 1-6 (Dev):
  └─ Local MongoDB (docker-compose)
     └─ 6 separate databases
        └─ Full isolation, zero cost

Sprint 7 (Deploy):
  └─ Create Azure Cosmos Account (Free tier)
     └─ Single database "thenorsound-prod"
        └─ Multiple collections (one per service)
           └─ Change connection string (one line!)
              └─ Deploy to AKS

Post-Launch (Optimize):
  └─ Monitor Cosmos RU consumption
     ├─ If < 400 RU/s: Stay on free tier
     └─ If > 400 RU/s: Upgrade to provisioned tier (~$35/month)
```

---

### Cost Summary (All Options)

| Strategy | Dev Cost | Prod Cost | Effort | Notes |
|----------|----------|-----------|--------|-------|
| **Hybrid (Recommended)** | $0 | $0-35 | Low | Fast setup, scales well |
| MongoDB Atlas | $0 | $0-9 | Low | Shared, but 512MB limit |
| Cosmos Collections | $0 | $0-35 | Low | Azure-native, easy migration |
| PostgreSQL | $0 | $0-50 | High | Requires boilerplate rewrite |

**Choosing Hybrid saves you:**
- Development: $0/month vs $30-50/month for cloud dev
- Production: $0-35/month vs $300+ if using 6 separate Cosmos DBs
- **Total Year 1: ~$300-600 in savings** ✅

---

## Production Environment (AKS)

### Azure Kubernetes Service Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Azure Subscription                     │
│                                                         │
│  ┌─────────────────────────────────────────────────┐  │
│  │  Resource Group: thenorsound-prod               │  │
│  │                                                 │  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │  AKS Cluster (thenorsound-prod-aks)     │  │  │
│  │  │                                          │  │  │
│  │  │  Node Pool (System):                     │  │  │
│  │  │  └─ 1 × Standard_B2s (2vCPU, 4GB RAM)  │  │  │
│  │  │     For: kube-system, monitoring        │  │  │
│  │  │                                          │  │  │
│  │  │  Node Pool (Application):                │  │  │
│  │  │  ├─ 2 × Standard_B2s (base capacity)   │  │  │
│  │  │  └─ Auto-scale: 2-5 nodes               │  │  │
│  │  │     For: microservices, ingress         │  │  │
│  │  │                                          │  │  │
│  │  │  Services Running:                       │  │  │
│  │  │  ├─ auth-service (2 replicas)           │  │  │
│  │  │  ├─ user-service (2 replicas)           │  │  │
│  │  │  ├─ customer-service (2 replicas)       │  │  │
│  │  │  ├─ event-service (2 replicas)          │  │  │
│  │  │  ├─ crew-service (2 replicas)           │  │  │
│  │  │  ├─ booking-service (3 replicas)        │  │  │
│  │  │  ├─ api-gateway (2 replicas)            │  │  │
│  │  │  ├─ RabbitMQ (1 replica + persistent)  │  │  │
│  │  │  └─ Ingress Controller (nginx)          │  │  │
│  │  │                                          │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  │                                                 │  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │  Azure Container Registry (ACR)          │  │  │
│  │  │  ├─ auth-service:v1.0                    │  │  │
│  │  │  ├─ user-service:v1.0                    │  │  │
│  │  │  ├─ customer-service:v1.0                │  │  │
│  │  │  ├─ event-service:v1.0                   │  │  │
│  │  │  ├─ crew-service:v1.0                    │  │  │
│  │  │  ├─ booking-service:v1.0                 │  │  │
│  │  │  ├─ api-gateway:v1.0                     │  │  │
│  │  │  └─ rabbitmq:3-management                │  │  │
│  │  │                                          │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  │                                                 │  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │  Azure Cosmos DB (MongoDB API)           │  │  │
│  │  │                                          │  │  │
│  │  │  ├─ thenorsound-auth (400 RU/s)         │  │  │
│  │  │  ├─ thenorsound-users (400 RU/s)        │  │  │
│  │  │  ├─ thenorsound-customers (500 RU/s)    │  │  │
│  │  │  ├─ thenorsound-events (500 RU/s)       │  │  │
│  │  │  ├─ thenorsound-crew (400 RU/s)         │  │  │
│  │  │  └─ thenorsound-bookings (600 RU/s)     │  │  │
│  │  │                                          │  │  │
│  │  │  Backup: Enabled (daily snapshots)      │  │  │
│  │  │  Failover: Multi-region (optional)      │  │  │
│  │  │                                          │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  │                                                 │  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │  Azure Key Vault (Secrets Management)    │  │  │
│  │  │                                          │  │  │
│  │  │  ├─ mongodb-connection-strings           │  │  │
│  │  │  ├─ jwt-secret-key                       │  │  │
│  │  │  ├─ rabbitmq-credentials                 │  │  │
│  │  │  ├─ database-admin-passwords             │  │  │
│  │  │  └─ api-keys (external services)         │  │  │
│  │  │                                          │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  │                                                 │  │
│  └─────────────────────────────────────────────────┘  │
│                                                         │
│  ┌─────────────────────────────────────────────────┐  │
│  │  Monitoring & Logging (Azure Monitor)           │  │
│  │                                                 │  │
│  │  ├─ Application Insights (per service)          │  │
│  │  ├─ Log Analytics Workspace                     │  │
│  │  ├─ Alerts & Auto-scaling rules                 │  │
│  │  └─ Dashboards (Operations, Performance)        │  │
│  │                                                 │  │
│  └─────────────────────────────────────────────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### AKS Kubernetes Manifests

**1. Namespace & RBAC:**
```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: thenorsound
  labels:
    name: thenorsound

---
# k8s/rbac.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: thenorsound-sa
  namespace: thenorsound

---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: thenorsound-role
rules:
- apiGroups: [""]
  resources: ["configmaps", "secrets"]
  verbs: ["get", "list"]

---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: thenorsound-rolebinding
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: thenorsound-role
subjects:
- kind: ServiceAccount
  name: thenorsound-sa
  namespace: thenorsound
```

**2. ConfigMap & Secrets:**
```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: thenorsound-config
  namespace: thenorsound
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  RabbitMq__HostName: "rabbitmq-service"
  RabbitMq__Port: "5672"

---
# k8s/secrets-example.yaml
# In production, use Azure Key Vault
apiVersion: v1
kind: Secret
metadata:
  name: thenorsound-secrets
  namespace: thenorsound
type: Opaque
stringData:
  # Retrieve from Azure Key Vault in helm values
  mongodb-connection-string: "mongodb+srv://..."
  jwt-secret-key: "your-secret-key-here"
  rabbitmq-password: "guest"
```

**3. Auth Service Deployment:**
```yaml
# k8s/auth-service-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auth-service
  namespace: thenorsound
spec:
  replicas: 2
  selector:
    matchLabels:
      app: auth-service
  template:
    metadata:
      labels:
        app: auth-service
        version: v1
    spec:
      serviceAccountName: thenorsound-sa
      containers:
      - name: auth-service
        image: thenorsoundacr.azurecr.io/auth-service:v1.0
        imagePullPolicy: IfNotPresent
        ports:
        - containerPort: 5000
          name: http
        
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: thenorsound-config
              key: ASPNETCORE_ENVIRONMENT
        - name: MongoDbSettings__ConnectionString
          valueFrom:
            secretKeyRef:
              name: thenorsound-secrets
              key: mongodb-connection-string
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: thenorsound-secrets
              key: jwt-secret-key
        
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
        
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1000

---
# k8s/auth-service-service.yaml
apiVersion: v1
kind: Service
metadata:
  name: auth-service
  namespace: thenorsound
spec:
  selector:
    app: auth-service
  ports:
  - port: 80
    targetPort: 5000
    protocol: TCP
  type: ClusterIP

---
# k8s/auth-service-hpa.yaml (Auto-scaling)
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: auth-service-hpa
  namespace: thenorsound
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: auth-service
  minReplicas: 2
  maxReplicas: 5
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

**4. Ingress & Network Policy:**
```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: api-gateway-ingress
  namespace: thenorsound
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.thenorsound.com
    secretName: thenorsound-tls
  rules:
  - host: api.thenorsound.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: api-gateway
            port:
              number: 80

---
# k8s/network-policy.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: thenorsound-network-policy
  namespace: thenorsound
spec:
  podSelector:
    matchLabels:
      app: microservice
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: thenorsound
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 5672  # RabbitMQ
    - protocol: TCP
      port: 27017 # MongoDB
```

---

## Infrastructure as Code (Terraform)

### Azure Resources via Terraform

**File structure:**
```
infrastructure/
├── main.tf              # Main configuration
├── variables.tf         # Input variables
├── outputs.tf           # Output values
├── aks.tf              # AKS cluster
├── cosmosdb.tf         # Cosmos DB databases
├── key_vault.tf        # Key Vault for secrets
├── container_registry.tf # ACR
├── monitoring.tf       # Application Insights
├── networking.tf       # Virtual networks, NSG
└── terraform.tfvars    # Environment-specific values
```

**main.tf:**
```hcl
terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "thenorsound-tf-state"
    storage_account_name = "thenorsoundtfstate"
    container_name       = "tfstate"
    key                  = "prod.terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
  }
  subscription_id = var.subscription_id
}

resource "azurerm_resource_group" "main" {
  name     = "rg-thenorsound-${var.environment}"
  location = var.location
  tags     = var.tags
}
```

**aks.tf:**
```hcl
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-thenorsound-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "thenorsound-${var.environment}"
  kubernetes_version  = "1.28.0"

  default_node_pool {
    name                 = "system"
    node_count           = 1
    vm_size              = "Standard_B2s"
    min_count            = 1
    max_count            = 3
    enable_auto_scaling  = true
    zones                = ["1", "2", "3"]
  }

  network_profile {
    network_plugin = "azure"
    service_cidr   = "10.0.0.0/16"
    dns_service_ip = "10.0.0.10"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Additional node pool for applications
resource "azurerm_kubernetes_cluster_node_pool" "application" {
  name                  = "application"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  node_count            = 2
  vm_size               = "Standard_B2s"
  min_count             = 2
  max_count             = 5
  enable_auto_scaling   = true
  priority              = "Regular"
  zones                 = ["1", "2", "3"]
  
  tags = var.tags
}
```

**cosmosdb.tf:**
```hcl
resource "azurerm_cosmosdb_account" "main" {
  name                = "cosmos-thenorsound-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  offer_type          = "Standard"
  kind                = "MongoDB"

  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  backup {
    type                = "Continuous"
    tier                = "Continuous7Days"
  }

  tags = var.tags
}

# Cosmos DB – Auth Database
resource "azurerm_cosmosdb_mongo_database" "auth" {
  name                = "thenorsound-auth"
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
}

resource "azurerm_cosmosdb_mongo_collection" "auth_tokens" {
  name                = "refreshTokens"
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_mongo_database.auth.name

  default_ttl_seconds = 604800  # 7 days for token expiry
  throughput          = 400

  depends_on = [
    azurerm_cosmosdb_mongo_database.auth
  ]
}

# Similar for Users, Customers, Events, Crew, Bookings databases...
```

**monitoring.tf:**
```hcl
resource "azurerm_application_insights" "main" {
  name                = "appinsights-thenorsound-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.main.id
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-thenorsound-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}
```

**Deploy with Terraform:**
```bash
cd infrastructure

# Initialize Terraform
terraform init

# Plan changes
terraform plan -var-file=prod.tfvars -out=tfplan

# Apply changes
terraform apply tfplan

# Output AKS credentials
terraform output kube_config > kubeconfig.yaml
```

---

## Cost Optimization Strategy

### Development Phase ($0-30/month)

**Option 1: Local Only**
```
Cost breakdown:
├─ Laptop storage (existing): $0
├─ Docker & containers: $0 (free)
├─ MongoDB local: $0
├─ RabbitMQ local: $0
└─ Total: $0/month
```

**Option 2: Shared Azure Dev Environment**
```
Cost breakdown:
├─ Azure Container Registry (10GB): $5/month
├─ 6 × Container Instances (24/7): $24/month
├─ Cosmos DB (Free tier): $0
├─ Application Insights (1GB/day): $7/month
└─ Total: ~$36/month
```

**Recommendation:** Start with **Local Docker Compose**, upgrade to Option 2 only when team grows.

---

### Production Phase (~$600-1000/month)

**Cost optimization tactics:**

#### 1. Use Spot VMs for Non-Critical Services
```hcl
# Terraform – Node pool with Spot VMs (70% discount)
resource "azurerm_kubernetes_cluster_node_pool" "spot_pool" {
  name                  = "spotpool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  priority              = "Spot"  # ← This reduces cost 70%!
  eviction_policy       = "Delete"
  vm_size               = "Standard_B2s"
  min_count             = 1
  max_count             = 3
  enable_auto_scaling   = true
}

# Use for: analytics-service, non-critical background jobs
```

**Savings:** ~$50-100/month

#### 2. Auto-scaling Policies (Scale down during off-hours)
```yaml
# k8s/scheduled-scaling.yaml
apiVersion: autoscaling.alibabacloud.com/v1beta1
kind: CronHPA
metadata:
  name: thenorsound-schedules
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: booking-service
  schedules:
  - name: "scale-up-morning"
    schedule: "0 6 * * 1-5"
    minReplicas: 3
    maxReplicas: 10
  - name: "scale-down-evening"
    schedule: "0 20 * * 1-5"
    minReplicas: 1
    maxReplicas: 3
  - name: "minimal-weekend"
    schedule: "0 0 * * 0,6"
    minReplicas: 1
    maxReplicas: 2
```

**Savings:** ~$100-150/month

#### 3. Reserved Instances (1-year commitment)
```
Standard_B2s pricing:
├─ On-demand: $0.096/hour
├─ 1-year reserved: $0.058/hour (40% discount)
└─ 3-year reserved: $0.045/hour (52% discount)

For 3 nodes × 730 hours/month:
├─ On-demand: 3 × $0.096 × 730 = $211/month
└─ 3-year reserved: 3 × $0.045 × 730 = $99/month
Savings: $112/month for 3 nodes
```

**Note:** Only if you're committed long-term.

#### 4. Cosmos DB Optimization
```
Provisioned throughput pricing:
├─ 400 RU/s: $23.80/month per DB
├─ Shared autoscale (400-4000 RU/s): ~$20/month per DB

Recommendation:
├─ High-traffic (bookings): 600 RU/s ($35.80)
├─ Medium (events, crew): 400 RU/s ($23.80 each)
├─ Low (auth, users): 300 RU/s ($17.85 each)
└─ Total for 6 DBs: ~$165/month (vs $340 if all high)
```

**Savings:** ~$100-150/month through right-sizing

#### 5. Azure Savings Plans
```
- Commitment-based pricing (1 or 3 years)
- Better rates than reserved instances
- More flexible (covers compute across SKUs)
- Potential: 20-30% savings overall
```

---

## Complete Cost Comparison

### Development Phase
```
┌──────────────────────────────────────┐
│ OPTION: Local Docker Compose         │
├──────────────────────────────────────┤
│ Container Registry: $0               │
│ Local storage: $0 (you own machine)  │
│ Databases: $0 (local)                │
│ Message queue: $0 (local)            │
├──────────────────────────────────────┤
│ TOTAL: $0/month ✅                   │
├──────────────────────────────────────┤
│ Pros:                                │
│ • No cloud costs                     │
│ • Full developer control             │
│ • Fast feedback loop                 │
│ • Offline development possible       │
│                                      │
│ Cons:                                │
│ • Not shared with team               │
│ • Mobile testing harder              │
│ • Load testing limited               │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ OPTION: Azure Dev Environment        │
├──────────────────────────────────────┤
│ Container Registry: $5               │
│ 6 Container Instances: $24           │
│ Cosmos DB (Free tier): $0            │
│ App Insights: $7                     │
├──────────────────────────────────────┤
│ TOTAL: ~$36/month                    │
├──────────────────────────────────────┤
│ Pros:                                │
│ • Shared with team                   │
│ • Real Azure environment             │
│ • Mobile testing possible            │
│ • Load testing ready                 │
│                                      │
│ Cons:                                │
│ • Recurring cost                     │
│ • Slightly slower than local         │
│ • Resource limits (ACI)              │
└──────────────────────────────────────┘
```

### Production Phase
```
BASELINE (2 nodes, minimal setup):
├─ AKS Nodes (2 × B2s, 730 hrs/mo): $140/month
├─ AKS Load Balancer: $60/month
├─ Cosmos DB (6 DBs, 400 RU/s avg): $165/month
├─ Container Registry: $5/month
├─ Application Insights: $50/month
├─ RabbitMQ (in-cluster): $0/month
└─ TOTAL: ~$420/month

OPTIMIZED (with Spot VMs + Reserved + Auto-scaling):
├─ AKS Nodes (1 primary + 2 spot): $90/month
├─ AKS Load Balancer: $60/month
├─ Cosmos DB (optimized RU/s): $120/month
├─ Container Registry: $5/month
├─ Application Insights: $35/month
└─ TOTAL: ~$310/month (26% savings)

SCALED UP (3 peak nodes, higher RU/s):
├─ AKS Nodes (3 × B4ms, 730 hrs/mo): $350/month
├─ AKS Load Balancer: $60/month
├─ Cosmos DB (6 DBs, 500 RU/s avg): $200/month
├─ Container Registry: $10/month
├─ Application Insights: $75/month
└─ TOTAL: ~$695/month
```

---

## Migration Dev → Prod

### Step-by-Step Promotion Pipeline

```
1. LOCAL DEVELOPMENT
   ├─ Developer pushes to feature branch
   ├─ Tests locally with docker-compose
   └─ Creates Pull Request

2. GITHUB CI/CD
   ├─ Run unit tests
   ├─ Run integration tests
   ├─ Build Docker image
   ├─ Push to ACR (dev tag)
   └─ Deploy to Azure Container Instances (optional)

3. STAGING ENVIRONMENT (Optional)
   ├─ Same infrastructure as prod
   ├─ With production-like data volume
   ├─ Run smoke tests
   ├─ Performance tests
   └─ Security scanning

4. PRODUCTION
   ├─ Manual approval required
   ├─ Rolling deployment to AKS
   ├─ Health checks pass
   ├─ Canary rollout (10% → 50% → 100%)
   └─ Monitor for errors

5. ROLLBACK (if needed)
   ├─ Automated if error rate > 5%
   ├─ Or manual kubectl rollout undo
   └─ Alert on-call engineer
```

### GitHub Actions Workflow

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [main]
    paths:
      - 'src/**'
      - '.github/workflows/deploy.yml'

env:
  REGISTRY: thenorsoundacr.azurecr.io
  IMAGE_TAG: ${{ github.sha }}

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Login to ACR
      uses: azure/docker-login@v1
      with:
        login-server: ${{ env.REGISTRY }}
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
    
    - name: Build and push Docker images
      run: |
        docker build -t ${{ env.REGISTRY }}/auth-service:${{ env.IMAGE_TAG }} ./AuthService
        docker push ${{ env.REGISTRY }}/auth-service:${{ env.IMAGE_TAG }}
        # ... repeat for other services
    
    - name: Deploy to AKS
      uses: Azure/k8s-deploy@v4
      with:
        action: deploy
        kubeconfig: ${{ secrets.KUBE_CONFIG }}
        namespace: thenorsound
        manifests: |
          k8s/auth-service-deployment.yaml
          k8s/user-service-deployment.yaml
          # ... all other manifests
        images: |
          ${{ env.REGISTRY }}/auth-service:${{ env.IMAGE_TAG }}
          ${{ env.REGISTRY }}/user-service:${{ env.IMAGE_TAG }}
          # ... all other images
        imagepullsecrets: |
          regcred
    
    - name: Run smoke tests
      run: |
        kubectl run smoke-test --image=curlimages/curl:latest \
          --rm -i --restart=Never -- \
          curl http://api-gateway/health
    
    - name: Notify Slack
      if: always()
      uses: slackapi/slack-github-action@v1
      with:
        webhook-url: ${{ secrets.SLACK_WEBHOOK }}
        payload: |
          {
            "text": "Deployment to production: ${{ job.status }}",
            "blocks": [
              {
                "type": "section",
                "text": {
                  "type": "mrkdwn",
                  "text": "*Deployment Status*: ${{ job.status }}\nCommit: ${{ github.sha }}\nAuthor: ${{ github.actor }}"
                }
              }
            ]
          }
```

---

## Monitoring & Logging

### Application Insights Integration

**In each service (Program.cs):**
```csharp
// Add Application Insights
services.AddApplicationInsightsTelemetry(
    options => options.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
);

// Enable dependency tracking
services.AddApplicationInsightsKubernetesEnricher();

// Custom metrics
services.AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>();

// Logging
var logger = services.BuildServiceProvider()
    .GetRequiredService<ILogger<Program>>();
```

**Custom metrics (example):**
```csharp
public class BookingService
{
    private readonly TelemetryClient _telemetryClient;

    public BookingService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public async Task CreateBookingAsync(CreateBookingCommand command)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Create booking logic
            
            // Track success
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _telemetryClient.TrackEvent("BookingCreated", 
                new Dictionary<string, string> 
                { 
                    { "EventId", command.EventId },
                    { "CrewId", command.CrewId }
                },
                new Dictionary<string, double>
                {
                    { "Duration", duration },
                    { "Amount", booking.TotalAmount }
                }
            );
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            throw;
        }
    }
}
```

### Alerting Rules

```yaml
# Create alerts in Azure Portal or via Terraform
Alerts:
├─ Error Rate > 5%
│  └─ Action: PagerDuty, Email
├─ Response Time > 2s (p95)
│  └─ Action: Email (informational)
├─ CPU > 80%
│  └─ Action: Auto-scale up
├─ Memory > 85%
│  └─ Action: Auto-scale up
├─ Pod crash loop
│  └─ Action: PagerDuty, Slack
└─ Database connection pool exhausted
   └─ Action: PagerDuty
```

---

## Security Best Practices

### 1. Network Security
```
- Virtual Network (vnet) for AKS cluster
- Network Security Groups (NSG) - firewall rules
- Private endpoints for Cosmos DB
- Azure Firewall for egress filtering
- DDoS protection: Azure DDoS Protection Standard
```

### 2. Identity & Access
```
- Azure RBAC: Role-based access to Azure resources
- Kubernetes RBAC: Role-based access to cluster
- Managed Identity: AKS nodes → Azure resources (no keys!)
- Pod Identity: Services use managed identities
- Key Vault: Centralized secret management
```

### 3. Container Security
```
- Image scanning: Vulnerability scanning in ACR
- Pod Security Policies: Restrict dangerous practices
- RBAC: Least privilege for service accounts
- Network policies: Restrict pod communication
- Read-only filesystems: Prevent tampering
```

### 4. Data Protection
```
- Encryption at rest: Cosmos DB (default enabled)
- Encryption in transit: TLS 1.2+ everywhere
- Backup: Daily snapshots in Cosmos DB
- Key rotation: Regular update of secrets in Key Vault
- GDPR compliance: Data residency in EU (optional)
```

### Example Security-Hardened Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: secure-service
spec:
  template:
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 2000
        seccompProfile:
          type: RuntimeDefault
      
      containers:
      - name: app
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop: [ALL]
        
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        
        volumeMounts:
        - name: tmp
          mountPath: /tmp
        
        livenessProbe: # Detect dead pods
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        
        readinessProbe: # Detect not-ready pods
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
      
      volumes:
      - name: tmp
        emptyDir: {}

---
# Network Policy: Only allow traffic from API Gateway
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: secure-service-policy
spec:
  podSelector:
    matchLabels:
      app: secure-service
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 27017  # MongoDB
    - protocol: TCP
      port: 5672   # RabbitMQ
```

---

## Implementation Roadmap

### Phase 0: Pre-Production (Week 1-2)
- [ ] Create Azure subscription & resource groups
- [ ] Setup Terraform for IaC
- [ ] Create Azure Cosmos DB (Free tier for dev)
- [ ] Create Azure Container Registry
- [ ] Setup Application Insights
- [ ] Create Key Vault with secrets
- [ ] Setup GitHub Actions CI/CD pipeline

### Phase 1: Local Development (Week 3-4)
- [ ] Finalize docker-compose.yml
- [ ] Test all 6 services locally
- [ ] Setup monitoring locally (ELK or Application Insights)
- [ ] Performance testing locally

### Phase 2: Staging Environment (Week 5-6)
- [ ] Deploy to Azure Container Instances (optional)
- [ ] Run end-to-end tests
- [ ] Performance testing under load
- [ ] Security scanning

### Phase 3: Promote to Production (Week 7)
- [ ] Create AKS cluster (3 nodes)
- [ ] Deploy all services to AKS
- [ ] Migrate DNS to prod endpoints
- [ ] Monitor 24/7 for first week

### Phase 4: Optimization (Week 8+)
- [ ] Analyze costs & optimize
- [ ] Implement auto-scaling
- [ ] Implement Spot VMs if cost-justified
- [ ] Setup reserved instances if committed

---

## Quick Reference: Command Cheat Sheet

```bash
# Azure CLI
az account show                           # Current subscription
az group create -n rg-prod -l northeurope # Create resource group
az aks create -g rg-prod -n aks-prod     # Create AKS cluster
az acr create -g rg-prod -n acrprod      # Create container registry

# Terraform
terraform init                            # Initialize
terraform plan -out=tfplan               # Plan changes
terraform apply tfplan                   # Apply
terraform destroy                        # Destroy all resources

# Kubernetes
kubectl apply -f k8s/                    # Deploy all manifests
kubectl get pods -n thenorsound          # List pods
kubectl logs pod-name -n thenorsound     # View logs
kubectl describe pod pod-name            # Pod details
kubectl scale deployment app --replicas=3 # Scale deployment
kubectl rollout undo deployment/app      # Rollback

# Docker
docker-compose up -d                     # Start dev environment
docker-compose down                      # Stop dev environment
docker-compose logs -f service-name      # Follow logs
```

---

## Conclusion

**Development:** Start with **$0 local Docker Compose**, scale to $30-40/month with Azure as team grows.

**Production:** Start with ~$400/month (2-3 nodes), optimize to ~$300/month with reserved instances & auto-scaling, scale to $600-1000/month at higher load.

**Key advantages:**
✅ Cost-effective development phase  
✅ Seamless scaling from dev to prod  
✅ Full Kubernetes capabilities at scale  
✅ Azure-native monitoring & logging  
✅ Infrastructure as Code (Terraform)  
✅ Automated CI/CD pipeline  

---

**Genererad:** 2026-02-09  
**Version:** 1.0  
**Status:** Ready for Implementation
