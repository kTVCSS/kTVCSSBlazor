using System;
using Microsoft.JSInterop;

namespace kTVCSSBlazor.Client.Services;

public class CryptoService
{
    private readonly IJSRuntime _jsRuntime;
    private const string SecretKey = "vJWXaXaVUSpj6bycLYblJot3EjwM2bjE";

    public CryptoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> EncryptAsync(string plaintext)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("cryptoHelper.encrypt", plaintext, SecretKey);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Encryption failed: {ex.Message}", ex);
        }
    }

    public async Task<string> DecryptAsync(string ciphertext)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("cryptoHelper.decrypt", ciphertext, SecretKey);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Decryption failed: {ex.Message}", ex);
        }
    }
}