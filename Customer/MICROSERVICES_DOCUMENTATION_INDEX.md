# 📚 ThenorSound Microservices Documentation Index

**Genererad:** 2026-02-09  
**Uppdaterad:** 2026-02-10

---

## 📋 Dokumentöversikt

Du har nu två kompletta specifikationsdokument:

### 1. **MICROSERVICES_ARCHITECTURE_SPEC.md** (Huvuddokument)
**Storlek:** ~30 000 ord  
**Format:** Markdown  
**Innehål:**
- ✅ Executive Summary
- ✅ Högnivå-arkitektur  
- ✅ Service Inventory (5 microservices)
  - Auth Service (Port 5001)
  - User Service (Port 5002) - Hanterar Customer & Crew users
  - Customer Service (Port 5003) - Hanterar EventOrganizers & CrewCompanies
  - Event Service (Port 5004)
  - Booking Service (Port 5005)
- ✅ Data Architecture & MongoDB Design
- ✅ Service Communication Patterns
- ✅ Migration Strategy (8 sprints)
- ✅ Deployment Architecture
- ✅ Integration Guide
- ✅ Development Standards
- ✅ Testing Standards

**Best för:** Utvecklare, arkitekter, project managers

### 2. **MICROSERVICES_ARCHITECTURE_DIAGRAMS.md** (Visuell guid)
**Storlek:** ~15 000 ord  
**Format:** Markdown + ASCII Diagrams  
**Innehål:**
- ✅ System Architecture Overview
- ✅ Request Flow - User Login
- ✅ Booking Creation Flow (Komplext cross-service)
- ✅ Authentication & Authorization Flow
- ✅ Data Consistency - Booking Saga
- ✅ Message Flow - Event Bus (RabbitMQ)
- ✅ Database Topology & Relationships
- ✅ Deployment Pipeline
- ✅ Error Handling & Retry Strategy
- ✅ User Role & Permission Matrix

**Best för:** Visuell överblick, flow-förståelse, presentation

### 3. **DATABASE_STRATEGY_SUMMARY.md** (⭐ NEW - Kostnadsoptimering)
**Storlek:** ~5 000 ord  
**Format:** Markdown   
**Innehål:**
- ✅ Problem: Azure Cosmos DB Free Tier = 1 DB (behövs 6)
- ✅ Rekommenderad lösning: Hybrid Dev/Prod Strategy
  - Dev: Local MongoDB (6 DBs, $0/month)
  - Prod: Azure Cosmos (1 DB + 6 collections, $0-35/month)
- ✅ Kostnadsanalys vs alternativ
- ✅ Implementering och migration
- ✅ Sparar $300-600/år vs naiv approach!

**Best för:** Kostnadsoptimering, produktionsplanering, database-val

### 4. **AZURE_INFRASTRUCTURE_GUIDE.md** (Deployment & DevOps)
**Storlek:** ~20 000 ord  
**Format:** Markdown + Terraform Code  
**Innehål:**
- ✅ Development environments (local Docker, Azure Container Instances)
- ✅ Database Strategy sektion (löser Cosmos DB-limitering)
- ✅ Production AKS setup (3 nodes, auto-scaling)
- ✅ Kubernetes manifests (Deployments, Services, Ingress, NetworkPolicy)
- ✅ Terraform Infrastructure as Code
- ✅ Cost optimization strategies (spot VMs, reserved instances, auto-scaling)
- ✅ Monitoring & Logging (Application Insights)

**Best för:** DevOps, infrastrukturplanering, drift

---

## 🎯 Hur du använder dessa dokument

### För utvecklare:
```
Start här:
1. Läs MICROSERVICES_ARCHITECTURE_SPEC.md → Executive Summary
2. Läs relevant Service-sektion (ex. Auth Service)
3. Titta på motsvarande diagram i DIAGRAMS.md
4. Implementera enligt Development Standards-sektionen
```

### För arkitekter:
```
1. MICROSERVICES_ARCHITECTURE_SPEC.md → Helhetsbild
2. MICROSERVICES_ARCHITECTURE_DIAGRAMS.md → Arkitektur-diagrammen
3. Data Architecture-sektionen → Databasdesign
4. Service Communication Patterns → Integration-strategi
```

### För project managers:
```
1. MICROSERVICES_ARCHITECTURE_SPEC.md → Executive Summary
2. Migration Strategy-sektionen → Sprint-planerare
3. Deployment Architecture → Release-tidsplan
```

---

## 📈 Migration Timeline (5-Service Architecture)

```
Sprint 1-2:   Auth Service + User Service (incl. crew as userType: 3)
Sprint 3-4:   Customer Service (incl. CrewCompanies) + Event Service
Sprint 5-6:   Booking Service (single combined sprint - most complex)
Sprint 7:     API Gateway + Frontend Integration
────────────────────────────────────────────────
Total:        7 sprints (ca 3-4 månader)

NOTE: NO SEPARATE CREW SERVICE
      Crew members = Users (userType: 3) in User Service
      CrewCompanies = Customers (customerType: 2) in Customer Service
```

---

## 🔗 Service Dependencies Map

```
Auth Service
  ├─ Depends on: MongoDB (thenorsound DB)
  └─ Used by: API Gateway, (all services validate tokens)

User Service (Hanterar ALL users: Admin, Customer, AND Crew members!)
  ├─ UserType: 1 = Admin
  ├─ UserType: 2 = Customer
  ├─ UserType: 3 = Crew (with extended properties: skills, SSN, bank, availability)
  ├─ Depends on: MongoDB (thenorsound DB), Auth Service
  └─ Used by: Booking Service (crew availability checks)
  ├─ Depends on: MongoDB, Auth Service
  ├─ Entities: userType: 1 (Admin), 2 (Customer), 3 (Crew)
  └─ Used by: API Gateway, Booking Service (crew members)

Customer Service (Hanterar EventOrganizers & CrewCompanies)
  ├─ Depends on: MongoDB
  ├─ Entities: customerType: 1 (EventOrganizer), 2 (CrewCompany)
  └─ Used by: User Service, Event Service, Booking Service (tariffs)

Event Service
  ├─ Depends on: MongoDB, Customer Service
  └─ Used by: Booking Service

Booking Service
  ├─ Depends on: Event Service, User Service (crew), Customer Service (tariffs)
  ├─ Pricing: Matchar crew skills med customer tariffs
  └─ Publishes events to: RabbitMQ (Notification, Analytics, Invoice)
```

---

## 📊 Database Strategy (Single Database - Dev & Prod)

**Both Development & Production Use Same Structure:**
| Environment | Database | Collections | Port | Cost |
|----------|----------|------------|------|------|
| Dev (Local) | thenorsound | 5 collections | 27017 | $0/month |
| Prod (Azure Cosmos) | thenorsound | 5 collections | N/A | $0-50/month |

**Collections (Same in Both):**
| Collection | Service | Entities | TTL Enabled |
|-----------|---------|----------|-------------|
| auth_refreshTokens | Auth Service | JWT refresh tokens | ✓ Yes (7 days) |
| users | User Service | Admin, Customer users, Crew members (userType: 1/2/3) | ✗ No |
| customers | Customer Service | EventOrganizers + CrewCompanies (customerType: 1/2) | ✗ No |
| events | Event Service | Events | ✗ No |
| bookings | Booking Service | Bookings + pricing | ✗ No |

**Strategy:** 
- **Dev:** 1 local MongoDB with "thenorsound" database + 5 collections
- **Prod:** 1 Azure Cosmos with "thenorsound" database + 5 collections (identical!)
- **Migration:** Connection string only (zero code changes!)
- **Code:** No changes needed - MongoDB driver abstracts collections perfectly
- **Savings:** $300-600/year vs naive 6-separate-Cosmos-DB approach
- **Bonus:** Perfect dev/prod parity = fewer deployment bugs

**Key Architectural Insights:**
- ❌ NO separate Crew Service - Crew members are Users (userType: 3)
- ✅ Customer Service handles BOTH company types (customerType: 1 & 2)
- ✅ Tariffs stored in Customer (type 1 only) - crew pricing uses these tariffs
- ✅ Crew members have skills array - matched against customer tariffs for pricing

**Full details:** See [DATABASE_STRATEGY_SUMMARY.md](DATABASE_STRATEGY_SUMMARY.md) ⭐

---

## 🚀 Getting Started Checklist

### Phase 0: Setup (Pre-Sprint 1)
- [ ] Review MICROSERVICES_ARCHITECTURE_SPEC.md (5 services, NOT 6!)
- [ ] Review MICROSERVICES_ARCHITECTURE_DIAGRAMS.md (crew integrated)
- [ ] **Review DATABASE_STRATEGY_SUMMARY.md** ⭐ (Single DB strategy!)
- [ ] Setup local Docker Compose with 1 MongoDB container (thenorsound DB)
- [ ] Verify docker-compose.yml has 5 services ONLY (no Crew Service!)
- [ ] Setup Docker & Docker Compose environment
- [ ] Create CI/CD pipeline (GitHub Actions)
- [ ] Setup monitoring (Application Insights)
- [ ] **FUTURE (Sprint 7+):** Setup Azure Cosmos DB (Free tier, 1 DB + 5 collections)

### Phase 1: Auth & User Services (Sprint 1-2)
- [ ] Create Auth Service project structure
- [ ] Implement LoginCommand → JWT tokens
- [ ] Implement RefreshTokenCommand
- [ ] Create User Service project structure
- [ ] **Implement User CRUD for ALL user types:**
  - [ ] Admin (userType: 1)
  - [ ] Customer (userType: 2)
  - [ ] Crew members (userType: 3) ← NEW: Crew is a user type!
- [ ] Implement crew member extended properties (skills, SSN, bank info, availability)
- [ ] Implement crew-specific endpoints (availability queries)
- [ ] Implement integration tests
- [ ] Deploy to dev environment

### Phase 2: Customer & Event Services (Sprint 3-4)
- [ ] Create Customer Service
- [ ] **Implement both customerTypes:**
  - [ ] EventOrganizer (customerType: 1) - has tariffs
  - [ ] CrewCompany (customerType: 2) - manages crew members
- [ ] Implement tariff management (ONLY for EventOrganizers)
- [ ] Create Event Service
- [ ] Implement event CRUD
- [ ] Integrate services with User Service
- [ ] Deploy to dev

### Phase 3: Booking Service (Sprint 5-6) - COMBINED SPRINT
- [ ] Create Booking Service (most complex service)
- [ ] **Implement cross-service calls:**
  - [ ] User Service (verify crew + check availability)
  - [ ] Event Service (verify event exists)
  - [ ] Customer Service (get tariffs)
- [ ] **Implement pricing logic:**
  - [ ] Match crew skills to customer tariffs
  - [ ] Calculate: tariff × duration = totalAmount
- [ ] Implement booking saga pattern for distributed transactions
- [ ] Deploy to dev

### Phase 4: API Gateway & Integration (Sprint 7)
- [ ] Create API Gateway project (YARP)
- [ ] Implement routing to 5 services (NOT 6!)
- [ ] Implement JWT validation middleware
- [ ] Update React frontend to use Gateway
- [ ] Test all endpoints

### Phase 5: Event Bus & Async (Sprint 8+)
- [ ] Setup RabbitMQ
- [ ] Implement message publishing in services
- [ ] Create Notification Service (future)
- [ ] Create Analytics Service (future)
- [ ] Setup monitoring & alerting

---

## 💡 Key Architectural Principles

### 1. Clean Architecture
```
API (Controllers, DTOs)
    ↓
Application (Commands, Queries, Handlers)
    ↓
Domain (Entities, Models, Interfaces)
    ↓
Infrastructure (Repositories, External Services)
```

### 2. CQRS (Command Query Responsibility)
```
Commands (Write): Create*, Update*, Delete*
Queries (Read): Get*, List*
```

### 3. DDD (Domain-Driven Design)
```
Each service models its own domain
Services communicate via APIs (not databases)
Bounded contexts clearly defined
```

### 4. MediatR Pattern
```
Controller → Request (Command/Query)
### 5. Saga Pattern for Distributed Transactions
```
CreateBookingCommand
  ├─ Verify Event exists (Event Service)
  ├─ Verify Crew availability (User Service - userType: 3)
  ├─ Get Tariffs & Calculate Price (Customer Service)
  ├─ Commit Booking
  └─ Compensation: rollback if any step fails
```

---

## 📞 Inter-Service Communication Summary

### Synchronous (REST)
```
Booking Service ─→ Event Service (verify event)
                 ─→ User Service (check crew availability)
                 ─→ Customer Service (get tariffs for pricing)
```

**When to use:** Need real-time response, validation, data enrichment

**Timeout:** 5 seconds  
**Retries:** 3 attempts with exponential backoff

### Asynchronous (Message Broker)
```
Booking Service ─→ RabbitMQ ─→ Notification Service
                              ─→ Analytics Service
                              ─→ Invoice Service
```

**When to use:** Don't need immediate response, fire-and-forget events

**Queue:** Durable, persisted  
**Retry:** Max 3 redeliveries, then DLQ

---

## 🔒 Security Considerations

### Authentication
- JWT tokens (15 min expiry)
- Refresh tokens (7 days, HTTP-only cookie)
- Token stored in secure storage

### Authorization
- Role-based access control (RBAC)
- Resource-level access checks
- CustomerId validation for data isolation

### Communication
- HTTPS only
- API Gateway validates all tokens
- Services trust X-User-Id header from Gateway

### Database
- No public internet access
- Connection string in Azure Key Vault
- User Secrets for local development

---

## 📝 Code Standards Summary

### Naming Conventions
```
Commands:     CreateUserCommand, UpdateEventCommand
Queries:      GetUserQuery, ListEventsQuery
Handlers:     CreateUserCommandHandler, GetUserQueryHandler
Entities:     UserEntity, EventEntity
Models:       User, Event
DTOs:         CreateUserRequestDto, UserResponseDto
Exceptions:   NotFoundException, ValidationException
```

### File Structure
```
Service/
├── API/
│   ├── Controllers/
│   ├── DTOs/
│   │   ├── Request/
│   │   └── Response/
│   └── Exceptions/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Clients/
│   └── Validators/
├── Domain/
│   ├── Entities/
│   ├── Models/
│   ├── Repositories/
│   └── Enums/
├── Infrastructure/
│   ├── Repositories/
│   ├── Clients/
│   └── Settings/
└── Tests/
    ├── Unit/
    ├── Integration/
    └── Fixtures/
```

### Testing Requirements
- **Unit Tests:** Command/Query Handlers, Business Logic
- **Integration Tests:** Controllers, Cross-service Calls
- **Coverage:** Minimum 80%
- **Framework:** xUnit + Moq

---

## 🎓 Learning Resources to Include

For implementation, team members should be familiar with:

1. **CQRS Pattern**
   - Commands vs Queries separation
   - MediatR library usage
   - Handler implementation

2. **Clean Architecture**
   - Dependency inversion
   - Separation of concerns
   - Layer responsibilities

3. **MongoDB**
   - Document structure
   - Indexing strategy
   - Connection pooling

4. **Docker**
   - Containerization
   - Docker Compose
   - Container networking

5. **.NET Best Practices**
   - Async/await patterns
   - Dependency injection
   - Error handling

6. **Testing**
   - Unit testing with xUnit
   - Mocking with Moq
   - Integration testing

---

## 📞 Contact & Support

For questions about this architecture:

1. **Spec Questions:** See MICROSERVICES_ARCHITECTURE_SPEC.md
2. **Flow Questions:** See MICROSERVICES_ARCHITECTURE_DIAGRAMS.md
3. **Implementation:** Reference Development Standards section
4. **Migration Planning:** Reference Migration Strategy section

---

## 📄 Vilka dokument finns?

### 1. **MICROSERVICES_ARCHITECTURE_SPEC.md** ⭐
   - Main specification (~30k ord)
   - 6 microservices full spec
   - CQRS, Commands, Queries
   - Development standards

### 2. **MICROSERVICES_ARCHITECTURE_DIAGRAMS.md** 📊
   - Visual flows & diagrams (~15k ord)
   - Request flows, data consistency
   - Message broker patterns
   - Permission matrix

### 3. **DATABASE_STRATEGY_SUMMARY.md** 💰 (⭐ NEW!)
   - Database cost optimization (~5k ord)
   - **Problem:** Cosmos DB free tier = 1 DB (need 6)
   - **Solution:** Hybrid Dev/Prod with Cosmos Collections
   - **Savings:** $300-600/year vs naive approach
   - Dev: $0 (local MongoDB × 6)
   - Prod: $0-35 (Cosmos × 1 DB + 6 collections)

### 4. **AZURE_INFRASTRUCTURE_GUIDE.md** ☁️
   - Azure-specific deployment (~20k ord)
   - **Dev: $0 (local) → $30 (Azure Container Instances)**
   - **Prod: $320-450/month (with Hybrid DB Strategy)**
   - Terraform IaC
   - Cost optimization
   - Database Strategy section (updated!)

### 5. **MICROSERVICES_DOCUMENTATION_INDEX.md**
   - This file - navigation guide

---

## 📄 Dokument Versioning

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-02-09 | Initial specification |
| | | - 6 microservices defined |
| | | - Complete migration path |
| | | - All deployment details |
| 1.1 | 2026-02-09 | Azure infrastructure added |
| | | - AKS deployment guide |
| | | - Cost optimization strategies |
| | | - Terraform IaC examples |
| 1.2 | 2026-02-09 | **Database Strategy optimization** ⭐ |
| | | - NEW: DATABASE_STRATEGY_SUMMARY.md |
| | | - Hybrid Dev/Prod approach |
| | | - Saves $300-600/year |
| | | - Production cost reduced to $320-450/month |
| TBD | TBD | Updates after Phase 1 |

---

## 📄 How to Convert to PDF

### Option 1: GitHub Markdown to PDF
```bash
# Install pandoc
choco install pandoc

# Convert markdown to PDF
pandoc MICROSERVICES_ARCHITECTURE_SPEC.md -o MICROSERVICES_ARCHITECTURE_SPEC.pdf
pandoc MICROSERVICES_ARCHITECTURE_DIAGRAMS.md -o MICROSERVICES_ARCHITECTURE_DIAGRAMS.pdf
```

### Option 2: VS Code Extension
- Install "Markdown PDF" extension
- Right-click → Markdown PDF → Export PDF

### Option 3: Online Converters
- https://md-to-pdf.herokuapp.com/
- https://markdowntopdf.com/

### Option 4: Print to PDF
- Open in browser
- Cmd+P (macOS) or Ctrl+P (Windows)
- Select "Save as PDF"

---

## ✅ Next Steps

1. **Review Database Strategy:** Read DATABASE_STRATEGY_SUMMARY.md first! ⭐
   - Understand the Hybrid Dev/Prod approach
   - Save $300-600/year vs naive approach
   - Production cost: $320-450/month (not $600-1000!)

2. **Review Architecture:** Go through spec and diagrams with team

3. **Approve:** Get stakeholder sign-off on architecture

4. **Setup:** Configure development environment
   - Start with docker-compose.yml (local MongoDB)
   - 6 separate databases for development
   - Cost: $0/month

5. **Start Implementation:** Begin Sprint 1 with Auth Service

6. **Plan Production:** 
   - Sprint 7: Switch to Azure Cosmos (1 DB + 6 collections)
   - Connection string change only (2 lines!)
   - Cost remains $0-35/month in free tier

7. **Iterate:** Update docs as you implement


---

**Documentation Complete!** 🎉

All documents are located in:
```
c:\Users\emfr\ThenorSound\thenorsound\

├── MICROSERVICES_ARCHITECTURE_SPEC.md (Main spec: ~30k words)
├── MICROSERVICES_ARCHITECTURE_DIAGRAMS.md (Diagrams: ~15k words)
├── DATABASE_STRATEGY_SUMMARY.md ⭐ (Cost optimization: ~5k words) NEW!
├── AZURE_INFRASTRUCTURE_GUIDE.md (Deployment & DevOps: ~20k words)
└── MICROSERVICES_DOCUMENTATION_INDEX.md (This file - Navigation)
```

**Total Documentation:** ~70k words of comprehensive microservices architecture 📚

**Cost Analysis with Single Database Strategy:**
```
Development:  $0/month (local Docker Compose - 1 DB, 6 collections)
Production:   $320-450/month (Cosmos + AKS - down from $600-1,100!)
Year 1:       ~$3,840-5,400 (vs original $7,200-13,200)

SAVES you $3,600-9,600 in Year 1! 💰

Bonus:
  ├─ Perfect dev/prod parity
  ├─ Trivial migration (connection string only)
  ├─ Zero code changes between environments
  └─ Fewer deployment bugs
```

Lycka till med implementationen! 🚀
