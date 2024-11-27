using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public interface IFactService
{
    public Task<IResult> FetchAndSaveFactAsync();
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

            var factEntry = $"Fact: {factResponse.Fact} | Length: {factResponse.Length}";
            if (!File.Exists(FileName))
            {
                using var strem = File.Create(FileName);
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
}