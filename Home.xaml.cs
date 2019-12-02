using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Linq;
using COMP4952.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace COMP4952
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// Adam King
    /// </summary>
    public partial class Home : Page
    {
        COMP4952PROJECTContext db;

        double OffsetX;
        double OffsetY;
        /// <summary>
        /// Home page constructor
        /// </summary>
        public Home()
        {
            initializeDBConnection();
            InitializeComponent();
            LoadWalls();
            LoadTableInfos();
            Application.Current.MainWindow.KeyDown += new KeyEventHandler(FloorBuilderKeyPress);
        }

        /// <summary>
        /// controls for moving the canvas around
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FloorBuilderKeyPress(object sender, KeyEventArgs e)
        {
            Console.WriteLine("KeyPress" + e.Key);
            if (Canvas_FB == null)
            {
                return;
            }
            if (e.Key == Key.Right || e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Down)
            {
                if (e.Key == Key.Right)
                    OffsetX += 5;
                if (e.Key == Key.Left)
                    OffsetX -= 5;
                if (e.Key == Key.Up)
                    OffsetY -= 5;
                if (e.Key == Key.Down)
                    OffsetY += 5;

                tt.X = OffsetX;
                tt.Y = OffsetY;
            }
            else if (e.Key == Key.OemMinus || e.Key == Key.OemPlus)
            {
                if (e.Key == Key.OemPlus)
                {
                    ZoomIn();
                }
                else
                {
                    ZoomOut();
                }
            }
        }


        /// <summary>
        /// Connects to the server
        /// </summary>
        private void initializeDBConnection()
        {

            var connection = SettingsFile.Default.ConnectionString;

            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            db = new COMP4952PROJECTContext(builder.Options);
            
        }

        /// <summary>
        /// Navigates to the floor builder page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToFloorBuilder_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("FloorBuilder.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Navigates to the staff screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToStaffScreen_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("StaffScreen.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Clears the tables and reloads them visually if a customer is added to a table
        /// </summary>
        private void ClearTables()
        {
            for(int i=0; i< Canvas_FB.Children.Count; i++)
            {
                if(Canvas_FB.Children[i].GetType() == typeof(Image)){
                    Canvas_FB.Children.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Clears the table images and reloads the
        /// </summary>
        public void Refresh()
        {
            ClearTables();
            LoadTableInfos();
        }

        /// <summary>
        /// Displays all tables from the database
        /// </summary>
        private void LoadTableInfos()
        {
            foreach (TableInfo t in db.TableInfo.ToList())
            {
                DisplayTable(t);
            }
        }

        /// <summary>
        /// Displays all the walls from the database
        /// </summary>
        private void LoadWalls()
        {
            foreach (Wall w in db.Wall.ToList())
            {
                DisplayWall(w);
            }
        }

        /// <summary>
        /// Displays a table on the canvas and displays the number of customers on it
        /// </summary>
        /// <param name="t"></param>
        private void DisplayTable(TableInfo t)
        {
            Image newImg = new Image();
            int tableId = t.TypeId;
            switch (tableId)
            {
                case 1:
                    newImg.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/circle_1.png"));
                    break;
                case 2:
                    newImg.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/square_2.png"));
                    break;
                case 3:
                    newImg.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/rectangle_3.png"));
                    break;
                default:
                    newImg.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/circle_1.png"));
                    break;
            }
            newImg.Width = 100;
            newImg.Height = 100;
            newImg.Tag = t.Id;
            FurnitureType ft = db.FurnitureType.Single(u => u.Id == tableId);
            newImg.Name = "Placed" + ft.Type + "_" + t.Id;
            newImg.MouseDown += Table_MouseDown;
            TextBox tableLabel = new TextBox();
            int custCount = db.Customer.Where(u => u.TableId == t.Id).ToList().Count;
            tableLabel.Text = custCount != 0 ? "" + custCount : "Table Available";
            tableLabel.Width = custCount != 0 ? 15 : 84;
            tableLabel.Height = custCount != 0 ? 20 : 18;
            tableLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            tableLabel.VerticalContentAlignment = VerticalAlignment.Center;
            tableLabel.IsHitTestVisible = false;
            Canvas_FB.Children.Add(newImg);
            Canvas_FB.Children.Add(tableLabel);
            Canvas.SetLeft(newImg, t.Xloc);
            Canvas.SetTop(newImg, t.Yloc);
            Console.WriteLine(newImg.Width / 2);
            Console.WriteLine(tableLabel.Width / 2);
            Console.WriteLine(newImg.Height / 2);
            Console.WriteLine(tableLabel.Height / 2);
            Canvas.SetLeft(tableLabel, t.Xloc + newImg.Width/2 - tableLabel.Width/2);
            Canvas.SetTop(tableLabel, t.Yloc + newImg.Height/2 - tableLabel.Height/2);
        }

        /// <summary>
        /// Displays a wall on the canvas
        /// </summary>
        /// <param name="w"></param>
        private void DisplayWall(Wall w)
        {
            Line newLine = new Line();
            newLine.Name = "PlacedWall";
            newLine.Tag = w.Id;
            newLine.Stroke = Brushes.Black;
            newLine.StrokeThickness = 5;
            newLine.X1 = w.X1loc;
            newLine.X2 = w.X2loc;
            newLine.Y1 = w.Y1loc;
            newLine.Y2 = w.Y2loc;
            //newLine.StrokeStartLineCap = PenLineCap.Round;
            //newLine.StrokeEndLineCap = PenLineCap.Round;
            Console.WriteLine(newLine.X1 + ", " + newLine.Y1 + ", " + newLine.X2 + ", " + newLine.Y2);
            Canvas_FB.Children.Add(newLine);
        }

        /// <summary>
        /// Table MouseDown handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Table_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OrderScreen os = new OrderScreen(int.Parse(((Image)sender).Tag.ToString()), this);
            os.Show();
        }

        /// <summary>
        /// Zoom in click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        /// <summary>
        /// Zoom out click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        /// <summary>
        /// Zooms in the canvas
        /// </summary>
        private void ZoomIn()
        {
            double scalingRate = 1.1;

            st.ScaleX *= scalingRate;
            st.ScaleY *= scalingRate;
        }

        /// <summary>
        /// Zooms out the canvas
        /// </summary>
        private void ZoomOut()
        {
            double scalingRate = 1.1;

            st.ScaleX /= scalingRate;
            st.ScaleY /= scalingRate;
        }
    }
}
