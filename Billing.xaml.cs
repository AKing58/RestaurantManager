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
    /// </summary>
    public partial class Billing : Window
    {
        COMP4952PROJECTContext db;
        TableInfo ti;

        /// <summary>
        /// Default constructor, unused
        /// </summary>
        public Billing() {}

        public Billing(int m)
        {
            InitializeComponent();
            initializeDBConnection();

            ti = db.TableInfo.Find(m);
            tableLabel.Content = "Table ID: " + ti.Id;
            loadCustomersInfo();
        }

        private void initializeDBConnection()
        {

            var connection = SettingsFile.Default.ConnectionString;

            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            db = new COMP4952PROJECTContext(builder.Options);

        }

        private void loadCustomersInfo()
        {
            List<Customer> customerLst = db.Customer.Where(u => u.TableId == ti.Id).ToList();
            decimal totalCost = 0;

            for (int i = 0; i < customerLst.Count; i++)
            {
                Customer cusTemp = customerLst[i];
                List<Orders> ordersLst = db.Orders.Where(o => o.CustId == int.Parse(cusTemp.Id.ToString())).ToList();
                decimal singleTotal = 0;

                foreach (Orders o in ordersLst)
                {
                    singleTotal += db.Item.Single(u => u.Id == o.ItemId).Cost;
                    totalCost += db.Item.Single(u => u.Id == o.ItemId).Cost;
                }

                var customer = (Label)this.FindName("costLabel" + (i+1));
                customer.Content = "$" + singleTotal;

            }

            var total = (Label)this.FindName("totalLabel");
            total.Content = "Total Cost: $" + totalCost;
        }

        private void backBtn_onClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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
