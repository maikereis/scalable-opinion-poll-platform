# Parrhesia Domain Design

## Design Estratégico

### 1. Linguagem Ubíqua

| Termo | Definição |
|-------|-----------|
| **Pesquisa (Survey)** | Conjunto de perguntas sobre um tema, com período definido de coleta |
| **Pergunta (Question)** | Pergunta que ajuda determinar opinião do votante |
| **Opção (Option)** | Alternativa que pode ser selecionada pelo votante |
| **Voto (Vote)** | Ato de escolher uma opção |
| **Votante (Voter)** | Usuário autenticado que participa |
| **Cédula (Ballot)** | Registro anônimo de voto computado |
| **Fingerprint** | Hash que identifica participação sem revelar identidade |
| **Período de Coleta** | Intervalo em que pesquisa aceita votos |
| **Resultado (Result)** | Agregação estatística dos votos |

### 2. Identificação de Subdomínios

| Tipo | Contexto | Responsabilidade |
|------|----------|------------------|
| **Core Domain** | Voting | Registro de votos com unicidade e anonimato |
| **Supporting** | Survey Management | Ciclo de vida das pesquisas |
| **Supporting** | Analytics & Reporting | Agregação de resultados e relatórios |
| **Generic** | Identity & Access | Autenticação social e controle de acesso |

### 3. Definição de Contextos Delimitados

#### Survey Management Context
- **Responsabilidade**: Gerenciar ciclo de vida completo das pesquisas
- **Linguagem**: Survey, Question, Option, Draft, Active, Closed
- **Fronteira**: Não conhece votos individuais

#### Voting Context
- **Responsabilidade**: Garantir votação única por usuário e anonimato
- **Linguagem**: Vote, Ballot, Fingerprint, Voter
- **Fronteira**: Recebe SurveyId, não gerencia ciclo de vida

#### Analytics & Reporting Context
- **Responsabilidade**: Consolidar estatísticas e relatórios
- **Linguagem**: Result, Summary, Percentage, TotalVotes
- **Fronteira**: Read-model, consome eventos

#### Identity & Access Context
- **Responsabilidade**: Autenticação e autorização
- **Linguagem**: User, Token, Permission, Scope
- **Fronteira**: Fornece identidade verificada

### 4. Mapa de Contexto

```
                                                            ┌───────────────────────────────┐
                                                            │                               │
                                                            │     Analytics & Reporting     │
               ┌─────────[Customer/Supplier]────────────────│           Context             │
               │                                            │        (Read Model)           │
               │                                            │                               │
               │                                            └───────────────┬───────────────┘
               │                                                            │                
               │                                                   [Customer/Supplier]       
               │                                                            │                
               │                                                            │                
┌──────────────┴───────────────┐                             ┌──────────────▼───────────────┐
│                              │                             │                              │
│                              │                             │                              │
│       Voting Context         │────[Customer/Supplier]──────►    Identity & Access         │
│        (Core Domain)         │                             │         Context              │
│                              │                             │                              │
│                              │                             │                              │
└──────────────▲───────────────┘                             └──────────────▲───────────────┘
               │                                                            │                
               │                                                            │                
               │                                                   [Customer/Supplier]       
      [Customer/Supplier]                                                   │                
               │                                             ┌──────────────┴───────────────┐
               │                                             │                              │
               │                                             │                              │
               │                                             │     Survey Management        │
               └─────────────────────────────────────────────│          Context             │
                                                             │                              │
                                                             │                              │
                                                             └──────────────────────────────┘
```

---

## Design Tático

### 5. Entidades e Objetos de Valor

**Princípios de Encapsulamento:**
- Aggregate Root controla coleções (nunca expõe `List<>.Add()`)
- Value Objects criados via Factory Pattern (construtor privado)
- Validações no momento da criação
- Coleções expostas como `IReadOnlyList<>`

#### Survey Management Context

```
┌─────────────────────────────────────────────────────────────────┐
│                    SURVEY (Aggregate Root)                      │
├─────────────────────────────────────────────────────────────────┤
│  - Id: SurveyId                                                 │
│  - Title: SurveyTitle                                           │
│  - Description: string                                          │
│  - Status: SurveyStatus                                         │
│  - CollectionPeriod: CollectionPeriod                           │
│  - _questions: List<Question>         [backing field privado]   │
│  - _options: List<Option>             [backing field privado]   │
│  - Settings: SurveySettings                                     │
│  - CreatedAt: DateTime                                          │
│  - UpdatedAt: DateTime                                          │
├─────────────────────────────────────────────────────────────────┤
│  + Questions: IReadOnlyList<Question>   [propriedade read-only] │
│  + Options: IReadOnlyList<Option>       [propriedade read-only] │
├─────────────────────────────────────────────────────────────────┤
│  FACTORY:                                                       │
│  + static Create(title, description, period, settings): Survey  │
│                                                                 │
│  MÉTODOS QUE CONTROLAM COLEÇÕES:                                │
│  + AddQuestion(text, order): void                               │
│  + AddOption(questionId, text, order): void                     │
│  + RemoveQuestion(questionId): void                             │
│                                                                 │
│  MÉTODOS DE CICLO DE VIDA:                                      │
│  + Activate(): void                                             │
│  + Close(): void                                                │
│  + IsAcceptingVotes(): bool                                     │
│                                                                 │
│  INVARIANTES PROTEGIDOS:                                        │
│  - Não pode modificar após Status = Active                      │
│  - Deve ter pelo menos 1 Question                               │
│  - Deve ter pelo menos 2 Options                                │
└─────────────────────────────────────────────────────────────────┘
           │
           │ contém 1..*
           ▼
┌─────────────────────────────────────────────────────────────────┐
│                        QUESTION (Entity)                        │
├─────────────────────────────────────────────────────────────────┤
│  - Id: QuestionId                                               │
│  - Text: QuestionText                                           │
│  - Order: int                                                   │
├─────────────────────────────────────────────────────────────────┤
│  FACTORY:                                                       │
│  + static Create(text, order): Question                         │
│                                                                 │
│  INVARIANTES:                                                   │
│  - Order >= 0                                                   │
│  - Text não vazio, max 500 chars                                │
└─────────────────────────────────────────────────────────────────┘
           │
           │ tem 2..10
           ▼
┌─────────────────────────────────────────────────────────────────┐
│                        OPTION (Entity)                          │
├─────────────────────────────────────────────────────────────────┤
│  - Id: OptionId                                                 │
│  - QuestionId: QuestionId                                       │
│  - Text: OptionText                                             │
│  - Order: int                                                   │
├─────────────────────────────────────────────────────────────────┤
│  FACTORY:                                                       │
│  + static Create(questionId, text, order): Option               │
│                                                                 │
│  INVARIANTES:                                                   │
│  - Order >= 0                                                   │
│  - Text não vazio, max 200 chars                                │
└─────────────────────────────────────────────────────────────────┘
```

**Value Objects - Survey Context:**

```
┌────────────────────────────────────────────────────────────────┐
│ SurveyId                                                       │
├────────────────────────────────────────────────────────────────┤
│  - Value: Guid                                                 │
├────────────────────────────────────────────────────────────────┤
│  + static NewId(): SurveyId                                    │
│  + static Create(value): SurveyId                              │
│  Validação: value != Guid.Empty                                │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ SurveyTitle                                                    │
├────────────────────────────────────────────────────────────────┤
│  - Value: string                                               │
├────────────────────────────────────────────────────────────────┤
│  + static Create(value): SurveyTitle                           │
│  Validações:                                                   │
│  - Não vazio                                                   │
│  - Max 500 caracteres                                          │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ SurveyStatus (Enumeration)                                     │
├────────────────────────────────────────────────────────────────┤
│  - Value: string                                               │
├────────────────────────────────────────────────────────────────┤
│  + static Draft: SurveyStatus                                  │
│  + static Active: SurveyStatus                                 │
│  + static Closed: SurveyStatus                                 │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ CollectionPeriod                                               │
├────────────────────────────────────────────────────────────────┤
│  - StartDate: DateTime                                         │
│  - EndDate: DateTime                                           │
├────────────────────────────────────────────────────────────────┤
│  + static Create(startDate, endDate): CollectionPeriod         │
│  + IsActive(now): bool                                         │
│  + HasEnded(now): bool                                         │
│  Validações:                                                   │
│  - EndDate > StartDate                                         │
│  - Duração: 1-30 dias                                          │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ QuestionText                                                   │
├────────────────────────────────────────────────────────────────┤
│  - Value: string                                               │
├────────────────────────────────────────────────────────────────┤
│  + static Create(value): QuestionText                          │
│  Validações: Não vazio, max 500 chars                          │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ OptionText                                                     │
├────────────────────────────────────────────────────────────────┤
│  - Value: string                                               │
├────────────────────────────────────────────────────────────────┤
│  + static Create(value): OptionText                            │
│  Validações: Não vazio, max 200 chars                          │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ SurveySettings                                                 │
├────────────────────────────────────────────────────────────────┤
│  - RequireDeviceValidation: bool                               │
│  - MaxVotesPerMinute: int                                      │
├────────────────────────────────────────────────────────────────┤
│  + static Create(requireDevice, maxVotes): SurveySettings      │
│  + static Default(): SurveySettings                            │
│  Validações: MaxVotesPerMinute > 0                             │
└────────────────────────────────────────────────────────────────┘
```

---

#### Voting Context

```
┌────────────────────────────────────────────────────────────────┐
│                   BALLOT (Aggregate Root)                      │
│                         [IMUTÁVEL]                             │
├────────────────────────────────────────────────────────────────┤
│  - Id: BallotId                                                │
│  - SurveyId: SurveyId                                          │
│  - QuestionId: QuestionId                                      │
│  - SelectedOptionId: OptionId                                  │
│  - VoterFingerprint: VoterFingerprint                          │
│  - CastedAt: DateTime                                          │
│  - DeviceInfo: DeviceInfo                                      │
├────────────────────────────────────────────────────────────────┤
│  FACTORY (único meio de criar):                                │
│  + static Cast(surveyId, questionId, optionId,                 │
│                fingerprint, deviceInfo): Ballot                │
│                                                                │
│  INVARIANTES:                                                  │
│  - Todos os IDs são obrigatórios                               │
│  - VoterFingerprint é obrigatório                              │
│  - Imutável após criação                                       │
└────────────────────────────────────────────────────────────────┘
```

**Value Objects - Voting Context:**

```
┌────────────────────────────────────────────────────────────────┐
│ VoterFingerprint                                               │
├────────────────────────────────────────────────────────────────┤
│  - Value: string (SHA256 hash em hex)                          │
├────────────────────────────────────────────────────────────────┤
│  + static Create(value): VoterFingerprint                      │
│  Validações:                                                   │
│  - Não vazio                                                   │
│  - Exatamente 64 caracteres                                    │
│  - Formato: [0-9A-F]{64}                                       │
│                                                                │
│  Geração: SHA256(UserId + SurveyId + SystemSalt)               │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│ DeviceInfo                                                     │
├────────────────────────────────────────────────────────────────┤
│  - DeviceId: string                                            │
│  - UserAgent: string                                           │
│  - IpHash: string (SHA256 do IP)                               │
├────────────────────────────────────────────────────────────────┤
│  + static Create(deviceId, userAgent, ipAddress): DeviceInfo   │
│  Validações:                                                   │
│  - DeviceId obrigatório                                        │
│  - IP hasheado para anonimização                               │
└────────────────────────────────────────────────────────────────┘
```

---

#### Analytics Context (Read Model)

```
┌────────────────────────────────────────────────────────────────┐
│                   SURVEY_RESULT (Read Model)                   │
├────────────────────────────────────────────────────────────────┤
│  - SurveyId: Guid                                              │
│  - TotalVotes: long                                            │
│  - LastUpdatedAt: DateTime                                     │
│  - QuestionResults: List<QuestionResult>                       │
└────────────────────────────────────────────────────────────────┘
           │
           │ contém
           ▼
┌────────────────────────────────────────────────────────────────┐
│                       QUESTION_RESULT                          │
├────────────────────────────────────────────────────────────────┤
│  - QuestionId: Guid                                            │
│  - TotalVotes: long                                            │
│  - OptionResults: List<OptionResult>                           │
└────────────────────────────────────────────────────────────────┘
           │
           │ contém
           ▼
┌────────────────────────────────────────────────────────────────┐
│                        OPTION_RESULT                           │
├────────────────────────────────────────────────────────────────┤
│  - OptionId: Guid                                              │
│  - VoteCount: long                                             │
│  - Percentage: decimal                                         │
└────────────────────────────────────────────────────────────────┘
```

---

### 6. Agregados

#### Survey Aggregate (Survey Management Context)

```
┌────────────────────────────────────────────────────────────────┐
│                        SURVEY AGGREGATE                        │
├────────────────────────────────────────────────────────────────┤
│  Root: Survey                                                  │
│  Entities: Question, Option                                    │
│  Boundary: Todas operações passam pela Survey                  │
├────────────────────────────────────────────────────────────────┤
│  INVARIANTES:                                                  │
│  ├─ Survey deve ter pelo menos 1 Question                      │
│  ├─ Question deve ter entre 2 e 10 Options                     │
│  ├─ Não pode modificar Survey após status = Active             │
│  ├─ CollectionPeriod.EndDate > StartDate                       │
│  ├─ CollectionPeriod duração entre 1 e 30 dias                 │
│  └─ Máximo 5 Surveys ativas simultaneamente (regra global)     │
└────────────────────────────────────────────────────────────────┘
```

#### Ballot Aggregate (Voting Context)

```
┌────────────────────────────────────────────────────────────────┐
│                        BALLOT AGGREGATE                        │
├────────────────────────────────────────────────────────────────┤
│  Root: Ballot                                                  │
│  Entities: (nenhuma - agregado simples)                        │
│  Boundary: Imutável após criação                               │
├────────────────────────────────────────────────────────────────┤
│  INVARIANTES:                                                  │
│  ├─ VoterFingerprint único por SurveyId                        │
│  ├─ SelectedOptionId deve pertencer ao QuestionId              │
│  ├─ Survey deve estar com status Active                        │
│  └─ CastedAt dentro do CollectionPeriod                        │
└────────────────────────────────────────────────────────────────┘
```

---

### 7. Repositórios

#### Survey Management Context

```
┌────────────────────────────────────────────────────────────────┐
│                      ISurveyRepository                         │
├────────────────────────────────────────────────────────────────┤
│  + GetByIdAsync(id): Task<Survey>                              │
│  + GetActiveAsync(): Task<IReadOnlyList<Survey>>               │
│  + CountActiveAsync(): Task<int>                               │
│  + AddAsync(survey): Task                                      │
│  + UpdateAsync(survey): Task                                   │
│  + ExistsAsync(id): Task<bool>                                 │
└────────────────────────────────────────────────────────────────┘
```

#### Voting Context

```
┌────────────────────────────────────────────────────────────────┐
│                      IBallotRepository                         │
├────────────────────────────────────────────────────────────────┤
│  + AddAsync(ballot): Task                                      │
│  + HasVotedAsync(fingerprint, surveyId): Task<bool>            │
│  + CountBySurveyAsync(surveyId): Task<long>                    │
│  + CountByOptionAsync(optionId): Task<long>                    │
└────────────────────────────────────────────────────────────────┘
```

#### Analytics Context

```
┌─────────────────────────────────────────────────────────────────┐
│                  ISurveyResultRepository                        │
├─────────────────────────────────────────────────────────────────┤
│  + GetBySurveyIdAsync(surveyId): Task<SurveyResult>             │
│  + UpsertAsync(result): Task                                    │
│  + IncrementVoteCountAsync(surveyId, questionId, optionId): Task│
└─────────────────────────────────────────────────────────────────┘
```

---

### 8. Serviços de Domínio

**Conceito**: Encapsulam regras de negócio que não pertencem a uma entidade ou coordenam múltiplos agregados.

```
┌────────────────────────────────────────────────────────────────┐
│                    VotingService                               │
│                  (Voting Context)                              │
├────────────────────────────────────────────────────────────────┤
│  Responsabilidade:                                             │
│  - Coordenar votação garantindo unicidade e anonimato          │
│                                                                │
│  Por que é Domain Service?                                     │
│  - Regra "1 voto por usuário" cruza agregado Ballot            │
│  - Verificação de Survey está em outro contexto                │
│  - Geração de fingerprint não pertence ao Ballot               │
├────────────────────────────────────────────────────────────────┤
│  + CastVoteAsync(userId, surveyId, questionId,                 │
│                  optionId, deviceInfo): Task<Result<Ballot>>   │
│                                                                │
│  Fluxo:                                                        │
│  1. Verificar se survey aceita votos (via ACL)                 │
│  2. Validar se option é válida                                 │
│  3. Gerar fingerprint                                          │
│  4. Verificar duplicidade                                      │
│  5. Criar Ballot                                               │
│  6. Persistir                                                  │
│  7. Publicar evento BallotCasted                               │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│                  FingerprintGenerator                          │
│                  (Voting Context)                              │
├────────────────────────────────────────────────────────────────┤
│  Responsabilidade:                                             │
│  - Gerar hash unidirecional para anonimização                  │
│                                                                │
│  Por que é Domain Service?                                     │
│  - É lógica de domínio (regra de anonimato)                    │
│  - Não pertence ao Ballot                                      │
│  - Requer conhecimento criptográfico (SHA256)                  │
├────────────────────────────────────────────────────────────────┤
│  + Generate(userId, surveyId): VoterFingerprint                │
│                                                                │
│  Implementação:                                                │
│  - SHA256(UserId:SurveyId:SystemSalt)                          │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│               SurveyActivationService                          │
│            (Survey Management Context)                         │
├────────────────────────────────────────────────────────────────┤
│  Responsabilidade:                                             │
│  - Garantir regras globais de ativação                         │
│                                                                │
│  Por que é Domain Service?                                     │
│  - Regra "máximo 5 surveys ativas" é global                    │
│  - Coordena validações que cruzam múltiplas instâncias         │
├────────────────────────────────────────────────────────────────┤
│  + ActivateSurveyAsync(surveyId): Task<Result>                 │
│                                                                │
│  Fluxo:                                                        │
│  1. Recuperar survey                                           │
│  2. Verificar se já está ativa                                 │
│  3. Validar regra global (máx 5 ativas)                        │
│  4. Validar survey está pronta                                 │
│  5. Ativar (método do agregado)                                │
│  6. Persistir                                                  │
│  7. Publicar evento SurveyActivated                            │
└────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                 ISurveyQueryService                             │
│              (Anti-Corruption Layer)                            │
├─────────────────────────────────────────────────────────────────┤
│  Responsabilidade:                                              │
│  - Proteger Voting Context de mudanças no Survey Context        │
│                                                                 │
│  Interface definida em: Voting Context                          │
│  Implementação em: Infrastructure                               │
├─────────────────────────────────────────────────────────────────┤
│  + GetSurveyStatusAsync(surveyId): Task<SurveyStatusDto>        │
│                                                                 │
│  SurveyStatusDto:                                               │
│  - IsActive: bool                                               │
│  - QuestionsWithOptions: Dictionary<QuestionId, List<OptionId>> │
│  - HasOption(questionId, optionId): bool                        │
└─────────────────────────────────────────────────────────────────┘
```

**Domain Services vs Application Services:**

| Aspecto | Domain Service | Application Service |
|---------|---------------|---------------------|
| **Camada** | Domain | Application |
| **Contém** | Lógica de negócio | Orquestração de casos de uso |
| **Conhece** | Agregados, VOs, Events | Domain Services, Repositories |
| **Exemplo** | VotingService | CastVoteUseCase |
| **Testabilidade** | Testes unitários puros | Testes de integração |

---

### 9. Eventos de Domínio

#### Survey Management Context

| Evento | Payload | Consumidores |
|--------|---------|--------------|
| `SurveyCreated` | SurveyId, Title, Questions[], CollectionPeriod | Analytics |
| `SurveyActivated` | SurveyId, ActivatedAt | Analytics, Voting |
| `SurveyClosed` | SurveyId, ClosedAt, Reason | Analytics, Voting |
| `SurveyUpdated` | SurveyId, ChangedFields | Analytics |

#### Voting Context

| Evento | Payload | Consumidores |
|--------|---------|--------------|
| `BallotCasted` | BallotId, SurveyId, QuestionId, OptionId, CastedAt | Analytics |
| `DuplicateVoteAttempted` | SurveyId, VoterFingerprint, AttemptedAt | Monitoring |

#### Fluxo de Eventos

```           
┌─────────────┐     SurveyActivated      ┌─────────────┐                                                   
│   Survey    │                          │  Analytics  │                                                   
│  Management ┼──────────────────────────►   Context   │                                                   
│             │                          │             │                                                   
└─────────────┘                          └──────▲──────┘                                                   
                                                │ BallotCasted                                             
                                                │                                                          
┌─────────────┐     BallotCasted         ┌──────┼──────┐                                                   
│   Voting    │                          │   Message   │                                                   
│   Context   ┼──────────────────────────►    Broker   │                                                   
│             │                          │             │                                                   
└─────────────┘                          └─────────────┘                                                   
```

---

## Apêndice: Decisões de Design

### Por que Ballot é Aggregate separado?

1. **Escala**: 100k votos/min causaria contenção massiva se Vote fosse parte de Survey
2. **Anonimato**: Separação física facilita garantir que não há link entre voto e votante
3. **Imutabilidade**: Ballots nunca são alterados após criação

### Por que usar Fingerprint ao invés de UserId?

1. **Anonimato**: Requisito explícito do negócio
2. **LGPD**: Minimização de dados pessoais
3. **Auditoria**: Ainda permite detectar tentativas de fraude sem identificar indivíduos

### Por que Analytics é Read Model?

1. **Performance**: Evita N+1 queries para calcular percentuais
2. **Consistência Eventual**: Requisito permite delay de 5s
3. **Escalabilidade**: Pode ser materializado em banco otimizado para leitura (Redis, ElasticSearch)

### Consistência: Forte vs Eventual

| Operação | Tipo | Justificativa |
|----------|------|---------------|
| Registro de voto | Forte | Voto computado não pode ser perdido |
| Verificação de duplicidade | Forte | Não pode permitir voto duplicado |
| Contadores de resultado | Eventual | Requisito permite delay de 5s |
| Visualização de pesquisa | Eventual | Pode cachear |

### Padrões Aplicados

1. **Factory Pattern**: Construtores privados + métodos estáticos `Create()`
2. **Aggregate Root**: Controla coleções internas via métodos públicos
3. **Value Object**: Imutável, comparação por valor, sem identidade
4. **Repository**: Apenas para Aggregate Roots
5. **Domain Service**: Lógica de negócio que cruza agregados
6. **Anti-Corruption Layer**: Protege core domain de contextos externos