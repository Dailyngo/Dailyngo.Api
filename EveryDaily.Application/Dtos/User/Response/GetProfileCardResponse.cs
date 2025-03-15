namespace EveryDaily.Application.Dtos.User.Response
{
    public class GetProfileCardResponse
    {

        public GetUserResponse GetUserResponse { get; set; }

        public int Follower { get; set; }
        public int FollowUp { get; set; }
        public int PostCount { get; set; }
    }

    public class GetUserResponse
    {
        public string FullName { get; set; }
    }
}
