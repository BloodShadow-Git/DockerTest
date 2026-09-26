# Пакеты

Чтобы собрать эту архитектуру (C# Консоль + FluentMigrator + Postgres + RabbitMQ + MassTransit + Outbox), вам понадобятся два инструмента на компьютере и несколько NuGet-пакетов в коде.
Вот финальный чек-лист того, что нужно установить.

## 1. Установить на компьютер (Инфраструктура)

* Docker Desktop (или Docker Engine + Compose) — чтобы запустить Postgres и RabbitMQ одной командой.

------------------------------

## 2. NuGet-пакеты для вашего C# проекта

Вам нужно добавить следующие библиотеки через менеджер пакетов Nuget (или команду dotnet add package):

## 📦 Для работы с базой данных (Postgres и Миграции)

* Npgsql (официальный драйвер для подключения к PostgreSQL).
* Dapper (легковесный ORM для выполнения быстрых SQL-запросов).
* FluentMigrator (основное ядро для миграций).
* FluentMigrator.Runner (инструмент, который запускает миграции внутри вашего кода).
* FluentMigrator.Runner.PostgreSQL (плагин для поддержки синтаксиса Postgres).

## 📦 Для работы с сообщениями (RabbitMQ + Автоматический Outbox)

Чтобы не писать таблицы Outbox и фоновые воркеры руками, используйте MassTransit. Он всё сделает сам:

* MassTransit.RabbitMQ (шина сообщений, которая связывает код с RabbitMQ).
* MassTransit.Dapper (интеграция MassTransit с Dapper — он сам создаст нужные таблицы Outbox в Postgres и будет автоматически пересылать сообщения).

------------------------------

## 🛠️ Как это выглядит в коде (Краткий старт)

После скачивания пакетов, инициализация вашей консоли в Program.cs сведется к простой настройке служб (Dependency Injection):

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string connectionString = "Host=localhost;Database=my_db;Username=postgres;Password=pass";

        // 1. Настраиваем миграции FluentMigrator
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Program).Assembly).For.Migrations());

        // 2. Настраиваем MassTransit + RabbitMQ + Outbox
        services.AddMassTransit(x =>
        {
            // Говорим MassTransit использовать Outbox на базе Dapper
            x.AddEntityFrameworkOutbox<YourDbContext>(o => { o.UsePostgres(); }); // если используется EF, но для Dapper у MassTransit есть аналогичные расширения Sql/Postgres

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("localhost", "/");
                cfg.ConfigureEndpoints(ctx);
            });
        });
    })
    .Build();
// Перед запуском приложения — запускаем миграцииusing (var scope = host.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp(); // Создаст таблицы приложения
}
await host.RunAsync();

Вам помочь составить готовый docker-compose.yml для запуска Postgres и RabbitMQ, чтобы вы могли сразу запустить и протестировать скачанные пакеты?
