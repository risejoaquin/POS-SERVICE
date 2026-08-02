using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PosCore.Models;

namespace PosCore.Views
{
    public partial class SettingsWindow : Window
    {
        private ShortcutManager _manager;
        public List<ShortcutConfig> Shortcuts { get; set; }
        
        public SettingsWindow(ShortcutManager manager)
        {
            InitializeComponent();
            _manager = manager;
            Shortcuts = new List<ShortcutConfig>();
            
            // Try loading default on first open based on selection or manager
            LoadToGrid(_manager.CurrentShortcuts);
        }

        private void LoadToGrid(List<ShortcutConfig> list)
        {
            Shortcuts.Clear();
            foreach(var s in list) 
            {
                Shortcuts.Add(new ShortcutConfig { 
                    Name = s.Name, 
                    Description = s.Description, 
                    Action = s.Action, 
                    Color = s.Color, 
                    Icon = s.Icon 
                });
            }
            GridShortcuts.ItemsSource = null;
            GridShortcuts.ItemsSource = Shortcuts;
        }

        private void CmbIndustry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbIndustry.SelectedItem is ComboBoxItem item)
            {
                string industry = item.Content.ToString();
                var tempManager = new ShortcutManager();
                // delete the user config locally so we can load the raw profile
                var userConfigPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "user_shortcuts.json");
                
                tempManager.LoadShortcuts(industry);
                LoadToGrid(tempManager.CurrentShortcuts);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _manager.SaveUserShortcuts(Shortcuts);
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
