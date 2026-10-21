using Microsoft.AspNetCore.Identity;

namespace SuBilet.Data;

// Identity'nin İngilizce hata mesajlarının Türkçesi (kayıt, şifre, rol işlemleri)
public class TurkceIdentityHatalari : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Bilinmeyen bir hata oluştu." };

    public override IdentityError ConcurrencyFailure() =>
        new() { Code = nameof(ConcurrencyFailure), Description = "Kayıt başka biri tarafından değiştirildi, sayfayı yenileyip tekrar deneyin." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "Şifre hatalı." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "Geçersiz bağlantı ya da kod." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = $"'{userName}' geçerli bir kullanıcı adı değil." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = $"'{email}' geçerli bir e-posta adresi değil." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"'{userName}' adresiyle kayıtlı bir hesap zaten var." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"'{email}' adresiyle kayıtlı bir hesap zaten var." };

    public override IdentityError InvalidRoleName(string? role) =>
        new() { Code = nameof(InvalidRoleName), Description = $"'{role}' geçerli bir rol adı değil." };

    public override IdentityError DuplicateRoleName(string role) =>
        new() { Code = nameof(DuplicateRoleName), Description = $"'{role}' rolü zaten var." };

    public override IdentityError UserAlreadyInRole(string role) =>
        new() { Code = nameof(UserAlreadyInRole), Description = $"Kullanıcı zaten '{role}' rolünde." };

    public override IdentityError UserNotInRole(string role) =>
        new() { Code = nameof(UserNotInRole), Description = $"Kullanıcı '{role}' rolünde değil." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter olmalıdır." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifre en az bir sembol içermelidir." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "Şifre en az bir rakam içermelidir." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "Şifre en az bir küçük harf içermelidir." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "Şifre en az bir büyük harf içermelidir." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Şifre en az {uniqueChars} farklı karakter içermelidir." };

    public override IdentityError UserLockoutNotEnabled() =>
        new() { Code = nameof(UserLockoutNotEnabled), Description = "Bu kullanıcı için hesap kilitleme açık değil." };

    public override IdentityError UserAlreadyHasPassword() =>
        new() { Code = nameof(UserAlreadyHasPassword), Description = "Kullanıcının zaten bir şifresi var." };
}
