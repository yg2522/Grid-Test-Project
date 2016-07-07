using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void OnAppStartup(object sender, StartupEventArgs e)
        {
            
            WpfApplication1.MainWindowViewModel mwvm = new MainWindowViewModel();
            WpfApplication1.MainWindow mw = new WpfApplication1.MainWindow()
            {
                DataContext = mwvm,
            };


            mw.Show();

            Application.Current.MainWindow = mw;
        }
    }
}
