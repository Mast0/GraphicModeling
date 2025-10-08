using System.Windows;
using System.Windows.Media;
namespace GraphicModelling;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += (s, e) => DrawGrid();
        this.SizeChanged += (s, e) => DrawGrid();
    }

    private void DrawGrid()
    {
        GridLinesX.Children.Clear();
        GridLinesY.Children.Clear();

        double step = 20;

        // Вертикальні лінії
        for (double x = 0; x < this.ActualWidth; x += step)
        {
            GridLinesX.Children.Add(new LineGeometry(new Point(x, 0), new Point(x, this.ActualHeight)));
        }

        // Горизонтальні лінії
        for (double y = 0; y < this.ActualHeight; y += step)
        {
            GridLinesY.Children.Add(new LineGeometry(new Point(0, y), new Point(this.ActualWidth, y)));
        }
    }
}