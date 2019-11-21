using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

namespace COMP4952LAB4P3
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Thread Baker_Prod_Thread; //the producer thread, in this case, a baker baking cakes. 
        private Thread CookieMonster_Cons_Thread; //the consumer thread, in this case, cookie monster.
        Random oven = new Random(); //oven, for getting random bake times becaue different cookies take different times to bake. 

        /// <summary>
        /// An item for the producer to produce
        /// </summary>
        struct Cookie
        {

            //generates a random consumption time. 
            public int consumptionTime { 
                get {
                    Random rnd = new Random();
                    int time = rnd.Next(1000, 2000);
                    return time;
                } 
            } 
        }

        private static Mutex mutex = new Mutex();
        private List<Cookie> plateOfCookies = new List<Cookie>(); //the cookie buffer
       
        int availableCookies = 0; //(FULL) slots are empty, full lists the most recent slot filled.
        int freeSpotsOnPlate = 10; //(EMPTY) maximum 10 cookies. 


        /// <summary>
        /// Decrement a semaphore
        /// </summary>
        /// <param name="S"></param>
        public void wait(ref int S)
        {
            

            while (S <= 0) {
                //do nothing
               
            }

            S--;
            
        }

        /// <summary>
        /// Increment a semaphore 
        /// </summary>
        /// <param name="S"></param>
        public void signal(ref int S)
        {
            S++;
        }

        /// <summary>
        /// Produces cookies & puts them in the next available slot in the buffer. 
        /// </summary>
        public void producer_Baker()
        {
            System.Diagnostics.Debug.WriteLine("Preparing Oven and Ingredients and Baking...");

            do
            {
                //System.Diagnostics.Debug.WriteLine("Producer: Mutex: " + mutex + ", avail: " + availableCookies + ", unavail:" + unavailableCookies);
                wait(ref freeSpotsOnPlate);
                mutex.WaitOne();


                //Produce cookies
                Cookie newCookie = produceCookie();
                //put the cookie on the plate.
                plateOfCookies.Add(newCookie);
                int cookiePlace = plateOfCookies.Count - 1;
                System.Diagnostics.Debug.WriteLine("New cookie in spot: " + cookiePlace);



                mutex.ReleaseMutex();
                signal(ref availableCookies);
                


            } while (true);


        }




        /// <summary>
        /// Consumes cookies at the given index. 
        /// </summary>
        /// <param name="cookieIndex"></param>
        public void consumer_CookieMonster()
        {
            System.Diagnostics.Debug.WriteLine("Mentally preparing for cookies...");
            
            do
            {
                
                wait(ref availableCookies);
                mutex.WaitOne();


                //remove last cookie from plate.
                int cookieIndex = plateOfCookies.Count -1;
                Cookie thisCookie = plateOfCookies.ElementAt(cookieIndex);
                plateOfCookies.RemoveAt(cookieIndex);


                mutex.ReleaseMutex();
                signal(ref freeSpotsOnPlate);


                //consume cookie
                consumeCookie(thisCookie, cookieIndex);
               
               

            } while (true);

        }



        private void beginThreads()
        {
            //Start baking cookies
            Baker_Prod_Thread = new Thread(new ThreadStart(producer_Baker));
            Baker_Prod_Thread.Start();


            //Start consuming cookies. 
            CookieMonster_Cons_Thread = new Thread(new ThreadStart(consumer_CookieMonster));
            CookieMonster_Cons_Thread.Start();
           
        }


        /// <summary>
        /// Consumes the cookie
        /// </summary>
        /// <param name="thisCookie"></param>
        /// <param name="thisIndex"></param>
        private void consumeCookie(Cookie thisCookie, int thisIndex)
        {
            System.Diagnostics.Debug.WriteLine("NOM NOM NOMING cookie: " + thisIndex);
            Thread.Sleep(thisCookie.consumptionTime);
        }



        /// <summary>
        /// Produces a new cookie. 
        /// </summary>
        /// <returns></returns>
        private Cookie produceCookie()
        {
            Cookie newCookie = new Cookie();
            Thread.Sleep(oven.Next(1000, 2000));
            return newCookie;

        }


        public MainWindow()
        {
            InitializeComponent();
            beginThreads();
        }



    }




}
