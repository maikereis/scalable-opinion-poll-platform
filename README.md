# Parrhesia — Scalable Opinion Poll Platform

![Built with .NET](https://img.shields.io/badge/Built%20with-.NET%209-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/API-ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server%202022-CC2927?logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Container-Docker-2496ED?logo=docker&logoColor=white)
![Entity Framework](https://img.shields.io/badge/ORM-Entity%20Framework%20Core-512BD4?logo=dotnet&logoColor=white)
![OpenAPI](https://img.shields.io/badge/Docs-OpenAPI%20%2F%20Swagger-85EA2D?logo=swagger&logoColor=black)

**Parrhesia** is a scalable opinion poll platform designed for large-scale distribution across social networks. The system is built to support millions of simultaneous users while ensuring data integrity and availability.

---

## Overview

The project consists of a robust API for creating, managing, and participating in opinion polls. The architecture focuses on high performance for vote processing and real-time result generation.

## Tech Stack

* **Runtime:** .NET 9 / ASP.NET Core
* **Database:** SQL Server 2022
* **ORM:** Entity Framework Core
* **Containerization:** Docker and Docker Compose
* **API Documentation:** OpenAPI / Swagger

## Architecture & Technical Documentation

Architecture details and design decisions can be found in the `docs/` directory:

* **Domain:** Business context and domain rules.
* **Requirements:** Functional and non-functional requirements definition.
* **Scalability:** Capacity estimation calculations for millions of users.
* **API:** Endpoint contracts and definitions.
* **Diagrams:** Visual system representation in Mermaid.

## Running the Project

### Prerequisites

* Docker and Docker Compose installed.
* .NET 9 SDK (for local execution outside the container).

### Quick Start

To spin up the full environment (API and Database), run the following commands from the project root:

```bash
# Build and start the containers
dotnet build
docker compose up -d --build
```

### Database Migrations

After the containers are up and running, apply the Entity Framework migrations:

```bash
dotnet ef database update --project src/Parrhesia.Infrastructure --startup-project src/Parrhesia.Api
```

## API Exploration & Postman

There are two main ways to test and integrate with Parrhesia's endpoints:

### 1. Swagger (Interactive UI)

The API exposes a Swagger interface for testing endpoints directly from the browser.

* **URL:** `http://localhost:8080/swagger` (available in `Development` environment).

### 2. Postman

For developers who prefer **Postman**, the API's technical specification follows the **OpenAPI 3.0** standard.

* **Import:** You can import the `openapi_spec.json` file directly into Postman to automatically generate a collection with all endpoints, including request models for votes and poll creation.
* **Headers:** Note that some voting endpoints require the `X-User-Id` header (UUID) for voter identification.

## Main Endpoints (API v1)

| Resource | Method | Description |
| --- | --- | --- |
| `/api/v1/surveys` | `GET` | Lists active, paginated polls. |
| `/api/v1/surveys` | `POST` | Creates a new poll. |
| `/api/v1/surveys/{id}/votes` | `POST` | Registers a vote for a specific question. |
| `/api/v1/surveys/{id}/results` | `GET` | Retrieves detailed results and vote totals. |
| `/api/v1/health` | `GET` | Health check (Liveness/Readiness). |

---
