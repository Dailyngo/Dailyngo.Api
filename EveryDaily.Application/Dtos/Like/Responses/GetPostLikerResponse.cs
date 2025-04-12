namespace EveryDaily.Application.Dtos.Like.Responses;

public class GetPostLikerResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsFollowed { get; set; }
}