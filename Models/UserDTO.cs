namespace RestApiTestAutomation.Models
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Location { get; set; }
        public WorkDTO Work { get; set; }

    }

}