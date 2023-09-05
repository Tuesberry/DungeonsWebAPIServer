# WebAPIServer
'Dungeons' 게임 API 서버입니다.

* ASP.NET Core Web API Server
* Mysql, Redis

## How to Use

먼저 ASP.NET Core 서버 파일을 다운 받는다. </br>

이 Web API Server를 정상적으로 실행하기 위해서는 Mysql과 Redis 서버가 필요하다. </br> Mysql 서버에는 database와 table이 만들어져 있어야 한다. </br>
그리고 MasterData 테이블에는 데이터가 있어야 한다. MasterData는 임의로 추가하면 된다.</br>

스키마 구조는 아래를 참고한다. </br>

[DB Schema 바로가기](https://github.com/Tuesberry/WebAPIServer/blob/main/DbSchema.md)</br>

### Docker Compose를 사용한 개발 환경 설정 

```YAML
version: '3'

services:
  mysql:
    image: mysql:latest
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: rkawk123
      MYSQL_USER: Tuesberry
      MYSQL_PASSWORD: rkawk123
    ports: 
      - "3306:3306"
    container_name: "docker-mysql"
    volumes:
      - ./db-data:/var/lib/mysql
  redis:
    image: redis:latest
    restart: always
    container_name: "docker-redis"
    command: redis-server --port 6379
    volumes:
      - ./redis-data:/data
    ports:
      - "6379:6379"
```



## How to Deploy

### Docker Compose를 이용한 서버 배포

1. Dockerfile과 docker-entrypoint.sh 파일을 API 서버 폴더와 같은 경로에 만든다.
2. Dockerfile을 통해 ASP.NET Core Docker Image를 빌드한다. 
```shell
docker build -t aspnetapp .
```
3. docker-compose 파일을 통해 도커 컴포즈를 실행한다.
```shell
docker-compose up
```
#### Dockerfile

[참고 ) ASP.NET Core Docker Image 만드는 방법](https://learn.microsoft.com/ko-kr/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-7.0)

```Docker
# building .NET container images
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY TuesberryAPIServer/*.csproj .
RUN dotnet restore --use-current-runtime

# copy everything else and build app
COPY TuesberryAPIServer/. .
RUN dotnet publish --use-current-runtime --self-contained false --no-restore -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# netcat
RUN apt-get update && apt-get install netcat-openbsd -y

# set workdir
WORKDIR /app

# copy file
COPY --from=build /app .
ADD docker-entrypoint.sh .

# run
RUN chmod +x docker-entrypoint.sh
ENTRYPOINT ["/bin/sh", "docker-entrypoint.sh"]
```

#### entrypoint.sh

```shell
#!/bin/bash

# wait for mysql docker to be running
while ! nc -w1 mysql 3306; do 
    echo "server wait"
    sleep 1 
done

# run api server
echo "api server start running"

dotnet TuesberryAPIServer.dll
```

#### docker-compose.yml

```YAML
version: '3'

services:
  mysql:
    image: mysql:latest
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: rkawk123
      MYSQL_USER: Tuesberry
      MYSQL_PASSWORD: rkawk123
    ports:
      - "3306:3306"
    container_name: "mysql-server"
    volumes:
      - ./db-data:/var/lib/mysql
  redis:
    image: redis:latest
    restart: always
    container_name: "redis-server"
    command: redis-server --port 6379
    volumes:
      - ./redis-data:/data
    ports:
      - "6379:6379"
  api_server:
    image: tuesberry/api_server:latest
    depends_on:
      - mysql
      - redis
    restart: always
    container_name: "api_server"      
    ports:
      - "8000:8000"
```

### Kubernetes를 이용한 서버 배포

```YAML
apiVersion: v1
kind: Service
metadata:
  name: mysql
spec:
  type: ClusterIP
  selector:
    app: mysql
  ports:
  - port: 3306
    targetPort: 3306
---
apiVersion: v1
kind: Service
metadata:
  name: redis
spec:
  type: ClusterIP
  selector:
    app: redis
  ports:
  - port: 6379
    targetPort: 6379
---
apiVersion: v1
kind: Service
metadata:
  name: apiserver
spec:
  type: NodePort
  ports:
  - port: 8000
    targetPort: 8000
    nodePort: 30001
  selector:
    app: dotnet-api-server
---
apiVersion: v1
kind: Pod
metadata:
  name: redis-server
  labels:
    app: redis
spec:
  containers:
  - name: redis-server
    image: redis
    ports:
    - containerPort: 6379
    command: ['redis-server']
    volumeMounts:
    - name: redis-storage
      mountPath: /data
  volumes:
  - name: redis-storage
    hostPath:
      path: /src/redis-data 
      type: Directory
---
apiVersion: v1
kind: Pod
metadata:
  name: mysql-served
  labels:
    app: mysql
spec:
  containers:
  - name: mysql-server
    image: mysql
    env:
    - name: MYSQL_ROOT_PASSWORD
      value: rkawk123
    - name: MYSQL_USER
      value: Tuesberry
    - name: MYSQL_PASSWORD
      value: rkawk1234
    ports:
    - containerPort: 3306
    volumeMounts:
    - name: mysql-data
      mountPath: /var/lib/mysql
  volumes:
  - name: mysql-data
    hostPath:
      path: /src/db-data
      type: Directory
---
apiVersion: v1
kind: Pod
metadata:
  name: dotnet-api-server
  labels:
    app: dotnet-api-server
spec:
  containers:
  - name: dotnet-api-server
    image: tuesberry/dotnet_api_server:latest
    ports:
    - containerPort: 8000
  initContainers:
  - name: init-mysql
    image: busybox:1.28
    command: ['sh', '-c', 'while ! nc -w1 mysql 3306; do echo wait mysql; sleep 1; done']
```