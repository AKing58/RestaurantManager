using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq;
using System.Collections.ObjectModel;

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
        public ObservableCollection<ScheduleItem> selectedEmployeesScheduleItem = new ObservableCollection<ScheduleItem>(); //holds the selected staff members schedule and availabilities
        public ObservableCollection<WeeklyFiveMinutes> complexScheduleRows = new ObservableCollection<WeeklyFiveMinutes>();

        /// <summary>
        /// Holds data for a row in the availability & scheduled tables
        /// </summary>
        public struct ScheduleItem
        {
            //availability
            public DateTime availableStartTime, availableEndTime; //the available block start and end times
            public DateTime scheduleStartTime, scheduleEndtime; //the scheduled start and end times for the available block.
            public string availabilityString { get; set; } //the display string for the available block
            public string scheduleString { get; set; } //the display string for the scheduled block


            public ScheduleItem(DateTime ast, DateTime aet, DateTime sst, DateTime set)
            {
                availableStartTime = ast;
                availableEndTime = aet;
                scheduleStartTime = sst;
                scheduleEndtime = set;

                availabilityString = availableStartTime.ToShortTimeString() + " - " + availableEndTime.ToShortTimeString();
                scheduleString = scheduleStartTime.ToShortTimeString() + " - " + scheduleEndtime.ToShortTimeString();


            }

        }

        public struct WeeklyFiveMinutes
        {
            String Sunday1 { get; set; }
            String Monday1 { get; set; }
            String Tuesday1 { get; set; }
            String Wednesday1 { get; set; }
            String Thursday1 { get; set; }
            String Friday1 { get; set; }
            String Saturday1 { get; set; }
            String Sunday2 { get; set; }
            String Monday2 { get; set; }
            String Tuesday2 { get; set; }
            String Wednesday2 { get; set; }
            String Thursday2 { get; set; }
            String Friday2 { get; set; }
            String Saturday2 { get; set; }

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
            gridEmployees.ItemsSource = employeeData;
            gridEmployees.IsReadOnly = true; //prevent editing of the grid
            gridEmployees.SelectionMode = DataGridSelectionMode.Single;

            DaysAvailabilityGrid.ItemsSource = selectedEmployeesScheduleItem;


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
                loadSelectedStaffsAvailabilityAndScheduleForDate(SelectedStaff, (DateTime)datePickerObj.SelectedDate);
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
            selectedEmployeesScheduleItem.Clear(); //erase it.

            //get the employees availability for the given date.
            HashSet<CurrentAvailabilities> thisDaysAvailabilties = db.CurrentAvailabilities
                                                .Where(ra => ra.StaffId == thisStaff.Id)
                                                .Where(ra => ra.BlockStartTime.Date == thisDate.Date)
                                                .Include(ra => ra.CurrentSchedule)
                                                .ToHashSet();

            writeDebug("found " + thisDaysAvailabilties.Count + " available blocks");

           //convert to struct
           foreach(CurrentAvailabilities thisAvailability in thisDaysAvailabilties)
            {
                //availability start and end times
                DateTime ast = thisAvailability.BlockStartTime;
                DateTime aet = thisAvailability.BlockEndTime;

                //scheduled times for each availability
                foreach(CurrentSchedule thisSchedule in thisAvailability.CurrentSchedule)
                {

                    //scheduled start and end times
                    DateTime sst = thisSchedule.BlockStartTime;
                    DateTime set = thisSchedule.BlockEndTime;

                    //create the schedule item
                    ScheduleItem thisItem = new ScheduleItem(ast, aet, sst, set);
                    
                    //add it to the list of schedule items. 
                    selectedEmployeesScheduleItem.Add(thisItem);
                }   
            }

            //bind the list to the datagrid
            DaysAvailabilityGrid.Items.Refresh();


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
