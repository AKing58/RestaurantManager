using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MoreLinq;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for StaffScreen.xaml
    /// </summary>
    public partial class StaffScreen : Window
    {

        private Models.COMP4952PROJECTContext db;
        public Staff SelectedStaff { get; set; }
        public HashSet<Staff> employeeData = new HashSet<Staff>();


        public StaffScreen()
        {
            
            db = new Models.COMP4952PROJECTContext(); //initialize the DB context
            InitializeComponent();

            //configure the data table
            employeeData = createDataTable(); //get the staff data to display on the grid.
            gridEmployees.DataContext = employeeData;
            gridEmployees.IsReadOnly = true; //prevent editing of the grid
            gridEmployees.SelectionMode = DataGridSelectionMode.Single;                
                            
        }

        /// <summary>
        /// Creates a datatable of Staff data, including titles. 
        /// </summary>
        /// <returns>Staff data as a dataTable</returns>
        private HashSet<Staff> createDataTable()
        {
            DataTable thisData = new DataTable("Staff");
            HashSet<Staff> allStaff = db.Staff.Include(s=>s.Title).ToHashSet();
            System.Diagnostics.Debug.WriteLine("found " + allStaff.Count + " staff members");

            foreach(Staff member in allStaff)
            {
                System.Diagnostics.Debug.WriteLine("Staff:" + member.Title.Title1);
            }

            return allStaff ;
        }


        /// <summary>
        /// Called when a row is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Staff thisStaff = gridEmployees.SelectedItem as Staff;
            if(thisStaff != null)
            {
                writeDebug("This staff:" + thisStaff.LastName);
            }
            else
            {
                writeDebug("It's null");
            }
            


        }




        private void writeDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// The user clicks the "add employee" button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddEmployeeBtn_Click(object sender, RoutedEventArgs e)
        {
            var newEmployeePopup = new NewEmployeePopup();
            newEmployeePopup.Show();

        }
    }
}
