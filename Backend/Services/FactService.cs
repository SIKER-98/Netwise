using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public interface IFactService
{
    Task<IResult> FetchAndSaveFactAsync();
    Task<IResult> GetSavedFactsAsync();
}

public class FactService : IFactService
{
    private readonly HttpClient _httpClient;
    private const string FileName = "data/facts.txt";

    public FactService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IResult> FetchAndSaveFactAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://catfact.ninja/fact");
            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem("Nie udało się pobrać danych z API",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            var content = await response.Content.ReadAsStringAsync();
            var factResponse = JsonSerializer.Deserialize<FactResponse>(content);

            if (factResponse == null || string.IsNullOrEmpty(factResponse.Fact))
            {
                return Results.Problem("Nie udało się przetworzyć odpowiedzi API",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            var factEntry = JsonSerializer.Serialize(factResponse);
            if (!File.Exists(FileName))
            {
                await using var stream = File.Create(FileName);
            }

            await File.AppendAllTextAsync(FileName, factEntry + Environment.NewLine);

            return Results.Ok(factResponse);
        }
        catch (Exception ex)
        {
            return Results.Problem("Wystąpił nieoczkiwany błąd: " + ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<IResult> GetSavedFactsAsync()
    {
        try
        {
            if (!File.Exists(FileName))
            {
                return Results.Ok(new List<FactResponse>());
            }

            var lines = await File.ReadAllLinesAsync(FileName);
            var facts = lines
                .Select(line => JsonSerializer.Deserialize<FactResponse>(line))
                .Where(fact => fact != null)
                .ToList();

            return Results.Ok(facts);
        }
        catch (Exception ex)
        {
            return Results.Problem("Wystąpił nieoczekiwany błąd: " + ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}