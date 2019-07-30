namespace RestApiTestAutomation.Models
{
    public class Work : WorkDTO
    {
        public override string ToString()
        {
            return $"WorkName: {Name}";
        }

    }
}