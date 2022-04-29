﻿using CheckDlc.Services;
using CommonPluginsShared;
using Playnite.SDK;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CheckDlc.Clients;
using CheckDlc.Models;
using System.Collections.Generic;
using CommonPluginsStores.Gog;
using CommonPluginsStores.Gog.Models;
using CommonPluginsStores.Models;
using CommonPluginsStores.Origin;

namespace CheckDlc.Views
{
    public partial class CheckDlcSettingsView : UserControl
    {
        private static IResourceProvider resources = new ResourceProvider();
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;


        public CheckDlcSettingsView()
        {
            InitializeComponent();

            // List features
            PART_FeatureDlc.ItemsSource = PluginDatabase.PlayniteApi.Database.Features.OrderBy(x => x.Name);

            // List GOG currencies
            GogApi gogApi = new GogApi();
            List<StoreCurrency> dataGog = gogApi.GetCurrencies();            
            PART_GogCurrency.ItemsSource = dataGog.OrderBy(x => x.currency).ToList();

            try
            {
                int idx = ((List<StoreCurrency>)PART_GogCurrency.ItemsSource).FindIndex(x => x.currency == PluginDatabase.PluginSettings.Settings.GogCurrency.currency);
                PART_GogCurrency.SelectedIndex = idx;
            }
            catch { }

            // List Origin currencies
            OriginApi originApi = new OriginApi();
            List<StoreCurrency> dataOrigin = originApi.GetCurrencies();            
            PART_OriginCurrency.ItemsSource = dataOrigin.OrderBy(x => x.currency).ToList();

            try
            {
                int idx = ((List<StoreCurrency>)PART_OriginCurrency.ItemsSource).FindIndex(x => x.country == PluginDatabase.PluginSettings.Settings.OriginCurrency.country);
                PART_OriginCurrency.SelectedIndex = idx;
            }
            catch { }
        }


        #region Tag
        private void ButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.AddTagAllGame();
        }

        private void ButtonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.RemoveTagAllGame();
        }
        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start((string)((FrameworkElement)sender).Tag);
        }


        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = int.Parse(((FrameworkElement)sender).Tag.ToString());
                ((ObservableCollection<string>)PART_IgnoredList.ItemsSource).RemoveAt(index);
                PART_IgnoredList.Items.Refresh();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            var item = PluginDatabase.PlayniteApi.Dialogs.SelectString(resources.GetString("LOCCommonInputItemIgnore"), resources.GetString("LOCCheckDlc"), string.Empty);
            if (!item.SelectedString.IsNullOrEmpty())
            {
                ((ObservableCollection<string>)PART_IgnoredList.ItemsSource).Add(item.SelectedString);
            }
        }
    }
}