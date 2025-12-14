using GraphicModelling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GraphicModelling.ViewModels;

public class Lab6ViewModel : BaseViewModel
{
    // Лінії поверхні та контуру для відображення
    public ObservableCollection<Segment> ProjectedSegments { get; set; }

    // Логіка ЛР3 для генерації пташки
    private Lab3ViewModel _lab3VM;

    #region Surface Parameters (Параболоїд)
    private double _paramC = 0.05; // Коефіцієнт "крутизни" параболоїда
    public double ParamC
    {
        get => _paramC;
        set { _paramC = value; OnPropertyChanged(); Update(); }
    }

    private double _maxRadius = 40; // Максимальний радіус (U)
    public double MaxRadius
    {
        get => _maxRadius;
        set { _maxRadius = value; OnPropertyChanged(); Update(); }
    }
    #endregion

    #region Contour (Texture) Mapping Parameters
    private double _contourScale = 0.5;
    public double ContourScale
    {
        get => _contourScale;
        set { _contourScale = value; OnPropertyChanged(); Update(); }
    }

    private double _contourOffsetX = 0; // Зсув по U
    public double ContourOffsetX
    {
        get => _contourOffsetX;
        set { _contourOffsetX = value; OnPropertyChanged(); Update(); }
    }

    private double _contourOffsetY = 0; // Зсув по V
    public double ContourOffsetY
    {
        get => _contourOffsetY;
        set { _contourOffsetY = value; OnPropertyChanged(); Update(); }
    }

    private double _contourRotation = 0; // Обертання контуру на поверхні
    public double ContourRotation
    {
        get => _contourRotation;
        set { _contourRotation = value; OnPropertyChanged(); Update(); }
    }
    #endregion

    #region 3D Transformations (World)
    private double _transX = 0, _transY = 0, _transZ = 0;
    public double TransX { get => _transX; set { _transX = value; OnPropertyChanged(); Update(); } }
    public double TransY { get => _transY; set { _transY = value; OnPropertyChanged(); Update(); } }
    public double TransZ { get => _transZ; set { _transZ = value; OnPropertyChanged(); Update(); } }

    private double _rotX = -20, _rotY = 30, _rotZ = 0;
    public double RotX { get => _rotX; set { _rotX = value; OnPropertyChanged(); Update(); } }
    public double RotY { get => _rotY; set { _rotY = value; OnPropertyChanged(); Update(); } }
    public double RotZ { get => _rotZ; set { _rotZ = value; OnPropertyChanged(); Update(); } }
    #endregion

    #region Projection (From Lab 5, Variant 16)
    private ProjectionPlane _selectedPlane = ProjectionPlane.Z;
    public ProjectionPlane SelectedPlane
    {
        get => _selectedPlane;
        set { _selectedPlane = value; OnPropertyChanged(); Update(); }
    }

    private double _projectionP = 0;
    public double ProjectionP
    {
        get => _projectionP;
        set { _projectionP = value; OnPropertyChanged(); Update(); }
    }
    #endregion

    #region Animation
    private DispatcherTimer _timer;
    private bool _isAnimating;
    public ICommand ToggleAnimationCommand { get; }
    public string AnimationBtnText => _isAnimating ? "Стоп" : "Старт";
    private double _animStep = 0.002;
    #endregion

    public Lab6ViewModel()
    {
        ProjectedSegments = new ObservableCollection<Segment>();

        // Ініціалізуємо 3-ю лабу, щоб отримати точки пташки
        _lab3VM = new Lab3ViewModel();
        _lab3VM.UpdateAndApplyTransforms();

        ToggleAnimationCommand = new MainViewModelCommand(ToggleAnimation);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnAnimationTick;

        Update();
    }

    public void Update()
    {
        ProjectedSegments.Clear();

        Brush surfaceColor = Brushes.LightGray;
        Brush contourColor = Brushes.Red;
        Brush axisColor = Brushes.Blue;

        // 1. Матриці трансформації
        var worldMatrix = Matrix4x4.Identity;
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.RotationX(RotX));
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.RotationY(RotY));
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.RotationZ(RotZ));
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.Translation(TransX, TransY, TransZ));

        var projectionMatrix = Matrix4x4.Identity;
        if (SelectedPlane == ProjectionPlane.Z) { projectionMatrix.M[2, 2] = 0; projectionMatrix.M[3, 2] = ProjectionP; }
        else if (SelectedPlane == ProjectionPlane.Y) { projectionMatrix.M[1, 1] = 0; projectionMatrix.M[3, 1] = ProjectionP; }
        else { projectionMatrix.M[0, 0] = 0; projectionMatrix.M[3, 0] = ProjectionP; }

        var finalMatrix = Matrix4x4.Multiply(worldMatrix, projectionMatrix);
        double screenCenterX = 400;
        double screenCenterY = 300;

        // Допоміжна функція проекції на екран
        Point ToScreen(Point3D p)
        {
            Point3D transP = Matrix4x4.Multiply(p, finalMatrix);
            double x, y;
            if (SelectedPlane == ProjectionPlane.Z) { x = transP.X; y = transP.Y; }
            else if (SelectedPlane == ProjectionPlane.Y) { x = transP.X; y = transP.Z; }
            else { x = transP.Z; y = transP.Y; }
            return new Point(screenCenterX + x, screenCenterY - y);
        }

        // 2. Побудова Параболоїду обертання
        int uSteps = 10; // Кроки по радіусу (кільця)
        int vSteps = 20; // Кроки по куту (сектори)

        List<Point3D> surfacePoints = new List<Point3D>();

        // Генеруємо сітку
        // Поздовжні лінії
        for (int j = 0; j < vSteps; j++)
        {
            double v = j * (2 * Math.PI / vSteps);
            Point3D? prev = null;
            for (int i = 0; i <= uSteps; i++)
            {
                double u = i * (MaxRadius / uSteps);
                Point3D curr = CalculateParaboloidPoint(u, v);
                if (prev != null) ProjectedSegments.Add(new Segment(ToScreen(prev), ToScreen(curr), surfaceColor));
                prev = curr;
            }
        }

        // Поперечні лінії
        for (int i = 1; i <= uSteps; i++)
        {
            double u = i * (MaxRadius / uSteps);
            Point3D? first = null;
            Point3D? prev = null;
            for (int j = 0; j <= vSteps; j++)
            {
                double v = j * (2 * Math.PI / vSteps);
                Point3D curr = CalculateParaboloidPoint(u, v);
                if (prev != null) ProjectedSegments.Add(new Segment(ToScreen(prev), ToScreen(curr), surfaceColor));
                else first = curr;
                prev = curr;
            }
            // Замикаємо коло
            if (prev != null && first != null)
                ProjectedSegments.Add(new Segment(ToScreen(prev), ToScreen(first), surfaceColor));
        }

        // 3. Накладання контуру

        var birdSegments = _lab3VM.TransformedCurve;

        // Центр обертання контуру
        double pivotX = 300;
        double pivotY = 300;

        foreach (var seg in birdSegments)
        {
            // Обробляємо початкову і кінцеву точку кожного сегмента
            Point3D pStart3D = Map2DTo3D(seg.StartPoint, pivotX, pivotY);
            Point3D pEnd3D = Map2DTo3D(seg.EndPoint, pivotX, pivotY);

            // Малюємо лінію контуру на поверхні
            ProjectedSegments.Add(new Segment(ToScreen(pStart3D), ToScreen(pEnd3D), contourColor));
        }

        DrawAxes(finalMatrix, screenCenterX, screenCenterY, axisColor);
    }

    // Рівняння параболоїда обертання
    private Point3D CalculateParaboloidPoint(double u, double v)
    {
        // u = радіус, v = кут
        double x = u * Math.Cos(v);
        double y = u * Math.Sin(v);
        double z = ParamC * u * u;
        return new Point3D(x, y, z);
    }

    // Функція мапінгу 2D точки на 3D поверхню
    private Point3D Map2DTo3D(Point p2d, double pivotX, double pivotY)
    {
        // 1. Центрування та базові 2D трансформації
        double x = p2d.X - pivotX;
        double y = p2d.Y - pivotY;

        x *= ContourScale;
        y *= ContourScale;

        double radContour = ContourRotation * Math.PI / 180.0;
        double xRot = x * Math.Cos(radContour) - y * Math.Sin(radContour);
        double yRot = x * Math.Sin(radContour) + y * Math.Cos(radContour);

        // 2. Мапінг
        // Ділимо координату на 1 радіан та додаємо зсув як кут.
        double v_surf = (xRot / 50.0) + (ContourOffsetX / 20.0);

        // Беремо базовий радіус та додаємо yRot, що впливає на висоту та радіус на стінці параболоїда.
        double u_surf = 25 + (yRot / 5.0) + ContourOffsetY;

        // Захист від від'ємного радіуса
        if (u_surf < 0) u_surf = 0;

        return CalculateParaboloidPoint(u_surf, v_surf);
    }

    private void DrawAxes(Matrix4x4 finalMatrix, double cx, double cy, Brush color)
    {
        Point3D origin = Matrix4x4.Multiply(new Point3D(0, 0, 0), finalMatrix);
        Point3D xAxis = Matrix4x4.Multiply(new Point3D(50, 0, 0), finalMatrix);
        Point3D yAxis = Matrix4x4.Multiply(new Point3D(0, 50, 0), finalMatrix);
        Point3D zAxis = Matrix4x4.Multiply(new Point3D(0, 0, 50), finalMatrix);

        Point ToScreen(Point3D p)
        {
            double x, y;
            if (SelectedPlane == ProjectionPlane.Z) { x = p.X; y = p.Y; }
            else if (SelectedPlane == ProjectionPlane.Y) { x = p.X; y = p.Z; }
            else { x = p.Z; y = p.Y; }
            return new Point(cx + x, cy - y);
        }

        ProjectedSegments.Add(new Segment(ToScreen(origin), ToScreen(xAxis), color));
        ProjectedSegments.Add(new Segment(ToScreen(origin), ToScreen(yAxis), color));
        ProjectedSegments.Add(new Segment(ToScreen(origin), ToScreen(zAxis), color));
    }

    private void ToggleAnimation(object obj)
    {
        _isAnimating = !_isAnimating;
        if (_isAnimating) _timer.Start(); else _timer.Stop();
        OnPropertyChanged(nameof(AnimationBtnText));
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        ParamC += _animStep;
        if (ParamC > 0.15 || ParamC < 0.01) _animStep *= -1;
        RotZ = (RotZ + 1) % 360;
        Update();
    }

    public override void UpdateAndApplyTransforms() { }
}
