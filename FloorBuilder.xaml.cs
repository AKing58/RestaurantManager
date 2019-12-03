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
        bool SnapSelected;
        bool DrawingSelected;
        bool DeletingSelected;
        bool isDrawing;

        double OffsetX;
        double OffsetY;

        int SnapAmount;


        /// <summary>
        /// Constructor for the FloorBuilder page
        /// Initializes variables and loads tables and walls
        /// </summary>
        public FloorBuilder()
        {
            initializeDBConnection();
            InitializeComponent();
            SnapSelected = false;
            DrawingSelected = false;
            DeletingSelected = false;
            SnapAmount = 50;
            LoadWalls();
            LoadTableInfos();
            Application.Current.MainWindow.KeyDown += new KeyEventHandler(FloorBuilderKeyPress);
        }

        /// <summary>
        /// Detects key presses for the purpose of manipulating the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FloorBuilderKeyPress(object sender, KeyEventArgs e)
        {
            Console.WriteLine("KeyPress" + e.Key);
            if(Canvas_FB == null)
            {
                return;
            }
            if(e.Key == Key.Right || e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Down)
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
            }else if(e.Key == Key.OemMinus || e.Key == Key.OemPlus)
            {
                if(e.Key == Key.OemPlus)
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
        /// Connects to the database
        /// </summary>
        private void initializeDBConnection()
        {

            var connection = SettingsFile.Default.ConnectionString;

            DbContextOptionsBuilder<COMP4952PROJECTContext> builder = new DbContextOptionsBuilder<COMP4952PROJECTContext>();
            builder.UseSqlServer(connection);

            db = new COMP4952PROJECTContext(builder.Options);
          
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
            newImg.MouseMove += Object_MouseOver;
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
            newLine.MouseDown += Wall_MouseDown;
            newLine.MouseMove += Object_MouseOver;
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
                if (SnapSelected)
                {
                    left = (int)Math.Round((decimal)(left / SnapAmount));
                    left = left * SnapAmount;
                    top = (int)Math.Round((decimal)(top / SnapAmount));
                    top = top * SnapAmount;
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
        /// Wall on mousedown that will delete the wall if currently isDeleting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Line curLine = (Line)sender;
            if (DeletingSelected)
            {
                DeleteObject(sender);
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
            Image curImg = (Image)sender;
            if (DeletingSelected && (curImg.Tag == null || !curImg.Tag.ToString().Contains("Tool")))
            {
                DeleteObject(sender);
            }
            else
            {
                CancelDrawAndDelete();
            }
            
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
            newImg.MouseMove += Object_MouseOver;
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
            {
                foreach(Customer c in db.Customer.Where(u => u.TableId == t.Id).ToList())
                {
                    foreach (Orders o in db.Orders.Where(u => u.CustId == c.Id).ToList())
                    {
                        db.Orders.Remove(o);
                    }
                    db.Customer.Remove(c);
                }
                db.TableInfo.Remove(t);
            }
                
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
                    if (i.Name.Contains("Tmp") && i.Visibility != Visibility.Hidden)
                        AddTableToDb(i);
                    else if (i.Name.Contains("deleteThis") && !i.Name.Contains("Tmp")) {
                        TableInfo t = db.TableInfo.Find(i.Tag);
                        foreach (Customer c in db.Customer.Where(u => u.TableId == t.Id).ToList())
                        {
                            foreach (Orders ord in db.Orders.Where(u => u.CustId == c.Id).ToList())
                            {
                                db.Orders.Remove(ord);
                            }
                            db.Customer.Remove(c);
                        }
                        db.TableInfo.Remove(t);
                    } else if (i.Name.Contains("Placed") && i.Visibility != Visibility.Hidden)
                    {
                        TableInfo t = db.TableInfo.Find(int.Parse(i.Tag.ToString()));
                        t.Xloc = (int)Canvas.GetLeft(i);
                        t.Yloc = (int)Canvas.GetTop(i);
                        db.Entry(t).State = EntityState.Modified;
                    }
                }else if(o is Line)
                {
                    Line l = (Line)o;
                    if (l.Name.Contains("tmp") && l.Visibility != Visibility.Hidden)
                    {
                        if (db.Wall.SingleOrDefault(u => (u.X1loc == l.X1 &&
                                              u.X2loc == l.X2 &&
                                              u.Y1loc == l.Y1 &&
                                              u.Y2loc == l.Y2) ||
                                              (u.X1loc == l.X2 &&
                                              u.X2loc == l.X1 &&
                                              u.Y1loc == l.Y2 &&
                                              u.Y2loc == l.Y1)) == null)
                        {
                            Wall w = new Wall();
                            w.X1loc = (int)l.X1;
                            w.X2loc = (int)l.X2;
                            w.Y1loc = (int)l.Y1;
                            w.Y2loc = (int)l.Y2;
                            db.Wall.Add(w);
                        }
                        
                    }else if (l.Name.Contains("deleteThis") && !l.Name.Contains("tmp"))
                    {
                        Wall wInfo = db.Wall.Find(l.Tag);
                        db.Wall.Remove(wInfo);
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
        /// Starts drawing a wall if the check button is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DrawingSelected && e.ChangedButton == MouseButton.Left)
            {
                Console.WriteLine("Starting to draw");
                startLoc = Mouse.GetPosition(Canvas_FB);
                curLine = new Line();
                curLine.MouseDown += Wall_MouseDown;
                curLine.MouseMove += Object_MouseOver;
                curLine.Name = "tmpWall";
                curLine.Stroke = Brushes.Black;
                curLine.StrokeThickness = 5;
                //curLine.StrokeStartLineCap = PenLineCap.Round;
                //curLine.StrokeEndLineCap = PenLineCap.Round;
                int tempX = ((int)Math.Round((startLoc.X / SnapAmount))) * SnapAmount;
                int tempY = ((int)Math.Round((startLoc.Y / SnapAmount))) * SnapAmount;

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
            if(curLine != null)
            {
                if (curLine.X1 == curLine.X2 && curLine.Y1 == curLine.Y2)
                {
                    Canvas_FB.Children.Remove(curLine);
                }else if (CheckIfWallExists(curLine))
                {
                    Canvas_FB.Children.Remove(curLine);
                }
                else if (db.Wall.SingleOrDefault(u => (u.X1loc == curLine.X1 &&
                                                 u.X2loc == curLine.X2 &&
                                                 u.Y1loc == curLine.Y1 &&
                                                 u.Y2loc == curLine.Y2) ||
                                                 (u.X1loc == curLine.X2 &&
                                                 u.X2loc == curLine.X1 &&
                                                 u.Y1loc == curLine.Y2 &&
                                                 u.Y2loc == curLine.Y1)) != null)
                {
                    Canvas_FB.Children.Remove(curLine);
                }
                curLine = null;
                isDrawing = false;
            }
        }

        private bool CheckIfWallExists(Line inputLine)
        {
            for(int i=0; i<Canvas_FB.Children.Count; i++)
            {
                if (Canvas_FB.Children[i] is Line)
                {
                    Line tempLine = (Line)Canvas_FB.Children[i];
                    if (inputLine != tempLine && ((inputLine.X1 == tempLine.X1 &&
                         inputLine.X2 == tempLine.X2 &&
                         inputLine.Y1 == tempLine.Y1 &&
                         inputLine.Y2 == tempLine.Y2) ||
                         (inputLine.X1 == tempLine.X2 &&
                         inputLine.X2 == tempLine.X1 &&
                         inputLine.Y1 == tempLine.Y2 &&
                         inputLine.Y2 == tempLine.Y1)))
                    {
                        return true;
                    }
                }
            }
            return false;
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

                int tempX = ((int)Math.Round((p.X / SnapAmount))) * SnapAmount;
                int tempY = ((int)Math.Round((p.Y / SnapAmount))) * SnapAmount;
                if(!Canvas_FB.Children.Contains(curLine) && (curLine.X2 != 0 || curLine.X2 != tempX) && (curLine.Y2 != 0 || curLine.Y2 != tempY))
                {
                    Canvas_FB.Children.Add(curLine);
                }
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

        /// <summary>
        /// Enables the delete tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteTool_Click(object sender, MouseButtonEventArgs e)
        {
            DrawingSelected = false;
            DrawTool.Opacity = 1;
            DeletingSelected = !DeletingSelected;
            Image tool = (Image)sender;
            tool.Opacity = DeletingSelected ? .5 : 1;
        }

        /// <summary>
        /// Enables snap to grid for tables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SnapToGrid_Click(object sender, RoutedEventArgs e)
        {
            SnapSelected = !SnapSelected;
            Image tool = (Image)sender;
            tool.Opacity = SnapSelected ? .5 : 1;
        }

        /// <summary>
        /// Toggles the ability to draw walls on and off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawWalls_Click(object sender, RoutedEventArgs e)
        {
            DeletingSelected = false;
            DeleteTool.Opacity = 1;
            DrawingSelected = !DrawingSelected;
            Image tool = (Image)sender;
            tool.Opacity = DrawingSelected ? .5 : 1;
        }

        /// <summary>
        /// deselects the draw and delete tools
        /// </summary>
        private void CancelDrawAndDelete()
        {
            DrawingSelected = false;
            DrawTool.Opacity = 1;
            DeletingSelected = false;
            DeleteTool.Opacity = 1;
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
        /// <summary>
        /// Sets the snap to grid to 25
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Snap25_Click(object sender, RoutedEventArgs e)
        {
            SnapAmount = 25;
            btnSnap25.IsEnabled = false;
            btnSnap50.IsEnabled = true;
        }

        /// <summary>
        /// Sets the snap to grid to 50
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Snap50_Click(object sender, RoutedEventArgs e)
        {
            SnapAmount = 50;
            btnSnap25.IsEnabled = true;
            btnSnap50.IsEnabled = false;
        }

        /// <summary>
        /// Used to delete multiple objects easily by allowing the user to click and drag
        /// across multiple objects at once to delete them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Object_MouseOver(object sender, MouseEventArgs e)
        {
            if (DeletingSelected && e.LeftButton == MouseButtonState.Pressed)
            {
                DeleteObject(sender);
            }
        }

        /// <summary>
        /// Marks the object for deletion and hides it
        /// </summary>
        /// <param name="sender"></param>
        private void DeleteObject(object sender)
        {
            if (sender is Image)
            {
                Image curImg = (Image)sender;
                if (DeletingSelected && (curImg.Tag == null || !curImg.Tag.ToString().Contains("Tool")))
                {
                    curImg.Name = "deleteThis" + curImg.Name;
                    curImg.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                Line curLine = (Line)sender;
                if (DeletingSelected)
                {
                    curLine.Name = "deleteThis" + curLine.Name;
                    curLine.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
