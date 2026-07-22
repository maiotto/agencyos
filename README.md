# AgencyOS

> AI Native Operating System for Creative Agencies

## Status

**Version:** v0.9.0

**Release:** MVP Completed

**Branch:** develop

**State:** Stable

---

## Visão

AgencyOS é uma plataforma AI Native para gestão operacional de empresas de serviços, iniciando pelo mercado de produção audiovisual.

O sistema integra processos comerciais, planejamento operacional, capacidade produtiva, recursos de execução, analytics e um Decision Engine capaz de gerar, avaliar, ranquear e explicar estratégias de entrega.

---

## Módulos do MVP

### Commercial

- Lead Management
- Client Management
- Contact Management
- Contract Management

### Operations

- Missions
- Tasks
- Execution Resources
- Assignments

### Analytics

- Capacity Planning
- Workload Analysis
- Availability Analysis
- Allocation Conflict Detection

### Decision Engine

- Delivery Strategy Builder
- Strategy Evaluation
- Strategy Ranking
- Explainability

---

## Arquitetura

- Clean Architecture
- Domain Driven Design (DDD)
- REST API
- Decision Intelligence
- AI Native Ready

---

## Tecnologias

### Backend

- .NET 8
- ASP.NET Core
- Entity Framework Core

### Database

- PostgreSQL
- Supabase

### Frontend

- React
- TypeScript

### Infrastructure

- Docker

---

## Estrutura

```text
agencyos/

backend/
frontend/
database/
docs/
supabase/
scripts/
prompts/
infrastructure/
```

---

## Executando o projeto

### Backend

```bash
cd backend/src
dotnet run --project AgencyOS.Api
```

### Testes

```bash
cd backend/src
dotnet test
```

### Swagger

```
http://localhost:5115/swagger
```

---

## Resultado do MVP

- Commercial Workflow
- Operational Workflow
- Analytics Engine
- Decision Engine
- Explainability

Todos os fluxos do MVP foram homologados.

72 testes automatizados aprovados.

---

## Licença

Privado – AgencyOS