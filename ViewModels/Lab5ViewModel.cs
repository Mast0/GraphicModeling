using GraphicModelling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GraphicModelling.ViewModels;

public enum ProjectionPlane { Z, Y, X }

public class Lab5ViewModel : BaseViewModel
{
    private Polyhedron _model;
    
    // Властивість для відображення ліній на екрані (2D проекції)
    public ObservableCollection<Segment> ProjectedSegments { get; set; }

    #region Parameters
    // Параметри фігури
    private double _prismHeight = 100;
    public double PrismHeight
    {
        get => _prismHeight;
        set { _prismHeight = value; RecreateModel(); }
    }

    private double _prismRadius = 50;
    public double PrismRadius
    {
        get => _prismRadius;
        set { _prismRadius = value; RecreateModel(); }
    }

    // Трансформації
    private double _transX = 0, _transY = 0, _transZ = 0;
    public double TransX { get => _transX; set { _transX = value; Update(); } }
    public double TransY { get => _transY; set { _transY = value; Update(); } }
    public double TransZ { get => _transZ; set { _transZ = value; Update(); } }

    private double _rotX = 0, _rotY = 0, _rotZ = 0;
    public double RotX { get => _rotX; set { _rotX = value; Update(); } }
    public double RotY { get => _rotY; set { _rotY = value; Update(); } }
    public double RotZ { get => _rotZ; set { _rotZ = value; Update(); } }


    // Проекція
    private ProjectionPlane _selectedPlane = ProjectionPlane.Z;
    public ProjectionPlane SelectedPlane
    {
        get => _selectedPlane;
        set { _selectedPlane = value; Update(); }
    }

    private double _projectionP = 0;
    public double ProjectionP
    {
        get => _projectionP;
        set { _projectionP = value; Update(); }
    }
    #endregion

    #region Animation
    private DispatcherTimer _timer;
    private bool _isAnimating;
    public ICommand ToggleAnimationCommand { get; }
    public string AnimationBtnText => _isAnimating ? "Стоп" : "Старт";

    // Напрямок анімації
    private double _animStep = 1.0;
    private double _currentAnimVal = 0;
    #endregion

    public Lab5ViewModel()
    {
        ProjectedSegments = new ObservableCollection<Segment>();

        RecreateModel();

        ToggleAnimationCommand = new MainViewModelCommand(ToggleAnimation);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnAnimationTick;
    }

    private void RecreateModel()
    {
        // Створюємо призму
        _model = Polyhedron.CreateStarPrism(_prismRadius, _prismRadius / 2.5, _prismHeight);
        Update();
    }

    public void Update() 
    {
        ProjectedSegments.Clear();
        if (_model == null) return;

        // Формуємо матрицю трансформації
        Matrix4x4 worldMatrix = Matrix4x4.Identity;
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.RotationX(RotX));
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.RotationY(RotY));
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.RotationZ(RotZ));
        worldMatrix = Matrix4x4.Multiply(worldMatrix, Matrix4x4.Translation(TransX, TransY, TransZ));

        // Формуємо матрицю проекції (ортографічна на X=p, Y=p або Z=p)
        Matrix4x4 projectionMatrix = Matrix4x4.Identity;

        if (SelectedPlane == ProjectionPlane.Z) // z = p
        {
            projectionMatrix.M[2, 2] = 0;
            projectionMatrix.M[3, 2] = ProjectionP;
        }
        else if (SelectedPlane == ProjectionPlane.Y) // y = p
        {
            projectionMatrix.M[1, 1] = 0;
            projectionMatrix.M[3, 1] = ProjectionP;
        }
        else // X = p
        {
            projectionMatrix.M[0, 0] = 0;
            projectionMatrix.M[3, 0] = ProjectionP;
        }

        // Комбінована матриця
        Matrix4x4 finalMatrix = Matrix4x4.Multiply(worldMatrix, projectionMatrix);

        // Центр екрану
        double screenCenterX = 400;
        double screenCenterY = 300;

        // Перетворюємо вершини
        var transformedVertices = new Point3D[_model.Vertices.Count];
        for (int i = 0; i < _model.Vertices.Count; i++)
        {
            transformedVertices[i] = Matrix4x4.Multiply(_model.Vertices[i], finalMatrix);
        }

        // Малюємо ребра
        foreach (var edge in _model.Edges)
        {
            Point3D p1 = transformedVertices[edge.StartIndex];
            Point3D p2 = transformedVertices[edge.EndIndex];

            double sx1, sy1, sx2, sy2;

            if (SelectedPlane == ProjectionPlane.Z)
            {
                // Дивимось на площину XY
                sx1 = p1.X; sy1 = p1.Y;
                sx2 = p2.X; sy2 = p2.Y;
            }
            else if (SelectedPlane == ProjectionPlane.Y)
            {
                // Дивимось на площину XZ
                sx1 = p1.X; sy1 = p1.Z;
                sx2 = p2.X; sy2 = p2.Z;
            }
            else // X
            {
                // Дивимось на площину YZ
                sx1 = p1.Z; sy1 = p1.Y;
                sx2 = p2.Z; sy2 = p2.Y;
            }

            // Інвертуємо Y для екрану і додаємо центр
            ProjectedSegments.Add(new Segment(
                new Point(screenCenterX + sx1, screenCenterY - sy1),
                new Point(screenCenterX + sx2, screenCenterY - sy2)
            ));
        }

        // Додаємо вісі координат
        DrawAxes(finalMatrix, screenCenterX, screenCenterY);
    }

    private void DrawAxes(Matrix4x4 finalMatrix, double cx, double cy)
    {
        Point3D origin = Matrix4x4.Multiply(new Point3D(0, 0, 0), finalMatrix);
        Point3D xAxis = Matrix4x4.Multiply(new Point3D(100, 0, 0), finalMatrix);
        Point3D yAxis = Matrix4x4.Multiply(new Point3D(0, 100, 0), finalMatrix);
        Point3D zAxis = Matrix4x4.Multiply(new Point3D(0, 0, 100), finalMatrix);

        // Допоміжна функція мапінгу
        Point ToScreen(Point3D p)
        {
            double x, y;
            if (SelectedPlane == ProjectionPlane.Z) { x = p.X; y = p.Y; }
            else if (SelectedPlane == ProjectionPlane.Y) { x = p.X; y = p.Z; }
            else { x = p.Z; y = p.Y; }
            return new Point(cx + x, cy - y);
        }

        // Додамо сегменти осей
        ProjectedSegments.Add(new Segment(ToScreen(origin), ToScreen(xAxis)));
        ProjectedSegments.Add(new Segment(ToScreen(origin), ToScreen(yAxis)));
        ProjectedSegments.Add(new Segment(ToScreen(origin), ToScreen(zAxis)));
    }

    private void ToggleAnimation(object obj)
    {
        _isAnimating = !_isAnimating;
        if (_isAnimating) _timer.Start(); else _timer.Stop();
        OnPropertyChanged(nameof(AnimationBtnText));
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        _currentAnimVal += _animStep;
        if (_currentAnimVal > 100 || _currentAnimVal < 0) _animStep *= -1;

        _prismHeight = 100 + _currentAnimVal;
        _model = Polyhedron.CreateStarPrism(_prismRadius, _prismRadius / 2.5, _prismHeight);

        RotY = (RotY + 2) % 360;
        RotX = (RotX + 1) % 360;

        Update();
        OnPropertyChanged(nameof(PrismHeight));
    }

    public override void UpdateAndApplyTransforms()
    {
        throw new NotImplementedException();
    }
}
