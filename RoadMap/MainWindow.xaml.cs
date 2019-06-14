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
        private IList<Street> teilstrecken;
        private IDictionary<Street, Line> keyValuePairs = new Dictionary<Street, Line>();
        private ObservableCollection<Transport> obsTransports = new ObservableCollection<Transport>();
        private ObservableCollection<Route> obsRouten = new ObservableCollection<Route>();
        private IList<Line> teilstreckesOfSelectedTransport = new List<Line>();
        private double sizeFactorX = 1;
        private double sizeFactorY = 1;


        public MainWindow()
        {
            InitializeComponent();
            try
            {
                db = Database.GetInstance();
                teilstrecken = db.GetStreets();
                DrawTeilstrecken();
                dgTransports.ItemsSource = obsTransports;
                dgRoutes.ItemsSource = obsRouten;
                foreach (Transport transport in db.ReadTransports()) { obsTransports.Add(transport); }
                foreach (Route route in db.GetRoutes()) { obsRouten.Add(route); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawTeilstrecken()
        {
            if (cvMap.ActualWidth != 0 && cvMap.ActualHeight != 0)
            {
                cvMap.Children.Clear();
                foreach (Street teilstrecke in teilstrecken)
                {
                    Line line = new Line();
                    if (teilstrecke.vonOrt.StartsWith("A"))
                        line.Stroke = Brushes.Red;
                    else if (teilstrecke.vonOrt.StartsWith("B"))
                        line.Stroke = Brushes.Blue;
                    else
                        line.Stroke = Brushes.Green;
                    sizeFactorX = cvMap.ActualWidth / 60000.0;
                    sizeFactorY = cvMap.ActualHeight / 30000.0;
                    line.X1 = teilstrecke.vonPoint.X * sizeFactorX;
                    line.X2 = teilstrecke.bisPoint.X * sizeFactorX;
                    line.Y1 = teilstrecke.vonPoint.Y * sizeFactorY;
                    line.Y2 = teilstrecke.bisPoint.Y * sizeFactorY;
                    line.StrokeThickness = 2;
                    cvMap.Children.Add(line);
                }
            }
        }

        private void CvMap_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(cvMap);
            Console.WriteLine("X: " + (int)(p.X / sizeFactorX) + ", Y: " + (int)(p.Y / sizeFactorX));
        }

        private void CvMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(relativeTo: cvMap);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cvMap.Width = (border.ActualWidth < border.ActualHeight * 2 ? border.ActualWidth : border.ActualHeight * 2) - 20;
            cvMap.Height = (border.ActualHeight * 2 < border.ActualWidth ? border.ActualHeight : border.ActualWidth / 2) - 20;
            if (teilstrecken != null)
                DrawTeilstrecken();
        }

        private void DgTransports_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Transport selectedTransport = (Transport)dgTransports.SelectedItem;
            if (selectedTransport != null)
            {
                double distance;
                IList<Street> teilstrecken = db.ReadTeilstreckenOfTransport(selectedTransport, out distance);
                lblDistance.Content = distance;
                DrawTeilstreckenOfTransport(teilstrecken);
            }
        }

        private void DrawTeilstreckenOfTransport(IList<Street> teilstrecken)
        {
            foreach (Line line in teilstreckesOfSelectedTransport) { cvMap.Children.Remove(line); }
            teilstreckesOfSelectedTransport.Clear();
            foreach (Street teilstrecke in teilstrecken)
            {
                Line line = new Line();
                line.Stroke = Brushes.Pink;
                sizeFactorX = cvMap.ActualWidth / 60000.0;
                sizeFactorY = cvMap.ActualHeight / 30000.0;
                line.X1 = teilstrecke.vonPoint.X * sizeFactorX;
                line.X2 = teilstrecke.bisPoint.X * sizeFactorX;
                line.Y1 = teilstrecke.vonPoint.Y * sizeFactorY;
                line.Y2 = teilstrecke.bisPoint.Y * sizeFactorY;
                line.StrokeThickness = 4;
                teilstreckesOfSelectedTransport.Add(line);
                cvMap.Children.Add(line);
            }
        }

        /// <summary>
        /// Creates new transport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRoute = (Route)dgRoutes.SelectedItem;
                if (selectedRoute == null) throw new Exception("Please select a route");
                db.AddTransport(selectedRoute);
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
    }
}