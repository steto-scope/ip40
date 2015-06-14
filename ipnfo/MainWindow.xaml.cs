﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ipnfo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            
            InitializeComponent();
            DataContextChanged += MainWindow_DataContextChanged;
           
        }

        void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
                ((MainViewModel)DataContext).PropertyChanged += mvm_PropertyChanged;
        }

        void mvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IPRangeStart" || e.PropertyName == "IPRangeEnd")
                mvm_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("StartStopButtonText"));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((MainViewModel)DataContext).FillRange();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            About a = new About();
            a.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            a.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DataContext!=null)
            ((MainViewModel)DataContext).Config.Save();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border b = (Border)sender;
            ((MainViewModel)DataContext).CallService((HostInformation)b.Tag, (PortInformation)b.DataContext);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
             Process.Start("explorer.exe", "::{7007ACC7-3202-11D1-AAD2-00805FC1270E}");            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string mmc = Environment.GetFolderPath(Environment.SpecialFolder.Windows)+"\\System32\\"+Thread.CurrentThread.CurrentCulture.ToString()+"\\WF.msc";
            Process.Start(mmc);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Process.Start("control.exe", "/name Microsoft.NetworkAndSharingCenter");            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataContext!=null)
            ((MainViewModel)DataContext).FireAllPropertiesChanged();
        }






    }
}