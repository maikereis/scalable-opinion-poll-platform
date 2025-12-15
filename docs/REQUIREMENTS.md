# Requisitos

Este documento define os requisitos funcionais e não-funcionais do sistema de pesquisas Parrhesia.

---

## Requisitos Funcionais

### Recursos Principais

#### 1. Gerenciamento de Pesquisas

- Sistema deve permitir criação de pesquisas com múltiplas escolhas
- Cada pesquisa deve conter:
    - Título/pergunta principal
    - Múltiplas opções de resposta (mínimo 2, máximo 10)
    - Data de início e término da coleta
- Sistema deve permitir ativação/desativação de pesquisas
- Sistema deve impedir modificação de pesquisas após início da coleta

#### 2. Coleta de Respostas

- Usuários podem visualizar pesquisas ativas
- Usuários podem selecionar uma única opção por pesquisa
- Sistema deve registrar timestamp de cada voto
- Sistema deve impedir votos duplicados do mesmo usuário
- Sistema deve validar a resposta antes de armazenar

#### 3. Armazenamento de Dados

- Sistema deve armazenar consistentemente todos os votos computados
- Sistema deve garantir integridade dos dados de votação
- Sistema deve manter histórico de pesquisas encerradas

#### 4. Relatórios e Resultados

- Sistema deve permitir visualização de resultados por usuários autorizados
- Relatórios devem incluir:
    - Contagem total de votos por opção
    - Percentual de cada opção
    - Total geral de participantes
    - Período de coleta
- Sistema deve gerar relatórios sumarizados após encerramento da pesquisa

#### 5. Controle de Acesso

- Sistema deve autenticar usuários administrativos
- Apenas usuários autorizados podem:
    - Criar pesquisas
    - Visualizar resultados detalhados
    - Exportar dados

#### 6. Fora do Escopo

- Pesquisas com múltiplas respostas por usuário
- Perguntas abertas ou dissertativas
- Análise avançada de dados ou segmentação
- Gamificação ou incentivos para participação
- Integração direta com redes sociais
- Comentários ou discussões sobre pesquisas

## Requisitos Não-Funcionais

#### 1. Disponibilidade

- 99.5% de uptime durante período de coleta ativa
- Sistema deve ser tolerante a falhas de componentes individuais

#### 2. Performance

- Latência para carregar formulário da pesquisa < 1s
- Latência para registrar voto < 500ms
- Latência para gerar relatórios básicos < 3s
- Sistema deve responder adequadamente mesmo sob alta carga

#### 3. Escalabilidade

- Suportar até 10 milhões de usuários visualizando pesquisas simultaneamente
- Suportar picos de até 100.000 votos por minuto
- Arquitetura monolítica com capacidade de escalonamento horizontal
- Auto-scaling baseado em demanda

#### 4. Consistência

- Consistência forte para registro de votos:
    - Todos os votos computados devem ser contabilizados corretamente
    - Não deve haver duplicação de votos do mesmo usuário
- Consistência eventual aceitável para:
    - Contadores de visualização de pesquisa
    - Estatísticas agregadas em tempo real (podem ter atraso de até 5s)
- Perda eventual de votos é aceitável desde que não comprometa a amostra estatística (< 0.1% de perda)

#### 5. Durabilidade

- Votos computados nunca devem ser perdidos após confirmação ao usuário
- Backups diários dos dados de pesquisas e votos
- RPO (Recovery Point Objective) < 1 hora
- RTO (Recovery Time Objective) < 4 horas

#### 6. Segurança

- Proteção contra votação automatizada (bots)
- Proteção contra ataques comuns (CSRF, SQL Injection, XSS)
- Rate limiting para prevenir abuso
- Anonimização de dados individuais nos relatórios
- Conformidade com LGPD (Lei Geral de Proteção de Dados)
- Dados de votos devem ser armazenados de forma segura

#### 7. Usabilidade

- Interface responsiva para mobile e desktop
- Formulário simples e intuitivo para votação
- Feedback visual claro após submissão de voto
- Tempo de carregamento otimizado para redes móveis

#### 8. Manutenibilidade

- Código seguindo padrões C# e .NET Framework
- Documentação técnica básica
- Logs estruturados para debugging
- Monitoramento de métricas críticas

## Assunções

- Taxa de conversão: ~5% dos usuários que visualizam efetivamente votam
- Distribuição de acessos: 70% mobile, 30% desktop
- Pico de acessos: primeiras 24 horas após divulgação nas redes sociais
- Duração média de pesquisa: 7 dias
- Simultaneidade real: mesmo com milhões de visualizações, pico de votação simultânea < 50.000 usuários

## Restrições

### Técnicas
- Tecnologia obrigatória: C# com .NET Framework
- Arquitetura: Monolítica
- Prazo de entrega: 8 semanas
- Equipe: 5 desenvolvedores

### Operacionais
- Limite de pesquisas ativas simultaneamente: 5
- Limite de opções por pesquisa: 10
- Duração mínima de pesquisa: 1 dia
- Duração máxima de pesquisa: 30 dias
- Rate limiting para votação: 1 voto a cada 30 segundos por IP
- Rate limiting para visualização: 100 requisições por minuto por IP

### Dados
- Tamanho máximo do título da pesquisa: 500 caracteres
- Tamanho máximo de cada opção: 200 caracteres
- Retenção de dados: dados brutos mantidos por 2 anos após encerramento