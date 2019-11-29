using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;

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
            thisStaff.Phone = formatPhoneNumber(phoneBox.Text);

            thisStaff.Title = titleChoicesCB.SelectedItem as Title;
            thisStaff.Rate = decimal.Parse(rateField.Text);

            db.Staff.Add(thisStaff);
            db.SaveChanges();
            ss.RefreshStaff();
            this.Close();
        }


        /// <summary>
        /// Converts a 10 character phone number into (306)-999-9999 format. 
        /// </summary>
        /// <param name="rawPhoneString"></param>
        /// <returns></returns>
        private String formatPhoneNumber(string rawPhoneString)
        {
            string returnString = rawPhoneString;

            try
            {
                string strphoneNumber = new string(rawPhoneString.Where(c => char.IsDigit(c)).ToArray());
                int intPhoneNumber = int.Parse(strphoneNumber);
                returnString = String.Format("{0:(###) ###-####}", intPhoneNumber);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine("Error converting phone number: " + err);
            }
            

            return returnString;
        }


        /// <summary>
        /// Writes to debug console
        /// </summary>
        /// <param name="message"></param>
        private void writeDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }


        /// <summary>
        /// Prevents the user from entering letters into the rate field. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rateField_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !isNonNumeric(e.Text); //prevent the user from typing non numeric characters. 
        }


        


        private static readonly Regex _regex = new Regex("[^0-9.-]+");


        /// <summary>
        /// Returns true if the strings only contains numbers. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool isNonNumeric(string text)
        {
            return !_regex.IsMatch(text);
        }

        /// <summary>
        /// Prevents the user from entering non-numeric values in the phone field. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void phoneBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !isNonNumeric(e.Text);
        }
    }
}
