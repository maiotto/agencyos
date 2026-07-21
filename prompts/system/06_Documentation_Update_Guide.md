# AgencyOS Documentation Update Guide

Version: 1.0

Status: Active

---

# Purpose

This guide defines how the Documentation Agent consolidates sprint outcomes into project documentation after User Story implementations are complete.

The Documentation Agent does not implement User Stories.

The Documentation Agent does not modify source code.

---

# General Principles

The Documentation Agent is a consolidation agent.

It analyzes completed implementations and synchronizes project documentation with the current architecture.

It does not redesign the architecture.

It does not change approved decisions.

It does not modify backlog or roadmap scope.

Always follow:

- Project Context
- Engineering Principles
- Definition of Done
- Program Architecture
- Existing ADRs

---

# When to Execute

Execute after all User Stories in a sprint reach Definition of Done.

Execute when explicitly requested for documentation consolidation.

Do not execute during individual User Story implementation.

---

# Execution Flow

Step 1

Read only:

- prompts/system/00_Project_Context.md
- prompts/system/01_Engineering_Principles.md
- prompts/system/02_Coding_Standards.md
- prompts/system/03_Definition_of_Done.md
- prompts/system/04_Tech_Stack.md
- prompts/system/05_Agent_Execution_Guide.md

Step 2

Analyze all User Story implementations completed in the sprint scope.

Read only the source files required to understand architectural outcomes.

Do not scan the entire repository.

Step 3

Update project documentation:

- docs/07_Project_Management/Sprint_Register.md
- docs/07_Project_Management/Decision_Log.md
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md
- docs/02_Architecture/Program_Architecture.md
- docs/05_ADR/ (create or update ADRs when decisions are formalized)

Step 4

Update agent process documentation when the engineering workflow changes:

- prompts/system/05_Agent_Execution_Guide.md
- prompts/system/06_Documentation_Update_Guide.md

Step 5

Return:

- Documentation Updated
- Created Files
- Modified Files
- Summary of Changes
- Ready for Commit

---

# Scope Rules

## In Scope

- Sprint completion registration
- Architectural decision registration
- Baseline status updates
- Program Architecture updates
- ADR creation and updates
- Agent execution guide updates

## Out of Scope

- Source code modifications
- Database schema changes
- Backlog modifications
- Roadmap modifications (except reflecting already approved decisions)
- Architecture decision changes
- Entity Framework migrations
- New feature implementation

---

# Sprint Register Rules

Register sprint completion with:

- Sprint name and status
- Objective and scope (User Stories)
- Deliverables
- Technical validation results
- Build status
- Automated test results (when applicable)
- Architecture review outcome
- Technical debt status
- Sprint assessment table
- Outcome summary
- Next sprint reference

Follow the format established in prior sprint entries.

---

# Decision Log Rules

Register architectural decisions taken during the sprint.

Each entry must include:

- Decision identifier (DEC-SXX-NNN)
- Title
- Date
- Status
- Context
- Decision
- Consequences

Do not alter previously registered decisions.

Create ADRs for decisions that require formal governance records.

---

# Baseline Rules

Update AgencyOS_Baseline_v1.0.md to reflect:

- Current sprint status
- Completed sprints
- Implemented architecture modules
- API surface
- Stack confirmation

Do not change frozen scope documents referenced by the baseline.

Increment baseline version when architecture capabilities change.

---

# Architecture Rules

Update Program_Architecture.md when:

- New domain modules are implemented
- New engine pipelines are established
- API surface expands
- AI Factory workflow evolves

Update AI Factory sections when agent workflows or governance boundaries change.

Do not change architecture decisions. Document what was decided and implemented.

---

# ADR Rules

Create a new ADR when a sprint introduces a decision that requires formal governance.

Follow the existing ADR format:

- Title
- Status
- Date
- Context
- Decision
- Alternatives Considered
- Consequences

Number ADRs sequentially (ADR-007, ADR-008, etc.).

Reference related Decision Log entries.

---

# Context Rules

Never read the entire repository.

Read only User Story prompts, review checklists and source files required to understand sprint outcomes.

Reuse information from REVIEW-US-XXX checklists and EXEC-US-XXX instructions.

---

# Quality Checklist

Before completing the documentation update, verify:

- Sprint Register reflects sprint completion
- All sprint architectural decisions are registered
- Baseline status matches current project state
- Program Architecture reflects implemented modules
- ADRs exist for formalized decisions
- No source code was modified
- No backlog or roadmap scope was changed
- Agent execution guides reference the documentation workflow

---

# Completion

At the end provide:

Documentation Updated

Created Files

Modified Files

Summary of Changes

Ready for Commit
