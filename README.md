# Project Management API

## Overview

This is a comprehensive Project Management API built with C\# and ASP.NET Core. The application provides a robust backend solution for managing projects, tasks, teams, and users in a collaborative environment. It features a clean architecture with proper separation of concerns, Docker containerization, and nginx load balancing for scalable deployment.

## 🚀 Features

### Core Functionality

- **Project Management**: Create, update, delete, and track projects with detailed metadata
- **Task Management**: Full lifecycle task management with assignments, priorities, and status tracking
- **Team Collaboration**: Organize users into teams with role-based permissions
- **User Management**: Complete user registration, authentication, and profile management
- **Dashboard Analytics**: Project progress tracking and team performance metrics


### Technical Features

- **RESTful API Design** following industry best practices
- **JWT Authentication** for secure user sessions
- **Role-Based Access Control (RBAC)** for granular permissions
- **Docker Containerization** for consistent deployment
- **Nginx Load Balancing** for improved performance and scalability
- **Entity Framework Core** for efficient database operations
- **Clean Architecture** with separation of concerns


## 🛠 Technology Stack

- **Backend Framework:** ASP.NET Core (Latest Version)
- **Programming Language:** C\# (99.7%)
- **Database ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **Containerization:** Docker \& Docker Compose
- **Web Server:** Nginx (for load balancing and reverse proxy)
- **Database:** SQL Server (configurable for other providers)


## 📋 Prerequisites

Before running this project, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized deployment)
- SQL Server or any Entity Framework Core supported database
- Visual Studio 2022 or VS Code (recommended IDEs)


## 🚀 Getting Started

### Local Development Setup

1. **Clone the repository:**

```bash
git clone https://github.com/hassanolaa/ProjectManagementAPI.git
cd ProjectManagementAPI
```

2. **Restore NuGet packages:**

```bash
dotnet restore
```

3. **Configure Database Connection:**

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectManagementDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

4. **Apply Database Migrations:**

```bash
dotnet ef database update
```

5. **Run the application:**

```bash
dotnet run --project TaskManagementAPI
```


The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Docker Deployment

#### Using Docker Compose (Recommended)

1. **Start the entire stack:**

```bash
docker-compose up -d
```


This will start:

- The API application
- Nginx reverse proxy/load balancer
- Database services (if configured)

2. **Access the application:**
    - API: `http://localhost:8080`
    - Nginx status: `http://localhost:80`

#### Manual Docker Build

1. **Build the Docker image:**

```bash
docker build -t projectmanagement-api .
```

2. **Run the container:**

```bash
docker run -d -p 8080:80 --name pm-api projectmanagement-api
```


## 📁 Project Structure

```
ProjectManagementAPI/
├── TaskManagementAPI/           # Main API project
│   ├── Controllers/             # API endpoints and controllers
│   ├── Services/               # Business logic layer
│   ├── Data/                   # Data access layer and DbContext
│   ├── Models/                 # Entity models and DTOs
│   ├── Middleware/             # Custom middleware components
│   ├── Migrations/             # Entity Framework migrations
│   └── Properties/             # Launch settings and configurations
├── nginx/                      # Nginx configuration files
├── docker-compose.yml          # Multi-container orchestration
├── docker-compose.override.yml # Development overrides
├── .dockerignore              # Docker ignore patterns
├── nginx.conf                 # Nginx server configuration
└── TaskManagementAPI.sln      # Solution file
```


## 🔌 API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/logout` - User logout


### Projects

- `GET /api/projects` - Get all projects
- `GET /api/projects/{id}` - Get project by ID
- `POST /api/projects` - Create new project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project


### Tasks

- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/{id}` - Get task by ID
- `GET /api/projects/{projectId}/tasks` - Get tasks by project
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `PATCH /api/tasks/{id}/status` - Update task status


### Teams

- `GET /api/teams` - Get all teams
- `GET /api/teams/{id}` - Get team by ID
- `POST /api/teams` - Create new team
- `PUT /api/teams/{id}` - Update team
- `DELETE /api/teams/{id}` - Delete team
- `POST /api/teams/{id}/members` - Add team member
- `DELETE /api/teams/{id}/members/{userId}` - Remove team member


### Users

- `GET /api/users` - Get all users (admin only)
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user profile
- `GET /api/users/profile` - Get current user profile


## 🔧 Configuration

### Environment Variables

Create a `.env` file for environment-specific configurations:

```env
# Database
CONNECTION_STRING=Server=localhost;Database=ProjectManagementDB;Trusted_Connection=true;

# JWT Settings
JWT_SECRET=your-super-secret-jwt-key-here
JWT_ISSUER=ProjectManagementAPI
JWT_AUDIENCE=ProjectManagementUsers
JWT_EXPIRY_MINUTES=60

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://+:443;http://+:80
```


### Database Configuration

The application supports multiple database providers through Entity Framework Core:

- **SQL Server** (default)
- **PostgreSQL**
- **MySQL**
- **SQLite** (for development)

Update the connection string in `appsettings.json` or use environment variables.

## 🧪 API Testing

### Using Swagger UI

When running in development mode, access the Swagger documentation at:

- `https://localhost:5001/swagger`


### Sample API Calls

#### Register a new user:

```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```


#### Create a new project:

```bash
curl -X POST "https://localhost:5001/api/projects" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Website",
    "description": "Company website redesign",
    "startDate": "2025-08-16",
    "endDate": "2025-12-31"
  }'
```


## 🔒 Security Features

- **JWT Authentication** with refresh token support
- **Role-based authorization** (Admin, Manager, Developer, User)
- **Input validation** and sanitization
- **CORS configuration** for cross-origin requests
- **Rate limiting** to prevent abuse
- **HTTPS enforcement** in production


## 🚀 Deployment

### Production Deployment with Docker

1. **Build production image:**

```bash
docker build -f Dockerfile.prod -t projectmanagement-api:prod .
```

2. **Deploy with Docker Compose:**

```bash
docker-compose -f docker-compose.prod.yml up -d
```


### Environment-Specific Configurations

- **Development:** `appsettings.Development.json`
- **Staging:** `appsettings.Staging.json`
- **Production:** `appsettings.Production.json`


## 🤝 Contributing

We welcome contributions to improve this project! Please follow these guidelines:

### Getting Started

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines

- Follow C\# coding conventions and best practices
- Write unit tests for new features
- Update documentation for API changes
- Ensure Docker builds pass
- Test with multiple database providers when possible


### Code Review Process

- All submissions require review
- Maintain backward compatibility
- Follow semantic versioning for releases


## 📊 Performance \& Monitoring

### Nginx Configuration

The included nginx configuration provides:

- **Load balancing** across multiple API instances
- **Reverse proxy** for improved security
- **Static file serving** for documentation
- **Gzip compression** for better performance


### Health Checks

- `GET /health` - Application health status
- `GET /health/ready` - Readiness probe for Kubernetes
- `GET /health/live` - Liveness probe for Kubernetes


## 📝 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 📞 Support

For questions, issues, or contributions:

- **GitHub Issues:** [Create an issue](https://github.com/hassanolaa/ProjectManagementAPI/issues)
- **Documentation:** Check the `/docs` folder for detailed API documentation
- **Contact:** [hassanolaa](https://github.com/hassanolaa)


## 🎯 Roadmap

Future enhancements planned:

- [ ] Real-time notifications with SignalR
- [ ] File upload and document management
- [ ] Advanced reporting and analytics
- [ ] Mobile app support
- [ ] Integration with third-party tools (Slack, Jira)
- [ ] Advanced project templates

***
