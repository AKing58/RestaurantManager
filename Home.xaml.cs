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

        /// <summary>
        /// Home page constructor
        /// </summary>
        public Home()
        {
            db = new COMP4952PROJECTContext();
            InitializeComponent();
            LoadWalls();
            LoadTableInfos();
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
        /// Displays a table on the canvas
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
            Canvas_FB.Children.Add(newImg);
            Canvas.SetLeft(newImg, t.Xloc);
            Canvas.SetTop(newImg, t.Yloc);
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

        private void Table_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OrderScreen os = new OrderScreen(int.Parse(((Image)sender).Tag.ToString()));
            os.Show();
        }
    }
}
