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
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Navigation;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for StaffScreen.xaml
    /// Eamonn Alphin
    /// </summary>
    public partial class StaffScreen : Page
    {

        private Models.COMP4952PROJECTContext db;
        public Staff SelectedStaff { get; set; } //the selected staff member
        public ObservableCollection<Staff> employeeData = new ObservableCollection<Staff>(); //holds all the staff members
        public ObservableCollection<ScheduleItem> selectedEmployeesScheduleItem = new ObservableCollection<ScheduleItem>(); //holds the selected staff members schedule and availabilities
        public ObservableCollection<WeeklyFiveMinutes> fiveMinuteRows = new ObservableCollection<WeeklyFiveMinutes>();

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


            public ScheduleItem(DateTime ast, DateTime aet, DateTime? sst, DateTime? set)
            {
                availableStartTime = ast;
                availableEndTime = aet;

                //if sst is null, set is null, and vice versa
                
                if(sst != null)
                {
                    scheduleStartTime = (DateTime)sst;
                    scheduleEndTime = (DateTime)set;
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




        public struct WeeklyFiveMinutes
        {
            public String time { get; set; }

            public Dictionary<int, string> dayOfSchedule;
            

            public String day0 { get; set; }
            public String day1 { get; set; }
            public String day2 { get; set; }
            public String day3 { get; set; }
            public String day4 { get; set; }
            public String day5 { get; set; }
            public String day6 { get; set; }
            public String day7 { get; set; }
            public String day8 { get; set; }
            public String day9 { get; set; }
            public String day10 { get; set; }
            public String day11 { get; set; }
            public String day12 { get; set; }
            public String day13 { get; set; }

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
            fiveMinScheduleGrid.ItemsSource = fiveMinuteRows;
            fiveMinScheduleGrid.IsReadOnly = true;
            fiveMinScheduleGrid.SelectionMode = DataGridSelectionMode.Single;

        }

        public void refreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshStaff();
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
                writeDebug("It's null");
                AddAvailSchedBtn.IsEnabled = false;
            }
            


        }

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

                ScheduleItem thisScheduleItem = new ScheduleItem(ast, aet, null, null);

                if(thisAvailability.CurrentSchedule.Count == 0)
                {
                    //staff is not scheduled for this availability.
                    selectedEmployeesScheduleItem.Add(thisScheduleItem);
                }
                else
                {

                    //scheduled times for each availability
                    foreach (CurrentSchedule thisSchedule in thisAvailability.CurrentSchedule)
                    {

                        //scheduled start and end times
                        DateTime sst = thisSchedule.BlockStartTime;
                        DateTime set = thisSchedule.BlockEndTime;

                        //create the schedule item
                        thisScheduleItem = new ScheduleItem(ast, aet, sst, set);

                        //add it to the list of schedule items. 
                        selectedEmployeesScheduleItem.Add(thisScheduleItem);
                    }
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
            fiveMinuteRows.Clear();
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

            TimeSpan oneDay = new TimeSpan(24, 0, 0);
            WeeklyFiveMinutes theseFiveMinutes = new WeeklyFiveMinutes();

            int rowCounter = 0;
            int timeInterval = 30; //the number of minutes each row is, so 30 means 30 minute intervals. 

          

            //for each row
            for (int fiveMinBlock = 0; fiveMinBlock < 24*60; fiveMinBlock += timeInterval)
            {
                DateTime startDay = new DateTime(thisRange.Start.Year, thisRange.Start.Month, thisRange.Start.Day, fiveMinBlock / 60, fiveMinBlock % 60, 0);
                DateTime endDay = new DateTime(thisRange.End.Year, thisRange.End.Month, thisRange.End.Day, fiveMinBlock / 60, fiveMinBlock % 60, 0);
                
                int dayCounter = 0;
               

                //for each column. 
                for (DateTime dateTime = startDay; dateTime <= endDay; dateTime = dateTime.Add(oneDay))
                {
                    //Debug.WriteLine("This time: " + dateTime.ToString());
                    
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
                        display = "Scheduled";
                        //check if the last addeded value was the same. 
                        var lastEntryRow = fiveMinuteRows.ElementAt(fiveMinuteRows.Count - 1);   
                        var lastEntryForThisDay = lastEntryRow.da
                        

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
                            display = "Available";
                            
                           
                        }
                        else
                        {
                            display = "-";

                        }

                    }

                    //Display the time in the time column every 60 minutes. 
                    if(fiveMinBlock%60 == 0)
                    {
                        theseFiveMinutes.time = new TimeSpan(fiveMinBlock / 60, fiveMinBlock % 60, 0).ToString();
                    }
                    else
                    {
                        theseFiveMinutes.time = "";
                    }
                   

                    

                    //determine which column we are on. 
                    switch (dayCounter)
                    {
                        case 0:
                            theseFiveMinutes.day0 = display;
                            break;
                        case 1:
                            theseFiveMinutes.day1 = display;
                            break;
                        case 2:
                            theseFiveMinutes.day2 = display;
                            break;
                        case 3:
                            theseFiveMinutes.day3 = display;
                            break;
                        case 4:
                            theseFiveMinutes.day4 = display;
                            break;
                        case 5:
                            theseFiveMinutes.day5 = display;
                            break;
                        case 6:
                            theseFiveMinutes.day6 = display;
                            break;
                        case 7:
                            theseFiveMinutes.day7 = display;
                            break;
                        case 8:
                            theseFiveMinutes.day8 = display;
                            break;
                        case 9:
                            theseFiveMinutes.day9 = display;
                            break;
                        case 10:
                            theseFiveMinutes.day10 = display;
                            break;
                        case 11:
                            theseFiveMinutes.day11 = display;
                            break;
                        case 12:
                            theseFiveMinutes.day12 = display;
                            break;
                        case 13:
                            theseFiveMinutes.day13 = display;
                            break;
                        default:
                            break;
                    }
                    dayCounter = dayCounter + 1;

                }

                

                fiveMinuteRows.Add(theseFiveMinutes);
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
                NewAvailSchedPopup newASPopup = new NewAvailSchedPopup(SelectedStaff, (DateTime)datePickerObj.SelectedDate);
                newASPopup.Show();
            }
        }

        private void GoToHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("Home.xaml", UriKind.Relative));
        }
    }
}
