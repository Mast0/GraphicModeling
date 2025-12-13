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

public class Lab3ViewModel : BaseViewModel
{
    // Оригінальні керуючі точки
    public ObservableCollection<FergusonSegment> OriginalSegments { get; set; }

    // Трансформовані лінії для малювання кривої
    public ObservableCollection<Segment> TransformedCurve { get; set; }

    // Трансформовані лінії скелета
    public ObservableCollection<Segment> TransformedControlPolygon { get; set; }

    // Трансформовані точки-маркери, які можна перетягувати
    public ObservableCollection<CircleShape> ControlPointsMarkers { get; set; }

    private bool _showPolygon = true;
    public bool ShowPolygon
    {
        get => _showPolygon;
        set { _showPolygon = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    #region Animation

    private DispatcherTimer _animationTimer;
    private bool _isAnimating = false;
    private double _animationProgress = 0;
    private bool _animationDirectionForward = true;
    public ICommand ToggleAnimationCommand { get; }
    public string AnimationButtonText => _isAnimating ? "Стоп анімація" : "Старт анімація";

    // Зберігаємо початковий стан для анімації
    private List<FergusonSegment> _startStateSegments;

    // Цільовий стан анімації
    private List<FergusonSegment> _targetStateSegments;

    #endregion

    public ICommand ResetCommand { get; }

    public Lab3ViewModel() : base()
    {
        OriginalSegments = new ObservableCollection<FergusonSegment>();
        TransformedCurve = new ObservableCollection<Segment>();
        TransformedControlPolygon = new ObservableCollection<Segment>();
        ControlPointsMarkers = new ObservableCollection<CircleShape>();

        CreateBirdContour();
        InitializeAnimationTargets();
        
        
        // Підписка на зміни в сегментах
        foreach (var seg in OriginalSegments)
        {
            seg.PropertyChanged += (s, e) => UpdateAndApplyTransforms();
        }

        UpdateAndApplyTransforms();

        ResetCommand = new MainViewModelCommand(Reset);

        ToggleAnimationCommand = new MainViewModelCommand(ToggleAnimation);
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMicroseconds(20) };
        _animationTimer.Tick += AnimationTick;
    }

    public override void UpdateAndApplyTransforms()
    {
        Matrix3x3 matrix = BuildTransformationMatrix(true);
        TransformedCurve.Clear();
        TransformedControlPolygon.Clear();
        ControlPointsMarkers.Clear();

        var s = PIXELS_IN_SANTIMETER;

        foreach (var seg in OriginalSegments)
        {
            // Трансформуємо ключові точки
            Point p0 = TransformationHelper.ApplyTransformations(seg.P0, matrix);
            Point p1 = TransformationHelper.ApplyTransformations(seg.P1, matrix);
            Point c0 = TransformationHelper.ApplyTransformations(seg.C0, matrix);
            Point c1 = TransformationHelper.ApplyTransformations(seg.C1, matrix);

            // Побудування кривої Фергюсона
            // Вектори дотичних для розрахунку
            Vector t0 = c0 - p0;
            Vector t1 = p1 - c1;

            // Розбиваємо на лінії
            int steps = 8;
            Point prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                double u = (double)i / steps;
                Point current = CalculateFergusonPoint(u, p0, p1, t0, t1);
                TransformedCurve.Add(new Segment(prev, current));
                prev = current;
            }

            // Візуалізація скелета
            if (ShowPolygon)
            {
                // Лінії від точок до їх важелів дотичних
                TransformedControlPolygon.Add(new Segment(p0, c0));
                TransformedControlPolygon.Add(new Segment(c0, c1));
                TransformedControlPolygon.Add(new Segment(c1, p1));

                // Додаємо точки
                ControlPointsMarkers.Add(new CircleShape(p0.X / s, p0.Y / s, 0.15)); // P0
                ControlPointsMarkers.Add(new CircleShape(c0.X / s, c0.Y / s, 0.1)); // C0
                ControlPointsMarkers.Add(new CircleShape(c1.X / s, c1.Y / s, 0.1)); // C1
                // P1 буде додано як P0 наступного сегмента, або окремо для останнього
                if (seg == OriginalSegments.Last())
                    ControlPointsMarkers.Add(new CircleShape(p1.X / s, p1.Y / s, 0.15)); // P1
            }
        }

        // Сітка та центр
        Matrix3x3 gridM = BuildTransformationMatrix(false);
        TransformedGridLines.Clear();
        foreach (var l in _originalGridLines)
        {
            TransformedGridLines.Add(new Segment(
                TransformationHelper.ApplyTransformations(l.StartPoint, gridM),
                TransformationHelper.ApplyTransformations(l.EndPoint, gridM)));
        }
        TransformedRotationCenter = TransformationHelper.ApplyTransformations(RotationCenter, matrix);
    }

    public void MovePoint(int segmentIndex, int pointType, Point newPos)
    {
        var seg = OriginalSegments[segmentIndex];

        Point target = newPos;

        switch (pointType)
        {
            case 0: seg.P0 = target; break;
            case 1: seg.C0 = target; break;
            case 2: seg.C1 = target; break;
            case 3: seg.P1 = target; break;
        }

        if (pointType == 3 && segmentIndex < OriginalSegments.Count - 1)
            OriginalSegments[segmentIndex + 1].P0 = target;
        if (pointType == 0 && segmentIndex > 0)
            OriginalSegments[segmentIndex - 1].P1 = target;

        // Замикання контуру якщо потрібно
        if (pointType == 3 && segmentIndex == OriginalSegments.Count - 1)
            OriginalSegments[0].P0 = target;
        if (pointType == 0 && segmentIndex == 0)
            OriginalSegments[OriginalSegments.Count - 1].P1 = target;
    }

    private void CreateBirdContour()
    {
        double s = PIXELS_IN_SANTIMETER;

        // 1. Кінчик дзьоба -> Середина дзьоба (верх)
        OriginalSegments.Add(new FergusonSegment(
            new Point(2 * s, 12 * s),
            new Point(4 * s, 15 * s),
            new Point(6 * s, 16 * s),
            new Point(8 * s, 15.5 * s)
        ));

        // 2. Середина дзьоба -> Лоб
        OriginalSegments.Add(new FergusonSegment(
            new Point(8 * s, 15.5 * s),
            new Point(9 * s, 15 * s),
            new Point(9.5 * s, 14.5 * s),
            new Point(10 * s, 13.5 * s)
        ));

        // 3. Голова (Лоб -> Потилиця)
        OriginalSegments.Add(new FergusonSegment(
            new Point(10 * s, 13.5 * s),
            new Point(10.5 * s, 12.5 * s),
            new Point(11.5 * s, 13 * s),
            new Point(12 * s, 12 * s)
        ));

        // 4. Шия та верх спини
        OriginalSegments.Add(new FergusonSegment(
            new Point(12 * s, 12 * s),
            new Point(12.5 * s, 11 * s),
            new Point(12.5 * s, 10 * s),
            new Point(12.5 * s, 9 * s)
        ));

        // 5. Спина та крило (опускаємось до хвоста)
        OriginalSegments.Add(new FergusonSegment(
            new Point(12.5 * s, 9 * s),
            new Point(12.5 * s, 8 * s),
            new Point(13 * s, 7 * s),
            new Point(12.5 * s, 6 * s)
        ));

        // 6. Хвіст (довгий, вниз)
        OriginalSegments.Add(new FergusonSegment(
            new Point(12.5 * s, 6 * s),
            new Point(12.2 * s, 4 * s),
            new Point(12.2 * s, 2 * s),
            new Point(12 * s, 0.5 * s)
        ));

        // 7. Низ хвоста (закруглення вліво)
        OriginalSegments.Add(new FergusonSegment(
            new Point(12 * s, 0.5 * s),
            new Point(11.5 * s, 0.5 * s),
            new Point(11 * s, 1 * s),
            new Point(11 * s, 3 * s)
        ));

        // 8. Живіт (нижня частина)
        OriginalSegments.Add(new FergusonSegment(
            new Point(11 * s, 3 * s),
            new Point(11 * s, 4 * s),
            new Point(10 * s, 4 * s),
            new Point(9 * s, 5 * s)
        ));

        // 9. Груди (опуклі)
        OriginalSegments.Add(new FergusonSegment(
            new Point(9 * s, 5 * s),
            new Point(8 * s, 6 * s),
            new Point(8 * s, 8 * s),
            new Point(9 * s, 9 * s)
        ));

        // 10. Основа дзьоба (знизу)
        OriginalSegments.Add(new FergusonSegment(
            new Point(9 * s, 9 * s),
            new Point(9.2 * s, 10 * s),
            new Point(9.2 * s, 11 * s),
            new Point(9 * s, 11.5 * s)
        ));

        // 11. Дзьоб знизу (основна частина)
        OriginalSegments.Add(new FergusonSegment(
            new Point(9 * s, 11.5 * s),
            new Point(8 * s, 11.2 * s),
            new Point(6 * s, 11 * s),
            new Point(5 * s, 11.2 * s)
        ));

        // 12. Дзьоб знизу (до кінчика)
        OriginalSegments.Add(new FergusonSegment(
            new Point(5 * s, 11.2 * s),
            new Point(4 * s, 11.5 * s),
            new Point(3 * s, 11.8 * s),
            new Point(2 * s, 12 * s)
        ));
    }

    private void InitializeAnimationTargets()
    {
        double s = PIXELS_IN_SANTIMETER;
        _startStateSegments = new List<FergusonSegment>();
        _targetStateSegments = new List<FergusonSegment>();

        // Копіюємо поточний стан як стартовий
        foreach (var seg in OriginalSegments)
        {
            _startStateSegments.Add(new FergusonSegment(seg.P0, seg.C0, seg.C1, seg.P1));
        }

        // --- ЦІЛЬОВА ФІГУРА: ТРИКУТНИК ---

        Point pA = new Point(4 * s, 4 * s);    // Лівий нижній кут
        Point pB = new Point(14 * s, 4 * s);   // Правий нижній кут
        Point pC = new Point(9 * s, 15 * s);   // Верхній кут

        // Допоміжна функція для створення прямого сегмента
        FergusonSegment CreateLineSeg(Point start, Point end)
        {
            return new FergusonSegment(start, start, end, end);
        }

        // --- Сторона 1: Нижня (A -> B) ---
        // Ділимо на 4 частини
        Vector vecAB = (pB - pA) / 4.0;
        for (int i = 0; i < 4; i++)
            _targetStateSegments.Add(CreateLineSeg(pA + vecAB * i, pA + vecAB * (i + 1)));

        // --- Сторона 2: Права (B -> C) ---
        Vector vecBC = (pC - pB) / 4.0;
        for (int i = 0; i < 4; i++)
            _targetStateSegments.Add(CreateLineSeg(pB + vecBC * i, pB + vecBC * (i + 1)));

        // --- Сторона 3: Ліва (C -> A) ---
        Vector vecCA = (pA - pC) / 4.0;
        for (int i = 0; i < 4; i++)
            _targetStateSegments.Add(CreateLineSeg(pC + vecCA * i, pC + vecCA * (i + 1)));
    }

    private Point CalculateFergusonPoint(double u, Point r0, Point r1, Vector tan0, Vector tan1)
    {
        // Базисні функції Фергюсона
        double u2 = u * u;
        double u3 = u2 * u;

        double h1 = 2 * u3 - 3 * u2 + 1;    // Коеф при r0
        double h2 = -2 * u3 + 3 * u2;       // Коеф при r1
        double h3 = u3 - 2 * u2 + u;        // Коеф при tan0
        double h4 = u3 - u2;                // Коеф при tan1

        double x = h1 * r0.X + h2 * r1.X + h3 * tan0.X + h4 * tan1.X;
        double y = h1 * r0.Y + h2 * r1.Y + h3 * tan0.Y + h4 * tan1.Y;

        return new Point(x, y);
    }

    private void Reset(object obj)
    {
        OriginalSegments.Clear();
        foreach (var x in _startStateSegments)
        {
            var newSeg = new FergusonSegment(x.P0, x.C0, x.C1, x.P1);
            newSeg.PropertyChanged += (s, e) => UpdateAndApplyTransforms();
            OriginalSegments.Add(newSeg);
        }

        _animationProgress = 0;
        _animationDirectionForward = true;
        _isAnimating = false;
        _animationTimer.Stop();
        OnPropertyChanged(nameof(AnimationButtonText));
        Scale = 1;
        Angle = 0;

        UpdateAndApplyTransforms();
    }

    #region Animation Methods

    private void ToggleAnimation(object obj)
    {
        _isAnimating = !_isAnimating;
        if (_isAnimating) _animationTimer.Start();
        else _animationTimer.Stop();
        OnPropertyChanged(nameof(AnimationButtonText));
    }

    private void AnimationTick(object sender, EventArgs e)
    {
        double step = 0.01;
        if (_animationDirectionForward) _animationProgress += step;
        else _animationProgress -= step;

        if (_animationProgress >= 1.0) { _animationProgress = 1.0; _animationDirectionForward = false; }
        if (_animationProgress <= 0.0) { _animationProgress = 0.0; _animationDirectionForward = true; }


        // Інтерполяція
        for (int i = 0; i < OriginalSegments.Count; i++)
        {
            var start = _startStateSegments[i];
            var end = _targetStateSegments[i];
            var current = OriginalSegments[i];

            current.P0 = Lerp(start.P0, end.P0, _animationProgress);
            current.P1 = Lerp(start.P1, end.P1, _animationProgress);
            current.C0 = Lerp(start.C0, end.C0, _animationProgress);
            current.C1 = Lerp(start.C1, end.C1, _animationProgress);
        }

    }

    private Point Lerp(Point a, Point b, double t)
    {
        return new Point(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    }

    #endregion
}
