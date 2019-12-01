using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using COMP4952.Models;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            if(SettingsFile.Default.ConnectionString == null)
            {
                //display server credentials
                ServerCredentials newCredentialRequest = new ServerCredentials();
                newCredentialRequest.Show();
            }
            else
            {
                //display the main window.
                MainWindow mainScreen = new MainWindow();
                mainScreen.Show();
            }
        }

    }
}
