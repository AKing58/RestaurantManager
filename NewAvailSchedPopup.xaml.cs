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
        HashSet<displayTimeItem> Times = new HashSet<displayTimeItem>();
        public HashSet<currentAvailItem> listOfCurrentAvails = new HashSet<currentAvailItem>();
        private CurrentSchedule newScheduleItem; //the new schedule to save
        private CurrentAvailabilities newAvailability = new CurrentAvailabilities(); //the new availability to save, if a new one is being made. 
        private CurrentAvailabilities existingAvailability = new CurrentAvailabilities(); //the existing availability, if one is chosen. 
        bool usingExistingAvailability = false;


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

            thisStaff = chosenStaff;
            
            //prepare the new availability item, in case the user chooses to make a new one
            newAvailability.StaffId = thisStaff.Id;
            

            //set the date picker, setting the date picker will also load the availabilties. 
            ChosenDateDP.SelectedDate = thisDate;
            ChosenDateDP.DisplayDate = thisDate;

        }


        /// <summary>
        /// Loads data for the related date, called when the date on the datepicker is changed.
        /// </summary>
        /// <param name="thisDate"></param>
        private void loadForDate(DateTime thisDate)
        {
            newAvailability.BlockStartTime = thisDate;
            newAvailability.BlockEndTime = new DateTime(thisDate.Year, thisDate.Month, thisDate.Day, 23, 59, 00); //set the default end time to 11:59PM

            Times = createTimes(thisDate);
            NewAvailETCB.ItemsSource = Times;
            NewAvailSTCB.ItemsSource = Times;
            NewSchedSTCB.ItemsSource = Times;
            NewSchedETCB.ItemsSource = Times;

            listOfCurrentAvails = getCurrentAvails(thisDate);
            ExistingAvailsCB.ItemsSource = listOfCurrentAvails;
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

            try
            {
               
                HashSet<CurrentAvailabilities> currentAvailabilities = db.CurrentAvailabilities
                                                                        .Where(CA => CA.StaffId == thisStaff.Id)
                                                                        .Where(CA => CA.BlockStartTime.Date == thisDate.Date)
                                                                        .ToHashSet();



                foreach (CurrentAvailabilities thisAvailability in currentAvailabilities)
                {
                    currentAvailItem thisAvailItem = new currentAvailItem(thisAvailability);

                    theseAvails.Add(thisAvailItem);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting current availabilities: " + e);
            }
            

            return theseAvails;
        }


        /// <summary>
        /// The user chose a different date, reload data. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChosenDateDP_dateChanged(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Changed date to: " + ChosenDateDP.SelectedDate.ToString());
            loadForDate((DateTime)ChosenDateDP.SelectedDate);
        }



        //MARK: SETTING AN EXISTING AVAILABILITY

        /// <summary>
        /// The user selectes an existing availability
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExistingAvailsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = true;

            currentAvailItem thisExistingAvailability = (currentAvailItem)ExistingAvailsCB.SelectedItem;
            existingAvailability = thisExistingAvailability.thisAvailability;

            if(newScheduleItem != null)
            {
                newScheduleItem.AvailabilityId = existingAvailability.Id;
                newScheduleItem.BlockStartTime = existingAvailability.BlockStartTime;
                newScheduleItem.BlockEndTime = existingAvailability.BlockEndTime;
            }
            
        }






        //MARK: SETTING A NEW AVAILABILITY

        /// <summary>
        /// The user sets a new AVailability start time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewAvailSTCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = false;
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewAvailSTCB.SelectedItem;
            DateTime chosenTime = chosenDisplayTimeItem.thisDateTime;


            //validate selection
            if(chosenTime < newAvailability.BlockEndTime)
            {
                newAvailability.BlockStartTime = chosenTime;
            }
            else
            {
                NewAvailSTCB.SelectedIndex = 0;
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
            DateTime chosenTime = chosenDisplayTimeItem.thisDateTime;


            //validate selection
            if (chosenTime > newAvailability.BlockStartTime)
            {
                newAvailability.BlockEndTime = chosenTime;
            }
            else
            {
                NewAvailETCB.SelectedIndex = 0; //reset. 
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
            DateTime chosenTime = chosenDisplayTimeItem.thisDateTime;
            Debug.WriteLine("New start time chosen: " + chosenTime.ToShortTimeString());

            //if a new schedule item doesn't exist, make it.
            if (newScheduleItem == null)
            {
                newScheduleItem = new CurrentSchedule();
                newScheduleItem.BlockEndTime = new DateTime(newScheduleItem.BlockStartTime.Year, newScheduleItem.BlockStartTime.Month, newScheduleItem.BlockStartTime.Day, 23, 59, 0);
            }
            else
            {
                //make sure the schedule end time is after it's start time.
                if (newScheduleItem.BlockEndTime <= newScheduleItem.BlockStartTime)
                {
                    newScheduleItem.BlockEndTime = new DateTime(newScheduleItem.BlockStartTime.Year, newScheduleItem.BlockStartTime.Month, newScheduleItem.BlockStartTime.Day, 23, 59, 0);
                }
            }

            CurrentAvailabilities thisAvailability;



            if (usingExistingAvailability)
            {
                thisAvailability = existingAvailability;
            }
            else
            {
                thisAvailability = newAvailability;
            }


            //the chosen start time must be after the availability start time, before the availability end time, and before the schedule end time. 
            if (chosenTime >= thisAvailability.BlockStartTime && chosenTime < thisAvailability.BlockEndTime && chosenTime < newScheduleItem.BlockEndTime)
            {
                Debug.WriteLine("in bounds");
                newScheduleItem.BlockStartTime = chosenTime;

            } else
            {
               
                Debug.WriteLine("out of bounds.");
                NewSchedSTCB.SelectedIndex = 0;
            }




            
        }

        /// <summary>
        /// the user sets a new schedule end time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSchedETCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewSchedETCB.SelectedItem;
            DateTime chosenTime = chosenDisplayTimeItem.thisDateTime;


            //if a new schedule item doesn't exist, make it.
            if (newScheduleItem == null)
            {
                newScheduleItem = new CurrentSchedule();
                newScheduleItem.BlockEndTime = new DateTime(newScheduleItem.BlockStartTime.Year, newScheduleItem.BlockStartTime.Month, newScheduleItem.BlockStartTime.Day, 23, 59, 0);

            }else
            {
                //make sure the schedule end time is after it's start time. 
                if (newScheduleItem.BlockEndTime <= newScheduleItem.BlockStartTime)
                {
                    newScheduleItem.BlockEndTime = new DateTime(newScheduleItem.BlockStartTime.Year, newScheduleItem.BlockStartTime.Month, newScheduleItem.BlockStartTime.Day, 23, 59, 0);
                }
            }

            CurrentAvailabilities thisAvailability;


            if (usingExistingAvailability)
            {
                thisAvailability = existingAvailability;
            }
            else
            {
                thisAvailability = newAvailability;
            }


            //the chosen end time must be before the availability end time, after the schedule start time, after the availability start time, 
            if (chosenTime <= thisAvailability.BlockEndTime && chosenTime > newScheduleItem.BlockStartTime && chosenTime > thisAvailability.BlockStartTime)
            {
                newScheduleItem.BlockEndTime = chosenTime;
            }
            else
            {
                NewSchedETCB.SelectedIndex = Times.Count-1;
            }
        }






        /// <summary>
        /// Checks if a given availability falls within an existing availability on the same day. 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private bool validUniqueAvailability(CurrentAvailabilities thisAvailability)
        {
           
            //check for avaialbilities for this staff member that encompass the given availability
            HashSet<CurrentAvailabilities> existingAvailabilities = db.CurrentAvailabilities
                                                            .Where(CA=>CA.StaffId == thisAvailability.StaffId)
                                                            .Where(CA => CA.BlockStartTime <= thisAvailability.BlockStartTime)
                                                            .Where(CA => CA.BlockEndTime >= thisAvailability.BlockEndTime)
                                                            .ToHashSet();

            if(existingAvailabilities.Count > 0)
            {
                return false; //the availability is NOT unique. 
            }
            else
            {
                return true; //the availability is unique.
            }
        }

        /// <summary>
        /// Ensure the schedule falls within it's availabilities time block
        /// </summary>
        /// <param name="currentSchedule"></param>
        /// <returns></returns>
        private bool validSchedule(CurrentSchedule currentSchedule)
        {

            if(currentSchedule.BlockStartTime >= currentSchedule.Availability.BlockStartTime && currentSchedule.BlockEndTime <= currentSchedule.Availability.BlockEndTime)
            {
                return true; //the schedule DOES fall within it's availability
            }
            else
            {
                return false; //the schedule does NOT fall within it's availability. 
            }



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
                if (newScheduleItem != null)
                {
                    Debug.WriteLine("using existing availability: " + newScheduleItem.AvailabilityId);
                    newScheduleItem.AvailabilityId = existingAvailability.Id;
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



    }

    

}
