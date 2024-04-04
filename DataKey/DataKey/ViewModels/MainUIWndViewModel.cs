using DataKey.Security;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace DataKey.ViewModels
{
    internal class MainUIWndViewModel : ViewModelBase
    {
        private const string PUBLIC_KEY = "publickey";
        private const string PRIVATE_KEY_PATH = "privatekeypath";

        public MainUIWndViewModel()
        {
            BrowsePrivateKeyCmd = new RelayCommand(() => BrowsePrivateKey());

            EncryptDataCmd = new RelayCommand(() => EncryptData());

            DecryptDataCmd = new RelayCommand(() => DecryptData());

            LoadPublicKey();
        }

        private string? publicKey = string.Empty;
        public string? PublicKey
        {
            get { return publicKey; }
            set { publicKey = value; RaisePropertyChanged(); }
        }

        private string? privateKeyPath = string.Empty;
        public string? PrivateKeyPath
        {
            get { return privateKeyPath; }
            set { privateKeyPath = value; RaisePropertyChanged(); }
        }

        private string? sourceData = string.Empty;
        public string? SourceData
        {
            get { return sourceData; }
            set { sourceData = value; RaisePropertyChanged(); }
        }

        private string? targetData = string.Empty;
        public string? TargetData
        {
            get { return targetData; }
            set { targetData = value; RaisePropertyChanged(); }
        }

        public RelayCommand BrowsePrivateKeyCmd { get; }

        public RelayCommand EncryptDataCmd { get; }

        public RelayCommand DecryptDataCmd { get; }

        public void BrowsePrivateKey()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "PEM files ( *. pem) | *. pem|All files ( *.* ) | *.* ";
            openFileDialog.InitialDirectory = "C:\\";
            if (openFileDialog.ShowDialog() == true)
            {
                PrivateKeyPath = openFileDialog.FileName;
            }
        }

        public void EncryptData()
        {
            SavePublicKey();

            if (string.IsNullOrEmpty(PublicKey) || string.IsNullOrEmpty(SourceData))
                return;

            TargetData = RsaCrypt.RSAEncrypt(SourceData, PublicKey.Trim());
        }

        public void DecryptData()
        {
            SavePublicKey();

            if (string.IsNullOrEmpty(PrivateKeyPath) || string.IsNullOrEmpty(SourceData))
                return;

            if (!Path.Exists(PrivateKeyPath))
            {
                MessageBox.Show($"{PrivateKeyPath} doesn't exist!");
                return;
            }

            string privateKey = string.Empty;
            try
            {
                privateKey = File.ReadAllText(PrivateKeyPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read {PrivateKeyPath}, error info: {ex.Message} .");
                return;
            }

            TargetData = RsaCrypt.RSADecrypt(SourceData, privateKey.Trim());
        }

        private void LoadPublicKey()
        {
            publicKey = ConfigurationManager.AppSettings[PUBLIC_KEY];
            privateKeyPath = ConfigurationManager.AppSettings[PRIVATE_KEY_PATH];

        }
        private void SavePublicKey()
        {
            if (string.IsNullOrEmpty(publicKey) && string.IsNullOrEmpty(privateKeyPath))
                return;

            var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = cfg.AppSettings.Settings;

            if (settings[PUBLIC_KEY] == null)
                settings.Add(PUBLIC_KEY, publicKey);
            else
                settings[PUBLIC_KEY].Value = publicKey;

            if (settings[PRIVATE_KEY_PATH] == null)
                settings.Add(PRIVATE_KEY_PATH, PrivateKeyPath);
            else
                settings[PRIVATE_KEY_PATH].Value = privateKeyPath;

            cfg.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
