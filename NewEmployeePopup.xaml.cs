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
    /// Interaction logic for NewEmployeePopup.xaml
    /// </summary>
    public partial class NewEmployeePopup : Window
    {

        private Models.COMP4952PROJECTContext db;
        public HashSet<Title> titles = new HashSet<Title>();
        StaffScreen ss;
        public NewEmployeePopup(StaffScreen staffScreen)
        {
            InitializeComponent();
            db = new Models.COMP4952PROJECTContext(); //initialize the DB context
            titles = getTitles();
            titleChoicesCB.ItemsSource = titles;
            ss = staffScreen;
        }




        /// <summary>
        /// Pulls the titles from the DB
        /// </summary>
        /// <returns></returns>
        private HashSet<Title> getTitles()
        {
            HashSet<Title> alltitles = new HashSet<Title>();

            alltitles = db.Title.ToHashSet();

            writeDebug("Found " + alltitles.Count + " titles");
            return alltitles;
        }

        /// <summary>
        /// user clicks the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); //close the window without saving.
        }

        /// <summary>
        /// User clicks the save button. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            //create a new staff member
            Staff thisStaff = new Staff();
            thisStaff.FirstName = firstNameBox.Text;
            thisStaff.LastName = lastNameBox.Text;
            thisStaff.Phone = phoneBox.Text;
            thisStaff.Title = titleChoicesCB.SelectedItem as Title;
            thisStaff.Rate = decimal.Parse(rateField.Text);

            db.Staff.Add(thisStaff);
            db.SaveChanges();
            ss.RefreshStaff();
            this.Close();
        }


        /// <summary>
        /// Writes to debug console
        /// </summary>
        /// <param name="message"></param>
        private void writeDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

    }
}
