using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace Server
{
    public partial class FormServer : Form
    {


        /// <summary>
        /// Eamonn: Initializes components. 
        /// </summary>
        public FormServer()
        {
            InitializeComponent();
        }


        private Socket connection; //The socket that the server will output to. 
        private Thread readThread; //Eamonn: the thread which reading will occur on. 
        private NetworkStream socketStream; //Eamonn: the stream for the socket connection
        private BinaryWriter writer; //Eamonn: used to write to the network stream
        private BinaryReader reader; //EAmonn: used to read from the network stream


        private delegate void DelegateGUI(string message); //Eamonn: A delegate to pass as a parameter


        /// <summary>
        /// Eamonn: Displays the message in the textbox (grey box with status messages)
        /// </summary>
        private void DisplayMessage(string message)
        {

            // An invoke is required if we want to access a control but aren't on the thread that made it. 
            if (textBoxView.InvokeRequired)
            {

                //Execute the delegate on the thread that owns the controls underlying window handle (i.e the main thread)
                //this DisplayMessage function behaves recursively, calling the DisplayMessage function on it's own thread, which 
                //means the invoke won't be required and the 'else' statement will execute, updating the contents of the textbox. 
                Invoke(new DelegateGUI(DisplayMessage),
                   new object[] { message });
            }
            else
                textBoxView.AppendText(message);//Otherwise, simply append the message to the contents of the textbox. 
        }

        private delegate void EnableOrDisableInputDelegate(bool value); //Eamonn: A delegate for the EnableOrDisableInput function.


        /// <summary>
        /// Eamonn: makes the communication box (white box at top) readonly or not. 
        /// </summary>
        /// <param name="value"></param>
        private void EnableOrDisableInput(bool value)
        {

            //An invoke is required if we want to access a control but aren't on the thread that made it. 
            if (textBoxCommunication.InvokeRequired)
            {

                 // Execute the delegate on the thread that wons the control.
                 //will call the EnableOrDisableInput function recursively, on its creating thread, and set it as readonly or not. 
                 Invoke(new EnableOrDisableInputDelegate(EnableOrDisableInput),
                   new object[] { value });
            }
            else
                textBoxCommunication.ReadOnly = value;//prevent or allow input to the textboxCommunication control. 
        }


        /// <summary>
        /// Eamonn: Handles the user typing in the connection box. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCommunication_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                //if the user presses the enter key and the box isn't readonly
                if (e.KeyCode == Keys.Enter && textBoxCommunication.ReadOnly == false)
                {
                    
                    //write to the TCP connection (i.e, send to the Clients)
                    writer.Write("SERVER: " + textBoxCommunication.Text);

                    //append to the contents of the status messages textbox:
                    textBoxView.Text += "\r\nSERVER: " + textBoxCommunication.Text;

                    //Erase the contents of communication box (so it can be reused):
                    textBoxCommunication.Clear();

                    //if the textbox is visible:
                    if (textBoxView.Visible)
                    {
                        //Scroll to the last entered client message in the status messages textbox. 
                        textBoxView.SelectionStart = textBoxView.TextLength;
                        textBoxView.ScrollToCaret();
                    }

                    //If the user types in "TERMINATE", close the connection. 
                    if (textBoxCommunication.Text == "TERMINATE")
                        connection.Close();
                }
            }
            catch (SocketException)
            {
                //catch any errors, and update the status messages box:
                textBoxView.Text += "\nError when writing";
            }
        }


        /// <summary>
        /// Eamonn: The main function of this program. Sets up and handles a TCP socket. 
        /// </summary>
        public void ThreadServer()
        {

            TcpListener listener; //prepare a tcp listener. 
            int counter = 1; //prepare a counter. 


            try
            {
                //set the local IP address of the server to localhost. 
                System.Net.IPAddress local = System.Net.IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(local, 50000); //initialize a TCP Listener on localhost, port 50000. 

                listener.Start();//start the listener. 

                //indefinitely.
                while (true)
                {
                    DisplayMessage("Server is ready\r\n"); //display in the status box. 


                    connection = listener.AcceptSocket(); //accept any pending connection requests. If one client disconnects and another is waiting, it will connect.  


                    socketStream = new NetworkStream(connection); //initialize a read/write stream to read/write data from the TCP connection.


                    writer = new BinaryWriter(socketStream); //connect the writer to the socket stream
                    reader = new BinaryReader(socketStream); //connect the reader to the socket stream. 

                    DisplayMessage("Connection " + counter + " received.\r\n"); //display the number of messages received. 


                    writer.Write("SERVER: Connection successful"); //write to the client that the connection was successful. 

                    EnableOrDisableInput(false); //allow the user to input to the communication textbox. 

                    string theReply = ""; //set the reply message to an empty string. 

                    do
                    {
                        try
                        {

                            theReply = reader.ReadString(); //read any incoming messages from clients. 


                            DisplayMessage("\r\n" + theReply); //display incoming messages in the textbox. 
                           
                        }
                        catch (Exception)
                        {

                            break;
                        }
                    } while (theReply != "CLIENT: TERMINATE" &&
                       connection.Connected); //continue as long as the client hasn't sent a "TERMINATE" message, and the connection is currently connected. 

                    DisplayMessage("\r\nUser terminated connection\r\n"); //display that the connection was terminated. 


                    //close all connections
                    writer.Close();
                    reader.Close();
                    socketStream.Close();
                    connection.Close();

                    EnableOrDisableInput(true); //disable input from the user. 
                    counter++; //increment the counter for the next connection.  
                }
            }
            catch (Exception error)
            {
                //display any errors in a message box. 
                MessageBox.Show(error.ToString());
            }
        }


        /// <summary>
        /// Eamonn: Called when the program is loaded. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormServer_Load(object sender, EventArgs e)
        {
            readThread = new Thread(new ThreadStart(ThreadServer)); //initialize a new thread with the main function on it. 
            readThread.Start(); //start the thread.
        }


        /// <summary>
        /// Eamonn: Called when the program is closing. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode); //send an exit code to the os. 
        }
        
        /// <summary>
        /// Eamonn: Called if the textbox visibility is changd. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxView_VisibleChanged(object sender, EventArgs e)
        {
            if (textBoxView.Visible)
            {
                //scrolls to the bottom of the status box. 
                textBoxView.SelectionStart = textBoxView.TextLength;
                textBoxView.ScrollToCaret();
            }
        }
        
    
    }
}
