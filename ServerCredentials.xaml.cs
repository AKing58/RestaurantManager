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
    /// Interaction logic for ServerCredentials.xaml
    /// </summary>
    public partial class ServerCredentials : Window
    {


        
        public connection thisConnection { get; set; }

  


        public struct connection
        {
            string type { get; set; }
            string dataSource { get; set; }
            string initialCatalog { get; set; }
            string integratedSecurity { get; set; }

            public string connectionString { get; set; }

            public connection(string newdataSource)
            {
                type = "SQLExpress";
                dataSource = newdataSource;
                initialCatalog = "COMP4952PROJECT";
                integratedSecurity = "True";
                connectionString = "Server=.\\" + type + ";Data Source=" + dataSource + ";Initial Catalog=" + initialCatalog + ";Integrated Security=" + integratedSecurity + ";";
            }

        }


        MainWindow thisWindow;

        public ServerCredentials(MainWindow main)
        {
            InitializeComponent();
            thisWindow = main;
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            thisConnection = new connection(serverTextBox.Text);

            //save the connection to settings. 

            var connection = thisConnection.connectionString;
            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            COMP4952PROJECTContext db = new COMP4952PROJECTContext(builder.Options);

            if (db.Database.CanConnect())
            {

                SettingsFile.Default.ConnectionString = thisConnection.connectionString;
                SettingsFile.Default.Save();

                if (!db.Wall.Any()) { 
                    thisWindow.frame.Source = new Uri("FloorBuilder.xaml", UriKind.Relative);
                }
                else
                {
                    thisWindow.frame.Source = new Uri("Home.xaml", UriKind.Relative);
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Unable to connect to server, try again.", "Alert");
            }
 
        






        }
    }
}
