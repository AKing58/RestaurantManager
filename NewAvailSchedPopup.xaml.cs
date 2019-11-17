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
using System.Diagnostics;

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
        private CurrentSchedule newScheduleItem; //the new schedule to save
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
            newAvailability.StaffId = thisStaff.Id;
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
                if(newScheduleItem != null)
                {
                    Debug.WriteLine("using existing availability: " + newScheduleItem.AvailabilityId);
                    db.CurrentSchedule.Add(newScheduleItem);
                }
                else
                {
                    //chose an existing availability, but didn't make a schedule.
                    Debug.WriteLine("Didn't set a schedule.");
                    this.Close();
                }
               
            }
            else
            {
                //save the new availability first
                db.CurrentAvailabilities.Add(newAvailability);
                Debug.WriteLine("Made a new availability: " + newAvailability.BlockStartTime.ToString() + " - " + newAvailability.BlockEndTime.ToString());
                db.SaveChanges(); //save the new availability so we can get it's ID.

                //check if we are saving a new schedule too. 
                if (newScheduleItem != null)
                {

                    newScheduleItem.AvailabilityId = newAvailability.Id;
                    db.CurrentSchedule.Add(newScheduleItem);
                    Debug.WriteLine("Made a new Schedule: " + newScheduleItem.BlockStartTime.ToString() + " - " + newScheduleItem.BlockEndTime.ToString());
                }
                
            }

            db.SaveChanges();

            Debug.WriteLine("Saved new availability/schedule.");
            

            this.Close();


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

            if(newScheduleItem != null)
            {
                newScheduleItem.AvailabilityId = existingAvailability.Id;
            }
            
        }


        /// <summary>
        /// The user sets a new AVailability start time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewAvailSTCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = false;
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewAvailSTCB.SelectedItem;
            newAvailability.BlockStartTime = chosenDisplayTimeItem.thisDateTime.TimeOfDay;
            
            if(newScheduleItem != null)
            {
                newScheduleItem.AvailabilityId = newAvailability.Id;
            }
           
        }

        /// <summary>
        /// The user sets a new availability end time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewAvailETCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = false;
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewAvailETCB.SelectedItem;
            newAvailability.BlockEndTime = chosenDisplayTimeItem.thisDateTime.TimeOfDay;
            
            if(newScheduleItem != null)
            {
                newScheduleItem.AvailabilityId = newAvailability.Id;
            }
            
        }


        /// <summary>
        /// the user sets a new schedule start time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSchedSTCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewSchedSTCB.SelectedItem;
            
            //if a new schedule item doesn't exist, make it.
            if (newScheduleItem == null)
            {
                newScheduleItem = new CurrentSchedule();
            }

            newScheduleItem.BlockStartTime = chosenDisplayTimeItem.thisDateTime.TimeOfDay;
        }

        /// <summary>
        /// the user sets a new schedule end time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSchedETCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewSchedETCB.SelectedItem;

            //if a new schedule item doesn't exist, make it. 
            if (newScheduleItem == null)
            {
                newScheduleItem = new CurrentSchedule();
            }
            newScheduleItem.BlockEndTime = chosenDisplayTimeItem.thisDateTime.TimeOfDay;
        }
    }

    

}
