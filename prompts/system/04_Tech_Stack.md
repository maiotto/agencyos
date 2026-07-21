# AgencyOS - Technology Stack

Version: 1.0

Status: Frozen for MVP

---

# Purpose

This document defines the official technology stack for the AgencyOS MVP.

Only the technologies listed in this document are approved for implementation unless an Architecture Decision Record (ADR) explicitly changes this baseline.

---

# Backend

Framework

- ASP.NET Core 8 Web API

Language

- C# 12

ORM

- Entity Framework Core 8

Validation

- FluentValidation

Documentation

- Swagger / OpenAPI

Logging

- Microsoft.Extensions.Logging

Dependency Injection

- Built-in ASP.NET Core Dependency Injection

---

# Frontend

Framework

- React 18

Language

- TypeScript

Build Tool

- Vite

Routing

- React Router

HTTP Client

- Axios

State Management

- React Context (MVP)

UI Components

- Material UI (MUI)

Forms

- React Hook Form

Validation

- Zod

---

# Database

Database

- PostgreSQL

Platform

- Supabase Cloud

Schema Management

- SQL Migrations

Migration Tool

- Supabase CLI

Authentication

- Supabase Auth (Future Sprint)

Storage

- Supabase Storage (Future Sprint)

Realtime

- Not used during MVP

---

# Development Tools

IDE

- Cursor

Architecture & Technical Lead

- ChatGPT

Source Control

- Git

Repository

- GitHub

API Testing

- Swagger UI

Terminal

- PowerShell

Database Administration

- Supabase Dashboard

---

# Project Structure

backend/

frontend/

database/

docs/

prompts/

scripts/

supabase/

---

# Coding Standards

Follow:

- Engineering Principles
- Coding Standards
- Definition of Done

No implementation may ignore these documents.

---

# Approved NuGet Packages

Microsoft.EntityFrameworkCore

Microsoft.EntityFrameworkCore.Design

Npgsql.EntityFrameworkCore.PostgreSQL

FluentValidation.AspNetCore

Swashbuckle.AspNetCore

---

# Approved NPM Packages

react

react-dom

typescript

vite

axios

react-router-dom

react-hook-form

zod

@mui/material

@emotion/react

@emotion/styled

---

# Not Approved for MVP

The following technologies are intentionally excluded from the MVP:

- Microservices
- MediatR
- CQRS
- Event Sourcing
- RabbitMQ
- Kafka
- Redis
- GraphQL
- SignalR
- Docker Compose
- Kubernetes

---

# Versioning Strategy

.NET

8 LTS

Node.js

Current LTS

React

18

TypeScript

Latest stable compatible with React 18

Entity Framework

8

PostgreSQL

Supabase Managed Version

---

# Guiding Principle

The objective of the MVP is to validate the product, not to maximize architectural complexity.

Every technology choice must prioritize:

- Simplicity
- Maintainability
- Productivity
- Long-term evolution