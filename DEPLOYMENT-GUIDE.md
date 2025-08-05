# Full Stack Deployment Guide

## 🚀 What's Now Deployed

### Complete Infrastructure:
- ✅ **API Application** (team03-webapi) - Port 8081
- ✅ **PostgreSQL Database** (team03-db) - Port 5432 (internal)
- ✅ **Elasticsearch** (team03-elasticsearch) - Port 9200

### New Features Added:

#### 1. **Optimized Upload Process**
- Excludes unnecessary files (bin, obj, .git, node_modules)
- Shows archive size before upload
- Retry logic for failed uploads (3 attempts)
- Disk space checking before upload

#### 2. **Full Stack Deployment**
- Uses docker-compose.production.yml
- Environment variables injection
- Service health checks
- Complete logging and monitoring

#### 3. **Database Integration**
- PostgreSQL 15 with initialization scripts
- Persistent data volumes
- Health checks for database connectivity
- UTF-8 encoding and proper collation

#### 4. **Production Environment**
- Secure environment variables
- Internal network communication
- Service dependencies and restart policies
- Comprehensive logging

## 🔧 Service URLs (Production):

| Service | URL | Description |
|---------|-----|-------------|
| **API Application** | http://vps.purintech.id.vn:8081 | Main application |
| **Swagger UI** | http://vps.purintech.id.vn:8081/swagger | API documentation |
| **Health Check** | http://vps.purintech.id.vn:8081/health | Service health |
| **Elasticsearch** | http://vps.purintech.id.vn:9200 | Search service |
| **Database** | Internal only (port 5432) | PostgreSQL |

## 🎯 Deployment Flow:

1. **Source Code**: Optimized archive creation and upload
2. **Docker Build**: API application image building  
3. **Stack Deployment**: docker-compose with all services
4. **Health Checks**: Database, Elasticsearch, API verification
5. **Status Report**: Complete service status and URLs

## 🛠️ What's Fixed:

- ❌ **Upload Failure**: Added retry logic and file size optimization
- ❌ **Single Container**: Now full stack with database
- ❌ **Manual Environment**: Automated environment variable injection
- ❌ **No Database**: PostgreSQL with initialization scripts
- ❌ **No Search**: Elasticsearch integration

The deployment will now handle the complete infrastructure needed for your application!
