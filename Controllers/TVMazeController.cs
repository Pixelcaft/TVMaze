using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TVMaze.Core;
using TVMaze.Repository;

namespace TVMaze.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public partial class TVMazeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TVMazeController> _logger;
        private readonly TvMazeContext _dbContext;



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

                // var castInfoList = new List<CastInfo>();

                //for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                for (int day = 1; day <= 1; day++)
                {

                    // Format the request URL for the /schedule endpoint with the specified year, month, and day.
                    var requestUrl = $"schedule?date={year}-{month:D2}-{day:D2}";

                    // Perform the request and receive the response as a string.
                    var response = await PerformRequestWithRateLimitAsync(requestUrl);

                    var scheduledData = JsonSerializer.Deserialize<List<ScheduleData>>(response);

                    foreach (var scheduleData in scheduledData)
                    {
                        var show = scheduleData.Show;
                        var showEntity = new Show
                        {
                            ShowID = show.Id,
                            Day = day,
                            Month = month,
                            Year = year,
                        };
                        _dbContext.Shows.Add(showEntity);

                        var castResponse = await PerformRequestWithRateLimitAsync($"shows/{show.Id}/cast");

                        var castsData = JsonSerializer.Deserialize<List<CastData>>(castResponse);

                        foreach (var castData in castsData)
                        {
                            var member = castData.Member;
                            var memberEntity = new Actor
                            {
                                ActorID = member.Id,
                                ActorName = member.Name,
                                ShowID = show.Id,
                            };

                            _dbContext.Actors.Add(memberEntity);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
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

        private async Task<List<TopActorInfo>> CalculateTopActorsAsync(int year, int month)
        {
            var topActorsList = new List<TopActorInfo>();

            try
            {
                var topActorsQuery = (from s in _dbContext.Shows
                                      join a in _dbContext.Actors on s.ShowID equals a.ShowID
                                      where s.Year == year && s.Month == month
                                      group new { s, a } by new { s.ShowID, a.ActorID, a.ActorName } into grouped
                                      select new TopActorInfo
                                      {
                                          ShowID = grouped.Key.ShowID,
                                          ActorID = grouped.Key.ActorID,
                                          ActorName = grouped.Key.ActorName,
                                          AppearanceCount = grouped.Count(),
                                      }).ToList();

                var totalShows = await _dbContext.Shows.CountAsync(s => s.Year == year && s.Month == month);

                // Bereken het percentage verschijning voor elke acteur en voeg toe aan de lijst
                foreach (var actorInfo in topActorsQuery)
                {
                    double percentage = (double)actorInfo.AppearanceCount / totalShows * 100;
                    actorInfo.Percentage = percentage;
                }

                // Sorteer de topActorsList op basis van het percentage in aflopende volgorde
                topActorsQuery = topActorsQuery.OrderByDescending(a => a.Percentage).Take(10).ToList();

                // Afronden van het percentage voor elke acteur
                foreach (var actorInfo in topActorsQuery)
                {
                    actorInfo.Percentage = Math.Round(actorInfo.Percentage, 2);
                }

                // Retourneer de topActorsQuery
                return topActorsQuery;
            }
            catch (Exception ex)
            {
                // Behandel eventuele fouten die optreden tijdens het uitvoeren van de query
                _logger.LogError($"Error calculating top actors: {ex.Message}");
                throw; // Gooi de uitzondering opnieuw om het probleem door te geven.
            }
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



    }
}