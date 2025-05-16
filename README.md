# Saga Orchestration Project

This project demonstrates a distributed transaction coordination system using the Saga pattern, integrating **MassTransit** and **RabbitMQ** for reliable messaging across microservices. It is designed for robust order processing, stock validation, and payment handling in a scalable, event-driven architecture.

## Overview

The solution consists of multiple microservices:
- **Order Service**: Handles order creation and management.
- **Stock Service**: Validates stock availability for incoming orders.
- **Payment Service**: Processes payments for approved orders.
- **State Machine Service**: Orchestrates the saga workflow, managing state transitions and coordinating between services.

## Features

- **Saga State Machine**: Manages the lifecycle of orders (Pending, Approved, Rejected) using a state machine pattern. State transitions are triggered asynchronously via RabbitMQ events.
- **Order Processing**: Receives and processes orders, forwarding them to the stock service for validation.
- **Stock Validation**: Checks stock availability. If stock is insufficient, the order is rejected; otherwise, it proceeds to payment.
- **Payment Handling**: Processes payment transactions. Successful payments approve the order; failures result in rejection.
- **Reliable Messaging**: Utilizes MassTransit and RabbitMQ for robust, decoupled communication between services.
- **Persistence**: Uses MongoDB for stock data and MSSQL for order and saga state persistence.

## Architecture

```bash
   [Order Service] → [Stock Service] → [Payment Service]
               ↘              ↘            ↘
         [State Machine Orchestrator (Saga)]
```
- All services communicate asynchronously via RabbitMQ.
- The State Machine orchestrates the workflow and manages state transitions.

## Technologies Used

- **.NET 8**: Core framework for all services.
- **MassTransit**: Saga orchestration and messaging.
- **RabbitMQ**: Message broker for inter-service communication.
- **Entity Framework Core**: ORM for database operations and migrations (Order and StateMachine services).
- **MongoDB**: Stock service database.
- **MSSQL**: Order service and saga state database.

## Getting Started

### Prerequisites
- .NET 8 SDK
- RabbitMQ
- MassTransit
- MongoDB (optional)
- SQL Server (optional)
- Entity Framework Core

### Setup
1. Clone the repository:
```bash
git clone <repository-url>
```
2. Configure connection strings in each service's `appsettings.json` for RabbitMQ, MongoDB, and MSSQL.
3. Run database migrations if needed.
4. Start RabbitMQ and MongoDB servers.
5. Build and run each service:
```bash
dotnet build
dotnet run
```

## Usage
- Create an order via the Order API.
- The saga orchestrator will coordinate stock validation and payment processing automatically.
- Order status will update based on the outcome of each step.