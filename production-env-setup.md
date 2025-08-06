# Production Environment Variables Setup

## 🔒 Recommended: GitLab CI/CD Variables (Secure)

Để bảo mật hơn, bạn nên set các biến environment trong GitLab Project Settings > CI/CD > Variables:

### Required Variables:
```
DB_CONNECTION_STRING = Host=database.purintech.id.vn;Port=5432;Database=MovieTheater;Username=postgres;Password=<Hu@nH0aH0n9>;
JWT_SECRET_KEY = GvlKp62o31vaP3WzWsQDO3hOQpZdbhtt
VNPAY_TMN_CODE = 7QRTMNBH
VNPAY_HASH_SECRET = 4046G9C8YY9H8WFSJG8AVB9VZNTT1D68
EMAIL_PASSWORD = jurk cvow hmtg dovz
```

### CI/CD Update để sử dụng variables:
```bash
docker run -d \
  --name team03-webapi \
  -p 8081:8081 \
  --restart unless-stopped \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__Local=$DB_CONNECTION_STRING" \
  -e "JwtSettings__SecretKey=$JWT_SECRET_KEY" \
  -e "VnPay__TmnCode=$VNPAY_TMN_CODE" \
  -e "VnPay__HashSecret=$VNPAY_HASH_SECRET" \
  -e "EmailSettings__Password=$EMAIL_PASSWORD" \
  team03-webapi:latest
```

## 📋 Current Implementation: Direct Environment Variables

Hiện tại đã inject trực tiếp tất cả variables từ appsettings.Production.json vào container.

### Environment Variables được inject:
- ✅ ASPNETCORE_ENVIRONMENT=Production
- ✅ ConnectionStrings__Local (Production database)
- ✅ JwtSettings (Complete configuration)
- ✅ VnPay (Payment gateway settings)
- ✅ EmailSettings (SMTP configuration)

### URL Endpoints (Production):
- 🌐 Application: http://vps.purintech.id.vn:8081
- 💳 VnPay Return URLs: http://vps.purintech.id.vn:8081/api/Payment/vnpay-return/*
- 📧 Email Service: Configured with Gmail SMTP

## 🔄 Port Synchronization Completed:
- ✅ Dockerfile: EXPOSE 8081
- ✅ CI/CD Pipeline: -p 8081:8081
- ✅ docker-compose.yml: "8081:8081"
- ✅ appsettings.Production.json: VnPay URLs port 8081
