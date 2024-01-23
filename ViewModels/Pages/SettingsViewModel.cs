// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Win32;
using Newtonsoft.Json;
using SRSwitcher.Models;
using System.Diagnostics;
using System.IO;
using Wpf.Ui.Controls;

namespace SRSwitcher.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private Wpf.Ui.Appearance.ThemeType _currentTheme = Wpf.Ui.Appearance.ThemeType.Unknown;

        [ObservableProperty]
        private Setting _settings;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            AppVersion = $"SRSwitcher - {GetAssemblyVersion()}";
            LoadSettingsData();
            _isInitialized = true;
        }

        private void LoadSettingsData()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var settingsData = JsonConvert.DeserializeObject<Setting>(json);
                Settings = settingsData;
            }
            else
            { 
                var settingsData = new Setting();
                settingsData.GamePath = @"C:\Games\Star Rail\Games\StarRail.exe";
                var json = JsonConvert.SerializeObject(settingsData);
                File.WriteAllText(path, json);
                Settings = settingsData;
            }
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeGamePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe";
            openFileDialog.InitialDirectory = @"c:\";
            if (openFileDialog.ShowDialog() == true)
            {
                var settingsData = new Setting();
                string path = openFileDialog.FileName;
                settingsData.GamePath = path;
                Settings = settingsData;
                var json = JsonConvert.SerializeObject(settingsData);
                var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                File.WriteAllText(jsonPath, json);
                Debug.WriteLine($"Selected path: {Settings.GamePath}");
            }
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Light)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Light;

                    break;

                default:
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Dark)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Dark;

                    break;
            }
        }
    }
}
