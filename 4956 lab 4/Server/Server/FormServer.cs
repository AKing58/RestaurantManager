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
        public FormServer()
        {
            InitializeComponent();
        }

        private Socket connection;
        private Thread readThread;
        private NetworkStream socketStream;
        private BinaryWriter writer;
        private BinaryReader reader;


        private delegate void DelegateGUI(string message);


        private void DisplayMessage(string message)
        {

            if (textBoxView.InvokeRequired)
            {

                Invoke(new DelegateGUI(DisplayMessage),
                   new object[] { message });
            }
            else
                textBoxView.AppendText(message);
        }

        private delegate void EnableOrDisableInputDelegate(bool value);


        private void EnableOrDisableInput(bool value)
        {
            if (textBoxCommunication.InvokeRequired)
            {

                Invoke(new EnableOrDisableInputDelegate(EnableOrDisableInput),
                   new object[] { value });
            }
            else
                textBoxCommunication.ReadOnly = value;
        }


        private void textBoxCommunication_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                if (e.KeyCode == Keys.Enter && textBoxCommunication.ReadOnly == false)
                {
                    writer.Write("SERVER: " + textBoxCommunication.Text);
                    textBoxView.Text += "\r\nSERVER: " + textBoxCommunication.Text;


                    textBoxCommunication.Clear();
                    if (textBoxView.Visible)
                    {
                        textBoxView.SelectionStart = textBoxView.TextLength;
                        textBoxView.ScrollToCaret();
                    }

                    if (textBoxCommunication.Text == "TERMINATE")
                        connection.Close();
                }
            }
            catch (SocketException)
            {
                textBoxView.Text += "\nError when writing";
            }
        }


        public void ThreadServer()
        {
            TcpListener listener;
            int counter = 1;


            try
            {

                System.Net.IPAddress local = System.Net.IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(local, 50000);

                listener.Start();

                while (true)
                {
                    DisplayMessage("Server is ready\r\n");


                    connection = listener.AcceptSocket();


                    socketStream = new NetworkStream(connection);


                    writer = new BinaryWriter(socketStream);
                    reader = new BinaryReader(socketStream);

                    DisplayMessage("Connection " + counter + " received.\r\n");


                    writer.Write("SERVER: Connection successful");

                    EnableOrDisableInput(false);

                    string theReply = "";

                    do
                    {
                        try
                        {

                            theReply = reader.ReadString();


                            DisplayMessage("\r\n" + theReply);
                           
                        }
                        catch (Exception)
                        {

                            break;
                        }
                    } while (theReply != "CLIENT: TERMINATE" &&
                       connection.Connected);

                    DisplayMessage("\r\nUser terminated connection\r\n");


                    writer.Close();
                    reader.Close();
                    socketStream.Close();
                    connection.Close();

                    EnableOrDisableInput(true);
                    counter++;
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            readThread = new Thread(new ThreadStart(ThreadServer));
            readThread.Start();
        }

        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }
        
        private void textBoxView_VisibleChanged(object sender, EventArgs e)
        {
            if (textBoxView.Visible)
            {
                textBoxView.SelectionStart = textBoxView.TextLength;
                textBoxView.ScrollToCaret();
            }
        }
        
    
    }
}
