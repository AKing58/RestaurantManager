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
        bool snapToGrid;
        public FloorBuilder()
        {
            db = new COMP4952PROJECTContext();
            InitializeComponent();
            snapToGrid = false;
            LoadTableInfos();
        }

        private void LoadTableInfos()
        {
            foreach(TableInfo t in db.TableInfo.ToList())
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
                    newImg.Source = RoundTable.Source;
                    break;
                case 2:
                    newImg.Source = SquareTable.Source;
                    break;
                case 3:
                    newImg.Source = RectangleTable.Source;
                    break;
                default:
                    newImg.Source = RoundTable.Source;
                    break;
            }
            Console.WriteLine(newImg.Source);
            newImg.Width = 100;
            newImg.Height = 100;
            FurnitureType ft = db.FurnitureType.Single(u => u.Id == tableId);
            newImg.Name = "Placed" + ft.Type;
            newImg.Tag = t.Id;
            newImg.MouseDown += Table_MouseDown;
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
                if (snapToGrid)
                {
                    left = (int)Math.Round((decimal)(left / 50));
                    left = left * 50;
                    top = (int)Math.Round((decimal)(top / 50));
                    top = top * 50;
                }
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
            Image curImg = (Image)sender;
            startLoc = new Point(0, 0);
            if (curImg.Name.Contains("Tmp") || curImg.Name.Contains("Placed"))
            {
                Console.WriteLine(e.GetPosition(Canvas_FB));
                //startLoc = e.GetPosition(Canvas_FB);
                img = (Image)sender;
            }
            else
            {
                Image newImg = DuplicateImage(((Image)sender));
                img = newImg;
            }
                
        }

        private Image DuplicateImage(Image inImage)
        {
            Image newImg = new Image();
            newImg.Source = new BitmapImage(new Uri(inImage.Source.ToString()));
            newImg.Name = "Tmp" + inImage.Name;
            newImg.Width = 100;
            newImg.Height = 100;
            newImg.MouseDown += Table_MouseDown;
            Canvas_FB.Children.Add(newImg);
            Canvas.SetLeft(newImg, 0);
            Canvas.SetTop(newImg, 0);
            return newImg;
        }

        private void Page_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(img != null)
                img = null;
        }

        private void AddToDb(Image input)
        {
            TableInfo ti = new TableInfo();
            int sourceId = GetTypeIdFromName(input.Name);
            ti.TypeId = sourceId;
            ti.Xloc = (int)Canvas.GetLeft(input);
            ti.Yloc = (int)Canvas.GetTop(input);
            db.TableInfo.Add(ti);
        }

        private void DeleteLayout()
        {
            Console.WriteLine("Deleting Layout");
            foreach(TableInfo t in db.TableInfo.ToList())
                db.TableInfo.Remove(t);
            db.SaveChanges();
        }
        
        private void GoToMain()
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("Home.xaml", UriKind.Relative));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            foreach(Image i in Canvas_FB.Children)
            {
                if (i.Name.Contains("Tmp"))
                    AddToDb(i);
                else if (i.Name.Contains("Placed"))
                {
                    //int underscoreLoc = i.Name.LastIndexOf('_');
                    TableInfo t = db.TableInfo.Find(int.Parse(i.Tag.ToString()));
                    t.Xloc = (int)Canvas.GetLeft(i);
                    t.Yloc = (int)Canvas.GetTop(i);
                    db.Entry(t).State = EntityState.Modified;
                }
            }
            db.SaveChanges();

            for (int i = 0; i < Canvas_FB.Children.Count;)
                Canvas_FB.Children.Remove(Canvas_FB.Children[0]);

            LoadTableInfos();
        }

        private void DeleteTables_Click(object sender, RoutedEventArgs e)
        {
            DeleteLayout();
            for(int i = 0; i< Canvas_FB.Children.Count;)
                Canvas_FB.Children.Remove(Canvas_FB.Children[0]);
        }

        private void GoToMain_Click(object sender, RoutedEventArgs e)
        {
            GoToMain();
        }

        private void SnapToGrid_Click(object sender, RoutedEventArgs e)
        {
            snapToGrid = ((MenuItem)sender).IsChecked;
        }
    }
}
