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
        StaffScreen ss; //the staff screen which created this popup. 
        Staff staffToEdit;

        public NewEmployeePopup(StaffScreen staffScreen, Staff existingStaffToEdit = null)
        {
            InitializeComponent();
            db = new Models.COMP4952PROJECTContext(); //initialize the DB context
            titles = getTitles();
            titleChoicesCB.ItemsSource = titles;
            ss = staffScreen;

            staffToEdit = existingStaffToEdit;

            if (staffToEdit != null)
            {
                firstNameBox.Text = staffToEdit.FirstName;
                lastNameBox.Text = staffToEdit.LastName;
                phoneBox.Text = staffToEdit.Phone;

                System.Diagnostics.Debug.WriteLine("Title: " + staffToEdit.Title.Title1);
                Title matchingTitle = titles.Where((thisTitle) => thisTitle.Title1 == staffToEdit.Title.Title1).First();
                int titleIndex = titleChoicesCB.Items.IndexOf(matchingTitle);
                System.Diagnostics.Debug.WriteLine("title indedx: " + titleIndex);

                titleChoicesCB.SelectedIndex = titleIndex;
                rateField.Text = staffToEdit.Rate.ToString();
            }

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
            string messageString = "";

            if (staffToEdit != null)
            {
                //save existing staff memeber
                //create a new staff member
                
                staffToEdit.FirstName = firstNameBox.Text;
                staffToEdit.LastName = lastNameBox.Text;
                staffToEdit.Phone = formatPhoneNumber(phoneBox.Text);

                staffToEdit.Title = titleChoicesCB.SelectedItem as Title;
                staffToEdit.Rate = decimal.Parse(rateField.Text);
                db.Staff.Update(staffToEdit);
                messageString = "Staff Member Updated!";

            }
            else
            {

                //create a new staff member
                Staff thisStaff = new Staff();
                thisStaff.FirstName = firstNameBox.Text;
                thisStaff.LastName = lastNameBox.Text;
                thisStaff.Phone = formatPhoneNumber(phoneBox.Text);

                thisStaff.Title = titleChoicesCB.SelectedItem as Title;
                thisStaff.Rate = decimal.Parse(rateField.Text);

                db.Staff.Add(thisStaff);
                messageString = "Staff Member Added!";

            }
            db.SaveChanges();

            MessageBox.Show(messageString);


            ss.RefreshStaff();
            this.Close();
        }


        /// <summary>
        /// Removes characters from phone string.
        /// </summary>
        /// <param name="rawPhoneString"></param>
        /// <returns></returns>
        private String formatPhoneNumber(string rawPhoneString)
        {
            string returnString = rawPhoneString;

            try
            {
                returnString = new string(rawPhoneString.Where(c => char.IsDigit(c)).ToArray());
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
