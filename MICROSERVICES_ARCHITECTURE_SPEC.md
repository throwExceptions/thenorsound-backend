# 🎵 ThenorSound Microservices Architecture Specification

**Dokument:** Microservices Architecture Design  
**Version:** 1.0  
**Datum:** 2026-02-09  
**Status:** Ready for Implementation  
**Projekt:** ThenorSound Sound Event Management Platform

---

## 📋 Innehållsförteckning

1. [Executive Summary](#executive-summary)
2. [Högnivå-arkitektur](#högnivå-arkitektur)
3. [Service Inventory](#service-inventory)
4. [Data Architecture](#data-architecture)
5. [Service Communication Patterns](#service-communication-patterns)
6. [Migration Strategy](#migration-strategy)
7. [Deployment Architecture](#deployment-architecture)
8. [Integration Guide](#integration-guide)
9. [Development Standards](#development-standards)

---

## Executive Summary

ThenorSound-plattformen migreras från monolitisk React-frontend + backend-API till en **microservices-arkitektur** med följande fördelar:

- **Oberoende skalning:** Varje service kan skalas separat
- **Parallell utveckling:** Teams kan arbeta på olika services
- **Enkel deployment:** Deploy services utan att påverka andra
- **Teknologisk flexibilitet:** Framtida byten av teknik per service
- **Klarare ansvarsområden:** Separation of Concerns

**Målarkitektur (5 Services - Crew Integrated):**
```
API Gateway (5000)
    ↓
[Auth] [User] [Customer] [Event] [Booking]
 5001   5002    5003      5004    5005
    ↓     ↓        ↓        ↓      ↓
Single MongoDB: "thenorsound"
Collections: auth_refreshTokens, users, customers, events, bookings

KEY CHANGE: Crew members are Users (userType: 3)
             CrewCompanies are Customers (customerType: 2)
```

---

## Högnivå-arkitektur

### System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     Frontend (React)                            │
│                  soundflow/ (JavasScript)                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                   API-Gateway (5000)                            │
│  - Request routing                                              │
│  - Auth validation (JWT tokens)                                 │
│  - Rate limiting                                                │
│  - Request logging                                              │
│  - CORS handling                                                │
└─────────────────────────────────────────────────────────────────┘
            ↓          ↓          ↓           ↓
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Auth Svc     │ │ User Svc     │ │ Customer Svc │ │ Event Svc    │
│ Port: 5001   │ │ Port: 5002   │ │ Port: 5003   │ │ Port: 5004   │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
       ↓               ↓                ↓               ↓
   Auth DB         User DB          Customer DB      Event DB
    (MongoDB)      (MongoDB)         (MongoDB)       (MongoDB)
                                      (includes both  (includes tariffs)
                                   EventOrganizers &
                                   CrewCompanies)

            ↓
┌──────────────┐  ┌──────────────────┐
│ Booking Svc  │  │ RabbitMQ/Kafka   │
│ Port: 5005   │  │ (Message Broker) │
└──────────────┘  └──────────────────┘
       ↓
 Booking DB
  (MongoDB)
```

---

## Service Inventory

### 1. Auth Service (Port 5001)

**Ansvar:**
- Användarautentisering (login/logout)
- JWT token generation & validation
- Token refresh logic
- OAuth integration (framtid)
- Session management

**Domain Entities:**
```csharp
// Domain/Entities/RefreshTokenEntity.cs
- Id: string (MongoDB ObjectId)
- UserId: string
- Token: string
- ExpiresAt: DateTime
- CreatedAt: DateTime
- UpdatedAt: DateTime

// Domain/Models/AuthToken.cs
- AccessToken: string
- RefreshToken: string
- ExpiresIn: int
- TokenType: "Bearer"
```

**CQRS Operations:**

**Commands:**
```csharp
LoginCommand
  ├── Email: string
  ├── Password: string
  └── Returns: LoginResponse { User, AccessToken, RefreshToken }

RefreshTokenCommand
  ├── RefreshToken: string
  └── Returns: TokenResponse { AccessToken, RefreshToken, ExpiresIn }

LogoutCommand
  ├── UserId: string
  └── Returns: SuccessResponse

RegisterCommand
  ├── Email: string
  ├── Password: string
  ├── FirstName: string
  ├── LastName: string
  └── Returns: UserResponse
```

**Queries:**
```csharp
ValidateTokenQuery
  ├── Token: string
  └── Returns: TokenValidationResponse { IsValid, UserId, Claims }

GetClaimsQuery
  ├── Token: string
  └── Returns: ClaimsResponse { UserId, Email, Roles }
```

**API Endpoints:**
```
POST   /api/auth/login              → LoginCommand
POST   /api/auth/refresh            → RefreshTokenCommand
POST   /api/auth/logout             → LogoutCommand
POST   /api/auth/register           → RegisterCommand
POST   /api/auth/validate           → ValidateTokenQuery
GET    /api/auth/claims             → GetClaimsQuery
```

**Dependencies:**
- User Service API (verifiera user exists)
- MongoDB (store refresh tokens)
- Serilog (logging)

**Security:**
- Hash passwords with bcrypt
- Use HTTPS only
- HTTP-only cookies för refresh tokens (recommended)
- Token expiration: 15 min (access), 7 days (refresh)

---

### 2. User Service (Port 5002)

**Ansvar:**
- User profile management (CRUD)
- User roles & permissions
- User-Customer relationships
- User preferences
- Access control lists

**Domain Entities:**
```csharp
// Domain/Entities/UserEntity.cs
- Id: string
- Email: string (unique)
- FirstName: string (for userType 2)
- LastName: string (for userType 2)
- Name: string (for userType 3 - crew members)
- UserType: enum (1=Admin, 2=Customer, 3=Crew)
- CustomerId: string (FK → Customer Service)
- IsActive: bool
- CreatedAt: DateTime
- UpdatedAt: DateTime

// EXTENDED PROPERTIES for userType: 3 (Crew Members)
- Skills: List<CrewSkill> (only for crew)
  ├── Category: enum (Musician, Technician, etc.)
  ├── Skill: enum (Drums, Guitar, Vocals, Mixing, etc.)
  ├── YearsOfExperience: int
- SocialSecurityNumber: string (only for crew - encrypted)
- BankAccount: BankAccountInfo (only for crew - encrypted)
  ├── AccountNumber: string
  ├── BankCode: string
  ├── HolderName: string
- References: string (only for crew)
- Availability: List<TimeSlot> (only for crew)
  ├── StartDate: DateTime
  ├── EndDate: DateTime
  ├── IsAvailable: bool

// Domain/Enums/UserType.cs
enum UserType
{
  Admin = 1,
  Customer = 2,
  Crew = 3
}

// Domain/Enums/CrewSkill.cs
public class CrewSkill
{
  public string Category { get; set; } // Musician, Technician, Sound Engineer
  public string Skill { get; set; }    // Drums, Guitar, Vocals, Mixing, etc.
  public int YearsOfExperience { get; set; }
  public ProficiencyLevel Proficiency { get; set; }
}
```

**CQRS Operations:**

**Commands:**
```csharp
CreateUserCommand
  ├── Email: string
  ├── FirstName: string (optional - for userType 2)
  ├── LastName: string (optional - for userType 2)
  ├── Name: string (optional - for userType 3 crew)
  ├── UserType: enum (1=Admin, 2=Customer, 3=Crew)
  ├── CustomerId: string (required for type 2&3)
  ├── Skills: List<CrewSkill> (only for userType 3)
  ├── SocialSecurityNumber: string (only for userType 3, encrypted)
  ├── BankAccount: BankAccountInfo (only for userType 3, encrypted)
  └── Returns: UserResponse

UpdateUserCommand
  ├── Id: string
  ├── FirstName: string (optional)
  ├── LastName: string (optional)
  ├── Name: string (optional - for crew)
  ├── Skills: List<CrewSkill> (optional - for crew)
  ├── Availability: List<TimeSlot> (optional - for crew)
  └── Returns: UserResponse

DeleteUserCommand
  ├── Id: string
  └── Returns: SuccessResponse

UpdateCrewAvailabilityCommand (NEW - for crew members)
  ├── UserId: string
  ├── StartDate: DateTime
  ├── EndDate: DateTime
  ├── IsAvailable: bool
  └── Returns: AvailabilityResponse
```

**Queries:**
```csharp
GetUserQuery
  ├── UserId: string
  └── Returns: UserResponse

GetUsersByCustomerIdQuery
  ├── CustomerId: string
  └── Returns: List<UserResponse>

GetAllUsersQuery
  ├── PageNumber: int (optional)
  ├── PageSize: int (optional)
  └── Returns: PaginatedResponse<UserResponse>

GetUserPermissionsQuery
  ├── UserId: string
  └── Returns: PermissionsResponse
```

**API Endpoints:**
```
GET    /api/users/{id}              → GetUserQuery
GET    /api/users                   → GetAllUsersQuery
GET    /api/users/customer/{customerId} → GetUsersByCustomerIdQuery
GET    /api/users/crew              → GetCrewMembersQuery (filter userType: 3)
GET    /api/users/{id}/availability → GetCrewAvailabilityQuery
POST   /api/users                   → CreateUserCommand
PUT    /api/users/{id}              → UpdateUserCommand
DELETE /api/users/{id}              → DeleteUserCommand
PUT    /api/users/{id}/availability → UpdateCrewAvailabilityCommand
```

**Dependencies:**
- Auth Service (validate auth tokens)
- Customer Service (verify CustomerId exists)
- MongoDB (store users)

**Data Validation:**
- Email: unique, valid format
- FirstName/LastName: 2-150 chars
- CustomerId: must exist in Customer Service

---

### 3. Customer Service (Port 5003)

**Ansvar:**
- Company/Customer profile (CRUD)
- Company details (org number, contact info)
- Tariff management (taxa för olika roller/skickligheter)
- Company settings
- Address management

**Domain Entities:**
```csharp
// Domain/Entities/CustomerEntity.cs
- Id: string
- Name: string
- OrgNumber: string (unique)
- Address: string
- City: string
- ZipCode: string
- Email: string
- Phone: string
- CustomerType: enum (1=EventOrganizer, 2=CrewCompany)
- Tariffs: List<Tariff> (only for customerType: 1 EventOrganizer)
  ├── Id: string
  ├── Category: string
  ├── Skill: string
  ├── TimeType: enum (Hour, Day, Event)
  ├── Amount: decimal (SEK per unit)
  └── Currency: string ("SEK")
- IsActive: bool
- CreatedAt: DateTime
- UpdatedAt: DateTime

// Domain/Enums/CustomerType.cs
enum CustomerType 
{ 
  EventOrganizer = 1,  // Books crew, has tariffs
  CrewCompany = 2      // Manages crew (users with userType: 3), no tariffs
}

// Domain/Models/TariffModel.cs
Used by: EventOrganizers to define pricing for different crew skills
Example matching logic in Booking Service:
{
  category: "Musician",
  skill: "Drums",
  timeType: "Hour",
  amount: 500  // SEK per hour
}
```

**CQRS Operations:**

**Commands:**
```csharp
CreateCustomerCommand
  ├── Name: string
  ├── OrgNumber: string
  ├── Address: string
  ├── City: string
  ├── ZipCode: string
  ├── Email: string
  ├── Phone: string
  ├── CustomerType: enum (1=EventOrganizer, 2=CrewCompany)
  └── Returns: CustomerResponse

UpdateCustomerCommand
  ├── Id: string
  ├── Name: string (optional)
  ├── Address: string (optional)
  ├── Email: string (optional)
  ├── Phone: string (optional)
  └── Returns: CustomerResponse

CreateTariffCommand (only for customerType: 1 - EventOrganizer)
  ├── CustomerId: string
  ├── Category: string (Musician, Technician, etc.)
  ├── Skill: string (Drums, Guitar, Vocals, Mixing, etc.)
  ├── TimeType: enum (Hour, Day, Event)
  ├── Amount: decimal
  └── Returns: TariffResponse

UpdateTariffCommand
  ├── TariffId: string
  ├── Amount: decimal (optional)
  └── Returns: TariffResponse

DeleteCustomerCommand
  ├── Id: string
  └── Returns: SuccessResponse
```

**Queries:**
```csharp
GetCustomerQuery
  ├── CustomerId: string
  └── Returns: CustomerResponse { Customer, Tariffs, Users (from User Service) }

GetTariffsByCustomerQuery
  ├── CustomerId: string
  └── Returns: List<TariffResponse>

GetCompanyDetailsQuery
  ├── CustomerId: string
  └── Returns: CompanyDetailsResponse

GetAllCustomersQuery
  ├── PageNumber: int (optional)
  ├── PageSize: int (optional)
  ├── CustomerType: enum (optional filter)
  └── Returns: PaginatedResponse<CustomerResponse>
```

**API Endpoints:**
```
GET    /api/customers/{id}          → GetCustomerQuery
GET    /api/customers               → GetAllCustomersQuery
POST   /api/customers               → CreateCustomerCommand
PUT    /api/customers/{id}          → UpdateCustomerCommand
DELETE /api/customers/{id}          → DeleteCustomerCommand
GET    /api/customers/{id}/tariffs  → GetTariffsByCustomerQuery
POST   /api/customers/{id}/tariffs  → CreateTariffCommand
PUT    /api/tariffs/{tariffId}      → UpdateTariffCommand
GET    /api/customers/{id}/details  → GetCompanyDetailsQuery
```

**Inter-Service Communication:**
```
GetCustomerQuery (include users):
  POST /api/users/customer/{customerId} → User Service
  Mappa results → CustomerResponse.Users
```

**Dependencies:**
- User Service (fetch users for customer)
- MongoDB (store customers & tariffs)

---

### 4. Event Service (Port 5004)

**Ansvar:**
- Event management (CRUD)
- Event scheduling
- Event status tracking
- Event details (performer, stage, equipment)
- Calendar/timeline

**Domain Entities:**
```csharp
// Domain/Entities/EventEntity.cs
- Id: string
- CustomerId: string (FK → Customer Service)
- Name: string
- Description: string
- StartDate: DateTime
- EndDate: DateTime
- Location: string
- EventType: enum (Concert, Festival, Corporate, etc.)
- Status: EventStatus enum (Draft, Scheduled, InProgress, Completed, Cancelled)
- Details: EventDetails object
  ├── Performer: string
  ├── Stage: string
  ├── Audience: int
  ├── Equipment: List<string>
- CreatedAt: DateTime
- UpdatedAt: DateTime

// Domain/Enums/EventStatus.cs
enum EventStatus { Draft = 1, Scheduled = 2, InProgress = 3, Completed = 4, Cancelled = 5 }
```

**CQRS Operations:**

**Commands:**
```csharp
CreateEventCommand
  ├── CustomerId: string
  ├── Name: string
  ├── Description: string
  ├── StartDate: DateTime
  ├── EndDate: DateTime
  ├── Location: string
  ├── EventType: enum
  ├── Details: EventDetailsDto
  └── Returns: EventResponse

UpdateEventCommand
  ├── Id: string
  ├── Name: string (optional)
  ├── Description: string (optional)
  ├── StartDate: DateTime (optional)
  ├── EndDate: DateTime (optional)
  ├── Location: string (optional)
  ├── Details: EventDetailsDto (optional)
  └── Returns: EventResponse

UpdateEventStatusCommand
  ├── Id: string
  ├── NewStatus: EventStatus
  └── Returns: EventResponse

DeleteEventCommand
  ├── Id: string
  └── Returns: SuccessResponse → Publishes: EventDeleted event
```

**Queries:**
```csharp
GetEventQuery
  ├── EventId: string
  └── Returns: EventResponse { Event, AssignedCrew (from Crew Service) }

GetEventsByCustomerQuery
  ├── CustomerId: string
  ├── PageNumber: int (optional)
  ├── PageSize: int (optional)
  └── Returns: PaginatedResponse<EventResponse>

GetEventsByDateRangeQuery
  ├── CustomerId: string
  ├── StartDate: DateTime
  ├── EndDate: DateTime
  └── Returns: List<EventResponse>

GetEventCalendarQuery
  ├── CustomerId: string
  ├── Month: int
  ├── Year: int
  └── Returns: CalendarResponse { Events by day }
```

**API Endpoints:**
```
GET    /api/events/{id}             → GetEventQuery
GET    /api/customers/{cid}/events  → GetEventsByCustomerQuery
GET    /api/events/calendar         → GetEventCalendarQuery
GET    /api/events/date-range       → GetEventsByDateRangeQuery
POST   /api/events                  → CreateEventCommand
PUT    /api/events/{id}             → UpdateEventCommand
PUT    /api/events/{id}/status      → UpdateEventStatusCommand
DELETE /api/events/{id}             → DeleteEventCommand
```

**Domain Events:**
```csharp
// For Booking Service to listen
EventCreated
  - EventId: string
  - CustomerId: string
  - Name: string
  - StartDate: DateTime
  - CreatedAt: DateTime

EventUpdated
  - EventId: string
  - UpdatedAt: DateTime

EventDeleted
  - EventId: string
  - DeletedAt: DateTime
```

**Dependencies:**
- Customer Service (validate CustomerId)
- Booking Service (listen for crew assignments)
- MongoDB (store events)

---

### 5. Booking Service (Port 5005)

**Ansvar:**
- Booking management (create, confirm, cancel)
- Crew-Event assignments
- Pricing calculation (från tariffs)
- Invoice generation
- Booking status tracking

**Domain Entities:**
```csharp
// Domain/Entities/BookingEntity.cs
- Id: string
- EventId: string (FK → Event Service)
- CrewId: string (FK → Crew Service)
- CustomerId: string (FK → Customer Service)
- BookingDate: DateTime
- StartTime: DateTime
- EndTime: DateTime
- Status: BookingStatus enum (Requested, Confirmed, Completed, Cancelled)
- Rate: decimal (calculated from tariff)
- TotalAmount: decimal
- Notes: string
- CreatedAt: DateTime
- UpdatedAt: DateTime

// Domain/Entities/InvoiceEntity.cs
- Id: string
- BookingId: string (FK)
- InvoiceNumber: string (unique)
- CustomerId: string
- Amount: decimal
- VatAmount: decimal
- TotalAmount: decimal
- IssuedDate: DateTime
- DueDate: DateTime
- PaidDate: DateTime (optional)
- Status: InvoiceStatus enum (Draft, Issued, Paid, Overdue, Cancelled)

// Domain/Enums/BookingStatus.cs
enum BookingStatus { Requested = 1, Confirmed = 2, Completed = 3, Cancelled = 4 }

// Domain/Enums/InvoiceStatus.cs
enum InvoiceStatus { Draft = 1, Issued = 2, Paid = 3, Overdue = 4, Cancelled = 5 }
```

**CQRS Operations:**

**Commands:**
```csharp
CreateBookingCommand
  ├── EventId: string
  ├── CrewId: string
  ├── StartTime: DateTime
  ├── EndTime: DateTime
  ├── Notes: string (optional)
  ├── Dependencies:
  │   ├── Event Service: GetEvent (verify event exists)
  │   ├── Crew Service: GetCrewAvailability (check crew is available)
  │   ├── Customer Service: GetTariffs (calculate rate)
  └── Returns: BookingResponse { Booking, CalculatedRate, TotalAmount }

ConfirmBookingCommand
  ├── BookingId: string
  ├── Publishes: BookingConfirmed → Notification Service, Invoice Service
  └── Returns: BookingResponse

CancelBookingCommand
  ├── BookingId: string
  ├── CancellationReason: string
  ├── Publishes: BookingCancelled → Notification Service
  └── Returns: SuccessResponse

GenerateInvoiceCommand
  ├── BookingId: string
  ├── Publishes: InvoiceGenerated
  └── Returns: InvoiceResponse

UpdateInvoiceStatusCommand
  ├── InvoiceId: string
  ├── NewStatus: InvoiceStatus
  └── Returns: InvoiceResponse
```

**Queries:**
```csharp
GetBookingQuery
  ├── BookingId: string
  └── Returns: BookingResponse { Booking + Event (from Event Svc) + Crew (from Crew Svc) }

GetBookingsByEventQuery
  ├── EventId: string
  └── Returns: List<BookingResponse>

GetBookingsByCrewQuery
  ├── CrewId: string
  ├── StartDate: DateTime (optional)
  ├── EndDate: DateTime (optional)
  └── Returns: List<BookingResponse>

GetBookingsByCustomerQuery
  ├── CustomerId: string
  ├── PageNumber: int (optional)
  ├── PageSize: int (optional)
  ├── Status: enum (optional filter)
  └── Returns: PaginatedResponse<BookingResponse>

GetPricingQuery
  ├── CrewId: string
  ├── TimeType: enum
  ├── Duration: int
  ├── Dependencies: Customer Service → GetTariffs
  └── Returns: PricingResponse { Rate, Duration, TotalAmount, Vat }

GetInvoiceQuery
  ├── InvoiceId: string
  └── Returns: InvoiceResponse

GetInvoicesByCustomerQuery
  ├── CustomerId: string
  ├── Status: enum (optional)
  └── Returns: List<InvoiceResponse>

GetBookingReportQuery
  ├── CustomerId: string
  ├── DateRange: DateRange
  ├── Aggregates: Bookings, Revenue, CrewUtilization
  └── Returns: ReportResponse
```

**API Endpoints:**
```
GET    /api/bookings/{id}           → GetBookingQuery
GET    /api/bookings/event/{eid}    → GetBookingsByEventQuery
GET    /api/bookings/crew/{cid}     → GetBookingsByCrewQuery
GET    /api/customers/{cid}/bookings → GetBookingsByCustomerQuery
POST   /api/bookings                → CreateBookingCommand
PUT    /api/bookings/{id}/confirm   → ConfirmBookingCommand
PUT    /api/bookings/{id}/cancel    → CancelBookingCommand
GET    /api/bookings/pricing        → GetPricingQuery
POST   /api/bookings/{id}/invoice   → GenerateInvoiceCommand
GET    /api/invoices/{id}           → GetInvoiceQuery
GET    /api/customers/{cid}/invoices → GetInvoicesByCustomerQuery
PUT    /api/invoices/{id}/status    → UpdateInvoiceStatusCommand
GET    /api/reports/bookings        → GetBookingReportQuery
```

**Inter-Service Communication:**
```
CreateBookingCommand:
  1. Event Service: GET /api/events/{id}
  2. Crew Service: GET /api/crew/{id}
  3. Crew Service: GET /api/crew/{id}/availability?date={date}
  4. Customer Service: GET /api/customers/{id}/tariffs
  5. Calculate totalAmount
  6. Save booking
  7. Publish: BookingCreated event

GetBookingQuery:
  1. GET booking from DB
  2. Event Service: GET /api/events/{id} (enrichment)
  3. Crew Service: GET /api/crew/{id} (enrichment)
  4. Return enriched response
```

**Domain Events (Async Publishing):**
```csharp
BookingCreated
  - BookingId: string
  - CrewId: string
  - EventId: string
  - CustomerId: string
  - TotalAmount: decimal
  - Subscribers: Notification Service, Analytics Service

BookingConfirmed
  - BookingId: string
  - CustomerId: string
  - Subscribers: Invoice Service, Notification Service

BookingCancelled
  - BookingId: string
  - CrewId: string
  - EventId: string
  - CancellationReason: string
  - Subscribers: Notification Service, Refund Service (future)

InvoiceGenerated
  - InvoiceId: string
  - BookingId: string
  - CustomerId: string
  - TotalAmount: decimal
  - DueDate: DateTime
  - Subscribers: Notification Service, Accounting Service (future)

InvoicePaymentReceived
  - InvoiceId: string
  - Amount: decimal
  - PaymentMethod: string
  - Subscribers: Notification Service, Reconciliation Service (future)
```

**Dependencies:**
- Event Service (verify events)
- Crew Service (verify crew & check availability)
- Customer Service (get tariffs for pricing)
- MongoDB (store bookings & invoices)
- Message Broker (publish events)

---

## Data Architecture

### Database Design

Single MongoDB Database (same for dev & prod):

```
Database: thenorsound

Collections:
├── auth_refreshTokens (TTL index on ExpiresAt)
│   └── Stores: RefreshTokens for auth
│
├── users
│   └── Stores: All users (userType: 1=Admin, 2=Customer, 3=Crew)
│       Extended crew properties: skills, SSN, bank info, availability
│
├── customers
│   └── Stores: All customers (customerType: 1=EventOrganizer, 2=CrewCompany)
│       Tariffs: Only for customerType 1 (EventOrganizers)
│
├── events
│   └── Stores: Event data (customerId, dates, status, details)
│
└── bookings
    ├── bookings: Crew-Event assignments with pricing
    └── invoices: Invoice records
```

### Database Indexing Strategy

**auth_refreshTokens:**
```
- Compound: (UserId, ExpiresAt) with TTL
- Uses: Fast token lookup + automatic cleanup
```

**users:**
```
- Unique: Email
- Regular: CustomerId
- Regular: UserType (for crew queries)
- Uses: Fast lookups, crew member filtering
```

**customers:**
```
- Unique: OrgNumber
- Regular: CustomerType (1=EventOrganizer, 2=CrewCompany)
- Uses: Type filtering, org number lookup

tariffs (sub-collection):
- Compound: (CustomerId, Category, Skill, TimeType)
- Uses: Fast tariff lookups for pricing calculations
```

**events:**
```
- Compound: (CustomerId, StartDate) sorted desc
- Regular: Status
- Uses: Calendar queries, status filtering
```

**bookings:**
```
- Compound: (EventId, CrewId) unique - prevents double-booking
- Regular: CustomerId, Status, CrewId
- Uses: Conflict detection, status filtering
```

**invoices:**
```
- Unique: InvoiceNumber
- Regular: CustomerId, Status
- Uses: Invoice lookup, status queries
```

### Data Consistency Strategy

**Synchronous Consistency:**
- Within a single service: ACID transactions (MongoDB multi-doc transactions)
- Between services: Compensating transactions (saga pattern)

**Example Saga - Booking Creation:**
```
1. Booking Service: CreateBooking (tentative)
   ├── If fails → Rollback
   
2. Event Service: ReserveEventSlot (verify event exists)
   ├── If fails → Booking Service: CancelBooking
   
3. User Service: CheckCrewAvailability (userType: 3)
   ├── If fails → Event Service: ReleaseEventSlot → Booking Service: CancelBooking
   
4. Customer Service: GetTariffs (customerType: 1 EventOrganizer)
   ├── Match crew skills to tariffs
   ├── If fails → User Service: ReleaseSlot → Booking Service: CancelBooking
   
5. Success: Publish BookingCreated event
```

**Asynchronous Consistency:**
- Event-driven: Use message broker (RabbitMQ/Kafka)
- Eventual consistency model
- Retry logic with dead-letter queues

---

## Service Communication Patterns

### 1. API Gateway Pattern

**API Gateway (Port 5000):**
```
Request Flow:
  1. Frontend → API Gateway
  2. API Gateway:
     ├── Extract JWT token from header
     ├── Call Auth Service → ValidateTokenQuery
     ├── If invalid: Return 401 Unauthorized
     └── If valid: Route to target service + inject user context
  3. Target Service processes request
  4. Response returned through Gateway
```

**Implementation (ASP.NET Core):**
```csharp
// API Gateway - Program.cs
builder.Services
    .AddYarp()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"));

// Authentication middleware
app.UseCustomJwtValidation(); // Call Auth Service

// YARP reverse proxy routing
app.UseRouting();
app.MapReverseProxy();
```

**Routing Configuration (appsettings.json):**
```json
{
  "ReverseProxy": {
    "Routes": {
      "Auth": {
        "ClusterId": "auth-cluster",
        "Match": { "Path": "/api/auth/{**catch-all}" }
      },
      "User": {
        "ClusterId": "user-cluster",
        "Match": { "Path": "/api/users/{**catch-all}" }
      },
      "Customer": {
        "ClusterId": "customer-cluster",
        "Match": { "Path": "/api/customers/{**catch-all}" }
      },
      "Event": {
        "ClusterId": "event-cluster",
        "Match": { "Path": "/api/events/{**catch-all}" }
      },
      "Booking": {
        "ClusterId": "booking-cluster",
        "Match": { "Path": "/api/bookings/{**catch-all}" }
      }
    },
    "Clusters": {
      "auth-cluster": { "Destinations": { "destination1": { "Address": "http://localhost:5001" } } },
      "user-cluster": { "Destinations": { "destination1": { "Address": "http://localhost:5002" } } },
      "customer-cluster": { "Destinations": { "destination1": { "Address": "http://localhost:5003" } } },
      "event-cluster": { "Destinations": { "destination1": { "Address": "http://localhost:5004" } } },
      "booking-cluster": { "Destinations": { "destination1": { "Address": "http://localhost:5005" } } }
    }
  }
}
```

### 2. Synchronous Service-to-Service Communication (REST)

**Use case:** Real-time data enrichment, validation

**Example: Booking Service → Customer Service (GetTariffs)**

```csharp
// Application/Clients/ICustomerServiceClient.cs
public interface ICustomerServiceClient
{
    Task<TariffResponse[]> GetTariffsByCustomerAsync(string customerId);
    Task<CustomerResponse> GetCustomerAsync(string customerId);
}

// Infrastructure/Clients/CustomerServiceClient.cs
public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerServiceClient> _logger;

    public CustomerServiceClient(HttpClient httpClient, ILogger<CustomerServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TariffResponse[]> GetTariffsByCustomerAsync(string customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/customers/{customerId}/tariffs");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TariffResponse[]>(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get tariffs for customer {CustomerId}", customerId);
            throw new ExternalServiceException("Failed to retrieve tariffs from Customer Service");
        }
    }
}

// Application/Commands/CreateBookingCommandHandler.cs
public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly IEventServiceClient _eventServiceClient;
    private readonly IUserServiceClient _userServiceClient; // Changed from ICrewServiceClient
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate event exists
        var eventData = await _eventServiceClient.GetEventAsync(request.EventId);
        if (eventData == null)
            throw new NotFoundException("Event", request.EventId);

        // 2. Check crew (user with userType: 3) availability
        var crew = await _userServiceClient.GetCrewMemberAsync(request.CrewId);
        if (crew == null || crew.UserType != 3)
            throw new NotFoundException("Crew member", request.CrewId);

        var crewAvailable = await _userServiceClient.IsCrewAvailableAsync(request.CrewId, request.StartTime, request.EndTime);
        if (!crewAvailable)
            throw new BusinessException("Crew member is not available for the requested time slot");

        // 3. Get tariffs for pricing (EventOrganizer: customerType 1)
        var tariffs = await _customerServiceClient.GetTariffsByCustomerAsync(eventData.CustomerId);
        if (tariffs == null || tariffs.Length == 0)
            throw new BusinessException("No tariffs configured for this event organizer");

        var rate = CalculateRate(tariffs, crew.Skills); // Match crew skills to tariffs

        // 4. Create booking
        var booking = new BookingEntity
        {
            EventId = request.EventId,
            CrewId = request.CrewId,
            CustomerId = eventData.CustomerId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Rate = rate,
            TotalAmount = rate * (request.EndTime - request.StartTime).TotalHours,
            Status = BookingStatus.Requested
        };

        await _bookingRepository.CreateAsync(booking);
        return _mapper.Map<BookingResponse>(booking);
    }
}

// Dependency Injection (Program.cs)
builder.Services
    .AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5003");
        client.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(200)));
```

### 3. Asynchronous Service Communication (Message Broker)

**Use case:** Events, notifications, non-blocking operations

**Example: Booking Service → Event Published → Notification Service (listen)**

**Publishing (Booking Service):**
```csharp
// Domain/Events/BookingConfirmedEvent.cs
public record BookingConfirmedEvent
{
    public string BookingId { get; init; }
    public string CustomerId { get; init; }
    public string CrewId { get; init; }
    public string EventId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Application/Commands/ConfirmBookingCommandHandler.cs
public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPublisher _mediator; // MediatR for internal publishing
    private readonly IMessagePublisher _messagePublisher; // RabbitMQ/Kafka
    private readonly IMapper _mapper;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;

    public async Task<BookingResponse> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null)
            throw new NotFoundException("Booking", request.BookingId);

        booking.Status = BookingStatus.Confirmed;
        await _bookingRepository.UpdateAsync(booking);

        // Publish event for other services to listen
        var bookingConfirmedEvent = new BookingConfirmedEvent
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            CrewId = booking.CrewId,
            EventId = booking.EventId,
            TotalAmount = booking.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync("booking.confirmed", bookingConfirmedEvent);
        _logger.LogInformation("Published BookingConfirmed event for booking {BookingId}", booking.Id);

        return _mapper.Map<BookingResponse>(booking);
    }
}

// Infrastructure/MessagePublishing/RabbitMqMessagePublisher.cs
public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqMessagePublisher> _logger;

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        using var channel = _connection.CreateModel();
        
        channel.ExchangeDeclare(
            exchange: "thenorsound.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(
            exchange: "thenorsound.events",
            routingKey: topic,
            basicProperties: null,
            body: body);

        _logger.LogInformation("Published message to topic {Topic}", topic);
    }
}

// Program.cs
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
builder.Services.AddSingleton(sp =>
{
    var connectionFactory = new ConnectionFactory
    {
        HostName = "rabbitmq-container",
        UserName = "guest",
        Password = "guest"
    };
    return connectionFactory.CreateConnection();
});
```

**Subscribing (Notification Service - future):**
```csharp
// Infrastructure/MessageConsuming/RabbitMqMessageConsumer.cs
public class RabbitMqMessageConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqMessageConsumer> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: "thenorsound.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        var queueName = _channel.QueueDeclare().QueueName;

        _channel.QueueBind(
            queue: queueName,
            exchange: "thenorsound.events",
            routingKey: "booking.*");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) => await HandleMessageAsync(ea);

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        var routingKey = ea.RoutingKey;

        try
        {
            if (routingKey == "booking.confirmed")
            {
                var @event = JsonSerializer.Deserialize<BookingConfirmedEvent>(body);
                // Handle booking confirmed: send email, SMS, etc.
                // Implementation here
            }
            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue
        }
    }
}
```

---

## Migration Strategy

### Phase 1: Foundation (Sprint 1-2) - Auth & User Services

**Sprint 1: Auth Service**
- [ ] Create Auth Service project structure
- [ ] Implement LoginCommand/Handler
- [ ] Implement RefreshTokenCommand/Handler
- [ ] Implement ValidateTokenQuery/Handler
- [ ] Setup MongoDB for refresh tokens
- [ ] Write unit tests (80%+ coverage)
- [ ] Integration tests with User Service
- [ ] Deploy to development environment
- [ ] Update frontend: Point auth requests to Auth Service

**Sprint 2: User Service**
- [ ] Create User Service project structure
- [ ] Implement CreateUserCommand/Handler
- [ ] Implement GetUserQuery/Handler
- [ ] Implement GetUsersByCustomerQuery/Handler
- [ ] Setup MongoDB for users
- [ ] Write unit tests (80%+ coverage)
- [ ] Integration with Auth Service
- [ ] Deploy and test with Auth Service
- [ ] Migrate existing user data

**Deliverables:**
- Auth Service running on port 5001
- User Service running on port 5002
- Both services accessible via API Gateway

---

### Phase 2: Customer & Event Services (Sprint 3-4)

**Sprint 3: Customer Service**
- [ ] Create Customer Service project structure
- [ ] Implement CreateCustomerCommand/Handler
- [ ] Implement GetCustomerQuery (include users from User Service)
- [ ] Implement tariff CRUD
- [ ] Setup MongoDB for customers
- [ ] Write integration tests
- [ ] Deploy to development

**Sprint 4: Event Service**
- [ ] Create Event Service project structure
- [ ] Implement CreateEventCommand/Handler
- [ ] Implement GetEventsByCustomerQuery/Handler
- [ ] Implement event status management
- [ ] Setup MongoDB for events
- [ ] Publish domain events (EventCreated, EventUpdated)
- [ ] Write integration tests
- [ ] Deploy and test

**Deliverables:**
- Customer Service (port 5003)
- Event Service (port 5004)
- Both services integrated with previous services

---

## Migration Strategy

**5-Service Microservices Architecture - Development Timeline**

The migration is organized in 3 phases (6 sprints total), integrating crew management directly into User Service and emphasizing tariff-based pricing.

### Phase 1: Foundation (Sprint 1-2) - Auth & User Services

**Sprint 1: Auth Service**
- [ ] Create Auth Service project structure (C#, .NET Core)
- [ ] Implement LoginCommand/Handler (email + password validation)
- [ ] Implement RefreshTokenCommand/Handler (7-day token rotation)
- [ ] Implement ValidateTokenQuery/Handler (JWT validation)
- [ ] Setup MongoDB collection: auth_refreshTokens
  - [ ] Create TTL index on ExpiresAt
  - [ ] Compound index: (UserId, ExpiresAt)
- [ ] Unit tests: 80%+ coverage
- [ ] Integration tests: Auth with User Service
- [ ] Deploy to development (Port 5001)
- [ ] Frontend update: Authorization header handling

**Sprint 2: User Service**
- [ ] Create User Service project structure
- [ ] Implement core user CRUD for all userTypes (1=Admin, 2=Customer, 3=Crew)
- [ ] Implement GetCrewMembersQuery (filter userType: 3)
- [ ] Implement crew-specific endpoints:
  - [ ] UpdateCrewAvailabilityCommand (create TimeSlot entries)
  - [ ] GetCrewAvailabilityQuery (date range query)
  - [ ] IsCrewAvailableAsync (check overlapping slots)
- [ ] Setup MongoDB collection: users
  - [ ] Unique index: Email
  - [ ] Regular index: CustomerId
  - [ ] Regular index: UserType (for crew filtering)
- [ ] Handle crew extended properties (Skills, SSN, BankInfo - encrypted)
- [ ] Unit tests: 80%+ coverage
- [ ] Deploy to development (Port 5002)

**Deliverables:**
- Auth Service running on port 5001 (JWT generation & validation)
- User Service running on port 5002 (all user types including crew)
- Single MongoDB database: thenorsound (2 collections: auth_refreshTokens, users)
- Both services accessible via API Gateway (port 5000)

---

### Phase 2: Customer & Event Services (Sprint 3-4)

**Sprint 3: Customer Service**
- [ ] Create Customer Service project structure
- [ ] Implement customer CRUD for both types:
  - [ ] customerType: 1 = EventOrganizer (has tariffs array)
  - [ ] customerType: 2 = CrewCompany (manages crew members via User Service)
- [ ] Implement Tariff CRUD (only for customerType: 1)
  - [ ] Create: { CustomerId, Category, Skill, TimeType, Amount }
  - [ ] Update: Amount modifications
  - [ ] Query: GetTariffsByCustomerAsync (fast lookup for pricing)
- [ ] Setup MongoDB collection: customers
  - [ ] Unique index: OrgNumber
  - [ ] Regular index: CustomerType (1 vs 2)
  - [ ] Tariffs: { Category, Skill, TimeType, Amount }
- [ ] Integration: Call User Service to fetch company users/crew
- [ ] Unit tests: 80%+ coverage
- [ ] Deploy to development (Port 5003)

**Sprint 4: Event Service**
- [ ] Create Event Service project structure
- [ ] Implement EventCommand/Handler:
  - [ ] CreateEventCommand (CustomerId required - EventOrganizer only)
  - [ ] UpdateEventCommand (name, description, dates, location, details)
  - [ ] UpdateEventStatusCommand (Draft → Scheduled → InProgress → Completed → Cancelled)
  - [ ] DeleteEventCommand
- [ ] Publish Domain Events:
  - [ ] EventCreated (for Booking Service to listen)
  - [ ] EventUpdated
  - [ ] EventDeleted
- [ ] Setup MongoDB collection: events
  - [ ] Compound index: (CustomerId, StartDate) sorted desc
  - [ ] Regular index: Status
- [ ] Calendar queries: GetEventsByDateRangeQuery, GetEventCalendarQuery
- [ ] Unit tests: 80%+ coverage
- [ ] Deploy to development (Port 5004)

**Deliverables:**
- Customer Service running on port 5003 (both company types + tariffs)
- Event Service running on port 5004 (event scheduling + publishing)
- MongoDB database expanded: thenorsound (4 collections total)
- All services integrated with previous services via REST + Message Bus

---

### Phase 3: Booking Service (Sprint 5-6)

**Sprint 5-6: Booking Service (Complex - Combined Sprint)**

This is the most complex service coordinating multiple dependencies:

- [ ] Create Booking Service project structure
- [ ] Implement CreateBookingCommand with cross-service validation:
  - [ ] Call Event Service: Verify event exists + startDate valid
  - [ ] Call User Service: Verify crew (userType: 3) exists + available
  - [ ] Call Customer Service: Get tariffs (customerType: 1 EventOrganizer)
  - [ ] Match crew skills to tariffs (pricing calculation)
  - [ ] Calculate: rate × duration = totalAmount
  - [ ] Create booking with Status: "Requested"
  - [ ] Publish: BookingCreated event
  
- [ ] Implement Booking Commands:
  - [ ] ConfirmBookingCommand (→ Status: "Confirmed" → Publish: BookingConfirmed)
  - [ ] CancelBookingCommand (→ Status: "Cancelled" → Publish: BookingCancelled)
  
- [ ] Implement Invoice Management:
  - [ ] GenerateInvoiceCommand (for confirmed bookings)
  - [ ] UpdateInvoiceStatusCommand (Draft → Issued → Paid → Overdue)
  
- [ ] Setup MongoDB collection: bookings
  - [ ] Unique compound: (EventId, CrewId) - prevents double-booking
  - [ ] Regular index: CustomerId (query by organizer)
  - [ ] Regular index: Status (filtering)
  
- [ ] Setup MongoDB collection: invoices
  - [ ] Unique index: InvoiceNumber
  - [ ] Regular index: CustomerId
  - [ ] Regular index: Status
  
- [ ] Publish Domain Events:
  - [ ] BookingCreated (subscribe: Notification Service)
  - [ ] BookingConfirmed (subscribe: Invoice Service, Notification Service)
  - [ ] BookingCancelled (subscribe: Notification Service)
  - [ ] InvoiceGenerated (subscribe: Notification Service, Accounting)
  
- [ ] Comprehensive integration tests:
  - [ ] Happy path: full booking lifecycle
  - [ ] Error scenarios: unavailable crew, missing tariffs, invalid event
  - [ ] Saga compensation: test rollbacks
  
- [ ] Unit tests: 80%+ coverage

**Deliverables:**
- Booking Service running on port 5005 (bookings + invoices + pricing)
- MongoDB complete: thenorsound (5 collections: auth_refreshTokens, users, customers, events, bookings)
- Full microservices architecture operational
- All services publishing/subscribing to RabbitMQ event bus

---

### Phase 4: Message Broker & API Gateway (Sprint 7)

**Sprint 7: Event-Driven Architecture**
- [ ] Setup RabbitMQ container (docker-compose)
- [ ] Implement event publishing in all services
  - [ ] Auth: UserLoggedIn
  - [ ] User: UserCreated, CrewAvailabilityChanged
  - [ ] Customer: CustomerCreated, TariffUpdated
  - [ ] Event: EventCreated, EventUpdated, EventDeleted
  - [ ] Booking: BookingCreated, BookingConfirmed, BookingCancelled, InvoiceGenerated
  
- [ ] Implement message consumers (future services):
  - [ ] Notification Service (emails, SMS)
  - [ ] Analytics Service (aggregation)
  - [ ] Accounting Service (invoice reconciliation)
  
- [ ] Setup dead-letter queues (DLQ) for failed messages
- [ ] Configure API Gateway (YARP) routing to 5 services
- [ ] Frontend integration: Update all API calls to route via Gateway

**Deliverables:**
- RabbitMQ running (asynchronous event communication)
- Event-driven architecture complete
- All services communicating via REST (sync) + RabbitMQ (async)

---

### Rollback Strategy

If any service deployment fails:

1. **Stop Gateway routes to new service**
2. **Restore requests to monolith**
3. **Keep service running for debugging**
4. **Analyze logs/errors**
5. **Fix issues & redeploy**

Example: If Event Service has issues:
```json
{
  "Event": {
    "ClusterId": "event-cluster-monolith",
    "Match": { "Path": "/api/events/{**catch-all}" }
  }
}
```

Reroute to original monolith endpoint temporarily.

---

## Deployment Architecture

### Docker Compose for Local Development

```yaml
# docker-compose.yml - UPDATED for 5-service architecture with single MongoDB
version: '3.8'

services:
  # Single MongoDB Instance (all services share database "thenorsound")
  mongodb:
    image: mongo:latest
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_DATABASE: thenorsound
    volumes:
      - mongodb-data:/data/db

  # RabbitMQ Message Broker
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

  # API Gateway (YARP - 5 services, NOT 6)
  api-gateway:
    build:
      context: ./ApiGateway
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
    depends_on:
      - auth-service
      - user-service
      - customer-service
      - event-service
      - booking-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  # ========== 5 MICROSERVICES ==========

  # Service 1: Auth Service
  auth-service:
    build:
      context: ./AuthService
      dockerfile: Dockerfile
    ports:
      - "5001:5000"
    depends_on:
      - mongodb
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq

  # Service 2: User Service (includes crew members - userType: 3)
  user-service:
    build:
      context: ./UserService
      dockerfile: Dockerfile
    ports:
      - "5002:5000"
    depends_on:
      - mongodb
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq

  # Service 3: Customer Service (EventOrganizers + CrewCompanies + Tariffs)
  customer-service:
    build:
      context: ./CustomerService
      dockerfile: Dockerfile
    ports:
      - "5003:5000"
    depends_on:
      - mongodb
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq

  # Service 4: Event Service
  event-service:
    build:
      context: ./EventService
      dockerfile: Dockerfile
    ports:
      - "5004:5000"
    depends_on:
      - mongodb
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq

  # Service 5: Booking Service (NO Crew Service - integrated into User Service)
  booking-service:
    build:
      context: ./BookingService
      dockerfile: Dockerfile
    ports:
      - "5005:5000"
    depends_on:
      - mongodb
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
      - MongoDbSettings__DatabaseName=thenorsound
      - RabbitMq__HostName=rabbitmq
      - ServiceEndpoints__EventService=http://event-service:5000
      - ServiceEndpoints__UserService=http://user-service:5000
      - ServiceEndpoints__CustomerService=http://customer-service:5000

volumes:
  mongodb-data:
  rabbitmq-data:
```

**Key Changes from Previous Docker Compose:**
- ✅ Single MongoDB container (instead of 6 separate ones)
- ✅ All services use same database: "thenorsound"
- ✅ Removed crew-mongodb (Crew integrated into User Service)
- ✅ Booking Service now on port 5005 (instead of 5006)
- ✅ 5 services (Auth, User, Customer, Event, Booking) - NOT 6

**Start services locally:**
```bash
docker-compose up -d

# Check all services are running
docker-compose ps

# Services accessible:
# - API Gateway: http://localhost:5000
# - Auth Service: http://localhost:5001
# - User Service: http://localhost:5002 (includes crew members)
# - Customer Service: http://localhost:5003 (EventOrganizers + CrewCompanies)
# - Event Service: http://localhost:5004
# - Booking Service: http://localhost:5005 (NO Crew Service!)
# - MongoDB: mongodb://localhost:27017/thenorsound
# - RabbitMQ Management: http://localhost:15672 (guest/guest)
```

**Stop all services:**
```bash
docker-compose down

# Clean slate (remove all volumes)
docker-compose down -v
```
# - Booking Service: http://localhost:5006
# - RabbitMQ Management: http://localhost:15672
```

### Production Deployment (Azure Container Instances / Kubernetes)

**Kubernetes Manifest Example:**

```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: thenorsound

---
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
    spec:
      containers:
      - name: auth-service
        image: acr.azurecr.io/thenorsound/auth-service:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: MongoDbSettings__ConnectionString
          valueFrom:
            secretKeyRef:
              name: mongodb-auth-secret
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## Integration Guide

### Frontend Integration (React - soundflow)

**Update API endpoints:**

```javascript
// Before (direct backend calls)
const API_BASE_URL = 'http://localhost:3001/api'

// After (via API Gateway)
const API_BASE_URL = 'http://localhost:5000/api'

// Usage remains the same:
export async function loginUser(email, password) {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  })
  return response.json()
}

// API Gateway routes automatically to Auth Service (5001)
```

### Authentication Flow

```
1. User enters credentials in React UI (soundflow)
   ↓
2. Frontend calls: POST /api/auth/login
   ↓
3. API Gateway receives request
   ├── Routes to Auth Service (5001) ← via YARP
   ├── Auth Service processes login
   ├── Returns: { accessToken, refreshToken }
   └── Sets refresh token in HTTP-only cookie
   ↓
4. Frontend stores accessToken in memory/localStorage
   ↓
5. Subsequent requests include: Authorization: Bearer {accessToken}
   ↓
6. API Gateway validates token with Auth Service
   ├── If valid: Forward request to target service
   ├── If expired: Frontend uses refresh token
   ├── If invalid: Return 401 Unauthorized
```

### Error Handling

**Standard Error Response Format:**

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Booking creation failed",
    "details": [
      {
        "field": "startDate",
        "message": "Start date cannot be in the past"
      }
    ],
    "timestamp": "2026-02-09T10:30:00Z",
    "traceId": "0HN123ABC456"
  }
}
```

**Error Codes:**

| Code | HTTP Status | Meaning |
|------|-------------|---------|
| VALIDATION_ERROR | 400 | Invalid input data |
| NOT_FOUND | 404 | Resource not found |
| CONFLICT | 409 | Resource already exists |
| UNAUTHORIZED | 401 | Missing/invalid authentication |
| FORBIDDEN | 403 | User lacks permissions |
| RATE_LIMITED | 429 | Too many requests |
| SERVICE_ERROR | 500 | Internal server error |
| SERVICE_UNAVAILABLE | 503 | Dependency service down |

---

## Development Standards

### Code Structure (per service)

```
ServiceName/
├── API/
│   ├── Controllers/
│   │   ├── BaseController.cs
│   │   └── [Domain]Controller.cs
│   ├── DTOs/
│   │   ├── Request/
│   │   │   └── Create[Domain]RequestDto.cs
│   │   │   └── Update[Domain]RequestDto.cs
│   │   └── Response/
│   │       ├── BaseResponseDto.cs
│   │       └── [Domain]ResponseDto.cs
│   ├── Exceptions/
│   │   ├── BadRequestException.cs
│   │   ├── NotFoundException.cs
│   │   └── DuplicateException.cs
│   ├── Middleware/
│   │   └── ErrorHandlingMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── Application/
│   ├── Commands/
│   │   ├── Create[Domain]Command.cs
│   │   ├── Create[Domain]CommandHandler.cs
│   │   ├── Update[Domain]Command.cs
│   │   └── Update[Domain]CommandHandler.cs
│   ├── Queries/
│   │   ├── Get[Domain]Query.cs
│   │   └── Get[Domain]QueryHandler.cs
│   ├── Clients/
│   │   └── I[ExternalService]Client.cs
│   ├── Validators/
│   │   └── [Command/Query]Validator.cs
│   └── ApplicationServiceCollectionExtensions.cs
│
├── Domain/
│   ├── Models/
│   │   └── [Domain].cs
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   └── [Domain]Entity.cs
│   ├── Repositories/
│   │   └── I[Domain]Repository.cs
│   ├── Enums/
│   │   └── [DomainEnum].cs
│   ├── Events/
│   │   └── [Domain]CreatedEvent.cs
│   └── MappingConfiguration/
│       └── [Domain]MappingConfig.cs
│
├── Infrastructure/
│   ├── Repositories/
│   │   └── [Domain]Repository.cs
│   ├── Clients/
│   │   └── [ExternalService]Client.cs
│   ├── Settings/
│   │   ├── MongoDbSettings.cs
│   │   └── [ServiceName]Settings.cs
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs
│   └── InfrastructureServiceCollectionExtensions.cs
│
├── Tests/
│   ├── Unit/
│   │   ├── [Domain]CommandHandlerTests.cs
│   │   ├── [Domain]QueryHandlerTests.cs
│   │   └── [Domain]RepositoryTests.cs
│   ├── Integration/
│   │   ├── [Domain]ControllerTests.cs
│   │   └── [Domain]ClientTests.cs
│   └── Fixtures/
│       └── [Domain]TestFixture.cs
│
├── [ServiceName].sln
└── README.md
```

### Testing Standards

**Unit Tests (per service):**
- Command/Query Handler tests
- Repository tests
- Validator tests
- Business logic tests

**Coverage requirement:** 80% minimum

**Integration Tests:**
- Controller tests
- Cross-service communication
- Database integration

**Example:**

```csharp
// Tests/Unit/CreateBookingCommandHandlerTests.cs
public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventServiceClient> _eventServiceClientMock;
    private readonly Mock<ICrewServiceClient> _crewServiceClientMock;
    private readonly Mock<ICustomerServiceClient> _customerServiceClientMock;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _eventServiceClientMock = new Mock<IEventServiceClient>();
        _crewServiceClientMock = new Mock<ICrewServiceClient>();
        _customerServiceClientMock = new Mock<ICustomerServiceClient>();

        _handler = new CreateBookingCommandHandler(
            _bookingRepositoryMock.Object,
            _eventServiceClientMock.Object,
            _crewServiceClientMock.Object,
            _customerServiceClientMock.Object,
            new MapperFixture().GetMapper(),
            LoggerFixture.GetLogger<CreateBookingCommandHandler>());
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateBooking()
    {
        // Arrange
        var command = new CreateBookingCommand
        {
            EventId = "event-123",
            CrewId = "crew-456",
            StartTime = DateTime.UtcNow.AddDays(7),
            EndTime = DateTime.UtcNow.AddDays(7).AddHours(2)
        };

        var eventData = new EventResponse { Id = "event-123", CustomerId = "customer-789", Name = "Test Event" };
        var tariffs = new[] { new TariffResponse { Amount = 500m } };

        _eventServiceClientMock
            .Setup(x => x.GetEventAsync("event-123"))
            .ReturnsAsync(eventData);

        _crewServiceClientMock
            .Setup(x => x.IsAvailableAsync("crew-456", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _customerServiceClientMock
            .Setup(x => x.GetTariffsByCustomerAsync("customer-789"))
            .ReturnsAsync(tariffs);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("event-123", result.EventId);
        Assert.Equal(1000m, result.TotalAmount); // 500 * 2 hours
        _bookingRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<BookingEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnavailableCrew_ShouldThrowException()
    {
        // Arrange
        var command = new CreateBookingCommand
        {
            EventId = "event-123",
            CrewId = "crew-456",
            StartTime = DateTime.UtcNow.AddDays(7),
            EndTime = DateTime.UtcNow.AddDays(7).AddHours(2)
        };

        _eventServiceClientMock
            .Setup(x => x.GetEventAsync("event-123"))
            .ReturnsAsync(new EventResponse { Id = "event-123", CustomerId = "customer-789" });

        _crewServiceClientMock
            .Setup(x => x.IsAvailableAsync("crew-456", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
```

---

## Conclusion

ThenorSound Microservices Architecture är en **produktion-klar design** som:

✅ **Separation of Concerns** - Tydliga ansvarsområden  
✅ **Clean Architecture** - CQRS, MediatR, DDD principer  
✅ **Skalbart** - Oberoende service-skalning  
✅ **Testbart** - Högkvalitets-tester möjliga  
✅ **Driftsäkert** - Documented deployment process  
✅ **Framtidssäkert** - Enkelt att utöka med nya services  

**Nästa steg:**
1. ✅ Godkänn denna arkitektur
2. ⬜ Starta Sprint 1 med Auth Service
3. ⬜ Etablera CI/CD pipeline
4. ⬜ Sätt upp monitoring/logging
5. ⬜ Migrera data från monolith

---

**Dokument slutförande:** 2026-02-09
**Version:** 1.0
**Status:** Ready for Implementation
