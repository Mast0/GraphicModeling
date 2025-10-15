using GraphicModelling.ViewModels;
using System.Windows;
using System.Windows.Media;
namespace GraphicModelling;

public partial class MainWindow : Window
{
    private bool _isDraggingRotationCenter = false;

    public MainWindow()
    {
        InitializeComponent();
        Grapich.SizeChanged += OnCanvasSizeChanged;
    }

    private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (this.DataContext is MainViewModel viewModel)
        {
            viewModel.UpdateCanvasSize(e.NewSize.Width, e.NewSize.Height);
        }
    }

    private void RotationCenter_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isDraggingRotationCenter = true;
        RotationCenter.CaptureMouse();
    }

    private void RotationCenter_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isDraggingRotationCenter = false;
        RotationCenter.ReleaseMouseCapture();
    }

    private void RotationCenter_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {   
        if (_isDraggingRotationCenter && this.DataContext is MainViewModel viewModel)
        {
            Point mousePosition = e.GetPosition(Grapich);

            // Обмежуємо координати, щоб центр обертання не виходив за межі канвасу
            double clampedX = Math.Max(0, Math.Min(mousePosition.X, Grapich.ActualWidth));
            double clampedY = Math.Max(0, Math.Min(mousePosition.Y, Grapich.ActualHeight));

            viewModel.RotationCenter = new Point(clampedX, clampedY);
        }   
    }
}