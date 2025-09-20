using System;
using System.Text.RegularExpressions;

namespace kTVCSSBlazor.Client.Services;

public class PasswordCheckService
{
    public bool ValidatePassword(string password)
    {
        return Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$");
    }
}
