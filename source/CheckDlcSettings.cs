﻿using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CheckDlc
{
    public class CheckDlcSettings : ObservableObject
    {
        #region Settings variables
        public bool MenuInExtensions { get; set; } = true;
        public DateTime LastAutoLibUpdateAssetsDownload { get; set; } = DateTime.Now;

        [DontSerialize]
        public bool SteamIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.SteamLibrary));
        [DontSerialize]
        public bool EpicIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.EpicLibrary)) || PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.LegendaryLibrary));
        [DontSerialize]
        public bool GogIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.GogLibrary));
        [DontSerialize]
        public bool OriginIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.OriginLibrary));

        public bool EnableTag { get; set; } = false;
        public bool AutoImport { get; set; } = true;

        public bool PriceNotification { get; set; } = false;

        public StoreCurrency GogCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };
        public StoreCurrency OriginCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };

        public ObservableCollection<string> IgnoredList { get; set; } = new ObservableCollection<string>();

        public GameFeature DlcFeature { get; set; } = null;

        private bool enableIntegrationButton = true;
        public bool EnableIntegrationButton { get => enableIntegrationButton; set => SetValue(ref enableIntegrationButton, value); }

        private bool enableIntegrationListDlcAll = true;
        public bool EnableIntegrationListDlcAll { get => enableIntegrationListDlcAll; set => SetValue(ref enableIntegrationListDlcAll, value); }

        private bool enableIntegrationListDlcOwned = true;
        public bool EnableIntegrationListDlcOwned { get => enableIntegrationListDlcOwned; set => SetValue(ref enableIntegrationListDlcOwned, value); }

        private bool enableIntegrationListDlcNotOwned = true;
        public bool EnableIntegrationListDlcNotOwned { get => enableIntegrationListDlcNotOwned; set => SetValue(ref enableIntegrationListDlcNotOwned, value); }

        private bool enableIntegrationButtonDetails = false;
        public bool EnableIntegrationButtonDetails { get => enableIntegrationButtonDetails; set => SetValue(ref enableIntegrationButtonDetails, value); }

        public SteamSettings SteamApiSettings { get; set; } = new SteamSettings();
        [DontSerialize]
        public EpicSettings EpicSettings { get; set; } = new EpicSettings();
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed
        private bool hasData = false;
        [DontSerialize]
        public bool HasData { get => hasData; set => SetValue(ref hasData, value); }

        private List<Dlc> listDlcs = new List<Dlc>();
        [DontSerialize]
        public List<Dlc> ListDlcs { get => listDlcs; set => SetValue(ref listDlcs, value); }
        #endregion  
    }

    public class CheckDlcSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CheckDlc Plugin;
        private CheckDlcSettings EditingClone { get; set; }

        private CheckDlcSettings settings;
        public CheckDlcSettings Settings { get => settings; set => SetValue(ref settings, value); }

        public CheckDlcSettingsViewModel(CheckDlc plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            CheckDlcSettings savedSettings = plugin.LoadPluginSettings<CheckDlcSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            Settings = savedSettings ?? new CheckDlcSettings();
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            // StoreAPI intialization
            if (settings.SteamIsEnabled)
            {
                CheckDlc.SteamApi.SaveCurrentUser();
                CheckDlc.SteamApi.CurrentAccountInfos = null;
                _ = CheckDlc.SteamApi.CurrentAccountInfos;
                if (Settings.SteamApiSettings.UseAuth)
                {
                    CheckDlc.SteamApi.CurrentAccountInfos.IsPrivate = true;
                }
            }

            if (settings.EpicIsEnabled)
            {
                CheckDlc.EpicApi.SaveCurrentUser();
                CheckDlc.EpicApi.CurrentAccountInfos = null;
                _ = CheckDlc.EpicApi.CurrentAccountInfos;
                if (Settings.EpicSettings.UseAuth)
                {
                    CheckDlc.EpicApi.CurrentAccountInfos.IsPrivate = true;
                }
            }

            Plugin.SavePluginSettings(Settings);
            CheckDlc.PluginDatabase.PluginSettings = this;
            OnPropertyChanged();
        }

        // Code execute when user decides to confirm changes made since BeginEdit was called.
        // Executed before EndEdit is called and EndEdit is not called if false is returned.
        // List of errors is presented to user if verification fails.
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }

    public class SteamSettings
    {
        public bool UseApi { get; set; } = false;
        public bool UseAuth { get; set; } = false;
    }

    public class EpicSettings
    {
        public bool UseAuth { get; set; } = true;
    }
}
