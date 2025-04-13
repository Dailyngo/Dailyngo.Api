using EveryDaily.Core.Entity;
using EveryDaily.Domain.Entities.Rank;
using EveryDaily.Domain.Enums.Rank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Domain.Entities.DailyHistory
{
    public class UserXpHistoryEntity : EntityBase
    {
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public int XpGained { get; set; }
        public XpActivityType Source { get; set; } 
        public UserEntity User { get; set; }
    }
}
