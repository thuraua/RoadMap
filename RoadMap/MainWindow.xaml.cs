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
        private IList<Teilstrecke> teilstrecken;
        private ObservableCollection<Transport> obsTransports = new ObservableCollection<Transport>();
        private double sizeFactorX = 1;
        private double sizeFactorY = 1;


        public MainWindow()
        {
            InitializeComponent();
            try
            {
                db = Database.GetInstance();
                teilstrecken = db.ReadTeilstrecken();
                DrawTeilstrecken();
                dgTransports.ItemsSource = obsTransports;
                foreach (Transport transport in db.ReadTransports()) { obsTransports.Add(transport); }
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
                foreach (Teilstrecke teilstrecke in teilstrecken)
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
    }
}