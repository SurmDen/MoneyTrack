# 💰 MoneyTrack - Система управления личными финансами

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-purple?style=for-the-badge&logo=.net)
![Entity Framework](https://img.shields.io/badge/Entity_Framework_Core-8.0-red?style=for-the-badge&logo=dotnet)
![SQLite](https://img.shields.io/badge/SQLite-3.x-blue?style=for-the-badge&logo=sqlite)
![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)
![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)

**MoneyTrack** - это полнофункциональное веб-приложение для управления личными финансами, разработанное как тестовое задание. Приложение демонстрирует современные подходы к разработке full-stack приложений на .NET платформе.

## 🎯 Основные возможности

### 💼 Управление кошельками
- **Создание кошельков** в разных валютах (RUB, USD, EUR)
- **Автоматический расчет баланса** - начальный баланс + доходы - расходы
- **Валидация операций** - предотвращение транзакций при недостаточном балансе
- **Мультивалютность** - автоматическая конвертация при переводах между разными валютами

### 📊 Финансовая аналитика
- **Группировка транзакций** по типам (Доходы/Расходы)
- **Сортировка транзакций** по дате и сумме
- **Топ-3 самые большие траты** для каждого кошелька
- **Месячная статистика** - сводка доходов и расходов за выбранный период

### 🔄 Операции с деньгами
- **Безопасные переводы** между кошельками
- **Контроль баланса** в реальном времени
- **Подробное описание** для каждой транзакции
- **Автоматическое обновление** данных после операций

## 🏗️ Архитектура приложения

### Многослойная архитектура
### MoneyTrackSolution
- 📁 MoneyTrack.Web/ # Веб-приложение (API + Frontend)
- 📁 MoneyTrack.Application/ # Слой приложения (интерфейсы)
- 📁 MoneyTrack.Domain/ # Доменный слой (сущности, DTO)
- 📁 MoneyTrack.Infrastructure/ # Инфраструктура (репозитории, сервисы)

### Ключевые технологии
- **Backend**: ASP.NET Core 8.0, Entity Framework Core, SQLite
- **Frontend**: Чистый HTML/CSS/JavaScript (без фреймворков)
- **API**: RESTful архитектура с JSON
- **База данных**: SQLite с миграциями Entity Framework
- **Кэширование**: In-memory кэширование курсов валют

## 🚀 Быстрый старт

### Предварительные требования
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Установка и запуск
### Клонирование репозитория
```
git clone https://github.com/SurmDen/moneytrack.git

cd moneytrack
```
### Восстановление зависимостей
```
dotnet restore
```
### Запуск приложения
```
dotnet run --project MoneyTrackSolution/MoneyTrack.Web/
```

### Приложение будет доступно по ссылке
http://localhost:port/main
