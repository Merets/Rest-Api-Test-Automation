namespace RestApiTestAutomation.Models
{
    public class Work
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public double Rating { get; set; }

        public override string ToString()
        {
            return $"WorkName: {Name}";
        }

    }
}