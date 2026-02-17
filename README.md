# 🚗 PolicyEventHub

**PolicyEventHub** is a modern ASP.NET Core–based car insurance management platform designed to simplify the vehicle insurance lifecycle.  
The system focuses on **annual car policy renewal**, policy event tracking, and scalable integration with external services.

Built with clean architecture principles, the project emphasizes maintainability, scalability, and clear separation of concerns.

---

## ✨ Features

- 🚘 Annual car insurance policy renewal
- 📅 Policy lifecycle and event tracking
- 🔄 Event-driven architecture
- 🔗 Third-party API integration
- ✅ Strong validation and error handling
- 🧩 Decoupled application layers
- 📦 Containerized environment with Docker

---

## 💻 Technologies Used

This project is built using modern .NET technologies and follows clean architecture and scalable design principles.

### Backend

- **C#**  
  The core programming language.

- **ASP.NET Core**  
  Web framework for building scalable APIs.

- **Entity Framework Core**  
  ORM tool for database interactions.

- **MediatR**  
  Enables decoupled communication between application components using the mediator pattern.

- **AutoMapper**  
  Simplifies object-to-object mapping between domain models and DTOs.

- **FluentValidation**  
  Provides strongly-typed and reusable validation rules.

- **CSharpFunctionalExtensions**  
  Functional programming patterns. Only the **Result pattern** is used for handling operation results.

- **Scrutor**  
  Assembly scanning and automatic service registration for Dependency Injection.

### Messaging & Infrastructure

- **Confluent.Kafka**  
  Integration with Apache Kafka for message-based communication and event streaming.

- **Docker**  
  Containerization for consistent environments and simplified deployment.

---

## 🏗 Architecture

The project follows **Clean Architecture** principles:

- Domain Layer — business rules and core entities  
- Application Layer — use cases and business logic  
- Infrastructure Layer — external integrations and persistence  
- API Layer — endpoints and request handling  

---

## 🚀 Getting Started

```bash
git clone https://github.com/HumayAghayeva/PolicyEventHub.git
cd PolicyEventHub
