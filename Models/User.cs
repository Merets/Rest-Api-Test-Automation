using System;
using System.Collections.Generic;
using System.Text;

namespace RestApiTestAutomation.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Location { get; set; }
        public Work Work { get; set; }
        public override bool Equals(object obj)
        {
            var other = obj as User;
            if (other == null)
                return false;

            if (this.Name != other.Name ||
                this.Age != other.Age ||
                this.Location != other.Location ||
                this.Work.Name != other.Work.Name ||
                this.Work.Location != other.Work.Location ||
                this.Work.Rating != other.Work.Rating
                )
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"Id: {Id}\t\tName: {Name}\t\tAge: {Age}\t\tLocation: {Location}\t\t{Work}";
        }
    }
}
