namespace TVMaze.Controllers
{
    public partial class TVMazeController
    {
        public class TopActorInfo
        {
            public int ShowID { get; set; }
            public int ActorID { get; set; }
            public string ActorName { get; set; }
            public int AppearanceCount { get; set; }
            public double Percentage { get; set; }
        }



    }
}