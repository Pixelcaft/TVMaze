namespace TVMaze.Repository
{
    public class Actor
    {
        public int ID { get; set; }
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public int ShowID { get; set; }

        // Navigatie-eigenschap voor het Show-model
        public Show Show { get; set; }
    }
}
