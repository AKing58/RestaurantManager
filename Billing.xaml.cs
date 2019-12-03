using COMP4952.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for Billing.xaml
    /// Justin Kwok
    /// </summary>
    public partial class Billing : Window
    {
        COMP4952PROJECTContext db;
        TableInfo ti;
        List<decimal> listCosts;

        /// <summary>
        /// Default constructor, unused
        /// </summary>
        public Billing() {}

        /// <summary>
        /// Billing constructor
        /// </summary>
        /// <param name="m"></param>
        public Billing(int m, List<decimal> costs)
        {
            InitializeComponent();
            initializeDBConnection();

            listCosts = costs;
            ti = db.TableInfo.Find(m);
            tableLabel.Content = "Table ID: " + ti.Id;
            loadCustomersInfo();
        }

        /// <summary>
        /// Connects page to database
        /// </summary>
        private void initializeDBConnection()
        {

            var connection = SettingsFile.Default.ConnectionString;

            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            db = new COMP4952PROJECTContext(builder.Options);

        }

        /// <summary>
        /// Loads the customer information from OrderScreen and sets prices of current orders
        /// </summary>
        private void loadCustomersInfo()
        {
            decimal totalCost = 0;
            for (int i = 0; i < listCosts.Count; i++)
            {
                var customer = (Label)this.FindName("costLabel" + (i + 1));
                customer.Content = "$" + listCosts[i];
                totalCost += listCosts[i];
                
            }
            
            var total = (Label)this.FindName("totalLabel");
            total.Content = "Total Cost: $" + totalCost;
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backBtn_onClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Changes billed buttons colour if clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void billingBtn_onClick(object sender, RoutedEventArgs e)
        {
            var billingBtn = (Button)sender;
            if (billingBtn.Background == Brushes.Green)
            {
                billingBtn.ClearValue(Button.BackgroundProperty);
            } else
            {
                billingBtn.Background = Brushes.Green;
            }
        }

        /// <summary>
        /// Changes all billed buttons colour if they are different from billAll button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void billAllBtn_onClick(object sender, RoutedEventArgs e)
        {
            var billAllBtn = (Button)sender;
            if (billAllBtn.Background == Brushes.Green)
            {
                for (int i = 0; i < 6; i++)
                {
                    var billingBtn = (Button)this.FindName("billingBtn" + (i + 1));
                    if (billingBtn.Background == Brushes.Green)
                    {
                        billingBtn.ClearValue(Button.BackgroundProperty);
                    }
                }
                billAllBtn.ClearValue(Button.BackgroundProperty);
            } else
            {
                for (int i = 0; i < 6; i++)
                {
                    var billingBtn = (Button)this.FindName("billingBtn" + (i + 1));
                    if (billingBtn.Background != Brushes.Green)
                    {
                        billingBtn.Background = Brushes.Green;
                    }
                }
                billAllBtn.Background = Brushes.Green;
            }
           
        }
    }
}
