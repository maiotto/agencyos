# AgencyOS Program Architecture

Version: 1.2

Status: Active

Date: 2026-07-21

---

# Overview

AgencyOS is organized as two independent strategic programs that share vision but evolve with separate delivery dependencies.

AgencyOS Program

│

├── Program A

│     AgencyOS Product

│

└── Program B

      AgencyOS AI Factory

AgencyOS AI Factory is an independent parallel program responsible for software engineering automation. It is not part of the AgencyOS MVP product scope.

See ADR-006 and DEC-008-008.

---

# Program A – AgencyOS Product

## Vision

AI-First Operational Decision Platform for service organizations.

## Mission

Transform sold contracts into the best execution strategy.

## MVP Status

The AgencyOS MVP backend is complete. Implemented capabilities span Commercial and Operations domains, Planning Engines and the Decision Engine pipeline.

The platform supports the operational decision flow:

Commercial → Delivery Strategy → Capacity Planning → AI Recommendation → Manager Approval

## Core Business Flow

Lead

↓

Client

↓

Client Contract

↓

Delivery Strategy

↓

Mission

↓

Tasks

↓

Execution Resources

↓

Capacity Planning

↓

AI Recommendations

↓

Manager Decision

↓

Execution

↓

Continuous Replanning

---

## Layered Architecture

Presentation

↓

Application

↓

Domain

↓

Infrastructure

Dependencies always point inward.

The Domain layer never depends on Infrastructure.

| Layer | Project |
|-------|---------|
| Presentation | AgencyOS.Api |
| Application | AgencyOS.Application |
| Domain | AgencyOS.Domain |
| Infrastructure | AgencyOS.Infrastructure |

Cross-cutting utilities reside in AgencyOS.Shared and are referenced without reversing layer dependencies.

---

## Implemented Business Domains

### Commercial

Manages the commercial lifecycle from opportunity to contract.

Entities: Lead, Client, ClientContact, ClientContract, Mission

### Operations

Transforms contracts into executable work.

Entities: Task, ExecutionResource, Assignment

### Planning Engines

Calculates operational intelligence from execution data through a progressive analytical pipeline.

Services:

- CapacityCalculatorService
- WorkloadCalculatorService
- AvailabilityEngineService
- AllocationConflictDetectionService

Pipeline:

Capacity

↓

Workload

↓

Availability

↓

Allocation Conflict Detection

Availability consumes Capacity and Workload results.

Allocation Conflict Detection consumes Availability results.

Each engine exposes its own REST controller and reuses upstream calculation results.

See DEC-007-001 through DEC-007-004.

### Decision Engine

Generates, evaluates, ranks and explains delivery strategies.

Services:

- DeliveryStrategyBuilderService
- DeliveryStrategyEvaluatorService
- DeliveryStrategyRankingService
- DeliveryStrategyExplanationService

Pipeline:

Delivery Strategy Builder

↓

Delivery Strategy Evaluator

↓

Delivery Strategy Ranking

↓

Delivery Strategy Explanation

The Evaluator reuses Planning Engines and Builder output.

Ranking reuses Evaluator output.

Explanation reuses Ranking output.

See DEC-008-001 and DEC-008-006.

---

## Decision Engine Architecture

The Decision Engine is the core differentiator of AgencyOS.

It does not make final decisions. It generates alternatives, measures them, ranks them and explains trade-offs.

### Stage 1 – Delivery Strategy Builder

Generates all valid execution strategy candidates from active Execution Resources and applicable policies.

Does not evaluate or rank.

### Stage 2 – Delivery Strategy Evaluator

Calculates objective operational metrics for each strategy using Capacity, Workload, Availability and Allocation Conflict Detection engines.

Does not rank or recommend.

### Stage 3 – Delivery Strategy Ranking

Orders evaluated strategies using configurable Company Decision Profiles.

Applies min-max normalization and weighted scoring.

Does not generate explanations.

### Stage 4 – Delivery Strategy Explanation

Produces structured, deterministic explanations of ranking results.

Uses reason codes and evaluation metrics.

Does not use LLM generation during the MVP.

---

## Company Decision Profiles

Ranking priorities are defined by Company Decision Profiles.

During the MVP, profiles are stored in application configuration and loaded through `ICompanyDecisionProfileRepository`.

Six default profiles are provided: Profit Maximization, Delivery Speed, Operational Stability, AI Adoption, Human Resource Optimization and Balanced Strategy.

See ADR-007 and DEC-008-002.

---

## API Surface

### Commercial and Operations

| Route | Purpose |
|-------|---------|
| missions | Mission management |
| leads | Lead management |
| clients | Client management |
| contacts | Client contact management |
| contracts | Client contract management |
| tasks | Task management |
| execution-resources | Execution resource management |
| assignments | Resource assignment management |

### Planning Engines

| Route | Purpose |
|-------|---------|
| capacity | Capacity calculation |
| workload | Workload calculation |
| availability | Availability calculation |
| allocation-conflicts | Allocation conflict detection |

### Decision Engine

| Route | Method | Purpose |
|-------|--------|---------|
| delivery-strategies/build | POST | Generate strategy candidates |
| delivery-strategies/{contractId} | GET | Retrieve strategies for a contract |
| delivery-strategies/evaluate | POST | Evaluate strategies with metrics |
| delivery-strategies/rank | POST | Rank strategies by decision profile |
| delivery-strategies/{strategyId}/explanation | GET | Explain a ranked strategy |

---

## Calculation Pattern

Pure business calculations are implemented as static classes in the Application layer.

Services orchestrate data access, logging and validation.

Examples:

- CapacityCalculation
- WorkloadCalculation
- AvailabilityCalculation
- AllocationConflictCalculation
- DeliveryStrategyGeneration
- DeliveryStrategyEvaluationCalculation
- DeliveryStrategyRankingCalculation
- DeliveryStrategyExplanationCalculation

This pattern ensures deterministic, testable logic without infrastructure dependencies.

See DEC-007-002.

---

## Database Governance

Schema is managed exclusively through Supabase SQL migrations.

Entity Framework Core is used for data access only.

Entity Framework migrations are not permitted.

---

## Seed Data Layer

Reference data for lookup tables is initialized through `supabase/seed.sql`.

Supabase executes the seed file automatically after migrations when `supabase db reset` runs (`config.toml` → `[db.seed]`).

### Governed Tables

| Table | Purpose |
|-------|---------|
| mission_type | Mission classification (Content Production, Marketing Campaign, etc.) |
| mission_status | Mission lifecycle states (Draft, Planned, In Progress, etc.) |
| task_type | Task classification (Production, Editing, Review, etc.) |
| task_status | Task lifecycle states aligned with `MissionTaskStatus` domain constants |

### Deterministic Identifiers

Seed records use fixed UUIDs under a documented namespace scheme. The same IDs are recreated on every reset.

Random UUID generation is not permitted in seed data.

See DEC-009-001 and `supabase/seed.sql`.

Transactional entities (Lead, Client, Mission, Task, etc.) are not seeded. They are created through application APIs during operational use.

---

# Program B – AgencyOS AI Factory

## Vision

AI executes. Humans govern.

## Purpose

Independent parallel program responsible for software engineering automation.

AgencyOS AI Factory accelerates AgencyOS delivery through AI-assisted development workflows.

AgencyOS AI Factory is not part of the AgencyOS MVP product scope.

Both programs share the AgencyOS vision. Delivery dependencies remain independent.

See ADR-006 and DEC-008-008.

---

## Agent Organization

### Management

- Product Owner Agent
- Scrum Master Agent
- Tech Lead Agent

### Architecture

- Solution Architect Agent

### Engineering

- Backend Agent
- Frontend Agent
- Database Agent
- DevOps Agent

### Quality

- QA Agent
- Code Review Agent
- Documentation Agent

### Knowledge Base

System prompts, execution guides, user story prompts and review checklists in `/prompts`.

---

## Engineering Workflow

### User Story Implementation

Backend Agent executes individual User Stories following:

- prompts/system/00_Project_Context.md
- prompts/system/01_Engineering_Principles.md
- prompts/system/02_Coding_Standards.md
- prompts/system/03_Definition_of_Done.md
- prompts/system/04_Tech_Stack.md
- prompts/system/05_Agent_Execution_Guide.md

Per-story prompts:

- US-XXX (requirements)
- EXEC-US-XXX (execution instructions)
- REVIEW-US-XXX (review checklist)

### Sprint Documentation Consolidation

After sprint completion, Documentation Agent executes the Documentation Update workflow.

See ADR-008 and prompts/system/06_Documentation_Update_Guide.md.

This workflow updates Sprint Register, Decision Log, Baseline, Architecture documents and ADRs without modifying source code.

---

## Governance Boundary

Until the AgencyOS MVP is delivered, the AI Factory shall not require changes to:

- Product Architecture
- Domain Model
- Product Roadmap
- Product Backlog
- Product Schedule

AgencyOS AI Factory accelerates AgencyOS.

AgencyOS never slows down to accommodate AI Factory.

---

## Evolution Roadmap

Phase 0 – Knowledge

Phase 1 – Product Owner Automation

Phase 2 – Scrum Master Automation

Phase 3 – Architecture Automation

Phase 4 – Engineering Automation

Phase 5 – Complete AI Factory Orchestration

---

# Cross-Program Relationship

| Aspect | Program A (Product) | Program B (AI Factory) |
|--------|---------------------|------------------------|
| Deliverable | Operational Decision Platform | Engineering Automation Platform |
| MVP Scope | Yes | No |
| Relationship | Core product | Independent parallel program |
| Repository | AgencyOS | AgencyOS-AI-Factory (future) |
| Governance | Product Owner | Tech Lead |
| Documentation | docs/ | prompts/ |

Both programs share the AgencyOS vision.

Delivery dependencies remain independent.
