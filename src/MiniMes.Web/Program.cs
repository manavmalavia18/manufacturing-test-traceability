using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Components;
using MiniMes.Web.Data;
using MiniMes.Web.Domain;
using MiniMes.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServer");
if (string.IsNullOrWhiteSpace(sqlServerConnection))
{
    builder.Services.AddDbContext<MiniMesDbContext>(options =>
        options.UseInMemoryDatabase("MiniMesDemo"));
}
else
{
    builder.Services.AddDbContext<MiniMesDbContext>(options =>
        options.UseSqlServer(sqlServerConnection, sql => sql.EnableRetryOnFailure()));
}

builder.Services.AddScoped<TestResultService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddProblemDetails();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAntiforgery();

var testsApi = app.MapGroup("/api/tests").WithTags("Manufacturing Tests");

testsApi.MapPost("/", async (TestSubmission submission, TestResultService service, CancellationToken ct) =>
{
    try
    {
        var result = await service.SubmitAsync(submission, ct);
        return Results.Ok(result);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["submission"] = [exception.Message]
        });
    }
});

testsApi.MapGet("/recent", async (int? count, TestResultService service, CancellationToken ct) =>
    Results.Ok(await service.GetRecentAsync(count ?? 20, ct)));

testsApi.MapGet("/serial/{serialNumber}", async (
    string serialNumber,
    TestResultService service,
    CancellationToken ct) =>
{
    var history = await service.GetHistoryAsync(serialNumber, ct);
    return history.Count == 0 ? Results.NotFound() : Results.Ok(history);
});

app.MapGet("/api/dashboard", async (TestResultService service, CancellationToken ct) =>
    Results.Ok(await service.GetSummaryAsync(ct)));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await SeedData.InitializeAsync(app.Services);
await app.RunAsync();

public partial class Program;
