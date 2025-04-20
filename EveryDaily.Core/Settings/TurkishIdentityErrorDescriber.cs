using Microsoft.AspNetCore.Identity;

public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter olmalıdır." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifre en az bir özel karakter içermelidir." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Şifre en az bir rakam içermelidir." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Şifre en az bir küçük harf içermelidir." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Şifre en az bir büyük harf içermelidir." };

    // Gerekirse diğer hata mesajlarını da override edebilirsin
}
