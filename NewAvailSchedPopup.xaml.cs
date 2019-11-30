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
        bool usingExistingAvailability = true;
        bool availabilitySelected = false; //start off with the user not having set an availability. 



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

            //disable scheduleing until an availability is chosen. 
            NewSchedETCB.IsEnabled = false;
            NewSchedSTCB.IsEnabled = false;


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
            NewSchedETCB.IsEnabled = false;
            NewSchedSTCB.IsEnabled = false;
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
            NewSchedETCB.IsEnabled = true;
            NewSchedSTCB.IsEnabled = true;

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
            NewSchedSTCB.IsEnabled = true;


            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewAvailSTCB.SelectedItem;
            DateTime chosenTime = chosenDisplayTimeItem.thisDateTime;


           
        }

        /// <summary>
        /// The user sets a new availability end time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewAvailETCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usingExistingAvailability = false;
            NewSchedETCB.IsEnabled = true;
            displayTimeItem chosenDisplayTimeItem = (displayTimeItem)NewAvailETCB.SelectedItem;
            DateTime chosenTime = chosenDisplayTimeItem.thisDateTime;

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

            newScheduleItem.BlockStartTime = chosenTime;
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

           
            newScheduleItem.BlockEndTime = chosenTime;

        }






        /// <summary>
        /// Checks if a given availability falls within an existing availability, and if it's valid. 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private bool uniqueAvailability(CurrentAvailabilities thisAvailability)
        {


            //check for existing availabilities that overlap with the start time of the new one
            int startTimeOverlap = db.CurrentAvailabilities
                                                             .Where(CA => CA.StaffId == thisAvailability.StaffId)
                                                             .Where(CA => thisAvailability.BlockStartTime >= CA.BlockStartTime)
                                                             .Where(CA => thisAvailability.BlockStartTime <= CA.BlockEndTime)
                                                             .Count();

            if (startTimeOverlap != 0)
            {
                return false;
            }
            else {

                //check fo existing availabilities that overlap iwth the end time of the new one. 
                int endTimeOverlap = db.CurrentAvailabilities
                                                                .Where(CA => CA.StaffId == thisAvailability.StaffId)
                                                                .Where(CA => thisAvailability.BlockEndTime >= CA.BlockStartTime)
                                                                .Where(CA => thisAvailability.BlockEndTime <= CA.BlockEndTime)
                                                                .Count();

                if (endTimeOverlap != 0)
                {
                    return false;
                }
                else
                {
                    //check for existing availabilities that occur within the start and end times of the new one. 
                    int newEncompassesOld = db.CurrentAvailabilities
                                                                        .Where(CA => CA.StaffId == thisAvailability.StaffId)
                                                                        .Where(CA => thisAvailability.BlockStartTime <= CA.BlockStartTime)
                                                                        .Where(CA => thisAvailability.BlockEndTime >= CA.BlockEndTime)
                                                                        .Count();

                    if(newEncompassesOld != 0)
                    {
                        return false;
                    }else
                    {
                        return true;
                    }

                }
      
            }

        }


        /// <summary>
        /// Returns true if the availability is a ordered correctly (start occures before end). 
        /// </summary>
        /// <param name="thisAvailability"></param>
        /// <returns></returns>
        private bool orderedAvailability(CurrentAvailabilities thisAvailability)
        {
            if(thisAvailability.BlockStartTime < thisAvailability.BlockEndTime)
            {
                return true;
            }else
            {
                return false;
            }
        }


        /// <summary>
        /// Cehcks if the availability is unique and the start and end times are in the right order.
        /// </summary>
        /// <param name="thisAvailability"></param>
        /// <returns></returns>
        private bool validAvailability(CurrentAvailabilities thisAvailability)
        {
            if(orderedAvailability(thisAvailability) && uniqueAvailability(thisAvailability))
            {
                return true;
            }else
            {
                return false;
            }
        }


        /// <summary>
        /// Returns true if the schedule is uniqe and doesn't overlap any others. 
        /// </summary>
        /// <param name="thisSchedule"></param>
        /// <returns></returns>
        private bool uniqueSchedule(CurrentSchedule thisSchedule)
        {

            //check for existing availabilities that overlap with the start time of the new one
            int startTimeOverlap = db.CurrentSchedule
                                    .Where(CA => thisSchedule.BlockStartTime >= CA.BlockStartTime)
                                    .Where(CA => thisSchedule.BlockStartTime <= CA.BlockEndTime)
                                    .Count();

            if (startTimeOverlap != 0)
            {
                return false;
            }
            else
            {

                //check fo existing availabilities that overlap iwth the end time of the new one. 
                int endTimeOverlap = db.CurrentSchedule     
                                    .Where(CA => thisSchedule.BlockEndTime >= CA.BlockStartTime)
                                    .Where(CA => thisSchedule.BlockEndTime <= CA.BlockEndTime)
                                    .Count();

                if (endTimeOverlap != 0)
                {
                    return false;
                }
                else
                {
                    //check for existing availabilities that occur within the start and end times of the new one. 
                    int newEncompassesOld = db.CurrentSchedule   
                                                .Where(CA => thisSchedule.BlockStartTime <= CA.BlockStartTime)
                                                .Where(CA => thisSchedule.BlockEndTime >= CA.BlockEndTime)
                                                .Count();

                    if (newEncompassesOld != 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }

            }


        }



        /// <summary>
        /// Ensure the schedule falls within it's availabilities time block
        /// </summary>
        /// <param name="currentSchedule"></param>
        /// <returns></returns>
        private bool validSchedule(CurrentSchedule currentSchedule)
        {

            bool withinAnAvailability = currentSchedule.BlockStartTime >= currentSchedule.Availability.BlockStartTime && currentSchedule.BlockEndTime <= currentSchedule.Availability.BlockEndTime;
            bool validSchedule = currentSchedule.BlockStartTime < currentSchedule.BlockEndTime;
            
            if (withinAnAvailability && validSchedule)
            {
                return true; //the schedule is valid
            }
            else
            {
                return false; //the schedule is not valid. 
            }

        }




        /// <summary>
        /// Saves the new schedule, if using an existing availability, or creates the new availability and saves the new schedule. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;
            string message = "";

            if (usingExistingAvailability)
            {
                if (newScheduleItem != null)
                {
                    Debug.WriteLine("using existing availability: " + newScheduleItem.AvailabilityId);
                    //get the mathcing availability

                    
                    
                    newScheduleItem.AvailabilityId = existingAvailability.Id;
                    newScheduleItem.Availability = existingAvailability;
                    


                    //verify the schedule.
                    if (validSchedule(newScheduleItem)){

                        if (uniqueSchedule(newScheduleItem))
                        {
                            db.CurrentSchedule.Add(newScheduleItem);
                            message = "Created a new schedule: \n" +
                                "" + newScheduleItem.BlockStartTime.ToShortDateString() + " " + newScheduleItem.BlockStartTime.ToShortTimeString() + "\n" +
                                "to\n" +
                                "" + newScheduleItem.BlockEndTime.ToShortDateString() + " " + newScheduleItem.BlockEndTime.ToShortTimeString();
                        }
                        else
                        {
                            message = "The employee is already scheduled within this time.\n Please check your entry again.";
                            error = true;
                        }

                    }
                    else
                    {
                        message = "The employee is not available at this time.\n Please check your entry again.";
                        error = true;
                    }

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

                // verify the new availability
                if (validAvailability(newAvailability)) {

                    db.CurrentAvailabilities.Add(newAvailability);
                    Debug.WriteLine("Made a new availability: " + newAvailability.BlockStartTime.ToString() + " - " + newAvailability.BlockEndTime.ToString());
                    db.SaveChanges(); //save the new availability so we can get it's ID.

                    //check if we are saving a new schedule too. 
                    if (newScheduleItem != null)
                    {

                        //validate the new schedule
                        if (validSchedule(newScheduleItem)) {


                            if (uniqueSchedule(newScheduleItem))
                            {

                                newScheduleItem.AvailabilityId = newAvailability.Id;
                                db.CurrentSchedule.Add(newScheduleItem);

                                message = "Created a new Availability and Schedule: \n" +
                                    "Available: \n" +
                                "" + newAvailability.BlockStartTime.ToShortDateString() + " " + newAvailability.BlockStartTime.ToShortTimeString() + "\n" +
                                "to\n" +
                                "" + newAvailability.BlockEndTime.ToShortDateString() + " " + newAvailability.BlockEndTime.ToShortTimeString() +
                                "and Scheduled: \n" +
                                "" + newScheduleItem.BlockStartTime.ToShortDateString() + " " + newScheduleItem.BlockStartTime.ToShortTimeString() + "\n" +
                                "to\n" +
                                "" + newScheduleItem.BlockEndTime.ToShortDateString() + " " + newScheduleItem.BlockEndTime.ToShortTimeString();
                            }
                            else
                            {
                                message = "The employee is already scheduled within this time.\n Please check your entry.";
                                error = true;
                            }

                        }

                    }else
                    {
                        message = "Created a new Availability: \n" +
                            "" + newAvailability.BlockStartTime.ToShortDateString() + " " + newAvailability.BlockStartTime.ToShortTimeString() + "\n" +
                            "to\n" +
                            "" + newAvailability.BlockEndTime.ToShortDateString() + " " + newAvailability.BlockEndTime.ToShortTimeString();

                        error = true;
                    }

                }
                else
                {
                    message = "New Availability overlaps with an existing availability. \nPlease adjust your dates or times, or delete the orignal availability.";
                    error = true;
                }

            }

            MessageBox.Show(message);

            if (!error)
            {
                db.SaveChanges();

                this.Close();
            }
            


        }



    }

    

}
