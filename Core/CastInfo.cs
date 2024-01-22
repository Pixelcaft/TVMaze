namespace TVMaze.Core
{
    public class CastInfo
    {
        public CastInfo()
        {
            Cast = new List<CastMember>();
        }
        public int ShowId { get; set; }
        public string ShowName { get; set; }
        public string ShowAirdate { get; set; }
        public List<CastMember> Cast { get; set; }
    }
}