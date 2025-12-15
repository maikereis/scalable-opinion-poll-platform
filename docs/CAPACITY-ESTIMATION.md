# Capacity Estimation - Parrhesia

Estimativas de capacidade para o sistema de pesquisas de opinião.

---

## User Metrics

### DAU/MAU Ratios

**Cenário: Pesquisa viral nas redes sociais durante período eleitoral**

- **MAU (Monthly Active Users)**: 10 milhões
- **DAU (Daily Active Users)**: 2 milhões (durante pico de campanha)
- **DAU/MAU ratio**: 20% (típico para conteúdo viral pontual)

### User Behavior Patterns

**Distribuição temporal:**
- Pico de acesso: primeiras 24-48h após divulgação
- 60% dos acessos nas primeiras 24h
- 25% dos acessos entre 24-48h
- 15% dos acessos restantes (dias 3-7)

**Taxa de conversão (visualização → voto):**
- 5% dos visitantes efetivamente votam
- 95% apenas visualizam e saem

**Padrões de horário:**
- Pico: 18h-23h (horário de Brasília) - 40% do tráfego diário
- Médio: 12h-18h - 35% do tráfego diário
- Baixo: 0h-12h - 25% do tráfego diário

---

## Throughput

### Write Operations (Votos)

**Média diária:**
- DAU: 2 milhões
- Taxa de conversão: 5%
- Votos por dia: 2M × 5% = **100.000 votos/dia**
- Votos por segundo (média): 100.000 / 86.400 = **~1,2 votos/segundo**

**Pico (primeiras 24h + horário nobre):**
- 60% dos votos em 24h = 60.000 votos
- 40% desses votos no horário de pico (5 horas): 24.000 votos
- Votos por segundo (pico): 24.000 / 18.000 = **~1,3 votos/segundo**
- **Com margem de segurança (3x)**: ~**4 votos/segundo**
- **Pico absoluto (viral spike - 10x)**: ~**40 votos/segundo**

### Read Operations (Visualizações)

**Média diária:**
- DAU: 2 milhões
- Cada usuário carrega o formulário: 1 vez
- Leituras por dia: **2 milhões/dia**
- Leituras por segundo (média): 2M / 86.400 = **~23 reads/segundo**

**Pico (primeiras 24h + horário nobre):**
- 60% dos acessos em 24h: 1,2 milhão
- 40% no horário de pico (5 horas): 480.000 acessos
- Leituras por segundo (pico): 480.000 / 18.000 = **~27 reads/segundo**
- **Com margem de segurança (5x)**: ~**135 reads/segundo**
- **Pico absoluto (viral spike - 20x)**: ~**2.700 reads/segundo**

### Read:Write Ratio

- **Média**: 23:1,2 ≈ **19:1**
- **Pico**: 27:1,3 ≈ **21:1**

Sistema claramente **read-heavy** (esperado para pesquisas públicas).

---

## Storage

### Data Types and Sizes

**1. Pesquisa (Survey)**
```
- id: GUID (16 bytes)
- title: VARCHAR(500) (~500 bytes)
- created_at: DATETIME (8 bytes)
- start_date: DATETIME (8 bytes)
- end_date: DATETIME (8 bytes)
- status: TINYINT (1 byte)
- created_by: GUID (16 bytes)

Total por pesquisa: ~557 bytes ≈ 0,5 KB
```

**2. Opção de Resposta (Survey Option)**
```
- id: GUID (16 bytes)
- survey_id: GUID (16 bytes)
- option_text: VARCHAR(200) (~200 bytes)
- order: INT (4 bytes)

Total por opção: ~236 bytes ≈ 0,25 KB
Por pesquisa (média 5 opções): 1,25 KB
```

**3. Voto (Vote)**
```
- id: GUID (16 bytes)
- survey_id: GUID (16 bytes)
- option_id: GUID (16 bytes)
- user_identifier: VARCHAR(64) (64 bytes - hash de IP/device)
- voted_at: DATETIME (8 bytes)
- metadata: JSON (~100 bytes - user agent, etc)

Total por voto: ~220 bytes ≈ 0,22 KB
```

**4. Índices (estimativa)**
- Índices em survey_id, option_id, user_identifier, voted_at
- Overhead de índices: ~30% do tamanho dos dados

### Storage Requirements

**Cenário: 1 ano de operação**

**Pesquisas:**
- 5 pesquisas ativas simultâneas
- Rotação mensal: 60 pesquisas/ano
- Storage: 60 × 0,5 KB = **30 KB** (negligível)

**Opções:**
- 60 pesquisas × 5 opções = 300 opções
- Storage: 300 × 0,25 KB = **75 KB** (negligível)

**Votos:**
- 100.000 votos/dia × 365 dias = 36,5 milhões de votos/ano
- Storage base: 36,5M × 0,22 KB = **8,03 GB**
- Com índices (30%): 8,03 × 1,3 = **10,4 GB/ano**

**Total Storage (1 ano):**
- Dados: **10,4 GB**
- Backups (3 cópias): **31,2 GB**
- Logs e metadata: **~5 GB**
- **Total: ~46 GB/ano**

### Growth Projections

- **Ano 1**: 46 GB
- **Ano 2**: 92 GB (crescimento linear - cenário conservador)
- **Com crescimento de 50% ao ano**: 
  - Ano 2: 69 GB
  - Ano 3: 103,5 GB

**Conclusão**: Storage não é gargalo crítico. Banco de dados relacional padrão suporta facilmente.

---

## Memory (Cache Requirements)

### Hot Data

**1. Pesquisas Ativas**
- 5 pesquisas simultâneas
- Dados completos (pesquisa + opções): ~2 KB cada
- Total: **10 KB** (negligível)

**2. Contadores em Tempo Real**
- 5 pesquisas × 5 opções = 25 contadores
- Cada contador: 8 bytes (INT64)
- Total: **200 bytes** (negligível)

**3. Cache de Prevenção de Duplicatas**
- Usuários únicos ativos em janela de 1 hora
- Estimativa: 100.000 usuários simultâneos (pico)
- Hash identifier: 64 bytes cada
- Total: 100.000 × 64 = **6,4 MB**
- Com overhead de hash table (2x): **~13 MB**

**4. Session Cache (se aplicável)**
- 100.000 sessões ativas
- 1 KB por sessão
- Total: **100 MB**

**5. Cache de Queries Frequentes**
- Resultados de pesquisas ativas
- 5 pesquisas × 10 KB cada = **50 KB**

### Total Memory Requirements

**Mínimo necessário:**
- Hot data + contadores: ~15 MB
- Cache de duplicatas: ~15 MB
- **Total: ~30 MB**

**Recomendado (com buffer):**
- Sessions: 100 MB
- Application cache: 50 MB
- Connection pools: 50 MB
- OS e overhead: 200 MB
- **Total: ~400 MB por instância**

**Para alta disponibilidade (4 instâncias):**
- **Total: ~1,6 GB** (muito modesto)

---

## Network Bandwidth

### Upload Bandwidth (Votos - Write)

**Payload médio de um voto:**
- Request headers: ~500 bytes
- Request body (survey_id, option_id): ~100 bytes
- Total request: ~600 bytes

**Response:**
- Headers + confirmation: ~300 bytes

**Total por voto**: ~900 bytes ≈ **1 KB**

**Bandwidth necessário:**
- **Média**: 1,2 votos/s × 1 KB = **1,2 KB/s** ≈ **0,01 Mbps**
- **Pico (3x)**: 4 votos/s × 1 KB = **4 KB/s** ≈ **0,03 Mbps**
- **Pico absoluto (10x)**: 40 votos/s × 1 KB = **40 KB/s** ≈ **0,3 Mbps**

### Download Bandwidth (Visualizações - Read)

**Payload médio de visualização:**
- HTML/CSS/JS inicial: ~200 KB (carregado uma vez, depois em cache)
- API call para buscar pesquisa: 
  - Request: ~400 bytes
  - Response: ~2 KB (pesquisa + opções)

**Para estimativa, considerando cache:**
- 30% carregam página completa: 200 KB
- 70% apenas API: 2 KB
- Média ponderada: (0,3 × 200) + (0,7 × 2) = **61,4 KB por visualização**

**Bandwidth necessário:**
- **Média**: 23 reads/s × 61,4 KB = **1,4 MB/s** ≈ **11 Mbps**
- **Pico (5x)**: 135 reads/s × 61,4 KB = **8,3 MB/s** ≈ **66 Mbps**
- **Pico absoluto (20x)**: 2.700 reads/s × 61,4 KB = **166 MB/s** ≈ **1,3 Gbps**

### Total Network

**Normal operation:**
- Download: 11 Mbps
- Upload: 0,01 Mbps
- **Total: ~11 Mbps**

**Pico esperado:**
- Download: 66 Mbps
- Upload: 0,03 Mbps
- **Total: ~66 Mbps**

**Pico absoluto (viral):**
- Download: 1,3 Gbps
- Upload: 0,3 Mbps
- **Total: ~1,3 Gbps**

**Recomendação**: Conexão de **2 Gbps** com CDN para servir assets estáticos.

---

## Compute (Server Requirements)

### Application Servers

**Capacidade por servidor (estimativa conservadora):**
- CPU: 4 cores modernos
- RAM: 8 GB
- Pode processar: ~1.000 requisições/segundo

**Servidores necessários:**

**Cenário normal (média):**
- Total requests: 23 + 1,2 = 24,2 req/s
- Servidores: 24,2 / 1.000 = **1 servidor** (mínimo)

**Cenário pico (5x read, 3x write):**
- Total requests: 135 + 4 = 139 req/s
- Servidores: 139 / 1.000 = **1 servidor**

**Cenário pico absoluto (viral):**
- Total requests: 2.700 + 40 = 2.740 req/s
- Servidores: 2.740 / 1.000 = **3 servidores**

**Configuração recomendada:**
- **Produção normal**: 2 servidores (HA + load balancing)
- **Auto-scaling**: até 4-6 servidores em picos extremos
- **Specs por servidor**: 4 vCPUs, 8 GB RAM

### Database Server

**Workload:**
- Writes: 1-40 votos/segundo
- Reads: 23-2.700 pesquisas/segundo
- Banco relacional (PostgreSQL) lida facilmente

**Configuração recomendada:**
- **Master**: 8 vCPUs, 32 GB RAM, SSD
- **Read replica** (opcional): mesmas specs para reads
- Connection pool: 100-200 conexões

### Load Balancer

- 1 instância (managed service recomendado)
- Health checks nos app servers
- Round-robin ou least connections

---

## Summary - Configuração Recomendada

### Produção (Normal Operation)

**Application Tier:**
- 2 instâncias: 4 vCPUs, 8 GB RAM cada
- Auto-scaling: 2-6 instâncias

**Database Tier:**
- 1 master: 8 vCPUs, 32 GB RAM, 100 GB SSD
- 1 read replica (opcional): mesmas specs

**Cache/Memory:**
- Redis: 2 GB (overkill, mas barato)

**Storage:**
- 100 GB para banco de dados (com crescimento de 2 anos)
- 100 GB para backups

**Network:**
- 2 Gbps de conexão
- CDN para assets estáticos

**Custo estimado (cloud):**
- App servers: $150-300/mês
- Database: $200-400/mês
- CDN: $50-100/mês
- Outros: $50/mês
- **Total: $450-850/mês**

### Capacidade para Picos

Com a configuração acima e auto-scaling:
- **Suporta**: 2.740 req/s (pico viral)
- **Usuários simultâneos**: 100.000+
- **Margem de segurança**: 2-3x acima do esperado