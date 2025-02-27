﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RestApiTestAutomation.Models
{
    public class User : UserDTO
    {
        public override bool Equals(object obj)
        {
            var other = obj as User;
            if (other == null)
                return false;

            if (AreUsersEqual(this, other))
                return true;
            return false;
        }

        private static bool AreUsersEqual(User thisUser, User otherUser)
        {
            return thisUser.Name == otherUser.Name &&
                    thisUser.Age == otherUser.Age &&
                    thisUser.Location == otherUser.Location &&
                    thisUser.Work.Name == otherUser.Work.Name &&
                    thisUser.Work.Location == otherUser.Work.Location &&
                    thisUser.Work.Rating == otherUser.Work.Rating;
        }

        public override string ToString()
        {
            return $"Id: {Id}\t\tName: {Name}\t\tAge: {Age}\t\tLocation: {Location}\t\t{Work}";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
