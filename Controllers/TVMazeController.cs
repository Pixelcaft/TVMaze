using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TVMaze.Core;
using TVMaze.Repository;

namespace TVMaze.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TVMazeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TVMazeController> _logger;
        private readonly TvMazeContext _dbContext;

        private async Task<bool> MonthYearExistsAsync(int year, int month)
        {
            if (await _dbContext.Shows.AnyAsync(s => s.Year == year && s.Month == month))
            {
                _logger.LogInformation($"Data for year {year} and month {month} already exists in the database.");

                // Bereken de top 10 acteurs.
                var topActorsResult = await CalculateTopActorsAsync(year, month);

                return true;
            }

            return false;
        }

        public TVMazeController(HttpClient httpClient, ILogger<TVMazeController> logger, TvMazeContext dbContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;

            // Set the base URL for the TVMaze API during initialization.
            _httpClient.BaseAddress = new Uri("https://api.tvmaze.com/");
        }

        [HttpGet("shows")]
        public async Task<IActionResult> GetShowsByMonthYearAsync([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                // Validate year and month
                if (year < 1900 || year > 2100 || month < 1 || month > 12)
                {
                    return BadRequest("Invalid year or month.");
                }

                bool monthYearExists = await MonthYearExistsAsync(year, month);

                if (monthYearExists)
                {
                    // Bereken de top 10 acteurs.
                    var topActorsResultMonthYear = await CalculateTopActorsAsync(year, month);

                    return Ok(new { TopActors = topActorsResultMonthYear });
                }

                var castInfoList = new List<CastInfo>();

                for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                //for (int day = 1; day <= 2; day++)
                {

                    // Format the request URL for the /schedule endpoint with the specified year, month, and day.
                    var requestUrl = $"schedule?date={year}-{month:D2}-{day:D2}";

                    // Perform the request and receive the response as a string.
                    var response = await PerformRequestWithRateLimitAsync(requestUrl);

                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var showElement in doc.RootElement.EnumerateArray())
                            {
                                var airdateProperty = showElement.GetProperty("airdate");
                                var showProperty = showElement.GetProperty("show");
                                var idProperty = showProperty.GetProperty("id");
                                var nameProperty = showProperty.GetProperty("name");

                                if (idProperty.ValueKind == JsonValueKind.Number)
                                {
                                    var showAirdate = airdateProperty.GetString();
                                    var showId = idProperty.GetInt32();
                                    var showName = nameProperty.GetString();

                                    try
                                    {
                                        var showEntity = new Show
                                        {
                                            ShowId = showId,
                                            Day = day,
                                            Month = month,
                                            Year = year,
                                        };
                                        _dbContext.Shows.Add(showEntity);

                                        await _dbContext.SaveChangesAsync();

                                        var castResponse = await PerformRequestWithRateLimitAsync($"shows/{showId}/cast");

                                       // var castlist2 = JsonSerializer.Deserialize <CastMember[]>(castResponse);

                                        var castData = JsonDocument.Parse(castResponse);
                                        var castList = castData.RootElement.EnumerateArray()
                                            .Select(actor => new CastMember
                                            {
                                                PersonId = actor.GetProperty("person").TryGetProperty("id", out var idElement) ? idElement.GetInt32() : 0,
                                                PersonName = actor.GetProperty("person").TryGetProperty("name", out var nameElement) ? nameElement.GetString() : "Unknown"
                                            })
                                            .ToList();

                                        castInfoList.Add(new CastInfo { ShowAirdate = showAirdate, ShowId = showId, ShowName = showName, Cast = castList });
                                        _logger.LogInformation($"Processing show: {showId}, {showName}");

                                        foreach (var castMember in castList)
                                        {
                                            var actorEntity = new Actor
                                            {
                                                ActorId = castMember.PersonId,
                                                ActorNames = castMember.PersonName,
                                                ShowID = showEntity.ID
                                            };
                                            _dbContext.Actors.Add(actorEntity);
                                        }

                                        await _dbContext.SaveChangesAsync();

                                    }
                                    catch (HttpRequestException ex)
                                    {
                                        _logger.LogError($"Error retrieving cast for show {showId}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }

                // Bereken de top 10 acteurs.
                var topActorsResult = await CalculateTopActorsAsync(year, month);

                return Ok(new { TopActors = topActorsResult });

            }
            catch (HttpRequestException)
            {
                // Handle HttpRequestException here if an error occurs while making the request.
                return StatusCode(500, "An error occurred while retrieving the data.");
            }
        }

        private async Task<List<object>> CalculateTopActorsAsync(int year, int month)
        {
            // Bereken het totale aantal acteurs in de Actors-tabel voor de opgegeven maand en jaar.
            var totalActors = await _dbContext.Actors
                .Join(_dbContext.Shows,
                      actor => actor.ShowID,
                      show => show.ID,
                      (actor, show) => new { actor, show })
                .Where(joined => joined.show.Year == year && joined.show.Month == month)
                .CountAsync();

            // Bereken het percentage voor elke acteur in de top 10 voor de opgegeven maand en jaar.
            var topActors = await _dbContext.Actors
                .Join(_dbContext.Shows,
                      actor => actor.ShowID,
                      show => show.ID,
                      (actor, show) => new { actor, show })
                .Where(joined => joined.show.Year == year && joined.show.Month == month)
                .GroupBy(joined => joined.actor.ActorId)
                .Select(group => new
                {
                    ActorId = group.Key,
                    Percentage = (double)group.Count() / totalActors * 100,
                    ActorName = group.First().actor.ActorNames ?? "Unknown"
                })
                .OrderByDescending(x => x.Percentage)
                .Take(10)
                .ToListAsync();

            return topActors.Cast<object>().ToList();
        }


        private async Task<string> PerformRequestWithRateLimitAsync(string requestUrl)
        {
            try
            {
                // Perform the request and delay to comply with the rate limit.
                await Task.Delay(500);

                // Perform the request and receive the response as a string.
                var response = await _httpClient.GetStringAsync(requestUrl);

                return response;
            }
            catch (Exception ex)
            {
                // Handle any errors that occur while making the request.
                _logger.LogError($"Error performing request: {ex.Message}");
                throw; // Re-throw the exception to propagate the issue.
            }
        }



    }
}