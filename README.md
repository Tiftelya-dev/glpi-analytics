# GLPI Analytics System

Система аналитики IT-заявок на основе GLPI и Docker.

## Технологии
- Docker + Docker Compose
- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL + MariaDB
- React + Vite + Recharts

## Архитектура
Система состоит из 5 Docker-контейнеров:
- **MariaDB** — база данных для GLPI
- **GLPI** — система управления заявками (порт 8080)
- **PostgreSQL** — база данных бэкенда
- **Backend** — ASP.NET Core REST API (порт 5000)
- **Frontend** — React приложение через Nginx (порт 3000)

## Запуск

1. Скопируй `.env.example` в `.env` и заполни токены GLPI
2. Запусти систему:
```bash
docker compose up -d --build
```
3. Открой браузер: http://localhost:3000

## Конфигурация
Создай файл `.env` по образцу:
```
MYSQL_ROOT_PASSWORD=rootpass123
MYSQL_DATABASE=glpi
MYSQL_USER=glpi
MYSQL_PASSWORD=glpipass123
POSTGRES_DB=tickets
POSTGRES_USER=postgres
POSTGRES_PASSWORD=pgpass123
GLPI_APP_TOKEN=твой_токен
GLPI_USER_TOKEN=твой_токен
```
