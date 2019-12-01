using COMP4952.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ServerCredentials newCredentialRequest = new ServerCredentials(this);

            if (SettingsFile.Default.ConnectionString == null || SettingsFile.Default.ConnectionString == "")
            {
                //display server credentials
                
                newCredentialRequest.Show();
            }
            else
            {


                var connection = SettingsFile.Default.ConnectionString;
                DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
                builder.UseSqlServer(connection);

                COMP4952PROJECTContext db = new COMP4952PROJECTContext(builder.Options);

                if (db.Database.CanConnect())
                {

                    if (!db.Wall.Any())
                    {
                        frame.Source = new Uri("FloorBuilder.xaml", UriKind.Relative);
                    }
                    else
                    {
                        frame.Source = new Uri("Home.xaml", UriKind.Relative);
                    }
                }
                else
                {
                    newCredentialRequest.Show();
                }



            }





            
        }
    }
}
