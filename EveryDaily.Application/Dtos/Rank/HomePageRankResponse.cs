using EveryDaily.Domain.Enums.Rank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Application.Dtos.Rank
{
    public class HomePageRankResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public RankEnum Rank { get; set; }
        public int Season { get; set; }
    }
}
