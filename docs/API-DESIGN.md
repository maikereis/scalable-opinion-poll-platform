# API Design - Parrhesia

Design de APIs RESTful para o sistema de pesquisas de opinião.

---

## Base URL

```
Production: https://api.parrhesia.com/v1
Staging: https://api-staging.parrhesia.com/v1
```

## Global Headers

**Request Headers:**
```http
Content-Type: application/json
Accept: application/json
X-Client-Version: 1.0.0
X-Device-ID: <unique_device_identifier>
```

**Response Headers:**
```http
Content-Type: application/json
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640000000
X-Request-ID: <uuid>
```

---

## 1. Authentication & Authorization

> **Nota**: O login social (ex: Google) é obrigatório para submeter votos, garantindo o requisito funcional de voto único por usuário.

**Modelo de Usuários:**
- **Usuários Públicos (Votantes)**: Autenticam-se via Provedor Social (Google, X, Facebook, etc.) e recebem um JWT interno (`accessToken`) que deve ser usado no `Authorization: Bearer` para votar.
- **Usuários administrativos**: Usam um Bearer Token JWT (próprio do sistema) para operações administrativas.

**Header de Autenticação (Para Usuários Administrativos e Votantes):**

```http
Authorization: Bearer <jwt_token>
```

**Scopes/Permissions:**
- `public:vote` - Permite submeter votos e checar status de voto (obtido via login social)
- `survey:read` - Ler pesquisas
- `survey:write` - Criar/editar pesquisas
- `survey:delete` - Deletar pesquisas
- `results:read` - Visualizar resultados
- `results:export` - Exportar dados

### 1.1 Public Authentication (Voter Login)

### Login via Google OAuth

Este endpoint recebe o token de identidade (ID Token) emitido pelo Google e o troca por um JWT interno da Parrhesia, necessário para votação.

```http
POST /auth/social/login
```

**Request Body:**

```json
{
  "provider": "google", // O único provedor suportado nesta fase
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "state": "csrf_token_from_client"  // Prevenir CSRF attacks
}
```

**Response: 200 OK**

```json
{
  "accessToken": "<jwt_token_interno>", // O token a ser usado no header 'Authorization'
  "tokenType": "Bearer",
  "expiresIn": 3600, // Tempo de expiração em segundos (1 hora)
  "user": {
    "id": "user_id_from_parrhesia_db",
    "isNewUser": true, // Indica se é a primeira vez que o usuário vota
    "displayName": "Nome do Usuário"
  }
}
```

**Error Responses:**

```json
// 400 Bad Request
{
  "error": {
    "code": "INVALID_SOCIAL_TOKEN",
    "message": "The provided Google token is invalid or expired."
  }
}
```

---

## Resource Management Endpoints

### 1. Survey Management (Admin)

#### Create Survey
```http
POST /surveys
Authorization: Bearer <token> // Token Administrativo
```

**Request Body:**
```json
{
  "title": "Qual candidato você prefere para prefeito?",
  "description": "Pesquisa de intenção de voto para eleições municipais 2026",
  "startDate": "2026-08-01T00:00:00Z",
  "endDate": "2026-08-15T23:59:59Z",
  "options": [
    {
      "text": "Candidato A",
      "order": 1
    },
    {
      "text": "Candidato B",
      "order": 2
    },
    {
      "text": "Candidato C",
      "order": 3
    },
    {
      "text": "Branco/Nulo",
      "order": 4
    }
  ],
  "settings": {
    "allowMultipleVotes": false,
    "requireDeviceValidation": true,
    "maxVotesPerMinute": 1000
  }
}
```

**Response: 201 Created**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Qual candidato você prefere para prefeito?",
  "description": "Pesquisa de intenção de voto para eleições municipais 2026",
  "status": "draft",
  "startDate": "2026-08-01T00:00:00Z",
  "endDate": "2026-08-15T23:59:59Z",
  "createdAt": "2026-07-25T10:30:00Z",
  "updatedAt": "2026-07-25T10:30:00Z",
  "options": [
    {
      "id": "650e8400-e29b-41d4-a716-446655440001",
      "text": "Candidato A",
      "order": 1
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440002",
      "text": "Candidato B",
      "order": 2
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440003",
      "text": "Candidato C",
      "order": 3
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440004",
      "text": "Branco/Nulo",
      "order": 4
    }
  ],
  "settings": {
    "allowMultipleVotes": false,
    "requireDeviceValidation": true,
    "maxVotesPerMinute": 1000
  }
}
```

**Error Responses:**
```json
// 400 Bad Request
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Validation failed",
    "details": [
      {
        "field": "options",
        "message": "Must have at least 2 options"
      },
      {
        "field": "endDate",
        "message": "End date must be after start date"
      }
    ]
  }
}

// 401 Unauthorized
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Invalid or missing authentication token"
  }
}

// 403 Forbidden
{
  "error": {
    "code": "FORBIDDEN",
    "message": "Insufficient permissions to create surveys"
  }
}
```

---

#### Update Survey
```http
PATCH /surveys/{surveyId}
Authorization: Bearer <token> // Token Administrativo
```

**Request Body:**
```json
{
  "title": "Qual candidato você prefere para prefeito? (Atualizado)",
  "description": "Nova descrição"
}
```

**Response: 200 OK**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Qual candidato você prefere para prefeito? (Atualizado)",
  "description": "Nova descrição",
  "status": "draft",
  "updatedAt": "2026-07-26T14:20:00Z",
  ...
}
```

**Constraints:**
- Só pode atualizar pesquisas em status `draft`
- Não pode modificar pesquisas ativas ou encerradas

**Error Responses:**
```json
// 409 Conflict
{
  "error": {
    "code": "SURVEY_ALREADY_ACTIVE",
    "message": "Cannot modify survey after it has started"
  }
}
```

---

#### Activate Survey
```http
POST /surveys/{surveyId}/activate
Authorization: Bearer <token> // Token Administrativo
```

**Response: 200 OK**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "active",
  "activatedAt": "2026-08-01T00:00:00Z",
  "message": "Survey successfully activated"
}
```

---

#### Deactivate Survey
```http
POST /surveys/{surveyId}/deactivate
Authorization: Bearer <token> // Token Administrativo
```

**Response: 200 OK**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "closed",
  "closedAt": "2026-08-15T23:59:59Z",
  "message": "Survey successfully closed"
}
```

---

#### Delete Survey
```http
DELETE /surveys/{surveyId}
Authorization: Bearer <token> // Token Administrativo
```

**Response: 204 No Content**

**Constraints:**
- Só pode deletar pesquisas em status `draft`
- Pesquisas ativas ou com votos não podem ser deletadas (soft delete apenas)

---

## Data Retrieval Endpoints

### 2. Public Survey Access

#### Get Active Surveys
```http
GET /surveys/active
```

**Query Parameters:**
```
?limit=10          # Default: 10, Max: 50
&offset=0          # Default: 0
&sortBy=createdAt  # Options: createdAt, startDate, title
&order=desc        # Options: asc, desc
```

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "Qual candidato você prefere para prefeito?",
      "description": "Pesquisa de intenção de voto para eleições municipais 2026",
      "startDate": "2026-08-01T00:00:00Z",
      "endDate": "2026-08-15T23:59:59Z",
      "totalVotes": 45230,
      "options": [
        {
          "id": "650e8400-e29b-41d4-a716-446655440001",
          "text": "Candidato A",
          "order": 1
        },
        {
          "id": "650e8400-e29b-41d4-a716-446655440002",
          "text": "Candidato B",
          "order": 2
        }
      ]
    }
  ],
  "pagination": {
    "total": 3,
    "limit": 10,
    "offset": 0,
    "hasMore": false
  }
}
```

---

#### Get Survey Details
```http
GET /surveys/{surveyId}
```

**Response: 200 OK**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Qual candidato você prefere para prefeito?",
  "description": "Pesquisa de intenção de voto para eleições municipais 2026",
  "status": "active",
  "startDate": "2026-08-01T00:00:00Z",
  "endDate": "2026-08-15T23:59:59Z",
  "createdAt": "2026-07-25T10:30:00Z",
  "totalVotes": 45230,
  "options": [
    {
      "id": "650e8400-e29b-41d4-a716-446655440001",
      "text": "Candidato A",
      "order": 1
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440002",
      "text": "Candidato B",
      "order": 2
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440003",
      "text": "Candidato C",
      "order": 3
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440004",
      "text": "Branco/Nulo",
      "order": 4
    }
  ],
  "userVote": null  // ou { "optionId": "...", "votedAt": "..." } se já votou
}
```

**Error Responses:**
```json
// 404 Not Found
{
  "error": {
    "code": "SURVEY_NOT_FOUND",
    "message": "Survey with id 550e8400-e29b-41d4-a716-446655440000 not found"
  }
}
```

---

### 3. Voting

#### Submit Vote
```http
POST /surveys/{surveyId}/votes
Authorization: Bearer <token_publico> // obtido via /auth/social/login
```

**Request Body:**
```json
{
  "optionId": "650e8400-e29b-41d4-a716-446655440001",
  "metadata": {
    "userAgent": "Mozilla/5.0...",
    "platform": "mobile" | "desktop",
    "referrer": "https://accounts.google.com/", // pode ser facebook.com, x.com, google.com ou qualquer outro
    "timestamp": "2026-08-02T15:30:45Z"
  }
}
```

**Response: 201 Created**
```json
{
  "voteId": "750e8400-e29b-41d4-a716-446655440010",
  "surveyId": "550e8400-e29b-41d4-a716-446655440000",
  "optionId": "650e8400-e29b-41d4-a716-446655440001",
  "votedAt": "2026-08-02T15:30:45Z",
  "message": "Vote successfully recorded"
}
```

**Error Responses:**
```json
// 400 Bad Request
{
  "error": {
    "code": "INVALID_OPTION",
    "message": "Option does not belong to this survey"
  }
}

// 401 Unauthorized
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Missing or invalid authentication token. Please log in to vote."
  }
}

// 409 Conflict - Already Voted
{
  "error": {
    "code": "ALREADY_VOTED",
    "message": "You have already voted in this survey (based on User ID)",
    "existingVote": {
      "optionId": "650e8400-e29b-41d4-a716-446655440001",
      "votedAt": "2024-08-02T10:15:30Z"
    }
  }
}

// 410 Gone - Survey Closed
{
  "error": {
    "code": "SURVEY_CLOSED",
    "message": "This survey is no longer accepting votes",
    "closedAt": "2026-08-15T23:59:59Z"
  }
}

// 429 Too Many Requests
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many vote attempts. Please try again later.",
    "retryAfter": 30
  }
}

// 503 Service Unavailable - High Load
{
  "error": {
    "code": "SERVICE_OVERLOADED",
    "message": "System is experiencing high load. Please try again.",
    "retryAfter": 5
  }
}
```

---

#### Check Vote Status
```http
GET /surveys/{surveyId}/votes/me
Authorization: Bearer <token_publico>
```

**Response: 200 OK**
```json
{
  "hasVoted": true,
  "vote": {
    "optionId": "650e8400-e29b-41d4-a716-446655440001",
    "votedAt": "2026-08-02T15:30:45Z"
  }
}
```

**Response: 200 OK (Not Voted)**
```json
{
  "hasVoted": false,
  "vote": null
}
```

**Error Responses:**

```json
// 401 Unauthorized
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Missing or invalid authentication token. Please log in to vote."
  }
}
```
---

### 4. Results & Analytics (Admin)

#### Get Survey Results
```http
GET /surveys/{surveyId}/results
Authorization: Bearer <token> // Token Administrativo
```

**Query Parameters:**
```
?includeTimeSeries=false    # Include hourly breakdown
&includeMetadata=false      # Include vote metadata
&format=json                # Options: json, csv
```

**Response: 200 OK**
```json
{
  "surveyId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Qual candidato você prefere para prefeito?",
  "status": "active",
  "period": {
    "start": "2026-08-01T00:00:00Z",
    "end": "2026-08-15T23:59:59Z"
  },
  "summary": {
    "totalVotes": 45230,
    "uniqueVoters": 45230,
    "avgVotesPerHour": 1884,
    "peakVotingTime": "2026-08-01T20:00:00Z"
  },
  "results": [
    {
      "optionId": "650e8400-e29b-41d4-a716-446655440001",
      "text": "Candidato A",
      "votes": 18092,
      "percentage": 40.0,
      "rank": 1
    },
    {
      "optionId": "650e8400-e29b-41d4-a716-446655440002",
      "text": "Candidato B",
      "votes": 13569,
      "percentage": 30.0,
      "rank": 2
    },
    {
      "optionId": "650e8400-e29b-41d4-a716-446655440003",
      "text": "Candidato C",
      "votes": 9046,
      "percentage": 20.0,
      "rank": 3
    },
    {
      "optionId": "650e8400-e29b-41d4-a716-446655440004",
      "text": "Branco/Nulo",
      "votes": 4523,
      "percentage": 10.0,
      "rank": 4
    }
  ],
  "generatedAt": "2026-08-05T10:00:00Z"
}
```

---

#### Get Results with Time Series
```http
GET /surveys/{surveyId}/results?includeTimeSeries=true
Authorization: Bearer <token> // Token Administrativo
```

**Response: 200 OK**
```json
{
  "surveyId": "550e8400-e29b-41d4-a716-446655440000",
  "summary": { ... },
  "results": [ ... ],
  "timeSeries": {
    "interval": "1h",
    "data": [
      {
        "timestamp": "2026-08-01T00:00:00Z",
        "totalVotes": 2341,
        "breakdown": [
          { "optionId": "650e8400-e29b-41d4-a716-446655440001", "votes": 936 },
          { "optionId": "650e8400-e29b-41d4-a716-446655440002", "votes": 702 },
          { "optionId": "650e8400-e29b-41d4-a716-446655440003", "votes": 468 },
          { "optionId": "650e8400-e29b-41d4-a716-446655440004", "votes": 235 }
        ]
      },
      {
        "timestamp": "2026-08-01T01:00:00Z",
        "totalVotes": 1987,
        "breakdown": [ ... ]
      }
    ]
  }
}
```

---

#### Export Results
```http
GET /surveys/{surveyId}/results/export
Authorization: Bearer <token> // Token Administrativo
```

**Query Parameters:**
```
?format=csv            # Options: csv, json, xlsx
&includeRawVotes=false # Include individual vote records
```

**Response: 200 OK**
```
Content-Type: text/csv
Content-Disposition: attachment; filename="survey-results-550e8400.csv"

Option,Votes,Percentage,Rank
"Candidato A",18092,40.0,1
"Candidato B",13569,30.0,2
"Candidato C",9046,20.0,3
"Branco/Nulo",4523,10.0,4
```

---

#### Get All Surveys (Admin)
```http
GET /surveys
Authorization: Bearer <token> // Token Administrativo
```

**Query Parameters:**
```
?status=active         # Options: draft, active, closed, all
&limit=20
&offset=0
&sortBy=createdAt
&order=desc
```

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "Qual candidato você prefere para prefeito?",
      "status": "active",
      "startDate": "2026-08-01T00:00:00Z",
      "endDate": "2026-08-15T23:59:59Z",
      "totalVotes": 45230,
      "createdAt": "2026-07-25T10:30:00Z"
    }
  ],
  "pagination": {
    "total": 15,
    "limit": 20,
    "offset": 0,
    "hasMore": false
  }
}
```

---

## Real-time Communication Protocols

### Server-Sent Events (SSE) for Live Updates

#### Subscribe to Live Results
```http
GET /surveys/{surveyId}/results/live
Authorization: Bearer <token> // Token Administrativo
```

**Response: 200 OK**
```
Content-Type: text/event-stream
Cache-Control: no-cache
Connection: keep-alive

event: vote
data: {"optionId":"650e8400-e29b-41d4-a716-446655440001","totalVotes":45231}

event: vote
data: {"optionId":"650e8400-e29b-41d4-a716-446655440002","totalVotes":45232}

event: summary
data: {"totalVotes":45232,"results":[...]}

event: ping
data: {"timestamp":"2026-08-02T16:00:00Z"}
```

**Event Types:**
- `vote`: Nova votação computada
- `summary`: Atualização completa dos resultados (a cada 5 segundos)
- `ping`: Keepalive (a cada 30 segundos)
- `error`: Erro na conexão

**Client Implementation Example:**
```javascript
const eventSource = new EventSource('/surveys/{surveyId}/results/live');

eventSource.addEventListener('vote', (event) => {
  const data = JSON.parse(event.data);
  console.log('New vote:', data);
});

eventSource.addEventListener('summary', (event) => {
  const data = JSON.parse(event.data);
  updateDashboard(data);
});
```

---

### WebSocket Alternative (Optional)

**Connection:**
```
ws://[api.parrhesia.com/v1/ws](https://api.parrhesia.com/v1/ws)
```

**Authentication:**
```json
{
  "type": "auth",
  "token": "Bearer <jwt_token>"
}
```

**Subscribe to Survey:**
```json
{
  "type": "subscribe",
  "surveyId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Receive Updates:**
```json
{
  "type": "vote",
  "surveyId": "550e8400-e29b-41d4-a716-446655440000",
  "optionId": "650e8400-e29b-41d4-a716-446655440001",
  "totalVotes": 45231
}
```

---

## Pagination & Filtering Strategies

### Cursor-Based Pagination (Recommended for Time-Series Data)

**For Vote History:**
```http
GET /surveys/{surveyId}/votes?cursor=eyJpZCI6MTIzNDU2&limit=100
Authorization: Bearer <token>
```

**Response:**
```json
{
  "data": [ ... ],
  "pagination": {
    "nextCursor": "eyJpZCI6MTIzNTU2fQ==",
    "hasMore": true
  }
}
```

**Benefits:**
- Consistent results even with new data
- Better performance for large datasets
- Ideal for real-time data streams

---

### Offset-Based Pagination (For Survey Lists)

```http
GET /surveys?limit=20&offset=40 // Para usuários admin, exige auth header
```

**Response:**
```json
{
  "data": [ ... ],
  "pagination": {
    "total": 150,
    "limit": 20,
    "offset": 40,
    "totalPages": 8,
    "currentPage": 3,
    "hasMore": true
  }
}
```

**Benefits:**
- Simpler to implement
- Shows total count
- Better for UIs with page numbers

---

### Filtering

**Multiple Filters:**
```http
GET /surveys?status=active&createdAfter=2024-08-01&createdBefore=2024-08-31&search=prefeito
```

**Supported Operators:**
- `eq`: Equals (default)
- `ne`: Not equals
- `gt`: Greater than
- `gte`: Greater than or equal
- `lt`: Less than
- `lte`: Less than or equal
- `in`: In list
- `contains`: String contains

**Complex Filter Example:**
```http
GET /surveys?totalVotes[gte]=10000&status[in]=active,closed&title[contains]=eleições
```

---

### Sorting

**Multiple Sort Fields:**
```http
GET /surveys?sortBy=totalVotes,createdAt&order=desc,asc
```

**Default Sorting:**
- Lists: `createdAt desc`
- Results: `votes desc` (ranking)

---

## Rate Limiting

### Rate Limit Rules

**Public Endpoints (Não Autenticados - GET /surveys/active):**
- Visualização de pesquisa (GET /surveys/active, GET /surveys/{id}): Ilimitado (via CDN)
- Rate limit geral: 100 requests/minute por IP

**Voting Endpoints (Autenticados - POST /surveys/{id}/votes):**
- Voting: 1 vote per survey per User ID (garantido pela lógica de negócio)
- Rate limit de tentativas: 5 attempts/minute per User ID (previne spam)
- Note: O 1 voto/30s era para prevenir múltiplos votos, mas com User ID único por survey, isso é desnecessário

**Admin Endpoints (Autenticados):**
- General: 1000 requests/minute per token
- Bulk operations: 10 requests/minute
- Export: 5 requests/minute

### Rate Limit Headers

```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1640995200
Retry-After: 30
```

### Rate Limit Response

```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Please try again later.",
    "retryAfter": 30,
    "limit": 100,
    "window": "1 minute"
  }
}
```

---

## Error Handling

### Standard Error Response Format

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": [ ... ],  // Optional
    "requestId": "req_550e8400",
    "timestamp": "2026-08-02T16:00:00Z"
  }
}
```

### HTTP Status Codes

| Code | Meaning | Use Case |
|------|---------|----------|
| 200 | OK | Successful GET, PATCH |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Missing/invalid auth token (Admin ou Público) |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate vote, invalid state |
| 410 | Gone | Survey closed/expired |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | System overload |

---

## Versioning Strategy

**URL Versioning:**
```
/v1/surveys
/v2/surveys  (future)
```

**Benefits:**
- Clear and explicit
- Easy to maintain multiple versions
- Simple routing

**Deprecation:**
- Minimum 6 months notice
- Sunset header: `Sunset: Sat, 31 Dec 2026 23:59:59 GMT`
- Deprecation warnings in responses

---

## Idempotency

### Idempotent Operations

**POST requests with Idempotency-Key:**
```http
POST /surveys/{surveyId}/votes
Authorization: Bearer <token_publico>
Idempotency-Key: unique-key-550e8400
```

**Response for duplicate request:**
```http
200 OK
Idempotent-Replayed: true
```

```json
{
  "voteId": "750e8400-e29b-41d4-a716-446655440010",
  "message": "Vote already recorded (idempotent response)"
}
```

**Key Requirements:**
- Client-generated UUID
- Scoped to: User ID + Survey ID (diferentes usuários podem usar mesma key)
- Stored for 24 hours
- Returns same response for same (User ID + Survey ID + Idempotency Key) tuple

---

## CORS Configuration

```http
Access-Control-Allow-Origin: [https://parrhesia.com](https://parrhesia.com)
Access-Control-Allow-Methods: GET, POST, PATCH, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, X-Device-ID
Access-Control-Max-Age: 86400
```

---

## Compression

**Compression Policy:**
- Text-based responses (JSON, CSV): compress if > 1KB
- Binary responses (if any): no compression
- Compression algorithms: gzip (priority), br (fallback)
- Level: 6 (balance between speed and ratio)

**Supported:**
```http
Accept-Encoding: gzip, deflate, br
Content-Encoding: gzip
```

**Always compress responses > 1KB**

---

## Caching Strategy

### Cache-Control Headers

**Static Survey Data:**
```http
Cache-Control: public, max-age=300, stale-while-revalidate=60
ETag: "550e8400-v1"
```

**Live Results:**
```http
Cache-Control: no-cache, must-revalidate
```

**Historical Results:**
```http
Cache-Control: public, max-age=86400, immutable
```

### Conditional Requests

```http
GET /surveys/{surveyId}
If-None-Match: "550e8400-v1"
```

**Response: 304 Not Modified** (if unchanged)

---

## API Client SDKs (Recommended)

### JavaScript/TypeScript
```javascript
import { ParrhesiaClient } from '@parrhesia/sdk';

const client = new ParrhesiaClient({
  apiKey: 'your-api-key',
  environment: 'production'
});

// 1. Social Login
const authResponse = await client.auth.socialLogin('google', 'google-id-token');
client.setAccessToken(authResponse.accessToken);

// 2. Vote
await client.surveys.vote('survey-id', 'option-id');

// 3. Get results (Admin only)
const results = await client.surveys.getResults('survey-id');
```

### C# (.NET)
```csharp
var client = new ParrhesiaClient("your-api-key");

// 1. Social Login
var authResponse = await client.Auth.SocialLoginAsync("google", "google-id-token");
client.SetAccessToken(authResponse.AccessToken);

// 2. Vote
await client.Surveys.VoteAsync(surveyId, optionId);

// 3. Get results (Admin only)
var results = await client.Surveys.GetResultsAsync(surveyId);
```