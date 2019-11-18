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
    /// Interaction logic for FloorBuilder.xaml
    /// </summary>
    public partial class FloorBuilder : Page
    {
        COMP4952PROJECTContext db;
        Point startLoc;
        Image img;
        public FloorBuilder()
        {
            db = new COMP4952PROJECTContext();
            InitializeComponent();
            LoadTableInfos();
        }

        private void LoadTableInfos()
        {
            foreach(TableInfo t in db.TableInfo.ToList())
            {
                DisplayTable(t);
            }
        }

        private void DisplayTable(TableInfo t)
        {
            Image newImg = new Image();
            int tableId = t.TypeId;
            switch (tableId)
            {
                case 1:
                    newImg.Source = RoundTable1.Source;
                    break;
                case 2:
                    newImg.Source = SquareTable2.Source;
                    break;
                case 3:
                    newImg.Source = RectangleTable3.Source;
                    break;
                default:
                    newImg.Source = RoundTable1.Source;
                    break;
            }
            newImg.Width = 100;
            newImg.Height = 100;
            Canvas_FB.Children.Add(newImg);
            Canvas.SetLeft(newImg, t.Xloc);
            Canvas.SetTop(newImg, t.Yloc);
        }

        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            if(img != null)
            {
                Point mousePos = Mouse.GetPosition(Canvas_FB);
                int left = (int)((mousePos.X - startLoc.X) - (img.Width / 2));
                int top = (int)((mousePos.Y - startLoc.Y) - (img.Height / 2));
                if (left >= 0)
                    Canvas.SetLeft(img, left);
                else
                    Canvas.SetLeft(img, 0);
                if (top >= 0)
                    Canvas.SetTop(img, top);
                else
                    Canvas.SetTop(img, 0);
            }
        }

        private void Table_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startLoc = new Point(0,0);
            Image newImg = DuplicateImage(((Image)sender));
            img = newImg;
        }

        private Image DuplicateImage(Image inImage)
        {
            Image newImg = new Image();
            newImg.Source = new BitmapImage(new Uri(inImage.Source.ToString()));
            newImg.Name = inImage.Name;
            newImg.Width = 100;
            newImg.Height = 100;
            Canvas_FB.Children.Add(newImg);
            Canvas.SetLeft(newImg, 0);
            Canvas.SetTop(newImg, 0);
            return newImg;
        }

        private void Page_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AddToDb(img);
            Console.WriteLine("Release");
            img = null;
        }

        private void AddToDb(Image input)
        {
            TableInfo ti = new TableInfo();
            int sourceId = (int)Char.GetNumericValue(input.Name.ToCharArray()[input.Name.Length-1]);
            ti.TypeId = sourceId;
            ti.Xloc = (int)Canvas.GetLeft(input);
            ti.Yloc = (int)Canvas.GetTop(input);
            db.TableInfo.Add(ti);
        }

        private void DeleteLayout()
        {
            foreach(TableInfo t in db.TableInfo.ToList())
            {
                db.TableInfo.Remove(t);
            }
            db.SaveChanges();
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("FloorBuilder.xaml",UriKind.Relative));
        }
        
        private void GoToMain()
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("Home.xaml", UriKind.Relative));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            db.SaveChanges();
        }

        private void DeleteTables_Click(object sender, RoutedEventArgs e)
        {
            DeleteLayout();
        }

        private void GoToMain_Click(object sender, RoutedEventArgs e)
        {
            GoToMain();
        }

    }
}
