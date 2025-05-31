# Hello World Demo

This example demonstrates the simplest ProtoScript program.
Follow these steps to run it using the website included in this repository.

## 1. Build the solution

From the repository root run:

```bash
dotnet build Ontology/Ontology8.sln
```

## 2. Launch the portal

Start the web portal which hosts the ProtoScript editor:

```bash
dotnet run --project Buffaly.Ontology.Portal
```

When the server starts it will open a browser window to `/protoscript`.

## 3. Load the example

In the editor choose **Load** and open `projects/hello.pts`.
This file lives under `Buffaly.Ontology.Portal/wwwroot/projects/hello.pts`.

## 4. Compile and run

Click **Compile** then **Run**. The output window will display
```
Hello World!
```
which comes from the `main()` function defined in `hello.pts`.
