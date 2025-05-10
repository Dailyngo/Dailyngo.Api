namespace EveryDaily.Domain.Prefix.ErrorMessage;

public static class PostErrorMessage
{
    public const string PostLimitExceeded = "Günlük gönderi limiti aşıldı. Lütfen yarın tekrar deneyin.";
    public const string PostNotFound = "Gönderi bulunamadı.";
    public const string NotPostOwner = "Bu gönderinin sahibi değilsiniz.";
    public const string ContentOrImageRequired = "Gönderi içeriği veya resim yüklenmelidir.";
}