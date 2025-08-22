using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services
{
    public class AccountService
    {
        private const string FileName = "accounts.json";

        public List<Account> LoadAccounts()
        {
            if (!File.Exists(FileName))
            {
                File.WriteAllText(FileName, "[]");
                return new List<Account>();
            }

            var json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
        }

        public void SaveAccounts(List<Account> accounts)
        {
            var json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
