# AgencyOS Baseline v1.0

**Versão:** 1.1

**Data:** 21/07/2026

---

# Objetivo

Desenvolver o MVP do AgencyOS, uma plataforma AI-First para apoio à decisão operacional de agências criativas, marketing, consultoria, software e serviços profissionais.

---

# Escopo do MVP

O MVP será desenvolvido em Sprints incrementais.

O backend do MVP valida o fluxo completo de decisão operacional:

Comercial → Delivery Strategy → Capacity Planning → AI Recommendation → Manager Approval

---

# Stack Tecnológica

- Supabase Cloud
- PostgreSQL
- React
- TypeScript
- ASP.NET Core 8
- C# 12
- Entity Framework Core 8
- FluentValidation
- GitHub
- Cursor

---

# Arquitetura Congelada

Os seguintes documentos passam a ser a referência oficial do projeto:

- Product Vision
- CDM
- DMS
- LDM
- Backlog do MVP

Nenhum destes documentos será alterado durante o desenvolvimento do MVP.

---

# Arquitetura Implementada

## Camadas

AgencyOS.Api

↓

AgencyOS.Application

↓

AgencyOS.Domain

AgencyOS.Infrastructure

AgencyOS.Shared

## Domínios Implementados

### Comercial

Lead → Client → Client Contact → Client Contract → Mission

### Operacional

Task → Execution Resource → Resource Assignment

### Inteligência Analítica

Capacity → Workload → Availability → Allocation Conflict Detection

### Decision Engine

Delivery Strategy Builder → Evaluator → Ranking → Explanation

## APIs REST Implementadas

### Comercial e Operacional

- missions
- leads
- clients
- contacts
- contracts
- tasks
- execution-resources
- assignments

### Inteligência Analítica

- capacity
- workload
- availability
- allocation-conflicts

### Decision Engine

- POST /delivery-strategies/build
- GET /delivery-strategies/{contractId}
- POST /delivery-strategies/evaluate
- POST /delivery-strategies/rank
- GET /delivery-strategies/{strategyId}/explanation

## Company Decision Profiles

Perfis de decisão configuráveis via `appsettings.json`:

- Profit Maximization
- Delivery Speed
- Operational Stability
- AI Adoption
- Human Resource Optimization
- Balanced Strategy

---

# Estrutura Oficial do Repositório

/backend

/frontend

/database

/docs

/infrastructure

/prompts

/scripts

/supabase

---

# Convenções

- UUID como chave primária
- snake_case para tabelas
- snake_case para colunas
- migrations versionadas pelo Supabase
- Git Flow (main/develop)
- DTOs para toda exposição de API
- FluentValidation para validação
- Entity Framework migrations não permitidas

---

# Sprints Concluídas

Sprint 0

- Ambiente de desenvolvimento
- GitHub
- Supabase
- Cursor
- Estrutura do projeto

Sprint 1

- Domínio Comercial inicial
- Migration inicial
- Banco criado
- Relacionamentos implementados

Sprint 5

- Commercial Foundation
- US-001 a US-005

Sprint 6

- Operational Foundation
- US-007 a US-009

Sprint 7

- Intelligent Capacity Planning
- US-010 a US-013

Sprint 8

- Delivery Strategy Engine
- US-014 a US-017

---

# Programas

## Program A – AgencyOS Product

Plataforma AI-First de apoio à decisão operacional.

## Program B – AgencyOS AI Factory

Plataforma de engenharia AI-First independente do MVP.

Ver ADR-006.

---

# Critério de Mudança

A partir da Baseline v1.0:

- Não alterar arquitetura sem ADR.
- Não alterar metodologia.
- Não alterar stack sem ADR.
- Não alterar modelagem do MVP sem aprovação explícita.

Toda evolução ocorrerá através de novas Sprints e ADRs.

---

# Status do Projeto

Sprint Atual:

Sprint 8 – Concluída

Decision Engine backend completo.

Próximo passo definido pelo Product Roadmap.
