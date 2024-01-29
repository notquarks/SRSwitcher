// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SRSwitcher.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace SRSwitcher.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        public DashboardViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
        }

        private readonly IContentDialogService _contentDialogService;

        private bool _isInitialized = false;

        [ObservableProperty]
        private ObservableCollection<string> _avatarUrls;

        [ObservableProperty]
        private string _selectedAvatarUrl;

        [ObservableProperty]
        private string _dialogResultText = string.Empty;

        [ObservableProperty]
        private int _counter = 0;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts;

        [ObservableProperty]
        private Setting _settings;

        [ObservableProperty]
        private string _usernameInput;
        [ObservableProperty]
        private string _uidInput;
        [ObservableProperty]
        private int _levelInput;

        public string Username
        {
            get { return _usernameInput; }
            set { SetProperty(ref _usernameInput, value); }
        }

        public string UID
        {
            get { return _uidInput; }
            set { SetProperty(ref _uidInput, value); }
        }

        public int Level
        {
            get { return _levelInput; }
            set { SetProperty(ref _levelInput, value); }
        }

        public void OnNavigatedFrom() { }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            var accountList = new List<Account>();
            LoadData();
            LoadSettingsData();
            _isInitialized = true;
        }

        [RelayCommand]
        private void LoadData()
        {
            var accPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "accounts.json");
            if(File.Exists(accPath))
            { 
                var accJson = File.ReadAllText(accPath);
                var accountList = JsonConvert.DeserializeObject<ObservableCollection<Account>>(accJson);
                Accounts = accountList;
            }
            else
            {
                var accountList = new ObservableCollection<Account>();
                var accJson = JsonConvert.SerializeObject(accountList, Formatting.Indented);
                File.WriteAllText(accPath, accJson);
                Accounts = accountList;
            }
        }

        [RelayCommand]
        private void LoadSettingsData()
        {
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
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

        [RelayCommand]
        private void PlayAccount(int Id)
        {
            // Apply token to registry
            string keyAndIV = GetEncryptionKeyAndIV();
            string token = Accounts.Where(x => x.Id == Id).FirstOrDefault().Token;
            string decryptedToken = DecryptToken(token, keyAndIV);
            //string decryptedToken = token;
            bool isRunning = Process.GetProcessesByName("StarRail").Length > 0;
            if(isRunning)
            {
                Process[] processes = Process.GetProcessesByName("StarRail");
                foreach (Process process in processes)
                {
                    process.Kill();
                }
            }
            ApplyTokenToReg(decryptedToken);

            var startInfo = new ProcessStartInfo(Settings.GamePath)
            {
                Verb = "runas"
            };
            Process.Start(startInfo);

            Debug.WriteLine($"Playing {Accounts.Where(x => x.Id == Id).FirstOrDefault().Username} Account");
        }

        [RelayCommand]
        private void AddAccount()
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "accounts.json");
            string keyAndIV = GetEncryptionKeyAndIV();
            string token = EncryptToken(GetRegistryValue(), keyAndIV);
            //string token = GetRegistryValue();
            int newId = GetNextAccountId();
            Debug.WriteLine($"Adding {Username} Account");
            Accounts.Add(new Account
            {
                Id = newId,
                Username = Username,
                UID = UID,
                Level = Level,
                Img = "https://upload-os-bbs.hoyolab.com/upload/2023/11/22/90ed0534fea5a132b90e798bd455b51c_2408891332889206101.png",
                //Username = "NewUser",
                //UID = Guid.NewGuid().ToString(),
                //Level = 1,
                Server = "Asia",
                Token = token,
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            });
            Username = string.Empty;
            UID = string.Empty;
            Level = 0;
            var newJson = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(path, newJson);
        }

        [RelayCommand]
        private void EditAccount(int Id)
        {
            //edit account item based on selected index
            //var account = Accounts[SelectedIndex];
            Debug.WriteLine($"Selected Account: {Accounts.Where(x => x.Id == Id).FirstOrDefault().Username}");

        }

        [RelayCommand]
        private void DeleteAccount(int Id)
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "accounts.json");
            Debug.WriteLine($"Deleted {Accounts.Where(x => x.Id == Id).FirstOrDefault().Username} Account");

            var index = Accounts.Where(x => x.Id == Id).FirstOrDefault();
            Accounts.Remove(index);

            var newJson = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(path, newJson);
        }

        [RelayCommand]
        private async Task OnShowDialog(object content)
        {
            var result = await _contentDialogService.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions()
                {
                    Title = "Save an Account",
                    Content = content,
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                }
            );

            switch (result) 
            {
                case ContentDialogResult.Primary:
                    AddAccount();
                    DialogResultText = "User saved their work";
                    break;
                default:
                    DialogResultText = "User cancelled the dialog";
                    break;
            }
            //DialogResultText = result switch
            //{
            //    ContentDialogResult.Primary => AddAccount(),
            //    ContentDialogResult.Secondary => "User did not save their work",
            //    _ => "User cancelled the dialog"
            //};
        }

        private int GetNextAccountId()
        {
            if (Accounts.Count == 0)
            {
                // If the collection is empty, start with Id = 1
                return 1;
            }
            else
            {
                // Otherwise, find the maximum Id and increment it
                int maxId = Accounts.Max(account => account.Id);
                return maxId + 1;
            }
        }

        private string GetRegistryValue()
        {
            string registryKeyPath = @"Software\Cognosphere\Star Rail";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
                {
                    if (key != null)
                    {
                        // Retrieve all value names under the specified registry key
                        string[] valueNames = key.GetValueNames();

                        // Look for a value name that matches the expected pattern
                        string expectedPattern = "MIHOYOSDK_ADL_PROD_OVERSEA_";
                        string matchingValueName = valueNames.FirstOrDefault(name => name.StartsWith(expectedPattern));

                        if (!string.IsNullOrEmpty(matchingValueName))
                        {
                            byte[] registryBytes = key.GetValue(matchingValueName) as byte[];
                            if (registryBytes != null)
                            {
                                string registryString = Encoding.UTF8.GetString(registryBytes);
                                registryString = registryString.TrimEnd('\0');

                                Debug.WriteLine($"Get Value: {registryString}");
                                return registryString;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing registry: {ex.Message}");
            }
            return string.Empty;
        }

        private void ApplyTokenToReg (string decryptedToken)
        {
            string registryKeyPath = @"Software\Cognosphere\Star Rail";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath, true))
                {
                    if (key != null)
                    {
                        // Retrieve all value names under the specified registry key
                        string[] valueNames = key.GetValueNames();

                        // Look for a value name that matches the expected pattern
                        string expectedPattern = "MIHOYOSDK_ADL_PROD_OVERSEA_";
                        string matchingValueName = valueNames.FirstOrDefault(name => name.StartsWith(expectedPattern));

                        if (!string.IsNullOrEmpty(matchingValueName))
                        {
                            byte[] registryBytes = Encoding.UTF8.GetBytes(decryptedToken);
                            key.SetValue(matchingValueName, registryBytes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing registry: {ex.Message}");
            }
        }

        private string EncryptToken(string token, string keyAndIV)
        {
            string[] parts = keyAndIV.Split(';');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid key and IV format");
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(parts[0]);
                aesAlg.IV = Convert.FromBase64String(parts[1]);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(token);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private string DecryptToken(string encryptedToken, string keyAndIV)
        {
            string[] parts = keyAndIV.Split(';');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid key and IV format");
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(parts[0]);
                aesAlg.IV = Convert.FromBase64String(parts[1]);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedToken)))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        private string GetEncryptionKeyAndIV()
        {
            string keyAndIVFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "keys.json");
            return File.Exists(keyAndIVFilePath) ? File.ReadAllText(keyAndIVFilePath) : GenerateAndSaveKeyAndIV(keyAndIVFilePath);
        }

        private string GenerateAndSaveKeyAndIV(string keyAndIVFilePath)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();
                string keyAndIV = Convert.ToBase64String(aesAlg.Key)+ ";" + Convert.ToBase64String(aesAlg.IV);
                File.WriteAllText(keyAndIVFilePath, keyAndIV);
                return keyAndIV;
            }
        }

    }
}
