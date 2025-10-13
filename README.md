# AlphaLising — Демонстрационный бэкенд

REST API на .NET 9 с кешированием (Redis) и хранением данных (MS SQL Server).  
Проект демонстрирует:
- Чистую архитектуру (Clean Architecture)
- Работу с EF Core, Redis, Docker
- Автоматическое создание БД и заполнение тестовыми данными через api запрос bulkproducts или bulkcatergories
- Эндпоинты http://localhost:5119/swagger/index.html

---

## 🚀 Как запустить (требуется только Docker)

1. Установите [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/kanzler26/AlphaLising.git
   cd AlphaLising
