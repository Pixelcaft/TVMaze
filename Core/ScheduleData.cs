using System.Text.Json.Serialization;

namespace TVMaze.Core
{
    public class ScheduleData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("airdate")]
        public string AirDate { get; set; }

        [JsonPropertyName("show")]
        public ShowData Show { get; set; }
    }
}
