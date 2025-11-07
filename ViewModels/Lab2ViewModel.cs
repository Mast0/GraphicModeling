using GraphicModelling.Helpers;
using GraphicModelling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GraphicModelling.ViewModels;

public class Lab2ViewModel : BaseViewModel
{
    private List<Point> _originalTrochoidPoints = new List<Point>();
    public ObservableCollection<Segment> TransformedTrochoid { get; set; }
    public ObservableCollection<Segment> TransformedTangentNormal { get; set; }

    #region Trochoid Parameters

    // Радіус кола, що котиться
    private double _r = 3.0 * PIXELS_IN_SANTIMETER;
    public double R
    {
        get => _r / PIXELS_IN_SANTIMETER;
        set { _r = value * PIXELS_IN_SANTIMETER; OnPropertyChanged(); UpdateCalculations(); UpdateAndApplyTransforms(); }
    }

    // Відстань точки до центру
    private double _h = 2.0 * PIXELS_IN_SANTIMETER;
    public double H
    {
        get => _h / PIXELS_IN_SANTIMETER;
        set { _h = value * PIXELS_IN_SANTIMETER; OnPropertyChanged(); UpdateCalculations(); UpdateAndApplyTransforms(); }
    }

    // Параметр 't' для дотичної/нормалі
    private double _t = 1.5 * Math.PI;
    public double T
    {
        get => _t;
        set { _t = value; OnPropertyChanged(); UpdateCalculations(); UpdateAndApplyTransforms(); }
    }

    #endregion

    #region Calculated Values

    private string _area = "";
    public string Area { get => _area; set { _area = value; OnPropertyChanged(); } }

    private string _arcLength = "";
    public string ArcLength { get => _arcLength; set { _arcLength = value; OnPropertyChanged(); } }

    private string _curvatureRadius = "";
    public string CurvatureRadius { get => _curvatureRadius; set { _curvatureRadius = value; OnPropertyChanged(); } }

    private string _inflectionPoints = "";
    public string InflectionPoints { get => _inflectionPoints; set { _inflectionPoints = value; OnPropertyChanged(); } }

    #endregion

    #region Animation

    private DispatcherTimer _animationTimer;
    private bool _isAnimating = false;
    private bool _hIncreasing = true;
    public ICommand ToggleAnimationCommand { get; }
    public string AnimationButtonText => _isAnimating ? "Зупинити анімацію" : "Почати анімацію";

    #endregion

    public Lab2ViewModel() : base()
    {
        TransformedTrochoid = new ObservableCollection<Segment>();
        TransformedTangentNormal = new ObservableCollection<Segment>();

        ToggleAnimationCommand = new MainViewModelCommand(ToggleAnimation);

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMicroseconds(20) // 50 кадрів/сек
        };
        _animationTimer.Tick += AnimationTick;

        UpdateCalculations();
        UpdateAndApplyTransforms();
    }

    private void TessellateTrochoid()
    {
        _originalTrochoidPoints.Clear();
        double tMin = 0.0;
        double tMax = 6 * Math.PI; // 3 арки
        double tStep = 0.25;

        // Формули трохоїди
        for (double t = tMin; t <= tMax; t += tStep)
        {
            double x = _r * t - _h * Math.Sin(t);
            double y = _r - _h * Math.Cos(t);

            // Додаємо зсув, щоб крива починалася не з краю екрану
            _originalTrochoidPoints.Add(new Point(x + PIXELS_IN_SANTIMETER, y + PIXELS_IN_SANTIMETER * 3));
        }
    }

    // Евклідові перетворення
    public override void UpdateAndApplyTransforms()
    {
        TessellateTrochoid(); // Перебудовуємо криву при зміні параметрів

        Matrix3x3 finalMatrix = BuildTransformationMatrix(true);

        // Трансформуємо Трохоїду
        TransformedTrochoid.Clear();
        for (int i = 0; i < _originalTrochoidPoints.Count - 1; i++)
        {
            Point start = TransformationHelper.ApplyTransformations(_originalTrochoidPoints[i], finalMatrix);
            Point end = TransformationHelper.ApplyTransformations(_originalTrochoidPoints[i + 1], finalMatrix);
            TransformedTrochoid.Add(new Segment(start, end));
        }

        // Побудова дотичної та нормалі
        BuildTangentAndNormal(finalMatrix);

        // Трансформуємо сітку
        Matrix3x3 gridFinalMatrix = BuildTransformationMatrix(false);
        TransformedGridLines.Clear();
        foreach (var orGL in _originalGridLines)
        {
            Point newStart = TransformationHelper.ApplyTransformations(orGL.StartPoint, gridFinalMatrix);
            Point newEnd = TransformationHelper.ApplyTransformations(orGL.EndPoint, gridFinalMatrix);
            TransformedGridLines.Add(new Segment(newStart, newEnd));
        }

        TransformedRotationCenter = TransformationHelper.ApplyTransformations(RotationCenter, finalMatrix);
    }

    //Побудова дотичної та нормалі
    private void BuildTangentAndNormal(Matrix3x3 matrix)
    {
        TransformedTangentNormal.Clear();

        // Поточна точка на кривій
        double x_t = _r * _t - _h * Math.Sin(_t);
        double y_t = _r - _h * Math.Cos(_t);
        Point p = new Point(x_t + PIXELS_IN_SANTIMETER, y_t + PIXELS_IN_SANTIMETER * 3);

        // Похідні
        double dx_dt = _r - _h * Math.Cos(_t);
        double dy_dt = _h * Math.Sin(_t);

        // Створюємо вектор дотичної
        Vector tangentVector = new Vector(dx_dt, dy_dt);
        if (tangentVector.Length > 0)
            tangentVector.Normalize();

        // Створюємо вектор нормалі (повертаємо на 90 градусів)
        Vector normalVector = new Vector(-tangentVector.Y, tangentVector.X);

        double lineLength = 2 * PIXELS_IN_SANTIMETER; // Довжина лінії дотичної/нормалі

        // Кінцеві точки дотичної
        Point t1 = p + tangentVector * lineLength;
        Point t2 = p - tangentVector * lineLength;

        // Кінцеві точки нормалі
        Point n1 = p + normalVector * lineLength;
        Point n2 = p - normalVector * lineLength;

        // Трансформуємо та додаємо сегменти
        TransformedTangentNormal.Add(new Segment(
            TransformationHelper.ApplyTransformations(t1, matrix),
            TransformationHelper.ApplyTransformations(t2, matrix)
        ));
        TransformedTangentNormal.Add(new Segment(
            TransformationHelper.ApplyTransformations(n1, matrix),
            TransformationHelper.ApplyTransformations(n2, matrix)
        ));
    }

    private void ToggleAnimation(object obj)
    {
        _isAnimating = !_isAnimating;

        if (_isAnimating)
            _animationTimer.Start();
        else
            _animationTimer.Stop();
        OnPropertyChanged(nameof(AnimationButtonText));
    }

    private void AnimationTick(object sender, EventArgs e)
    {
        double step = 0.05;
        double maxH = R * 2.0; // Максимальна відстань
        double minH = 0.1;

        double currentH = H; // Повертаємо в см

        if (_hIncreasing)
        {
            currentH += step;
            if (currentH >= maxH)
            {
                currentH = maxH;
                _hIncreasing = false;
            }
        }
        else
        {
            currentH -= step;
            if (currentH < minH)
            {
                currentH = minH;
                _hIncreasing = true;
            }
        }

        H = currentH;
    }

    private void UpdateCalculations()
    {
        // Площа
        double areaVal = 2 * Math.PI * _r * _r + Math.PI * _h * _h;
        Area = $"Площа (1 арка): {areaVal / (PIXELS_IN_SANTIMETER * PIXELS_IN_SANTIMETER):F2} см^2";

        // Довжина дуги
        Func<double, double> arcFunc = t => Math.Sqrt(_r * _r + _h * _h - 2 * _r * _h * Math.Cos(t));
        double arcLengthVal = NumericalIntegrate(arcFunc, 0, 2 * Math.PI, 1000);
        ArcLength = $"Довжина дуги (1 арка): {arcLengthVal / PIXELS_IN_SANTIMETER:F2} см";

        // Радіус кривизни (в точці T)
        double num = Math.Pow(_r * _r + _h * _h - 2 * _r * _h * Math.Cos(_t), 1.5);
        double den = Math.Abs(_r * _h * Math.Cos(_t) - _h * _h);

        if (Math.Abs(den) < 1e-6)
        {
            CurvatureRadius = "Радіус кривизни: ∞ (точка перегину)";
        }
        else
        {
            double curvatureVal = num / den;
            CurvatureRadius = $"Радіус кривизни (в т. T): {curvatureVal / PIXELS_IN_SANTIMETER:F2} см";
        }

        // Точки перегину
        if (_h > _r)
        {
            InflectionPoints = "Точки перегину: Немає (h > r)";
        }
        else
        {
            double t_inflect = Math.Acos(_h / _r);
            InflectionPoints = $"Точки перегину (на 1 арці): t = {t_inflect:F2} та t = {2 * Math.PI - t_inflect:F2}";
        }
    }

    // Простий метод трапецій для чисельного інтегрування
    private double NumericalIntegrate(Func<double, double> f, double a, double b, int n)
    {
        double h = (b - a) / n;
        double sum = 0.5 * (f(a) + f(b));
        for (int i = 1; i < n; i++)
        {
            sum += f(a + i * h);
        }
        return sum * h;
    }
}
