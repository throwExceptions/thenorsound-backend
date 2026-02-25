# Authentication Flow

Auth service utfärdar JWT access tokens och refresh tokens.
User och Customer service validerar access tokens lokalt.

---

## Tjänster och ansvar

| Tjänst | Ansvar |
|--------|--------|
| Auth service | Utfärdar tokens, hanterar lösenord, lagrar refresh tokens |
| User service | Validerar JWT på alla endpoints utom `GET /by-email` |
| Customer service | Validerar JWT på alla endpoints utom `GET /{id}` |

Undantagen (`by-email` och `/{id}`) är interna service-to-service endpoints
som anropas utan JWT — Auth → User respektive User → Customer.

---

## 1. Skapa användare + registrera credentials

Görs i ett anrop från frontend. User service ansvarar för att registrera credentials i Auth service.

```
POST /api/User
{ "email": "anna@example.com", "firstName": "Anna", "lastName": "Svensson",
  "password": "hemligt123", "role": 2, "customerId": "abc123", "phone": "..." }
```

**User service:**
1. Validerar request (FluentValidation)
2. Kontrollerar att email inte är tagen
3. Skapar User-dokument i MongoDB
4. `POST /api/Auth/credentials` — anropar Auth service internt med `{ userId, email, password }`

**Auth service:**
5. `BCrypt.HashPassword("hemligt123", workFactor: 12)` → `"$2a$12$xyz..."`
6. Sparar `{ userId, email, passwordHash }` i `credentials`-collection

**Rollback:**
Om steg 4–6 misslyckas → User service kör `DELETE /api/User/{id}` och kastar exception.
Användaren skapas alltid atomärt — aldrig utan credentials, aldrig credentials utan user.

> **Intern endpoint (Admin/Superuser kan fortfarande anropa direkt):**
> `POST /api/Auth/credentials  { email, password }` — Auth anropar User service för att verifiera att user finns

---

## 2. Logga in

```
POST /api/Auth/login
{ "email": "anna@example.com", "password": "hemligt123" }
```

**Auth service:**
1. Hämtar credential från MongoDB via email
2. `BCrypt.Verify("hemligt123", "$2a$12$xyz...")` — fel lösenord → 401
3. `GET /api/User/by-email/anna@example.com` — hämtar namn, roll, customerId
4. Skapar JWT access token (signerat med `SecretKey`, 15 min)
5. Skapar refresh token (64 kryptografiskt slumpmässiga bytes)
6. Sparar refresh token + utgångstid i MongoDB
7. Returnerar:

```
HTTP Body:   { "result": { "token": "eyJ...", "expires": 900000 } }
HTTP Cookie: refreshToken=abc123  (HttpOnly, Secure, SameSite=Strict, 7 dagar)
             Path=/api/Auth/refresh  ← skickas BARA till refresh-endpoint
```

---

## 3. Anropa skyddade endpoints

```
GET /api/User
Authorization: Bearer eyJ...
```

**User/Customer service:**
1. JWT-middleware läser `Authorization`-headern
2. Verifierar signaturen lokalt med `SecretKey` — **ingen DB-träff**
3. Kontrollerar `exp` (utgångstid)
4. Giltig → request når controller
5. Ogiltig/utgången → `401 Unauthorized`

**JWT-innehåll (claims):**

| Claim | Värde |
|-------|-------|
| `sub` | userId |
| `email` | anna@example.com |
| `firstname` | Anna |
| `lastname` | Svensson |
| `role` | 2 |
| `customerId` | abc123 |
| `customerType` | 1 (Customer) eller 2 (Crew) |
| `exp` | Unix-timestamp (UtcNow + 15 min) |

---

## 4. Förnya token (access token utgånget)

```
POST /api/Auth/refresh
Cookie: refreshToken=abc123   ← skickas automatiskt av browsern
```

**Auth service:**
1. Läser `refreshToken` från cookie
2. Hittar credential i MongoDB via refresh token — hittades inte → 401
3. Kontrollerar `refreshTokenExpiry < UtcNow` — utgånget → 401
4. `GET /api/User/by-email/...` — hämtar aktuell användardata
5. Skapar nytt access token
6. Skapar nytt refresh token (**token rotation** — gammalt invalideras direkt)
7. Sparar nytt refresh token i MongoDB
8. Returnerar nytt access token i body + ny refreshToken-cookie

---

## 5. Logga ut

```
POST /api/Auth/logout
Cookie: refreshToken=abc123
```

**Auth service:**
1. Hittar credential via refresh token
2. Sätter `refreshToken = null`, `refreshTokenExpiry = null` i MongoDB
3. Raderar cookien i response
4. Svarar alltid `200` — avslöjar inte om token var giltig

---

## Varför två tokens?

| | Access token | Refresh token |
|---|---|---|
| Livslängd | 15 minuter | 7 dagar |
| Lagras | Hos klienten (memory/localStorage) | HTTP-only cookie |
| Skickas | Vid varje API-request (Authorization-header) | Bara till `/api/Auth/refresh` |
| Valideringsmetod | Stateless — verifieras lokalt med SecretKey | Stateful — slås upp i MongoDB |
| Kan invalideras direkt | Nej | Ja (logout, misstänkt aktivitet) |
| Läsbar av JavaScript | Ja | Nej (HttpOnly) |

---

## Säkerhetsmodell

| Hot | Skydd |
|-----|-------|
| Access token stjäls | Skadan varar max 15 min |
| XSS-attack försöker stjäla cookie | HttpOnly — JavaScript kan inte läsa refresh token |
| Refresh token stjäls och används | Token rotation — det gamla refresh token slutar gälla direkt |
| Brute-force mot lösenord | BCrypt workFactor 12 ≈ 250 ms per försök |
| Enumeration (felaktigt email vs lösenord) | Identiskt felmeddelande oavsett vilket fält som är fel |
| Replay-attack med utgånget refresh token | `refreshTokenExpiry` kontrolleras i DB |

---

## Konfiguration

### Delade inställningar (samma värden i alla tre tjänster)

| Inställning | Värde |
|-------------|-------|
| `JwtSettings:Issuer` | `ThenorSound` |
| `JwtSettings:Audience` | `ThenorSound` |
| `JwtSettings:SecretKey` | Sätts via user secrets / Azure env var |
| `JwtSettings:ExpirationMinutes` | `15` (endast Auth service) |

### Sätta upp lokalt (första gången)

```bash
# Auth service
cd Auth/API
dotnet user-secrets set "JwtSettings:SecretKey" "<minst 32 tecken>"
dotnet user-secrets set "MongoDbSettings:ConnectionString" "<cosmos-connection-string>"

# User service
cd User/API
dotnet user-secrets set "JwtSettings:SecretKey" "<samma nyckel>"

# Customer service
cd Customer/API
dotnet user-secrets set "JwtSettings:SecretKey" "<samma nyckel>"
```

### Azure miljövariabler

Alla tre tjänster behöver:
```
JwtSettings__SecretKey=<nyckel>
JwtSettings__Issuer=ThenorSound
JwtSettings__Audience=ThenorSound
```

---

## 6. Email-uppdatering (sync User → Auth)

När en användares email ändras i User service synkas den automatiskt till Auth service.

```
Frontend
  │  PUT /api/User/{id}  { email: "ny@exempel.se", ... }
  ▼
User service
  │  1. Kontrollerar att ny email inte är tagen (GetByEmailAsync)
  │  2. Sparar oldEmail = user.Email
  │  3. Uppdaterar user.email i MongoDB
  │  4. PUT /api/Auth/credentials/email  { oldEmail, newEmail }
  ▼
Auth service
  │  5. Hittar credential via oldEmail
  │  6. Kontrollerar att newEmail inte är tagen i credentials
  │  7. Uppdaterar credentials.email i MongoDB
```

---

## 7. Lazy migration — CustomerType

Användare skapade innan `CustomerType`-fältet lades till har `customerType: 0` i MongoDB.

Migreras automatiskt vid inloggning i `GetUserByEmailQueryHandler`:

```
1. Hämtar user via email
2. Om user.CustomerType == 0 OCH user.CustomerId är satt:
   a. GET /api/Customer/{id} → hämtar kund från Customer service
   b. user.CustomerType = customer.CustomerType
   c. Sparar uppdaterad user i MongoDB
3. Returnerar user (nu med korrekt customerType)
```

Användaren får korrekt `customerType` i JWT vid **nästa inloggning**.

---

## 8. Roller och behörigheter

### Roller (Role enum — User service + JWT)

| Roll      | Värde | Beskrivning                                            |
|-----------|-------|--------------------------------------------------------|
| Superuser | 1     | Fullständig åtkomst, hanteras via `/superusers`-sidan  |
| Admin     | 2     | Hanterar sin kunds användare och data                  |
| User      | 3     | Vanlig användare — **behörigheter ej spec:ade ännu**   |

### CustomerType (CustomerType enum — Customer + User service + JWT)

| Typ      | Värde | Beskrivning       |
|----------|-------|-------------------|
| Customer | 1     | Arrangör/bolag     |
| Crew     | 2     | Crewbolag          |

### Backend — User service behörigheter

| Åtgärd              | Superuser (1) | Admin (2)     | User (3)          |
|---------------------|---------------|---------------|-------------------|
| Skapa användare     | Alla kunder   | Egen kund     | ❌ Ej spec:at     |
| Uppdatera användare | Alla          | Egen kund     | ❌ Ej spec:at     |
| Ta bort användare   | Alla          | Egen kund     | ❌ Ej spec:at     |
| Byta CustomerId     | ✅ Ja         | ❌ Nej        | ❌ Nej            |
| Rensa CustomerId    | ✅ Ja (`""`)  | ❌ Nej        | ❌ Nej            |

### Frontend — UI-behörigheter (SideMenu + ManageCustomer)

| Variabel              | Villkor                      | Effekt                                      |
|-----------------------|------------------------------|---------------------------------------------|
| `canAccessAdmin`      | role === 1 eller role === 2  | Visar "Administratör" i sidomenyn           |
| `canAccessCrew`       | customerType === 2           | Visar "Crew"-sektionen i sidomenyn          |
| `canAccessOrganizer`  | customerType === 1           | Visar "Arrangör"-sektionen i sidomenyn      |
| `canAddUser`          | role === 1 eller role === 2  | Visar "Ny användare"-knapp + formulär       |
| `canEditUser(u)`      | role 1/2, eller u.email === loggedInUser.email | Visar Redigera-knapp      |
| `canDeleteUser(u)`    | role === 1 eller role === 2  | Visar Ta bort-knapp                         |

> Superusers länkade till en kund är **read-only** i ManageCustomers (visas men kan inte redigeras/tas bort).
>
> **TODO:** Rollbehörigheter för Role=3 (User) är inte spec:ade ännu.

---

## Öppna endpoints (kräver ingen JWT)

| Tjänst | Endpoint | Anledning |
|--------|----------|-----------|
| Auth | `POST /api/Auth/login` | Är inloggnings-endpoint |
| Auth | `POST /api/Auth/refresh` | Använder refresh token-cookie istället |
| Auth | `POST /api/Auth/logout` | Måste kunna nås utan giltigt access token |
| Auth | `POST /api/Auth/credentials` | Anropas av User service vid skapande av användare |
| Auth | `PUT /api/Auth/credentials/email` | Anropas av User service vid email-ändring |
| User | `GET /api/User/by-email/{email}` | Anropas av Auth service (service-to-service) |
| Customer | `GET /api/Customer/{id}` | Anropas av User service (service-to-service, inkl. lazy migration) |
