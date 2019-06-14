using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RoadMap
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database db;
        private IList<Street> streets;
        private IDictionary<string, Line> streetToLineMapping = new Dictionary<string, Line>();
        private ObservableCollection<Transport> obsTransports = new ObservableCollection<Transport>();
        private ObservableCollection<Route> obsRoutes = new ObservableCollection<Route>();
        private ObservableCollection<Street> obsStreets = new ObservableCollection<Street>();
        private IList<Line> streetsOfSelectedTransport = new List<Line>();
        private IList<Street> highlightedStreets = new List<Street>();
        private double sizeFactorX = 1;
        private double sizeFactorY = 1;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                db = Database.GetInstance();
                streets = db.GetStreets();
                DrawStreets();
                dgTransports.ItemsSource = obsTransports;
                dgRoutes.ItemsSource = obsRoutes;
                dgStreets.ItemsSource = obsStreets;
                foreach (Transport transport in db.ReadTransports()) { obsTransports.Add(transport); }
                foreach (Route route in db.GetRoutes()) { obsRoutes.Add(route); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawStreets()
        {
            if (cvMap.ActualWidth != 0 && cvMap.ActualHeight != 0)
            {
                cvMap.Children.Clear();
                foreach (Street street in streets)
                {
                    Line line = new Line();
                    if (street.From.StartsWith("A"))
                        line.Stroke = Brushes.Red;
                    else if (street.From.StartsWith("B"))
                        line.Stroke = Brushes.Blue;
                    else
                        line.Stroke = Brushes.Green;
                    sizeFactorX = cvMap.ActualWidth / 60000.0;
                    sizeFactorY = cvMap.ActualHeight / 30000.0;
                    line.X1 = street.FromPoint.X * sizeFactorX;
                    line.X2 = street.ToPoint.X * sizeFactorX;
                    line.Y1 = cvMap.ActualHeight - street.FromPoint.Y * sizeFactorY;
                    line.Y2 = cvMap.ActualHeight - street.ToPoint.Y * sizeFactorY;
                    line.StrokeThickness = 1;
                    cvMap.Children.Add(line);
                    streetToLineMapping.Add(street.ID, line);
                }
            }
        }

        private void CvMap_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(cvMap);
            lblDistance.Content = "X: " + (int)(p.X / sizeFactorX) + ", Y: " + (int)(30000 - p.Y / sizeFactorY);
            Console.WriteLine("X: " + (int)(p.X / sizeFactorX) + ", Y: " + (int)(cvMap.ActualHeight - p.Y / sizeFactorY));
        }

        private void CvMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(relativeTo: cvMap);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cvMap.Width = (border.ActualWidth < border.ActualHeight * 2 ? border.ActualWidth : border.ActualHeight * 2) - 20;
            cvMap.Height = (border.ActualHeight * 2 < border.ActualWidth ? border.ActualHeight : border.ActualWidth / 2) - 20;
            if (streets != null)
                DrawStreets();
        }

        private void DgTransports_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Transport selectedTransport = (Transport)dgTransports.SelectedItem;
            if (selectedTransport != null)
            {
                double distance;
                IList<Street> streets = db.GetStreetsOfTransport(selectedTransport, out distance);
                lblDistance.Content = distance;
                DrawStreetsOfTransport(streets);
            }
        }

        private void DrawStreetsOfTransport(IList<Street> streets)
        {
            foreach (Line line in streetsOfSelectedTransport) { cvMap.Children.Remove(line); }
            MessageBox.Show(streets.Count + " streets affected");
            streetsOfSelectedTransport.Clear();
            foreach (Street street in streets)
            {
                Line line = new Line();
                line.Stroke = Brushes.Pink;
                sizeFactorX = cvMap.ActualWidth / 60000.0;
                sizeFactorY = cvMap.ActualHeight / 30000.0;
                line.X1 = street.FromPoint.X * sizeFactorX;
                line.X2 = street.ToPoint.X * sizeFactorX;
                line.Y1 = street.FromPoint.Y * sizeFactorY;
                line.Y2 = street.ToPoint.Y * sizeFactorY;
                line.StrokeThickness = 4;
                streetsOfSelectedTransport.Add(line);
                cvMap.Children.Add(line);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRoute = (Route)dgRoutes.SelectedItem;
                if (selectedRoute == null) throw new Exception("Please select a route");
                db.AddTransport(selectedRoute);
                foreach (Transport transport in db.ReadTransports()) { obsTransports.Add(transport); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnFinishTransport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedTransport = (Transport)dgTransports.SelectedItem;
                if (selectedTransport == null) throw new Exception("Please select a transport");
                db.finishTransport(selectedTransport);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnReconnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DgStreets_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Street selectedStreet = (Street)dgStreets.SelectedItem;
            IList<Street> streets = new List<Street>();
            streets.Add(selectedStreet);
            ToggleStreetsHighlighting(streets);
        }

        private void DgRoutes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Route selectedRoute = (Route)dgRoutes.SelectedItem;
            IList<Street> streets = db.GetStreetsOfRoute(selectedRoute);
            ToggleStreetsHighlighting(streets);
            obsStreets.Clear();
            foreach (Street street in streets) obsStreets.Add(street);
        }

        private void ToggleStreetsHighlighting(IList<Street> streets)
        {
            foreach (Street street in highlightedStreets) streetToLineMapping[street.ID].StrokeThickness = 1;
            highlightedStreets.Clear();
            foreach (Street street in streets)
            {
                highlightedStreets.Add(street);
                streetToLineMapping[street.ID].StrokeThickness = 3;
            }

        }
    }
}