using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for StaffScreen.xaml
    /// Eamonn Alphin
    /// </summary>
    public partial class StaffScreen : Window
    {

        private Models.COMP4952PROJECTContext db;
        public Staff SelectedStaff { get; set; } //the selected staff member
        public HashSet<Staff> employeeData = new HashSet<Staff>(); //holds all the staff members
        public HashSet<ScheduleItem> selectedEmployeesScheduleItem = new HashSet<ScheduleItem>(); //holds the selected staff members schedule and availabilities

        /// <summary>
        /// Holds data for a row in the availability & scheduled tables
        /// </summary>
        public struct ScheduleItem
        {
            //availability
            public TimeSpan availableStartTime, availableEndTime; //the available block start and end times
            public TimeSpan scheduleStartTime, scheduleEndtime; //the scheduled start and end times for the available block.
            public String availabilityString; //the display string for the available block
            public string scheduleString; //the display string for the scheduled block


            public ScheduleItem(TimeSpan ast, TimeSpan aet, TimeSpan sst, TimeSpan set)
            {
                availableStartTime = ast;
                availableEndTime = aet;
                scheduleStartTime = sst;
                scheduleEndtime = set;

                availabilityString = availableStartTime.ToString() + " - " + availableEndTime.ToString();
                scheduleString = scheduleStartTime.ToString() + " - " + scheduleEndtime.ToString();


            }

        }


        public StaffScreen()
        {
            
            db = new Models.COMP4952PROJECTContext(); //initialize the DB context
            InitializeComponent();
            AddAvailSchedBtn.IsEnabled = false;
            datePickerObj.SelectedDate = DateTime.Now;
            datePickerObj.DisplayDate = DateTime.Now;
            //configure the data table
            employeeData = getStaffData(); //get the staff data to display on the grid.
            gridEmployees.DataContext = employeeData;
            gridEmployees.IsReadOnly = true; //prevent editing of the grid
            gridEmployees.SelectionMode = DataGridSelectionMode.Single;                
                            
        }

        /// <summary>
        /// Creates a datatable of Staff data, including titles. 
        /// </summary>
        /// <returns>Staff data as a dataTable</returns>
        private HashSet<Staff> getStaffData()
        {
           
            HashSet<Staff> allStaff = db.Staff.Include(s=>s.Title).ToHashSet();
            System.Diagnostics.Debug.WriteLine("found " + allStaff.Count + " staff members");

            foreach(Staff member in allStaff)
            {
                System.Diagnostics.Debug.WriteLine("Staff:" + member.Title.Title1);
            }

            return allStaff ;
        }


        /// <summary>
        /// Called when a an employee in the staff table is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Staff thisStaff = gridEmployees.SelectedItem as Staff;
            if(thisStaff != null)
            {
                writeDebug("Chosen staff:" + thisStaff.LastName);
                SelectedStaff = thisStaff;
                selectedNameLabel.Content = SelectedStaff.FirstName + " " + SelectedStaff.LastName;
                AddAvailSchedBtn.IsEnabled = true;
            }
            else
            {
                writeDebug("It's null");
                AddAvailSchedBtn.IsEnabled = false;
            }
            


        }


        /// <summary>
        /// Loads the given staff members availability and schedule for a given date.
        /// </summary>
        /// <param name="thisStaff">the staff object to view</param>
        /// <param name="thisDate">The date to view</param>
        private void loadSelectedStaffsAvailabilityAndScheduleForDate(Staff thisStaff, DateTime thisDate)
        {
            
            //get the employees availability for the given date.
            HashSet<CurrentAvailabilities> thisDaysAvailabilties = db.CurrentAvailabilities
                                                .Where(ra => ra.StaffId == thisStaff.Id)
                                                .Where(ra => ra.Date == thisDate.Date)
                                                .Include(ra => ra.CurrentSchedule)
                                                .ToHashSet();

            writeDebug("found " + thisDaysAvailabilties.Count + " available blocks");

           //convert to struct
           foreach(CurrentAvailabilities thisAvailability in thisDaysAvailabilties)
            {
                //availability start and end times
                TimeSpan ast = thisAvailability.BlockStartTime;
                TimeSpan aet = thisAvailability.BlockEndTime;

                //scheduled times for each availability
                foreach(CurrentSchedule thisSchedule in thisAvailability.CurrentSchedule)
                {

                    //scheduled start and end times
                    TimeSpan sst = thisSchedule.BlockStartTime;
                    TimeSpan set = thisSchedule.BlockEndTime;

                    //create the schedule item
                    ScheduleItem thisItem = new ScheduleItem(ast, aet, sst, set);
                    
                    //add it to the list of schedule items. 
                    selectedEmployeesScheduleItem.Add(thisItem);
                }   
            }


            //bind the list ot the datagrid
            DaysAvailabilityGrid.DataContext = selectedEmployeesScheduleItem;

        }



        /// <summary>
        /// Loads the visual availability for this staff member for the given date range.
        /// </summary>
        /// <param name="thisStaff">the staff member to view</param>
        /// <param name="thisRange">the date range to view</param>
        private void loadSelectedEmployeesSchedule(Staff thisStaff, CalendarDateRange thisRange)
        {

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

        /// <summary>
        /// User clicks the add availability or schedule button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            //new availability or schedule popup
            if(SelectedStaff != null)
            {
                NewAvailSchedPopup newASPopup = new NewAvailSchedPopup(SelectedStaff, (DateTime)datePickerObj.SelectedDate);
                newASPopup.Show();
            }
           

        }


    }
}
