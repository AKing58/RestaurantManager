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

namespace Client
{
    public partial class FormClient : Form
    {


        /// <summary>
        /// Eamonn: Initializes thew view. 
        /// </summary>
        public FormClient()
        {
            InitializeComponent();
        }



        private NetworkStream output;  //Eamonn: the output stream for the socket connection.            
        private BinaryWriter writer;   //Eamonn: used to write to the network stream.   
        private BinaryReader reader;   //Eamonn: used to read from the network stream. 
        private Thread readThread;  //Eamonn: The thread which reading will occur on. 
        private string message = ""; //Eamonn: the message from the server. 

                       

        private delegate void DelegateGUI(string message); //Eamonn: set up a delegate to pass as a parameter

        
        /// <summary>
        /// Eamonn: Displays the message in the textbox (grey box with status messages)
        /// </summary>
        /// <param name="message">The message to display</param>
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
                textBoxView.AppendText(message); //Otherwise, simply append the message to the contents of the textbox. 
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
                //Execute the delegate on the thread that wons the control. 
                //will call the EnableOrDisableInput function recursively, on it's creating thread, and set it as readonly or not. 
                Invoke(new EnableOrDisableInputDelegate(EnableOrDisableInput),
                   new object[] { value });
            } 
            else 
                textBoxCommunication.ReadOnly = value; //prevent or allow input to the textboxCommunication control. 
        } 

        
      
        /// <summary>
        /// Eamonn sets up and handles a TCPClient stream. 
        /// The main function of this program.
        /// </summary>
        public void ThreadClient()
        {
            TcpClient client; //a new TCP client. 

            try
            {
                DisplayMessage("Attempting connection\r\n"); //Eamonn: updates the textBox with the text. 

                client = new TcpClient(); //instantiates a new TCP client
                client.Connect("127.0.0.1", 50000); // attempts to connect to local host on port 50000
                output = client.GetStream(); //instantiates the network stream to 'output' to start receiving data. 
                       
                writer = new BinaryWriter(output); //for writing to the stream
                reader = new BinaryReader(output); //for reading from the stream

                DisplayMessage("\r\nGot I/O streams\r\n");
                EnableOrDisableInput(false); //sets the textBoxCommunication to NOT readonly. 
                

                //As long as the Server doesn't end the connection...
                do
                {
                    try
                    {
                                
                        message = reader.ReadString(); //read from the network output
                        DisplayMessage("\r\n" + message);//display the data from the network output. 
                        
                    } 
                    catch (Exception)
                    {
                        //something went wrong, terminate the process and send an appropriate exit code to the operating system. 
                        System.Environment.Exit(System.Environment.ExitCode);
                    } 
                } while (message != "SERVER: TERMINATE");

                
                //Close everything to prevent future issues. 
                writer.Close(); //close the connection of the writer
                reader.Close(); //close the connection of the reader
                output.Close(); //close the connection to the network stream
                client.Close(); //Dispose of the CP instance and close the TCP connection. 

                Application.Exit(); //exit the application. 
            } 
            catch (Exception error)
            {
                //somethign went wrong during the course of the code. 
                //Create a popup displaying the error & terminate the process with an appropriate exit code. 
                MessageBox.Show(error.ToString(), "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(System.Environment.ExitCode);
            } 
        } 

       
        /// <summary>
        /// Eamonn: Handles the user typing in the connection box. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCommunication_KeyDown_1(object sender, KeyEventArgs e)
        {
            try
            {
                //if the user presses the enter key and the box isn't readonly
                if (e.KeyCode == Keys.Enter && textBoxCommunication.ReadOnly == false)
                {
                    //write to the TCP connection (i.e, send to the Server)
                    writer.Write("CLIENT: " + textBoxCommunication.Text);

                    //append to the contents of the status messages textbox:
                    textBoxView.Text += "\r\nCLIENT: " + textBoxCommunication.Text;
                    
                    //Erase the contents of communication box (so it can be reused):
                    textBoxCommunication.Clear();

                    //if the textbox is visible:
                    if (textBoxView.Visible)
                    {
                        //Scroll to the last entered client message in the status messages textbox. 
                        textBoxView.SelectionStart = textBoxView.TextLength;
                        textBoxView.ScrollToCaret();
                    }
                } 
            }
            catch (SocketException)
            {
                //catch any errors, and update the status messages box:
                textBoxView.Text += "\nError writing object";
            } 
        } 


        /// <summary>
        /// Eamonn: Called when the form is loaded and ready. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormClient_Load(object sender, EventArgs e)
        {
            //Create a new thread to run the threadClient method (the main method)
            readThread = new Thread(new ThreadStart(ThreadClient));
            readThread.Start(); //start the new thread. 
        }


        /// <summary>
        /// Eamonn: Called when the form is being closed. 
        /// Send an exit code to the OS. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        
        /// <summary>
        /// Eamonn: Called when the status textbox's visibility is canged. 
        /// Scrolls to the bottom of the contents. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxView_VisibleChanged_1(object sender, EventArgs e)
        {
            if (textBoxView.Visible)
            {
                textBoxView.SelectionStart = textBoxView.TextLength;
                textBoxView.ScrollToCaret();
            }

        }
        
    }
}
