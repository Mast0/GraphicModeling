using GraphicModelling.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GraphicModelling.Views
{
    /// <summary>
    /// Interaction logic for Lab3View.xaml
    /// </summary>
    public partial class Lab3View : UserControl
    {
        private bool _isDraggingRotationCenter = false;

        private bool _isDragging = false;
        private int _draggedSegmentIndex = -1;
        private int _draggedPointType = -1; // 0=P1, 1=C0, 2=C1, 3=P1

        public Lab3View()
        {
            InitializeComponent();
        }

        private void Grapich_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as Lab3ViewModel;
            if (vm == null) return;

            Point mousePos = e.GetPosition(Grapich);

            // Проходимо по всіх сегментах і шукаємо найближчу точку
            double threshold = 10.0;
            
            // Можемо переміщати точки лише в оригінальному стані
            if (vm.Angle == 0 && vm.Scale == 1 && !vm.IsAffine && !vm.IsProjective)
            {
                for (int i = 0; i < vm.OriginalSegments.Count; i++)
                {
                    var seg = vm.OriginalSegments[i];
                    if ((seg.P0 - mousePos).Length < threshold) { StartDrag(i, 0); return; }
                    if ((seg.C0 - mousePos).Length < threshold) { StartDrag(i, 1); return; }
                    if ((seg.C1 - mousePos).Length < threshold) { StartDrag(i, 2); return; }
                    if ((seg.P1 - mousePos).Length < threshold) { StartDrag(i, 3); return; }
                }
            }
            else
            {
                MessageBox.Show("Скиньте трансформації для редагування точок!");
            }
        }

        private void Grapich_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _draggedSegmentIndex = -1;
            Grapich.ReleaseMouseCapture();
        }

        private void Grapich_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && DataContext is Lab3ViewModel vm)
            {
                Point mousePos = e.GetPosition(Grapich);

                vm.MovePoint(_draggedSegmentIndex, _draggedPointType, mousePos);
            }
        }

        private void StartDrag(int segIndex, int pointType)
        {
            _isDragging = true;
            _draggedSegmentIndex = segIndex;
            _draggedPointType = pointType;
            Grapich.CaptureMouse();
        }

        private void RotationCenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingRotationCenter = true;
            RotationCenter.CaptureMouse();
        }

        private void RotationCenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingRotationCenter = false;
            RotationCenter.ReleaseMouseCapture();
        }

        private void RotationCenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingRotationCenter && this.DataContext is BaseViewModel viewModel)
            {
                Point mousePosition = e.GetPosition(Grapich);

                double clampedX = Math.Max(0, Math.Min(mousePosition.X, Grapich.ActualWidth));
                double clampedY = Math.Max(0, Math.Min(mousePosition.Y, Grapich.ActualHeight));

                viewModel.RotationCenter = new Point(clampedX, clampedY);
            }
        }
    }
}
