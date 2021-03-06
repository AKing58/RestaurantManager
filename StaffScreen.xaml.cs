﻿using COMP4952.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Globalization;

namespace COMP4952
{




    /// <summary>
    /// Interaction logic for StaffScreen.xaml
    /// Author: Eamonn Alphin
    /// </summary>
    public partial class StaffScreen : Page
    {

        private Models.COMP4952PROJECTContext db;
        public Staff SelectedStaff { get; set; } //the selected staff member
        public ObservableCollection<Staff> employeeData = new ObservableCollection<Staff>(); //holds all the staff members
        public ObservableCollection<ScheduleItem> selectedEmployeesScheduleItem = new ObservableCollection<ScheduleItem>(); //holds the selected staff members schedule and availabilities
        public ObservableCollection<cellValue[]> scheduleDataGridRows = new ObservableCollection<cellValue[]>();


        /// <summary>
        /// Holds data for a row in the availability & scheduled tables
        /// </summary>
        public struct ScheduleItem
        {
            //availability
            public DateTime availableStartTime, availableEndTime; //the available block start and end times
            public DateTime scheduleStartTime, scheduleEndTime; //the scheduled start and end times for the available block.
            public string availabilityString { get; set; } //the display string for the available block
            public string scheduleString { get; set; } //the display string for the scheduled block

            public CurrentAvailabilities thisAvailability; //the availability
            public CurrentSchedule thisSchedule; //the schedule, if it exists. 


            public ScheduleItem(CurrentAvailabilities ta, CurrentSchedule ts = null)
            {

                thisAvailability = ta;
                thisSchedule = ts;

                availableStartTime = thisAvailability.BlockStartTime;
                availableEndTime = thisAvailability.BlockEndTime;


                //if sst is null, set is null, and vice versa
                
                if(ts != null)
                {
                    scheduleStartTime = thisSchedule.BlockStartTime;
                    scheduleEndTime = (DateTime)thisSchedule.BlockEndTime;
                    scheduleString = scheduleStartTime.ToShortTimeString() + " - " + scheduleEndTime.ToShortTimeString();
                } else
                {
                    scheduleStartTime = new DateTime();
                    scheduleEndTime = new DateTime();
                    scheduleString = "";
                }
                
                

                availabilityString = availableStartTime.ToShortTimeString() + " - " + availableEndTime.ToShortTimeString();
                


            }

        }





        private void initializeDBConnection()
        {

            var connection = SettingsFile.Default.ConnectionString;

            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            db = new COMP4952PROJECTContext(builder.Options);
          
        }


        public StaffScreen()
        {

            initializeDBConnection();
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

            buildVisualScheduleDataGrid();
            fiveMinScheduleGrid.IsReadOnly = true;
            fiveMinScheduleGrid.SelectionMode = DataGridSelectionMode.Single;
            fiveMinScheduleGrid.ItemsSource = scheduleDataGridRows;
        }


        public void RefreshStaff()
        {
            employeeData.Clear();
            employeeData = getStaffData();
            gridEmployees.ItemsSource = employeeData;
            gridEmployees.Items.Refresh();

            if (SelectedStaff != null)
            {
                loadEmployeeData(SelectedStaff);
            }
        }

        /// <summary>
        /// Called when the user selects a new date.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void datePickerObj_DateChanged(object sender, RoutedEventArgs e)
        {
            if(SelectedStaff != null)
            {
                loadSelectedStaffsAvailabilityAndScheduleForDate(SelectedStaff, (DateTime)datePickerObj.SelectedDate);
            }
            
        }


        /// <summary>
        /// Creates a datatable of Staff data, including titles. 
        /// </summary>
        /// <returns>Staff data as a dataTable</returns>
        private ObservableCollection<Staff> getStaffData()
        {
           
            HashSet<Staff> allStaff = db.Staff.Include(s=>s.Title).ToHashSet();
            System.Diagnostics.Debug.WriteLine("found " + allStaff.Count + " staff members");
            ObservableCollection<Staff> observableStaff = new ObservableCollection<Staff>();

            foreach(Staff member in allStaff)
            {
                System.Diagnostics.Debug.WriteLine("Staff:" + member.Title.Title1);
                observableStaff.Add(member);
            }

            return observableStaff;
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
                loadEmployeeData(thisStaff);

            }
            else
            {
                AddAvailSchedBtn.IsEnabled = false;
            }
            


        }


        /// <summary>
        /// Loads data for the chosen employee
        /// </summary>
        /// <param name="thisStaff"></param>
        private void loadEmployeeData(Staff thisStaff)
        {
            writeDebug("Chosen staff:" + thisStaff.LastName);
            SelectedStaff = thisStaff;
            selectedNameLabel.Content = SelectedStaff.FirstName + " " + SelectedStaff.LastName;
            loadSelectedStaffsAvailabilityAndScheduleForDate(SelectedStaff, (DateTime)datePickerObj.SelectedDate);
            AddAvailSchedBtn.IsEnabled = true;

            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            TimeSpan twoWeeks = new TimeSpan(24 * 14, 0, 0);
            DateTime twoWeeksFromToday = today.Add(twoWeeks);
            CalendarDateRange thisRange = new CalendarDateRange(today, twoWeeksFromToday);


            loadSelectedEmployeesSchedule(thisStaff, thisRange);
        }


        /// <summary>
        /// Reloads the data in the tables for the give staff member and the given date range.
        /// </summary>
        /// <param name="thisStaff"></param>
        /// <param name="thisDateRange"></param>
        public void reloadScheduleAndAvailabilityTables(Staff thisStaff, CalendarDateRange thisDateRange)
        {
            writeDebug("Reloading shedule and availability tables");
            loadSelectedStaffsAvailabilityAndScheduleForDate(thisStaff, thisDateRange.Start);
            loadSelectedEmployeesSchedule(thisStaff, thisDateRange);
        }
        


        /// <summary>
        /// Loads the given staff members list of availability and schedule for a given date.
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
            foreach (CurrentAvailabilities thisAvailability in thisDaysAvailabilties)
            {
                //availability start and end times

                ScheduleItem thisScheduleItem = new ScheduleItem(thisAvailability);

                if (thisAvailability.CurrentSchedule.Count == 0)
                {
                    //staff is not scheduled for this availability.
                    selectedEmployeesScheduleItem.Add(thisScheduleItem);
                }
                else
                {

                    //scheduled times for each availability
                    foreach (CurrentSchedule thisSchedule in thisAvailability.CurrentSchedule)
                    {

                        //create the schedule item
                        thisScheduleItem = new ScheduleItem(thisSchedule.Availability, thisSchedule);

                        //add it to the list of schedule items. 
                        selectedEmployeesScheduleItem.Add(thisScheduleItem);
                    }
                }

            }

            //bind the list to the datagrid
            DaysAvailabilityGrid.Items.Refresh();

        }


        /// <summary>
        /// Sets up the columns of the visual scheduler
        /// </summary>
        private void buildVisualScheduleDataGrid()
        {
           
            TimeSpan oneDay = new TimeSpan(24, 0, 0);

            int columns = 15; // 2 weeks. + 1 column to list time. 

            //create the time column
            DataGridTextColumn timeColumn = new DataGridTextColumn() { Binding = new Binding("[0].display") };
            timeColumn.Header = "Time";
            fiveMinScheduleGrid.Columns.Add(timeColumn);


            //add all the columns, bind them to their related column in the array.  
            for (int i = 1; i <= columns; i++)
            {
                DataGridTextColumn thisColumn = new DataGridTextColumn() { Binding = new Binding("[" + i.ToString() + "].display") };
                thisColumn.Header = "Date";
                fiveMinScheduleGrid.Columns.Add(thisColumn);
            }
        }




        /// <summary>
        /// STruct to use to fill each cell in the schedule grid. 
        /// </summary>
        public struct cellValue
        {
            public string type { get; set; }
            public string display { get; set; }
        }

        public cellValue[] dataGridRowValues = new cellValue[] { };

        /// <summary>
        /// Loads the visual availability for this staff member for the given date range.
        /// </summary>
        /// <param name="thisStaff">the staff member to view</param>
        /// <param name="thisRange">the date range to view</param>
        private void loadSelectedEmployeesSchedule(Staff thisStaff, CalendarDateRange thisRange)
        {
            
            scheduleDataGridRows.Clear();
           
            HashSet<CurrentAvailabilities> theseAvailabilities = db.CurrentAvailabilities
                                                                        .Where(CA => CA.StaffId == thisStaff.Id)
                                                                        .Where(CA => CA.BlockStartTime.Date >= thisRange.Start.Date)
                                                                        .Where(CA => CA.BlockStartTime.Date <= thisRange.End.Date)
                                                                        .Include(CA => CA.CurrentSchedule)
                                                                        .ToHashSet();



            HashSet<CurrentSchedule> thisSchedule = new HashSet<CurrentSchedule>();

            //separate out the schedules from each availability. 
            foreach(CurrentAvailabilities thisAVailability in theseAvailabilities)
            {
                thisSchedule.UnionWith(thisAVailability.CurrentSchedule.ToHashSet());
            }



            string display = "";
            string type = "";

            TimeSpan oneDay = new TimeSpan(24, 0, 0);
            

            int rowCounter = 0;
            int timeInterval = 30; //the number of minutes each row is, so 30 means 30 minute intervals. 
            int columns = 15; // 2 weeks. + 1 column to list time. 

            

            //for each row
            
            for (int fiveMinBlock = 0; fiveMinBlock < 24*60; fiveMinBlock += timeInterval)
            {
                
                
                dataGridRowValues = new cellValue[columns+1];

                //Display the time in the time column every 60 minutes. 
                if (fiveMinBlock % 60 == 0)
                {
                   dataGridRowValues[0].display = new TimeSpan(fiveMinBlock / 60, fiveMinBlock % 60, 0).ToString();
                }
                else
                {
                    dataGridRowValues[0].display = "";
                }


                DateTime startDay = new DateTime(thisRange.Start.Year, thisRange.Start.Month, thisRange.Start.Day, fiveMinBlock / 60, fiveMinBlock % 60, 0);
                DateTime endDay = new DateTime(thisRange.End.Year, thisRange.End.Month, thisRange.End.Day, fiveMinBlock / 60, fiveMinBlock % 60, 0);
                


                int dayCounter = 1;
               

                //for each column. 
                for (DateTime dateTime = startDay; dateTime <= endDay; dateTime = dateTime.Add(oneDay))
                {
                    //Debug.WriteLine("This time: " + dateTime.ToString());

                    DataGridTextColumn thisColumn = (DataGridTextColumn)fiveMinScheduleGrid.Columns.ElementAt(dayCounter);
                    thisColumn.Header = dateTime.ToShortDateString();


                    bool withinSchedule = false;
                    DateTime schedStart = new DateTime();
                    DateTime schedEnd = new DateTime();
                    DateTime availStart = new DateTime();
                    DateTime availEnd = new DateTime();


                    //foreach block of schedule in the selected employees scheduled dates. 
                    foreach (CurrentSchedule thisScheduleDate in thisSchedule)
                    {
                        CalendarDateRange thisBlock = new CalendarDateRange(thisScheduleDate.BlockStartTime, thisScheduleDate.BlockEndTime);
                        
                        if(withinDateRange(dateTime, thisBlock))
                        {
                            //Debug.WriteLine("Within Shcedule");
                            withinSchedule = true;
                            schedStart = thisBlock.Start;
                            schedEnd = thisBlock.End;
                            
                        }
                    }

                    if (withinSchedule)
                    {
                        type = "scheduled";
                        display = "Scheduled";                      

                    } else
                    {
                        bool withinAvailability = false;
                        foreach (CurrentAvailabilities thisAvailability in theseAvailabilities)
                        {
                            CalendarDateRange thisBlock = new CalendarDateRange(thisAvailability.BlockStartTime, thisAvailability.BlockEndTime);
                            if (withinDateRange(dateTime, thisBlock))
                            {
                                //Debug.WriteLine("Within AVailability");
                                withinAvailability = true;
                                availStart = thisBlock.Start;
                                availEnd = thisBlock.End;
                            }
                        }

                        if (withinAvailability)
                        {
                            type = "available";
                            display = "Available";

                        }
                        else
                        {
                            display = "-";

                        }

                    }


                    cellValue thisCellValue = new cellValue();
                    thisCellValue.type = type;
                    thisCellValue.display = display;

                    dataGridRowValues[dayCounter] = thisCellValue;
                        
                    dayCounter = dayCounter + 1;

                }

                

                scheduleDataGridRows.Add(dataGridRowValues);
                rowCounter = rowCounter + 1;

            }



            
            fiveMinScheduleGrid.Items.Refresh();
           
            

        }




        /// <summary>
        /// Returns true if the date is within the date range.
        /// </summary>
        /// <param name="thisDate"></param>
        /// <param name="thisRange"></param>
        /// <returns></returns>
        private bool withinDateRange(DateTime thisDate, CalendarDateRange thisRange) {

            //Debug.WriteLine("Checking " + thisDate.ToString() + " between " + thisRange.Start.ToString() + " and " + thisRange.End.ToString());

            return (thisRange.Start <= thisDate && thisDate <= thisRange.End);

        }

        
        /// <summary>
        /// Used for debugging
        /// </summary>
        /// <param name="message"></param>
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
            var newEmployeePopup = new NewEmployeePopup(this);
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
                NewAvailSchedPopup newASPopup = new NewAvailSchedPopup(SelectedStaff, (DateTime)datePickerObj.SelectedDate, this);
                newASPopup.Show();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("Home.xaml", UriKind.Relative));
        }


        /// <summary>
        /// handles the user right clicking and choosing "edit" on the staff grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            
            writeDebug("Editing staff member");

            //Get the MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the item that was right clicked. 
            var item = (DataGrid)contextMenu.PlacementTarget;

            //get the cells (the row) that was selected.
            Staff staffToEdit= (Staff)item.SelectedCells[0].Item;

            //call a new employee popup with the staff member data. 
            var newEmployeePopup = new NewEmployeePopup(this, staffToEdit);
            newEmployeePopup.Show();


        }


        /// <summary>
        /// handles the user right clicking and choosing "delete" on the staff grid. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {


            writeDebug("Deleting staff member");
            
            //Get the MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the item that was right clicked. 
            var item = (DataGrid)contextMenu.PlacementTarget;

            //get the cells (the row) that was selected.
            Staff staffToDelete = (Staff)item.SelectedCells[0].Item;

            confirmDeleteEmployee(staffToDelete);

        }


        /// <summary>
        /// Confirms the deletion of an employee. 
        /// </summary>
        /// <param name="staffToDelete"></param>
        private void confirmDeleteEmployee(Staff staffToDelete)
        {

            string displayMessage = "Are you sure you want to delete: \n" + staffToDelete.Id + ":" + staffToDelete.FirstName + " " + staffToDelete.LastName + "?";


            MessageBoxResult result = MessageBox.Show(displayMessage, "Alert", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:

                    //Remove the toDeleteFromBindedList object from your ObservableCollection
                    employeeData.Remove(staffToDelete);

                    //remove the object from entity framework
                    db.Staff.Remove(staffToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Employee Deleted.");
                    break;
                case MessageBoxResult.No:
                    break;
            }

        }


        /// <summary>
        /// Handles the user deleting an availability
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAvailability_Click(object sender, RoutedEventArgs e)
        {
            writeDebug("Deleting Availability");

            //Get the MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the item that was right clicked. 
            var item = (DataGrid)contextMenu.PlacementTarget;

            //get the cells (the row) that was selected.
            ScheduleItem ScheduleItemToDelete = (ScheduleItem)item.SelectedCells[0].Item;

            CurrentAvailabilities availabilityToDelete = ScheduleItemToDelete.thisAvailability;


            confirmDeleteAvailability(availabilityToDelete);
        }


        /// <summary>
        /// Confirms if the user wants to delete the given availabilities. 
        /// </summary>
        /// <param name="itemToDelete"></param>
        private void confirmDeleteAvailability(CurrentAvailabilities itemToDelete)
        {

            string startString = itemToDelete.BlockStartTime.ToShortDateString() + " " + itemToDelete.BlockStartTime.ToShortTimeString();
            string endString = itemToDelete.BlockEndTime.ToShortDateString() + " " + itemToDelete.BlockEndTime.ToShortTimeString();


            string displayMessage = "Are you sure you want to delete: \n" + startString + " : " + endString  + "\n and related schedules?";


            MessageBoxResult result = MessageBox.Show(displayMessage, "Alert", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:

                    //remove the object from entity framework
                    db.CurrentAvailabilities.Remove(itemToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Availability and Schedules Deleted.");
                    loadEmployeeData(SelectedStaff);

                    break;
                case MessageBoxResult.No:
                    break;
            }

        }


        /// <summary>
        /// Handles the user deleting an scheudle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            writeDebug("Deleting Schedule");

            //Get the MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the item that was right clicked. 
            var item = (DataGrid)contextMenu.PlacementTarget;

            //get the cells (the row) that was selected.
            ScheduleItem ScheduleItemToDelete = (ScheduleItem)item.SelectedCells[0].Item;

            CurrentSchedule scheduleToDelete = ScheduleItemToDelete.thisSchedule;


            confirmDeleteSchedule(scheduleToDelete);
        }


        /// <summary>
        /// Confirms if the user wants to delete the given schedule. 
        /// </summary>
        /// <param name="itemToDelete"></param>
        private void confirmDeleteSchedule(CurrentSchedule itemToDelete)
        {
            if(itemToDelete != null)
            {
                string startString = itemToDelete.BlockStartTime.ToShortDateString() + " " + itemToDelete.BlockStartTime.ToShortTimeString();
                string endString = itemToDelete.BlockEndTime.ToShortDateString() + " " + itemToDelete.BlockEndTime.ToShortTimeString();


                string displayMessage = "Are you sure you want to delete: \n" + startString + " : " + endString + "?";


                MessageBoxResult result = MessageBox.Show(displayMessage, "Alert", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:

                        //remove the object from entity framework
                        db.CurrentSchedule.Remove(itemToDelete);
                        db.SaveChanges();
                        MessageBox.Show("Schedule Deleted.");
                        loadEmployeeData(SelectedStaff);

                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
           
        }


    }
}
