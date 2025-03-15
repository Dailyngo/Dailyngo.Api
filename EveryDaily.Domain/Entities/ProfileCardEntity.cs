using EveryDaily.Core.Entity;
using Microsoft.AspNetCore.Identity;

namespace EveryDaily.Domain.Entities;

public class ProfileCardEntity : EntityBase
{

    public Guid UserId { get; set; }

    public int Follower { get; set; }
    public int FollowUp { get; set; }
    public int PostCount { get; set; }

    public UserEntity User { get; set; }


}
