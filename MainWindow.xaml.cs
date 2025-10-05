using GraphicModelling.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using LineSegment = GraphicModelling.Models.LineSegment;

namespace GraphicModelling;

public partial class MainWindow : Window
{
    public double TranslateX
    {
        get => double.TryParse(tb_translateX.Text, out var res) ? res : 0;
        set => tb_translateX.Text = value.ToString();
    }

    public double TranslateY
    {
        get => double.TryParse(tb_translateY.Text, out var res) ? res : 0;
        set => tb_translateY.Text = value.ToString();
    }

    public double ScaleX
    {
        get => double.TryParse(tb_scaleX.Text, out var res) ? res : 0;
        set => tb_scaleX.Text = value.ToString();
    }

    public double ScaleY
    {
        get => double.TryParse(tb_scaleY.Text, out var res) ? res : 0;
        set => tb_scaleY.Text = value.ToString();
    }

    public double RotateAngle
    {
        get => double.TryParse(tb_rotationAngle.Text, out var res) ? res : 0;
        set => tb_rotationAngle.Text = value.ToString();
    }

    public double ShearX
    {
        get => double.TryParse(tb_shearX.Text, out var res) ? res : 0;
        set => tb_shearX.Text = value.ToString();
    }

    public double ShearY
    {
        get => double.TryParse(tb_shearY.Text, out var res) ? res : 0;
        set => tb_shearY.Text = value.ToString();
    }

    public double ProjectFactor
    {
        get => double.TryParse(tb_projectionFactor.Text, out var res) ? res : 0;
        set => tb_projectionFactor.Text = value.ToString();
    }

    public MainWindow()
    {
        InitializeComponent();
        MainViewModelHelper.LineSegments = new ObservableCollection<LineSegment>();
        MainViewModelHelper.AddSampleSquare();
    }

    private void UpdateDrawing()
    {
        if (CoordinatePlane == null) return;

        Dispatcher.BeginInvoke(() =>
        {
            CoordinatePlane.Children.Clear();
            DrawCoordinateSystem();

            foreach (var s in MainViewModelHelper.LineSegments)
            {
                var line = new Line
                {
                    X1 = s.StartPoint.X,
                    Y1 = s.StartPoint.Y,
                    X2 = s.EndPoint.X,
                    Y2 = s.EndPoint.Y,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2
                };
                CoordinatePlane.Children.Add(line);

                AddPoint(s.StartPoint, Brushes.Red);
                AddPoint(s.EndPoint, Brushes.Red);
            }

            CoordinatePlane.InvalidateVisual();
            CoordinatePlane.UpdateLayout();
        });
    }

    private void DrawCoordinateSystem()
    {
        var width = CoordinatePlane.ActualWidth;
        var height = CoordinatePlane.ActualHeight;

        var xAxis = new Line
        {
            X1 = 0,
            Y1 = height / 2,
            X2 = width,
            Y2 = height / 2,
            Stroke = Brushes.LightGray,
            StrokeThickness = 1
        };
        CoordinatePlane.Children.Add(xAxis);

        var yAxis = new Line
        {
            X1 = width / 2,
            Y1 = 0,
            X2 = width / 2,
            Y2 = height,
            Stroke = Brushes.LightGray,
            StrokeThickness = 1
        };
        CoordinatePlane.Children.Add(yAxis);

        for (int i = 50; i < width; i += 50)
        {
            var verticalLine = new Line
            {
                X1 = i,
                Y1 = 0,
                X2 = i,
                Y2 = height,
                Stroke = Brushes.LightGray,
                StrokeThickness = 0.5
            };
            CoordinatePlane.Children.Add(verticalLine);
        }

        for (int i = 50; i < height; i += 50)
        {
            var horizontalLine = new Line
            {
                X1 = 0,
                Y1 = i,
                X2 = width,
                Y2 = i,
                Stroke = Brushes.LightGray,
                StrokeThickness = 0.5
            };
            CoordinatePlane.Children.Add(horizontalLine);
        }
    }

    private void AddPoint(Point point, Brush brush)
    {
        var ellipse = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = brush,
            Margin = new Thickness(point.X - 3, point.Y - 3, 0, 0)
        };
        CoordinatePlane.Children.Add(ellipse);
    }

    private void CoordinatePlane_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateDrawing();
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateDrawing();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateDrawing();
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        
        if (double.TryParse(StartXTextBox.Text, out double startX) &&
            double.TryParse(StartYTextBox.Text, out double startY) &&
            double.TryParse(EndXTextBox.Text, out double endX) &&
            double.TryParse(EndYTextBox.Text, out double endY))
        {
            MainViewModelHelper.LineSegments.Add(new LineSegment
            {
                StartPoint = new Point(startX, startY),
                EndPoint = new Point(endX, endY)
            });

            StartXTextBox.Clear();
            StartYTextBox.Clear();
            EndXTextBox.Clear();
            EndYTextBox.Clear();
            UpdateDrawing();
        } 
        else
        {
            MessageBox.Show("Будь ласка, введіть правильні числові значення для координат.");
        }
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        MainViewModelHelper.ClearAll();
        UpdateDrawing();
    }

    private void Translate_Click(object sender, RoutedEventArgs e)
    {
        MainViewModelHelper.ApplyTranslation(TranslateX, TranslateY);
        UpdateDrawing();
    }

    private void Scale_Click(object sender, RoutedEventArgs e)
    {
        MainViewModelHelper.ApplyScaling(ScaleX, ScaleY);
        UpdateDrawing();
    }

    private void Rotate_Click(object sender, RoutedEventArgs e)
    {
        MainViewModelHelper.ApplyRotation(RotateAngle);
        UpdateDrawing();
    }

    private void Shear_Click(object sender, RoutedEventArgs e)
    {
        MainViewModelHelper.ApplyShear(ShearX, ShearY);
        UpdateDrawing();
    }

    private void Project_Click(object sender, RoutedEventArgs e)
    {
        MainViewModelHelper.ApplyProjection(ProjectFactor);
        UpdateDrawing();
    }
}