using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PosCore.Models;

namespace PosCore
{
    public class ShortcutManager
    {
        private readonly string _profilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "atajos-industria.json");
        private readonly string _userConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_shortcuts.json");

        public List<ShortcutConfig> CurrentShortcuts { get; private set; } = new List<ShortcutConfig>();

        public ShortcutManager()
        {
            LoadShortcuts();
        }

        public void LoadShortcuts(string industry = null)
        {
            if (industry == null && File.Exists(_userConfigPath))
            {
                var userJson = File.ReadAllText(_userConfigPath);
                try {
                    CurrentShortcuts = JsonSerializer.Deserialize<List<ShortcutConfig>>(userJson);
                    return;
                } catch { }
            }

            if (File.Exists(_profilesPath))
            {
                var json = File.ReadAllText(_profilesPath);
                var profiles = JsonSerializer.Deserialize<List<IndustryProfile>>(json);
                
                var profile = industry != null 
                    ? profiles.FirstOrDefault(p => p.IndustryName == industry) 
                    : profiles.FirstOrDefault(); // default to first

                if (profile != null)
                {
                    CurrentShortcuts = profile.Shortcuts;
                    return;
                }
            }

            CurrentShortcuts = GenerateDefaultFallback();
        }

        public void SaveUserShortcuts(List<ShortcutConfig> shortcuts)
        {
            CurrentShortcuts = shortcuts;
            var json = JsonSerializer.Serialize(shortcuts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_userConfigPath, json);
        }

        private List<ShortcutConfig> GenerateDefaultFallback()
        {
            var list = new List<ShortcutConfig>();
            for (int i = 0; i < 9; i++)
            {
                list.Add(new ShortcutConfig { Name = $"Atajo {i+1}", Icon = "⚡", Action = "None", Color = "#F3F4F6", Description = "" });
            }
            return list;
        }
    }
}
