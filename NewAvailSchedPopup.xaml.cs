using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using COMP4952.Models;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for NewAvailSchedPopup.xaml
    /// </summary>
    public partial class NewAvailSchedPopup : Window
    {
        private COMP4952PROJECTContext db;
        public Staff thisStaff;
        public HashSet<currentAvailItem> listOfCurrentAvails = new HashSet<currentAvailItem>();
        private CurrentSchedule newScheduleItem = new CurrentSchedule(); //the new schedule to save
        private CurrentAvailabilities newAvailability = new CurrentAvailabilities(); //the new availability to save, if a new one is being made. 
        private CurrentAvailabilities existingAvailability = new CurrentAvailabilities(); //the existing availability, if one is chosen. 
        bool usingExistingAvailability = true;


        /// <summary>
        /// For binding to the list of exsiting availabilities
        /// </summary>
        public struct currentAvailItem
        {
            public string displayString { get; set; }
            public CurrentAvailabilities thisAvailability;

            public currentAvailItem(CurrentAvailabilities availability)
            {
                thisAvailability = availability;
                displayString = thisAvailability.BlockStartTime.ToString() + " - " + thisAvailability.BlockEndTime.ToString();
                displayString = "Time!";
            }
        }

        /// <summary>
        /// For displaying time options in the start and end time dropdown menus. 
        /// </summary>
        public struct displayTimeItem
        {
            public string displayString { get; set; }
            public DateTime thisDateTime;

            public displayTimeItem(DateTime thisTime)
            {
                thisDateTime = thisTime;
                displayString = thisDateTime.ToShortTimeString();
            }
        }


        public NewAvailSchedPopup(Staff chosenStaff, DateTime thisDate)
        {
            InitializeComponent();
            db = new Models.COMP4952PROJECTContext();

            //set the date picker. 
            ChosenDateDP.SelectedDate = thisDate;
            ChosenDateDP.DisplayDate = thisDate;
            thisStaff = chosenStaff;
            
            //prepare the new availability item, in case the user chooses to make a new one
            newAvailability.Staff = thisStaff;
            newAvailability.Date = thisDate;


            HashSet<displayTimeItem> Times = createTimes(thisDate);
            NewAvailETCB.ItemsSource = Times;
            NewAvailSTCB.ItemsSource = Times;
            NewSchedSTCB.ItemsSource = Times;
            NewSchedETCB.ItemsSource = Times;

            listOfCurrentAvails = getCurrentAvails(thisDate);
            ExitingAvailsCB.ItemsSource = listOfCurrentAvails;

        }

        /// <summary>
        /// Returns midnight on the given day. 
        /// </summary>
        /// <param name="thisDate"></param>
        /// <returns></returns>
        private DateTime convertDateToMidnight(DateTime thisDate)
        {
            DateTime thisMidnight = new DateTime(thisDate.Year, thisDate.Month, thisDate.Day, 0, 0, 0);
            return thisMidnight;
        }


        /// <summary>
        /// Creates a list of times in 5 minute intervals for the chosen date. 
        /// </summary>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        private HashSet<displayTimeItem> createTimes(DateTime selectedDate)
        {
            HashSet<displayTimeItem> theseTimes = new HashSet<displayTimeItem>();
            
            int thisYear = selectedDate.Year;
            int thisMonth = selectedDate.Month;
            int thisDay = selectedDate.Day;


            for (int i= 0; i < 24*60; i+=5)
            {
                DateTime thisTime = new DateTime(thisYear, thisMonth, thisDay, i / 60, i % 60, 0);

                displayTimeItem thisTimeItem = new displayTimeItem(thisTime);


                theseTimes.Add(thisTimeItem);
            }


            return theseTimes;
        }


        /// <summary>
        /// Creates a hashset of the staff members current availabilities for the chosen day. 
        /// </summary>
        /// <param name="thisDate"></param>
        /// <returns></returns>
        private HashSet<currentAvailItem> getCurrentAvails(DateTime thisDate)
        {
            HashSet<currentAvailItem> theseAvails = new HashSet<currentAvailItem>();

            HashSet<CurrentAvailabilities> currentAvailabilities = db.CurrentAvailabilities
                                                                        .Where(CA => CA.Date == thisDate)
                                                                        .Where(CA => CA.StaffId == thisStaff.Id)
                                                                        .ToHashSet();

            foreach(CurrentAvailabilities thisAvailability in currentAvailabilities)
            {
                currentAvailItem thisAvailItem = new currentAvailItem(thisAvailability);

                theseAvails.Add(thisAvailItem);
            }

            return theseAvails;
        }

        
        /// <summary>
        /// Saves the new schedule, if using an existing availability, or creates the new availability and saves the new schedule. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (usingExistingAvailability)
            {
                db.CurrentSchedule.Add(newScheduleItem);
            }
            else
            {
                db.CurrentAvailabilities.Add(newAvailability);
                db.CurrentSchedule.Add(newScheduleItem);
            }

            db.SaveChanges();


        }


        /// <summary>
        /// The user selectes an existing availability
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitingAvailsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = true;

            currentAvailItem thisExistingAvailability = (currentAvailItem)ExitingAvailsCB.SelectedItem;
            existingAvailability = thisExistingAvailability.thisAvailability;
            newScheduleItem.Availability = existingAvailability;
        }


        /// <summary>
        /// The user sets a new AVailability start time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewAvailSTCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = false;
            DateTime chosenBlock = (DateTime)NewAvailSTCB.SelectedItem;
            newAvailability.BlockStartTime = chosenBlock.TimeOfDay;
            newScheduleItem.Availability = newAvailability;
        }

        /// <summary>
        /// The user sets a new availability end time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewAvailETCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = false;
            DateTime chosenBlock = (DateTime)NewAvailETCB.SelectedItem;
            newAvailability.BlockEndTime = chosenBlock.TimeOfDay;
            newScheduleItem.Availability = newAvailability;
        }


        /// <summary>
        /// the user sets a new schedule start time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSchedSTCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime chosenBlock = (DateTime)NewSchedSTCB.SelectedItem;
            newScheduleItem.BlockStartTime = chosenBlock.TimeOfDay;
        }

        /// <summary>
        /// the user sets a new schedule end time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSchedETCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime chosenBlock = (DateTime)NewSchedETCB.SelectedItem;
            newScheduleItem.BlockEndTime = chosenBlock.TimeOfDay;
        }
    }
}
