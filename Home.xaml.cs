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
    /// </summary>
    public partial class Home : Page
    {
        COMP4952PROJECTContext db;
        public Home()
        {
            db = new COMP4952PROJECTContext();
            InitializeComponent();
            LoadTableInfos();
        }

        private void GoToFloorBuilder_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("FloorBuilder.xaml", UriKind.Relative));
        }

        private void LoadTableInfos()
        {
            foreach (TableInfo t in db.TableInfo.ToList())
            {
                DisplayTable(t);
            }
        }
        private int GetTypeIdFromName(string input)
        {
            if (input.Contains("Round"))
                return 1;
            else if (input.Contains("Square"))
                return 2;
            else if (input.Contains("Rectangle"))
                return 3;

            return 1;
        }

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
            FurnitureType ft = db.FurnitureType.Single(u => u.Id == tableId);
            newImg.Name = "Placed" + ft.Type + "_" + t.Id;
            //newImg.MouseDown += Table_MouseDown;
            Canvas_FB.Children.Add(newImg);
            Canvas.SetLeft(newImg, t.Xloc);
            Canvas.SetTop(newImg, t.Yloc);
        }
    }
}
