# Bank Project - API with Messaging

This project is the back-end of a digital bank, built in .NET 8, which allows the registration of clients, agencies, and the asynchronous request of banking products using messaging (RabbitMQ).

## 1. Team Identification
- **Name:** João Gabriel 
- **Name:** Caio Lucas 

## 2. Chosen Banking Product and Justification
**Chosen Product:** Loan (Empréstimo).
**Justification:** The contracting of a loan should never be synchronous, as it requires consultation and analysis of credit scores in third-party institutions (such as Serasa/SPC). This communication can be slow or suffer math/network instabilities. Shifting this validation to an asynchronous background processing absorbs these impacts without blocking the client's initial flow.

## 3. Queue Modeling Decision
**Approach Used:** A single queue (`contratacao-solicitada`).
**Justification:** We opted to keep the design simple by using only one queue that unifies all pending contract requests. As the duo's scope encompasses a specific business rule (Loan), centralization reduces infrastructure complexity. The processor (`BackgroundService`) reads the payload, identifies the ID, and directs the rules (such as randomized score calculation).

## 4. How to run locally

### Prerequisites
- .NET 8.0 SDK
- Docker & Docker Compose
- Access to FIAP VPN or unlocked connection to Oracle database.

### Starting RabbitMQ
At the root of the project, where the `docker-compose.yml` file is located, run:

```sh
docker-compose up -d
```

### Running Migrations and the API

```sh
cd BankProject.Api
# Run migrations to create tables in Oracle
dotnet ef database update

# Start the API
dotnet run
```
Swagger will be accessible at: `http://localhost:5000/swagger` (or the port specified in the prompt).

## 6. Available Endpoints

### Create Agency
`POST /api/agencias`

```json
{
  "nome": "Agencia Central",
  "numero": "001"
}
```

### Create Client (Individual)
`POST /api/clientes/pf`

```json
{
  "nome": "João Silva",
  "cpf": "12345678901",
  "dataNascimento": "2000-01-01T00:00:00Z",
  "agenciaId": 1
}
```

### Request Contract (Asynchronous)
`POST /api/contratacoes`

```json
{
  "clienteId": 1,
  "agenciaId": 1,
  "produtoId": 1
}
```
**API Immediate Response (202 Accepted):**

```json
{
  "status": "PENDENTE",
  "id": 1,
  "clienteId": 1
}
```

### Check Contract Status
`GET /api/contratacoes/1`

```json
{
  "status": "APROVADA",
  "mensagemMotivo": "",
  "id": 1
}
```

## 7. How to Execute Automated Tests
At the root of the project, run the command:

```sh
dotnet test
