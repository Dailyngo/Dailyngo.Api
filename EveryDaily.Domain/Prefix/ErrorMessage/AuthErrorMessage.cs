namespace EveryDaily.Domain.Prefix.ErrorMessage;

public static class AuthErrorMessage
{
    public const string InvalidEmailAddress = "Sadece edu.tr uzantılı e-posta adresleri kabul edilmektedir.";
    public const string UserNameAlreadyExists = "Kullanıcı adı zaten kullanımda.";
    public const string InvalidEmailFormat = "Geçersiz e-posta formatı.";
    public const string UserNotFound = "Kullanıcı bulunamadı.";
    public const string InvalidPassword = "Hatalı şifre girişi.";
    public const string PasswordsDoNotMatch = "Şifreler uyuşmuyor.";
    public const string EmailVerificationCodeNotMatch = "E-posta doğrulama kodu eşleşmedi.";
    public const string EmailVerificationCodeNotFound = "E-posta doğrulama kodu bulunamadı. Lütfen tekrar deneyin.";
    
}