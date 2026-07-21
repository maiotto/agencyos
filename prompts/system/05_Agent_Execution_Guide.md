# AgencyOS Backend Agent Execution Guide

Version: 1.0

Status: Frozen for MVP

---

# Purpose

This guide defines how Backend Agents must execute User Stories inside AgencyOS.

Its objective is to minimize context usage, maximize implementation consistency and avoid unnecessary repository exploration.

---

# General Principles

The Backend Agent is an implementation agent.

It does not redesign the architecture.

It does not redefine requirements.

It only implements the approved User Story.

Always follow:

- Project Context
- Engineering Principles
- Coding Standards
- Definition of Done
- Tech Stack

---



# Execution Flow

Step 1

Read only:

- prompts/system/00_Project_Context.md
- prompts/system/01_Engineering_Principles.md
- prompts/system/02_Coding_Standards.md
- prompts/system/03_Definition_of_Done.md
- prompts/system/04_Tech_Stack.md

Step 2

Read:

US-XXX

EXEC-US-XXX

Step 3

Read only the source files required for the implementation.

Do not scan the entire repository.

Do not explore unrelated folders.

Step 4

Implement the User Story.

Step 5

Execute

dotnet build

Fix every warning.

Fix every error.

Step 6

Execute REVIEW-US-XXX.

Apply corrections when necessary.

Step 7

Update:

- Swagger
- HTTP Tests

Step 8

Return:

- Summary
- Files created
- Files modified
- Assumptions
- Remaining issues

---



# Context Rules

Never read the entire repository.

Never scan all folders.

Never search unrelated files.

Read only what is necessary.

Reuse existing implementations whenever possible.

Avoid duplicated business logic.

---



# Scope Rules

Never modify another User Story.

Never implement future functionality.

Never redesign architecture.

Never change Domain Model unless explicitly required.

Never create new database tables unless required.

---

---

# Cross User Story Rule

If implementation requires entities from future User Stories, the Agent may create only the minimum structural elements required to satisfy compile-time dependencies.

Business rules, services, controllers, repositories, validators and application logic belonging to future User Stories shall never be implemented.

Future User Stories remain responsible for their own business behavior.

# Build Rules

The implementation is complete only when:

dotnet build

returns

0 Errors

0 Warnings

---



# Completion

At the end provide:

Implementation Summary

Created Files

Modified Files

Build Status

Assumptions

Ready for Commit