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
    /// Adam King
    /// </summary>
    public partial class FloorBuilder : Page
    {
        COMP4952PROJECTContext db;
        Point startLoc;
        Image img;

        Line curLine;
        bool snapToGrid;
        bool isDrawing;
        /// <summary>
        /// Constructor for the FloorBuilder page
        /// Initializes variables and loads tables and walls
        /// </summary>
        public FloorBuilder()
        {
            db = new COMP4952PROJECTContext();
            InitializeComponent();
            snapToGrid = false;
            isDrawing = false;

            LoadWalls();
            LoadTableInfos();
        }

        /// <summary>
        /// Displays all the tables from the database
        /// </summary>
        private void LoadTableInfos()
        {
            foreach(TableInfo t in db.TableInfo.ToList())
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
        /// Gets the type of table given the inputted string
        /// </summary>
        /// <param name="input">input name of table</param>
        /// <returns></returns>
        private int GetTypeIdFromName(string input)
        {
            /*
            if (input.Contains("Round"))
                return 1;
            else if (input.Contains("Square"))
                return 2;
            else if (input.Contains("Rectangle"))
                return 3;
            */
            return db.FurnitureType.Single(u => input.Contains(u.Type)).Id;
        }

        /// <summary>
        /// Displays a table on the canvas
        /// </summary>
        /// <param name="t">TableInfo object from database</param>
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

        /// <summary>
        /// Displays a wall on the canvas
        /// </summary>
        /// <param name="w">Wall object from database</param>
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
        /// Called when the mouse moves anywhere on the page.
        /// Is used to drag the currently selected table image around the canvas
        /// </summary>
        /// <param name="sender">Image table</param>
        /// <param name="e">mouse event</param>
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

        /// <summary>
        /// Called when a table is clicked
        ///     - If this table was already on the canvas, it selects this table and allows you to move it around.
        ///     - If this table was selected from the panel on the left, it creates a 
        ///       new table onto the canvas and allows you to move it around.
        ///       
        ///     - Turns off wall drawing if it was active when clicking a table
        /// </summary>
        /// <param name="sender">Image table</param>
        /// <param name="e">mouse event</param>
        private void Table_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DrawWallCheck.IsChecked)
            {
                DrawWallCheck.IsChecked = false;
                DrawWallCheck.SetResourceReference(MenuItem.BackgroundProperty, SystemColors.ControlBrushKey);
            }
            
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

        /// <summary>
        /// Creates a new Image object with the necessary properties from the image passed in
        /// </summary>
        /// <param name="inImage">The image to be copied</param>
        /// <returns>a new image</returns>
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

        /// <summary>
        /// Sets the currently selected image to null, ceasing the drag function
        /// </summary>
        /// <param name="sender">Page</param>
        /// <param name="e">mouse event</param>
        private void Page_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(img != null)
                img = null;
        }

        /// <summary>
        /// Parses the Image passed through and creates a new TableInfo and adds it to the database
        /// </summary>
        /// <param name="input">Image that is passed through</param>
        private void AddTableToDb(Image input)
        {
            TableInfo ti = new TableInfo();
            int sourceId = GetTypeIdFromName(input.Name);
            ti.TypeId = sourceId;
            ti.Xloc = (int)Canvas.GetLeft(input);
            ti.Yloc = (int)Canvas.GetTop(input);
            db.TableInfo.Add(ti);
        }

        /// <summary>
        /// Clears everything in the layout
        /// </summary>
        private void DeleteLayout()
        {
            Console.WriteLine("Deleting Layout");
            foreach(TableInfo t in db.TableInfo.ToList())
                db.TableInfo.Remove(t);
            foreach (Wall w in db.Wall.ToList())
                db.Wall.Remove(w);
            db.SaveChanges();
        }
        
        /// <summary>
        /// Navigates to the home page
        /// </summary>
        private void GoToMain()
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("Home.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Saves the newly added objects and updates the TableInfos
        /// Updates all TableInfos whether or not it was changed
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            foreach(object o in Canvas_FB.Children)
            {
                if(o is Image)
                {
                    Image i = (Image)o;
                    if (i.Name.Contains("Tmp"))
                        AddTableToDb(i);
                    else if (i.Name.Contains("Placed"))
                    {
                        //int underscoreLoc = i.Name.LastIndexOf('_');
                        TableInfo t = db.TableInfo.Find(int.Parse(i.Tag.ToString()));
                        t.Xloc = (int)Canvas.GetLeft(i);
                        t.Yloc = (int)Canvas.GetTop(i);
                        db.Entry(t).State = EntityState.Modified;
                    }
                }else if(o is Line)
                {
                    Line l = (Line)o;
                    if (l.Name.Contains("tmp"))
                    {
                        Wall w = new Wall();
                        w.X1loc = (int)l.X1;
                        w.X2loc = (int)l.X2;
                        w.Y1loc = (int)l.Y1;
                        w.Y2loc = (int)l.Y2;
                        db.Wall.Add(w);
                    }
                }
            }

            db.SaveChanges();

            for (int i = 0; i < Canvas_FB.Children.Count;)
                Canvas_FB.Children.Remove(Canvas_FB.Children[0]);

            LoadTableInfos();
            LoadWalls();
        }

        /// <summary>
        /// Deletes the layout clears the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteLayout_Click(object sender, RoutedEventArgs e)
        {
            DeleteLayout();
            for(int i = 0; i< Canvas_FB.Children.Count;)
                Canvas_FB.Children.Remove(Canvas_FB.Children[0]);
        }

        /// <summary>
        /// Goes to the home page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToMain_Click(object sender, RoutedEventArgs e)
        {
            GoToMain();
        }

        /// <summary>
        /// Enables snap to grid for tables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SnapToGrid_Click(object sender, RoutedEventArgs e)
        {
            snapToGrid = ((MenuItem)sender).IsChecked;
        }

        /// <summary>
        /// Toggles the ability to draw walls on and off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawWalls_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            if(!mi.IsChecked)
                mi.SetResourceReference(MenuItem.BackgroundProperty, SystemColors.ControlBrushKey);
            else
                mi.SetResourceReference(MenuItem.BackgroundProperty, SystemColors.ControlDarkBrushKey);
        }

        /// <summary>
        /// Starts drawing a wall if the check button is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DrawWallCheck.IsChecked && e.ChangedButton == MouseButton.Left)
            {
                Console.WriteLine("Starting to draw");
                startLoc = Mouse.GetPosition(Canvas_FB);
                curLine = new Line();
                curLine.Name = "tmpWall";
                curLine.Stroke = Brushes.Black;
                curLine.StrokeThickness = 5;
                //curLine.StrokeStartLineCap = PenLineCap.Round;
                //curLine.StrokeEndLineCap = PenLineCap.Round;
                Canvas_FB.Children.Add(curLine);
                int tempX = ((int)Math.Round((startLoc.X / 50))) * 50;
                int tempY = ((int)Math.Round((startLoc.Y / 50))) * 50;

                curLine.X1 = tempX;
                curLine.Y1 = tempY;
                isDrawing = true;
            }
        }

        /// <summary>
        /// Stops drawing 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            curLine = null;
            isDrawing = false;
        }

        /// <summary>
        /// Draws the currently drawing wall
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (curLine != null && isDrawing)
            {
                Point p = Mouse.GetPosition(Canvas_FB);

                int tempX = ((int)Math.Round((p.X / 50))) * 50;
                int tempY = ((int)Math.Round((p.Y / 50))) * 50;

                curLine.X2 = tempX;
                curLine.Y2 = tempY;
            }
        }

        /// <summary>
        /// Cancels the currently drawing line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_RightClick(object sender, MouseButtonEventArgs e)
        {
            Canvas_FB.Children.Remove(curLine);
            curLine = null;
            isDrawing = false;
        }
    }
}
