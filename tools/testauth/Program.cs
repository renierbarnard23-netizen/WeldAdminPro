// tools/testauth/Program.cs
using System;
using WeldAdminPro.Core.Security;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0 || args[0].Equals("interactive", StringComparison.OrdinalIgnoreCase))
            return Interactive();

        var cmd = args[0].ToLowerInvariant();
        if (cmd == "hash")
        {
            var pw = args.Length > 1 ? args[1] : ReadPasswordPrompt("Enter password to hash: ");
            Console.WriteLine(AuthService.HashPassword(pw));
            return 0;
        }
        else if (cmd == "verify")
        {
            if (args.Length < 3) { Console.WriteLine("Usage: verify <stored-hash> <candidate-password>"); return 2; }
            var stored = args[1];
            var candidate = args[2];
            var ok = AuthService.VerifyPassword(stored, candidate);
            Console.WriteLine($"VerifyPassword -> {ok}");
            return ok ? 0 : 1;
        }
        else
        {
            Console.WriteLine("Unknown command. Use 'hash', 'verify' or 'interactive'.");
            return 2;
        }
    }

    static int Interactive()
    {
        Console.WriteLine("AuthService test - interactive\n");
        var pw = ReadPasswordPrompt("Enter a password to hash: ");
        var stored = AuthService.HashPassword(pw);
        Console.WriteLine("\nStored hash:\n" + stored + "\n");

        var candidate = ReadPasswordPrompt("Enter candidate password to verify: ");
        var ok = AuthService.VerifyPassword(stored, candidate);
        Console.WriteLine($"\nVerify result: {ok}");
        return ok ? 0 : 1;
    }

    static string ReadPasswordPrompt(string prompt)
    {
        Console.Write(prompt);
        var pw = string.Empty;
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && pw.Length > 0) { pw = pw[0..^1]; Console.Write("\b \b"); }
            else if (!char.IsControl(key.KeyChar)) { pw += key.KeyChar; Console.Write('*'); }
        }
        Console.WriteLine();
        return pw;
    }
}
