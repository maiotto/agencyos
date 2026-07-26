# AgencyOS AI Factory

Version: 1.0

Status: Active

Date: 2026-07-21

---

# Overview

This section is the entry point for AgencyOS AI Factory documentation.

AgencyOS AI Factory is an independent strategic program responsible for AI-assisted software engineering automation. It accelerates AgencyOS delivery without changing the product itself.

See [ADR-006](../05_ADR/ADR-006_AgencyOS_AI_Factory.md) and [Program Architecture](../02_Architecture/Program_Architecture.md#program-b--agencyos-ai-factory).

---

# Vision

AI executes. Humans govern.

Humans remain responsible for product vision, strategy, governance and final decisions.

---

# Mission

Build and evolve an AI-First engineering platform that automates software delivery workflows — from product ownership and sprint management through architecture, implementation, quality assurance and documentation — while keeping AgencyOS implementation focus intact.

---

# Relationship with AgencyOS

AgencyOS and AgencyOS AI Factory are parallel programs within the same ecosystem.

| Aspect | Program A — AgencyOS Product | Program B — AgencyOS AI Factory |
|--------|------------------------------|----------------------------------|
| Deliverable | Operational Decision Platform | Engineering Automation Platform |
| MVP Scope | Yes | No |
| Role | Core product | Independent parallel program |
| Repository | AgencyOS | AgencyOS-AI-Factory (future) |
| Documentation | `docs/` | `docs/06_AI_Factory/` and `prompts/` |

**Principle:** AgencyOS AI Factory accelerates AgencyOS. AgencyOS never slows down to accommodate AI Factory.

Both programs share vision. Delivery dependencies remain independent.

---

# Why AI Factory Is a Separate Program

AgencyOS AI Factory is established as an independent program for these reasons:

1. **Scope isolation** — The AI Factory is not part of the AgencyOS MVP. Product delivery and engineering automation evolve on separate tracks.
2. **Governance boundary** — Until the AgencyOS MVP is delivered, the AI Factory shall not require changes to Product Architecture, Domain Model, Product Roadmap, Product Backlog or Product Schedule.
3. **Independent evolution** — Agent workflows, prompts and orchestration can mature without blocking or redirecting product implementation.
4. **Clear accountability** — AgencyOS remains the product. AI Factory remains the engineering platform.

---

# Documentation Structure

| Document | Purpose |
|----------|---------|
| [README.md](./README.md) | Entry point — vision, mission, relationship with AgencyOS and documentation map |
| [AI_Factory_Architecture.md](./AI_Factory_Architecture.md) | Agent organization, engineering workflow, governance boundaries and cross-program integration |
| [AI_Factory_Roadmap.md](./AI_Factory_Roadmap.md) | Phased evolution from knowledge base through complete orchestration |
| [Agent_Catalog.md](./Agent_Catalog.md) | Agent roles, responsibilities, inputs, outputs and runtime profiles |

---

# Related References

- [ADR-006 — AgencyOS AI Factory](../05_ADR/ADR-006_AgencyOS_AI_Factory.md)
- [ADR-008 — Documentation Update Workflow](../05_ADR/ADR-008_Documentation_Update_Workflow.md)
- [Program Architecture — Program B](../02_Architecture/Program_Architecture.md#program-b--agencyos-ai-factory)
- [AgencyOS Baseline — Programas Paralelos](../07_Project_Management/AgencyOS_Baseline_v1.0.md#programas-paralelos)
