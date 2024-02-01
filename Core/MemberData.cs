using System.Text.Json.Serialization;

namespace TVMaze.Core
{
    public class MemberData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}