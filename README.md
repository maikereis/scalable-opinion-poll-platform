# Parrhesia

A **Parrhesia** é uma plataforma escalável de pesquisas de opinião projetada para distribuição em larga escala em redes sociais. O sistema foi concebido para suportar milhões de usuários simultâneos, garantindo a integridade e a disponibilidade dos dados.

---

## Visão Geral

O projeto consiste em uma API robusta para criação, gestão e participação em pesquisas de opinião. A arquitetura foca em alta performance para processamento de votos e geração de resultados em tempo real.

## Stack Tecnológica

* **Runtime:** .NET 9 / ASP.NET Core
* **Banco de Dados:** SQL Server 2022
* **ORM:** Entity Framework Core
* **Containerização:** Docker e Docker Compose
* **Documentação de API:** OpenAPI / Swagger

## Arquitetura e Documentação Técnica

O detalhamento da arquitetura e decisões de design podem ser encontrados no diretório `docs/`:

* **Domínio:** Contexto de negócio e regras de domínio.
* **Requisitos:** Definição de requisitos funcionais e não-funcionais.
* **Escalabilidade:** Cálculos de estimativa de capacidade para milhões de usuários.
* **API:** Contratos e definições de endpoints.
* **Diagramas:** Representação visual do sistema em Mermaid.

## Execução do Projeto

### Pré-requisitos

* Docker e Docker Compose instalado.
* SDK do .NET 9 (para execução local fora do container).

### Inicialização Rápida

Para subir o ambiente completo (API e Banco de Dados), utilize os comandos abaixo na raiz do projeto:

```bash
# Compilação e inicialização dos containers
dotnet build
docker compose up -d --build

```

### Migrações de Banco de Dados

Após os containers estarem operacionais, aplique as migrações do Entity Framework:

```bash
dotnet ef database update --project src/Parrhesia.Infrastructure --startup-project src/Parrhesia.Api

```

## Exploração da API e Postman

Existem duas formas principais de testar e integrar com os endpoints da Parrhesia:

### 1. Swagger (UI Interativa)

A API expõe uma interface Swagger que permite testar os endpoints diretamente do navegador.

* **URL:** `http://localhost:8080/swagger` (disponível em ambiente de `Development`).

### 2. Postman

Para desenvolvedores que preferem o **Postman**, a especificação técnica da API segue o padrão **OpenAPI 3.0**.

* **Importação:** Você pode importar o arquivo `openapi_spec.json` diretamente no Postman para gerar automaticamente uma coleção com todos os endpoints, incluindo modelos de requisição de votos e criação de pesquisas.
* **Headers:** Lembre-se que alguns endpoints de votação exigem o header `X-User-Id` (UUID) para identificação do votante.

## Endpoints Principais (API v1)

| Recurso | Método | Descrição |
| --- | --- | --- |
| `/api/v1/surveys` | `GET` | Lista pesquisas ativas e paginadas. |
| `/api/v1/surveys` | `POST` | Criação de uma nova pesquisa. |
| `/api/v1/surveys/{id}/votes` | `POST` | Registro de voto para uma questão específica. |
| `/api/v1/surveys/{id}/results` | `GET` | Recupera resultados detalhados e totais de votos. |
| `/api/v1/health` | `GET` | Verificação de integridade (Liveness/Readiness). |

---

Deseja que eu crie um arquivo de ambiente (`.env.example`) para facilitar a configuração do Postman ou prefere focar em outra seção?