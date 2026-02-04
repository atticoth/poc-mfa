# POC MFA (React + ASP.NET Core)

## Estrutura de pastas

```
backend/
  src/
    Application/
    Controllers/
    Domain/
    Infrastructure/
    Program.cs
    PocMfa.Api.csproj
    appsettings.json
frontend/
  src/
    components/
    context/
    guards/
    hooks/
    pages/
    services/
  index.html
  package.json
  tsconfig.json
  vite.config.ts
```

## Backend (.NET 8)

### Principais decisões técnicas
- **ASP.NET Identity** para hash seguro, lockout, tokens de reset e gestão de usuários.
- **JWT + Refresh Token** com rotação e persistência em banco (hash do token, nunca o valor puro).
- **Rate limiting** por janela fixa para mitigar brute force.
- **Auditoria** com tabela dedicada para eventos de autenticação.
- **HTTPS/HSTS/CORS** configurados no pipeline.

### Migrations
```bash
cd backend/src
 dotnet ef migrations add InitialCreate
 dotnet ef database update
```

## Frontend (Vite + React)

### Armazenamento seguro de JWT
- **Access token em memória** (state do React).
- **Refresh token em cookie HttpOnly** (emitido pelo backend).
- **Proxy de desenvolvimento**: o Vite redireciona `/api` para `https://localhost:5001` com `secure: false` para evitar erros de certificado durante o desenvolvimento.

## Como rodar

### Backend
```bash
cd backend/src
 dotnet restore
 dotnet run
```

### Frontend
```bash
cd frontend
 npm install
 npm run dev
```

## Boas práticas para produção
- Use **Azure Key Vault** ou **AWS Secrets Manager** para secrets.
- Enforce **TLS 1.2+** e **HSTS** com preload.
- Ative **rotation de chaves** e **observabilidade** (Application Insights, CloudWatch).
- Configure **CORS** com domínios exatos.
- Aplique **firewall de aplicação** (WAF) e **rate limiting** por IP + usuário.

## Deploy (Azure/AWS)

### Azure
1. Crie um App Service para o backend e Static Web App para o frontend.
2. Configure Connection String e secrets no App Service.
3. Use Azure SQL para o banco.

### AWS
1. Use ECS/Fargate ou Elastic Beanstalk para o backend.
2. S3 + CloudFront para o frontend.
3. RDS SQL Server para o banco.
