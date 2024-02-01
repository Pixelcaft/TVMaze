using System.Text.Json.Serialization;

namespace TVMaze.Core
{
    public class CastData
    {
        [JsonPropertyName("person")]
        public MemberData Member { get; set; }
    }
}