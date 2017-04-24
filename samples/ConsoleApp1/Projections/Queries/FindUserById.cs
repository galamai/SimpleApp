using SimpleApp.Querying;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Projections.Queries
{
    public class FindUserById : ICriterion<User>
    {
        public Guid UserId { get; }
        
        public FindUserById(Guid userId)
        {
            UserId = userId;
        }
    }
}
