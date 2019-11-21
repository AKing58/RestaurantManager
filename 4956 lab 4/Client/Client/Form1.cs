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
        public FormClient()
        {
            InitializeComponent();
        }
        private NetworkStream output;             
        private BinaryWriter writer;     
        private BinaryReader reader;   
        private Thread readThread; 
        private string message = "";

                       
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

        
      
        
        public void ThreadClient()
        {
            TcpClient client;

            try
            {
                DisplayMessage("Attempting connection\r\n");

                client = new TcpClient();
                client.Connect("127.0.0.1", 50000);
                output = client.GetStream();
                       
                writer = new BinaryWriter(output);
                reader = new BinaryReader(output);

                DisplayMessage("\r\nGot I/O streams\r\n");
                EnableOrDisableInput(false); 
                
                do
                {
                    try
                    {
                                
                        message = reader.ReadString();
                        DisplayMessage("\r\n" + message);
                        
                    } 
                    catch (Exception)
                    {
                        
                        System.Environment.Exit(System.Environment.ExitCode);
                    } 
                } while (message != "SERVER: TERMINATE");

                
                writer.Close();
                reader.Close();
                output.Close();
                client.Close();

                Application.Exit();
            } 
            catch (Exception error)
            {
                
                MessageBox.Show(error.ToString(), "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(System.Environment.ExitCode);
            } 
        } 

       
        private void textBoxCommunication_KeyDown_1(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && textBoxCommunication.ReadOnly == false)
                {
                    writer.Write("CLIENT: " + textBoxCommunication.Text);
                    textBoxView.Text += "\r\nCLIENT: " + textBoxCommunication.Text;
                    textBoxCommunication.Clear();
                    if (textBoxView.Visible)
                    {
                        textBoxView.SelectionStart = textBoxView.TextLength;
                        textBoxView.ScrollToCaret();
                    }
                } 
            }
            catch (SocketException)
            {
                textBoxView.Text += "\nError writing object";
            } 
        } 

        private void FormClient_Load(object sender, EventArgs e)
        {
            readThread = new Thread(new ThreadStart(ThreadClient));
            readThread.Start();
        }

        private void FormClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        
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
