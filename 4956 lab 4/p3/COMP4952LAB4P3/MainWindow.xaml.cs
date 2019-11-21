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

        /// <summary>
        /// An item for the producer to produce
        /// </summary>
        struct Cookie
        {
            public String cookieType {
                get { return "Chocolate Chip"; }
            }
        }

        private static Mutex mutex = new Mutex();
        private List<Cookie> plateOfCookies = new List<Cookie>(); //the cookie buffer
       
        int availableCookies = 0; //(FULL) slots are empty, full lists the most recent slot filled.
        int unavailableCookies = 10; //(EMPTY) maximum 10 cookies. 


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
        public void produceCookies()
        {
            System.Diagnostics.Debug.WriteLine("Baking...");

            do
            {
                System.Diagnostics.Debug.WriteLine("Producer: Mutex: " + mutex + ", avail: " + availableCookies + ", unavail:" + unavailableCookies);
                wait(ref unavailableCookies);
                mutex.WaitOne();

                //Produce cookies
                Thread.Sleep(1000);
                Cookie newCookie = new Cookie();
                //put the cookie on the plate.
                plateOfCookies.Add(newCookie);
                //System.Diagnostics.Debug.WriteLine("Produced cookie in spot: " + availableCookies);


                mutex.ReleaseMutex();
                signal(ref availableCookies);
                


            } while (true);


        }




        /// <summary>
        /// Consumes cookies at the given index. 
        /// </summary>
        /// <param name="cookieIndex"></param>
        public void consumeCookies()
        {
            System.Diagnostics.Debug.WriteLine("Consuming...");
            
            do
            {
                System.Diagnostics.Debug.WriteLine("Consumer: Mutex: " + mutex + ", avail: " + availableCookies + ", unavail:" + unavailableCookies);
                wait(ref availableCookies);
                mutex.WaitOne();

                //remove cookie from plate.
                plateOfCookies.RemoveAt(availableCookies);

                mutex.ReleaseMutex();
                signal(ref unavailableCookies);

                //consume cookie
                //System.Diagnostics.Debug.WriteLine("NOM NOM NOMING cookie in spot: " + availableCookies);
                Thread.Sleep(500);

            } while (true);

        }



        private void beginThreads()
        {
            //Start baking cookies
            Baker_Prod_Thread = new Thread(new ThreadStart(produceCookies));
            Baker_Prod_Thread.Start();


            //Start consuming cookies. 
            CookieMonster_Cons_Thread = new Thread(new ThreadStart(consumeCookies));
            CookieMonster_Cons_Thread.Start();
            


        }




        public MainWindow()
        {
            InitializeComponent();
            beginThreads();
        }



    }




}
