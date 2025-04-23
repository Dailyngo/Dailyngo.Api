namespace EveryDaily.Application.Dtos.User.Response
{
    public class GetProfileCardResponse
    {
        public GetUserResponse GetUserResponse { get; set; }
        public int? PostCount { get; set; }
        public int Follower { get; set; }
        public int Following { get; set; }
        public string? Bio { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFollowed { get; set; }
        public bool IsSendFollowRequest { get; set; }
        public string? SendFollowRequestId { get; set; }
        public bool IsReceiverFollowRequest { get; set; }
        public string? SendReceiverRequestId { get; set; }
    }

    public class GetUserResponse
    {
        public string? ProfilePicture { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
    }
}