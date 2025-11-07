using GraphicModelling.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphicModelling.Views
{
    /// <summary>
    /// Interaction logic for Lab1View.xaml
    /// </summary>
    public partial class Lab1View : UserControl
    {
        private bool _isDraggingRotationCenter = false;

        public Lab1View()
        {
            InitializeComponent();
            Grapich.SizeChanged += OnCanvasSizeChanged;
        }

        private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.DataContext is Lab1ViewModel viewModel)
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
            if (_isDraggingRotationCenter && this.DataContext is Lab1ViewModel viewModel)
            {
                Point mousePosition = e.GetPosition(Grapich);

                // Обмежуємо координати, щоб центр обертання не виходив за межі канвасу
                double clampedX = Math.Max(0, Math.Min(mousePosition.X, Grapich.ActualWidth));
                double clampedY = Math.Max(0, Math.Min(mousePosition.Y, Grapich.ActualHeight));

                viewModel.RotationCenter = new Point(clampedX, clampedY);
            }
        }
    }
}
