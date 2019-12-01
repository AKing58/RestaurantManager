using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using COMP4952.Models;
using Microsoft.EntityFrameworkCore;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for OrderScreen.xaml
    /// Justin Kwok
    /// Adam King
    /// </summary>
    public partial class OrderScreen : Window
    {
        COMP4952PROJECTContext db;
        TableInfo ti;
        Customer cus;
        private int custIndex = 0;

        /// <summary>
        /// Default constructor (Unused)
        /// </summary>
        public OrderScreen() { }

        /// <summary>
        /// Order Screen constructor
        /// </summary>
        /// <param name="m"></param>
        public OrderScreen(int m)
        {
            
            InitializeComponent();
            initializeDBConnection();

            ti = db.TableInfo.Find(m);
            tableLabel.Content = "Table ID: " + ti.Id;
            LoadCustomers();

        }


        private void initializeDBConnection()
        {

            var connection = SettingsFile.Default.ConnectionString;

            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            db = new COMP4952PROJECTContext(builder.Options);
           
        }

        /// <summary>
        /// Loads customers that belong to the current table onto the customer buttons
        /// </summary>
        private void LoadCustomers()
        {
            List<Customer> customerLst = db.Customer.Where(u => u.TableId == ti.Id).ToList();
            Console.WriteLine(customerLst.Count + " customers on table " + ti.Id);
            Console.WriteLine(customerLst);
            for (int i = 1; i < 7; i++)
            {
                Console.WriteLine(i);
                var customer = (Button)this.FindName("customer" + i + "_Btn");
                if (i > customerLst.Count)
                    return;
                if (customer.IsEnabled == false)
                {
                    customer.IsEnabled = true;

                    var removeBtn = (Button)this.FindName("removeCust" + i + "_Btn");
                    removeBtn.IsEnabled = true;

                    customer.Tag = customerLst[i-1].Id;
                }
            }
            Customer tempCust = db.Customer.SingleOrDefault(u => u.TableId == ti.Id);
            if (tempCust != null)
                cus = tempCust;
        }

        /// <summary>
        /// Adds a customer to the table on button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addCustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            for (int i=1; i<7; i++)
            {
                var customer = (Button)this.FindName("customer" + i + "_Btn");
                if (customer.IsEnabled == false)
                {
                    customer.IsEnabled = true;

                    var removeBtn = (Button)this.FindName("removeCust" + i + "_Btn");
                    removeBtn.IsEnabled = true;

                    Customer person = new Customer();
                    person.Table = ti;
                    person.TableId = ti.Id;
                    db.Customer.Add(person);
                    db.SaveChanges();

                    customer.Tag = person.Id;
                    customer.Background = Brushes.Green;
                    cus = person;
                    custIndex = i;
                    break;
                }
                else
                {
                    customer.ClearValue(Button.BackgroundProperty);
                }
            }

        }

        /// <summary>
        /// Exits the window to the previous screen without saving
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Changes which customer's orders are currently being displayed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customerBtn_Click(object sender, RoutedEventArgs e)
        {
            var customerBtn = (Button)sender;
            customerBtn.Background = Brushes.Green;

            for (int i = 1; i < 7; i++)
            {
                var customer = (Button)this.FindName("customer" + i + "_Btn");
                if (customer.Name != customerBtn.Name && customer.IsEnabled)
                {
                    customer.ClearValue(Button.BackgroundProperty);
                }
            }

            cus = db.Customer.Find(int.Parse(customerBtn.Tag.ToString()));
            ReselectItems();
        }

        /// <summary>
        /// Sets item buttons for the currently selected customer
        /// </summary>
        private void ReselectItems()
        {
            foreach (object o in orderGrid.Children)
            {
                if (o is Button && ((Button)o).ToolTip != null && ((Button)o).ToolTip.ToString() == "FoodItem")
                {
                    Button b = (Button)o;
                    if(db.Orders.SingleOrDefault(u => u.CustId == cus.Id && u.ItemId == int.Parse(b.Tag.ToString())) != null)
                    {
                        b.Background = Brushes.Green;
                    }
                    else
                    {
                        b.Background = Brushes.Red;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the order to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            db.SaveChanges();
        }

        /// <summary>
        /// Removes the customer from the table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeCustBtn_Click(object sender, RoutedEventArgs e)
        {
            var removeBtn = (Button)sender;
            var index = removeBtn.Tag.ToString();

            removeBtn.IsEnabled = false;

            var customer = (Button)this.FindName("customer" + index + "_Btn");
            int tempCustId = int.Parse(customer.Tag.ToString());
            customer.ClearValue(Button.BackgroundProperty);

            List<Orders> temp = db.Orders.Where(u => u.CustId == tempCustId).ToList();
            foreach (Orders o in temp) {
                db.Orders.Remove(o);
            }
            db.SaveChanges();

            Customer cusTemp = db.Customer.SingleOrDefault(u => u.Id == tempCustId);
            db.Customer.Remove(cusTemp);
            db.SaveChanges();
            customer.IsEnabled = false;

        }

        /// <summary>
        /// Adds or removes the item from the current order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void item_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if(cus != null)
            {
                if (button.Background != Brushes.Green)
                {
                    Orders order = new Orders();
                    order.ItemId = int.Parse(button.Tag.ToString());
                    order.CustId = cus.Id;
                    db.Orders.Add(order);

                    button.Background = Brushes.Green;
                }
                else
                {
                    Orders temp = db.Orders.SingleOrDefault(u => u.CustId == cus.Id && u.ItemId == int.Parse(button.Tag.ToString()));
                    if (temp != null)
                        db.Orders.Remove(temp);
                    button.Background = Brushes.Red;
                }
            }
        }

    }
}
