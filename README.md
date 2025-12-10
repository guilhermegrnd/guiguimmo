# Guiguimmo - Online RPG

A modern, multiplayer online RPG (MMORPG) built with a microservices architecture. Experience real-time gameplay with character management, global interactions, and a robust game engine powering it all.

## 🎮 Features

- **Real-Time Multiplayer** - SignalR-powered live gameplay and interactions
- **Character Management** - Create and manage multiple characters with unique attributes
- **Microservices Architecture** - Scalable, independent services for characters, game, identity, and global systems
- **Message-Driven** - Apache Kafka for asynchronous communication between services
- **OpenID Connect Authentication** - Secure user authentication and authorization
- **Modern UI** - React 19 frontend with TypeScript and Tailwind CSS
- **ASP.NET Core 9.0** - Enterprise-grade backend services

## 📋 Architecture

### Services

- **Guiguimmo.App** - React/Vite frontend application
- **Guiguimmo.Characters** - Character creation and management service
- **Guiguimmo.Game** - Core game engine with real-time multiplayer support
- **Guiguimmo.Global** - Global game state and services
- **Guiguimmo.Identity** - User authentication and OpenID Connect provider
- **Guiguimmo.Consumer** - Asynchronous message consumer for event processing
- **Guiguimmo.Common** - Shared libraries and utilities

### Infrastructure

- **MongoDB** - NoSQL database for flexible data storage
- **Apache Kafka** - Event streaming and asynchronous messaging
- **Docker Compose** - Containerized development environment

## 🚀 Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) or Docker + Docker Compose
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)

### Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/guilhermegrnd/guiguimmo.git
   cd guiguimmo
   ```

2. **Add Common package as source**
   ```bash
   dotnet nuget add source ./packages -n guiguimmopackages
   ```

3. **Start infrastructure services**
   ```bash
   docker-compose up -d
   ```
   This starts:
   - MongoDB (port 27017)
   - Zookeeper (port 2181)
   - Apache Kafka (port 9092)

4. **Build and run backend services**
   
   Open separate terminals for each service:

   ```bash
   # Terminal 1: Identity Service
   cd Guiguimmo.Identity
   dotnet run

   # Terminal 2: Game Service
   cd Guiguimmo.Game
   dotnet run

   # Terminal 3: Characters Service
   cd Guiguimmo.Characters
   dotnet run

   # Terminal 4: Global Service
   cd Guiguimmo.Global
   dotnet run

   # Terminal 5: Consumer Service
   cd Guiguimmo.Consumer
   dotnet run
   ```

5. **Run the frontend application**
   ```bash
   cd Guiguimmo.App
   npm install
   npm run dev
   ```

   The app will be available at `http://localhost:3000`

## 📦 Technology Stack

### Frontend
- **React 19** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **Tailwind CSS** - Utility-first styling
- **React Router** - Client-side routing
- **SignalR Client** - Real-time communication
- **OIDC Client** - OpenID Connect authentication

### Backend
- **.NET 9.0** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **OpenIddict** - OpenID Connect & OAuth 2.0 provider
- **SignalR** - Real-time communication hubs
- **Kafka** - Event streaming

### Infrastructure
- **MongoDB** - Document database
- **Apache Kafka** - Message broker
- **Docker** - Containerization

## 📁 Project Structure

```
guiguimmo/
├── Guiguimmo.App/              # React frontend application
│   ├── src/
│   │   ├── components/         # React components
│   │   ├── pages/             # Page components
│   │   ├── context/           # React context
│   │   ├── hooks/             # Custom React hooks
│   │   ├── api/               # API client
│   │   ├── oidc/              # OIDC configuration
│   │   └── types/             # TypeScript types
│   └── vite.config.js
├── Guiguimmo.Characters/       # Character service
│   ├── Controllers/
│   ├── Models/
│   ├── Dtos/
│   └── Extensions/
├── Guiguimmo.Game/             # Game engine & multiplayer
│   ├── Services/
│   ├── Hubs/                   # SignalR hubs
│   ├── Models/
│   └── Utils/
├── Guiguimmo.Global/           # Global game state
├── Guiguimmo.Identity/         # Authentication service
├── Guiguimmo.Consumer/         # Kafka message consumer
├── Guiguimmo.Common/           # Shared utilities & interfaces
├── packages/                   # NuGet packages
└── docker-compose.yml          # Docker Compose configuration
```

## 🛠️ Common Tasks

### View Service Logs
```bash
# Docker Compose logs
docker-compose logs -f kafka
docker-compose logs -f mongo
```

### Stop Infrastructure
```bash
docker-compose down
```

### Clean Up (remove volumes)
```bash
docker-compose down -v
```

### Build Frontend
```bash
cd Guiguimmo.App
npm run build
```

### Lint Frontend Code
```bash
cd Guiguimmo.App
npm run lint
```

## 📝 Configuration

Each service has configuration files:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development overrides

Key configuration areas:
- Database connection strings (MongoDB)
- Kafka broker addresses
- OIDC settings
- CORS allowed origins
- API endpoints

## 🔐 Authentication

The project uses OpenID Connect (OIDC) for authentication:
- Identity service acts as the OIDC provider
- Frontend uses `oidc-client-ts` library for secure login flow
- All API requests include bearer token authentication

## 📊 Real-Time Communication

SignalR hubs enable real-time features:
- Player position updates
- Chat messages
- Combat events
- Status notifications

## 🤝 Contributing

1. Create a feature branch
2. Implement your changes
3. Test thoroughly
4. Submit a pull request

## 📄 License

[Add appropriate license information]

## 📧 Support

For issues, questions, or contributions, please open an issue on GitHub.

---

**Happy adventuring!** ⚔️🛡️✨