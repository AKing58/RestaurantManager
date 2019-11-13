using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MoreLinq;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for StaffScreen.xaml
    /// </summary>
    public partial class StaffScreen : Window
    {

        private Models.COMP4952PROJECTContext db;


        public StaffScreen()
        {
            
            db = new Models.COMP4952PROJECTContext();
            InitializeComponent();

            DataTable employeeData = createDataTable();
            gridEmployees.ItemsSource = employeeData.DefaultView;

        }

        private DataTable createDataTable()
        {
            DataTable thisData = new DataTable("Staff");
            HashSet<Staff> allStaff = db.Staff.ToHashSet();
            System.Diagnostics.Debug.WriteLine("found " + allStaff.Count + " staff members");
            thisData = allStaff.ToDataTable();
            return thisData;
        }

    }
}
