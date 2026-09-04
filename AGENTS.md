# Agent Guide

## Project

WebLinks is a .NET 10 Avalonia desktop application for tracking links and events. The solution is named `personal-event-tracker.sln`; the application project is `WebLinks.csproj`.

## Commands

```sh
dotnet build personal-event-tracker.sln
dotnet run --project WebLinks.csproj
dotnet watch run --project personal-event-tracker.sln
dotnet publish personal-event-tracker.sln
```

There is currently no test project or automated test suite. Run the build after source changes.

## Structure

- `Program.cs`, `App.axaml`, and `App.axaml.cs`: application startup and Avalonia composition.
- `Views/`: Avalonia XAML views and code-behind; `MainWindow` hosts the primary UI.
- `ViewModels/`: ReactiveUI view models and presentation logic.
- `Models/`: persisted domain records and shared interfaces.
- `Repositories/`: TSV persistence, link metadata retrieval, filesystem paths, and external integrations.
- `Assets/`: Avalonia resources.
- `Settings.json`: local runtime configuration; do not assume its paths apply to another machine.

## Conventions

- Use ReactiveUI properties with `RaiseAndSetIfChanged`.
- Prefer constructor injection for datasources and external services.
- Keep Avalonia views consistent with the existing `x:DataType` and compiled-binding patterns.
- Persisted item types implement `Models/Interfaces/IItem.cs` and use the existing `[Table("...")]` naming convention for TSV files.
- Preserve existing namespace conventions, including the inconsistency between `Repositories` and `WebLinks.Repositories`.
- Preserve the existing spelling of `Repositories/FileRepsitory.cs` unless a coordinated rename is required.

## Data Behavior

`TsvDatasource` stores tab-delimited data under the configured datasource path. Dates use `yyyy-MM-dd HH:mm:ss`. Runtime settings and data directories are created or resolved by `ViewModels/Settings.cs` and `Repositories/Paths.cs`.

When changing persistence or relationships, account for existing behavior: missing related TSV rows can fail through `First(...)`, and `TsvDatasource.Update<T>` currently has event-file behavior that may be intentional or may require a focused fix.

## Validation

Use `dotnet build personal-event-tracker.sln` as the default validation command. There are known unfinished paths that throw `NotImplementedException`, including parts of `LinksViewModel` and `TsvDatasource`; do not broaden unrelated fixes while working on a focused task.

See [README.md](README.md) for the brief project description and [WebLinks.csproj](WebLinks.csproj) for target framework and package versions.
