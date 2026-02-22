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

## 1. Registrera lösenord

Görs en gång per användare. Kräver att användaren redan finns i User service.

```
POST /api/Auth/credentials
{ "email": "anna@example.com", "password": "hemligt123" }
```

**Auth service:**
1. `GET /api/User/by-email/anna@example.com` — verifierar att användaren finns
2. Kontrollerar att credentials inte redan finns i MongoDB
3. `BCrypt.HashPassword("hemligt123", workFactor: 12)` → `"$2a$12$xyz..."`
4. Sparar `{ userId, email, passwordHash }` i `credentials`-collection

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

## Öppna endpoints (kräver ingen JWT)

| Tjänst | Endpoint | Anledning |
|--------|----------|-----------|
| Auth | `POST /api/Auth/login` | Är inloggnings-endpoint |
| Auth | `POST /api/Auth/refresh` | Använder refresh token-cookie istället |
| Auth | `POST /api/Auth/logout` | Måste kunna nås utan giltigt access token |
| Auth | `POST /api/Auth/credentials` | Registrering, anropas av admin innan login |
| User | `GET /api/User/by-email/{email}` | Anropas av Auth service (service-to-service) |
| Customer | `GET /api/Customer/{id}` | Anropas av User service (service-to-service) |
