# SmsTest - Тестовое задание

## Описание
Решение тестового задания.

**Состав:**
- `SmsTestLibrary` - библиотека для HTTP/gRPC вызовов
- `SmsTestConsole` - консольное приложение
- `SmsTestWpf` - WPF приложение
- `SmsTest.Tests` - unit-тесты

**Стек:** .NET 8, EF Core, PostgreSQL, Serilog, gRPC, xUnit


Применение миграции

bash
cd src/SmsTestConsole
dotnet ef database update

Запуск приложения

bash
dotnet run --project src/SmsTestConsole
Режимы работы
Режим	UseGrpc	UseMockData
HTTP	false	false
gRPC	true	false
Mock	false	true
?? Тесты
bash
dotnet test