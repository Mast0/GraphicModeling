using GraphicModelling.Helpers;
using GraphicModelling.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GraphicModelling.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Segment> _originalSegments;
    private readonly List<Segment> _originalGridLines = new List<Segment>();
    //private Segment _originalAxisX;
    //private Segment _originalAxisY;

    public ObservableCollection<Segment> TransformedSegments { get; set; }
    public ObservableCollection<Segment> TransformedGridLines { get; set; }

    private Segment _transformedAxisX;
    public Segment TransformedAxisX
    {
        get => _transformedAxisX;
        set { _transformedAxisX = value; OnPropertyChanged(); }
    }

    private Segment _transformedAxisY;
    public Segment TransformedAxisY
    {
        get => _transformedAxisY;
        set { _transformedAxisY = value; OnPropertyChanged(); }
    }

    #region Transformation props
    private double _angle;
    public double Angle
    {
        get => _angle;
        set { _angle = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    private Point _rotationCenter = new Point(15, 15);
    public Point RotationCenter
    {
        get => _rotationCenter;
        set 
        { 
            _rotationCenter = value; 
            OnPropertyChanged();
            UpdateAndApplyTransforms(); 
        }
    }

    // Матричні параметри для афінної трансформації
    private double _m00 = 1, _m01 = 0, _m10 = 0, _m11 = 1, _m20 = 0, _m21 = 0;
    private double _m02 = 0, _m12 = 0;

    public double M00 { get => _m00; set { _m00 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // xx
    public double M01 { get => _m01; set { _m01 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // xy
    public double M10 { get => _m10; set { _m10 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // yx
    public double M11 { get => _m11; set { _m11 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // yy
    public double M20 { get => _m20; set { _m20 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // dx (offset)
    public double M21 { get => _m21; set { _m21 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // dy (offset)
    public double M02 { get => _m02; set { _m02 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // wx (projective)
    public double M12 { get => _m12; set { _m12 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // wy (projective)

    private bool _isAffine;
    public bool IsAffine
    {
        get => _isAffine;
        set { _isAffine = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    private bool _isProjective;
    public bool IsProjective
    {
        get => _isProjective;
        set { _isProjective = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }
    #endregion

    #region Commands
    public ICommand ResetTransformCommand { get; }
    #endregion

    public MainViewModel()
    {
        TransformedSegments = new ObservableCollection<Segment>();
        TransformedGridLines = new ObservableCollection<Segment>();

        CreateModel();
        //CreateOriginalShape();
        //_originalAxisX = new Segment(new Point(0, 0), new Point(0, 600));
        //_originalAxisY = new Segment(new Point(0, 0), new Point(600, 0));
        ResetTransformCommand = new MainViewModelCommand(ResetTransform);

        UpdateAndApplyTransforms();
    }

    public void UpdateCanvasSize(double width, double height)
    {
        //_originalAxisX = new Segment(new Point(0, 0), new Point(0, width));
        //_originalAxisY = new Segment(new Point(0, 0), new Point(height, 0));
        CreateOriginalGrid(width, height, 35);
        UpdateAndApplyTransforms();
    }

    private void CreateOriginalShape()
    {
        _originalSegments = new ObservableCollection<Segment>
        {
            new Segment(1, 1, 2, 1),
            new Segment(2, 1, 2, 2, 60, false),
            new Segment(2, 2, 1, 2),
            new Segment(1, 2, 1, 1)
        };
    }

    private void CreateModel()
    {
        _originalSegments = new ObservableCollection<Segment>
        {
            new Segment(2, 15, 7, 15),
            new Segment(7, 16, 7, 4),
            new Segment(2, 10, 2, 15),
            new Segment(2, 10, 4.1, 10),
            new Segment(4.1, 10, 7, 8),
            new Segment(7, 16, 9, 16),
            new Segment(9, 16, 9, 3.8),
            new Segment(9, 3.8, 8, 3.8),
            new Segment(8, 3.8, 8, 4),
            new Segment(8, 4, 7, 4),
            //new Segment(4.1, 12.5, 5.2, 12.5, 180)
        };
    }

    private void CreateOriginalGrid(double width, double height, int step)
    {
        _originalGridLines.Clear();
        for (int x = 0; x <= width; x += step)
            _originalGridLines.Add(new Segment(new Point(x, 0), new Point(x, height)));
        for (int y = 0; y <= height; y += step)
            _originalGridLines.Add(new Segment(new Point(0, y), new Point(width, y)));
    }

    private Matrix3x3 BuildTransformationMatrix(bool isRotate)
    {
        Matrix3x3 userMatrix = Matrix3x3.Identity;
        if (IsAffine || IsProjective)
        {
            userMatrix = new Matrix3x3(
                M00, M01, IsProjective ? M02 : 0,
                M10, M11, IsProjective ? M12 : 0,
                M20, M21, 1);
        }

        if (isRotate)
        {
            Matrix3x3 rotationMatrix = Matrix3x3.CreateRotation(Angle, RotationCenter);

            return Matrix3x3.Multiply(rotationMatrix, userMatrix);
        }
        else return userMatrix;
    }

    private void UpdateAndApplyTransforms()
    {
        Matrix3x3 finalMatrix = BuildTransformationMatrix(true);

        // -----------------
        // ------Фігура-----
        // -----------------
        TransformedSegments.Clear();
        foreach (var orSeg in _originalSegments)
        {
            // Поточний сегмент дуга?
            if (orSeg.Radius > 0 && orSeg.StartPoint != orSeg.EndPoint)
            {
                // Розбиваємо дугу на набір точок
                var arcPoints = GeometryHelper.TessellateArc(orSeg);

                // Трансформуємо кожну точку і створюємо з них маленькі прямі відрізки
                for (int i = 0; i < arcPoints.Count - 1; i++)
                {
                    var transformedStart = TransformationHelper.ApplyTransformations(arcPoints[i], finalMatrix);
                    var transformedEnd = TransformationHelper.ApplyTransformations(arcPoints[i + 1], finalMatrix);
                    TransformedSegments.Add(new Segment(transformedStart, transformedEnd));
                }
            }
            else
            {
                Point newStart = TransformationHelper.ApplyTransformations(orSeg.StartPoint, finalMatrix);
                Point newEnd = TransformationHelper.ApplyTransformations(orSeg.EndPoint, finalMatrix);
                TransformedSegments.Add(new Segment(newStart, newEnd));
            }
        }

        Matrix3x3 gridFinalMatrix = BuildTransformationMatrix(false);
        // -----------------
        // ------Сітка------
        // -----------------
        TransformedGridLines.Clear();
        foreach (var orGL in _originalGridLines)
        {
            Point newStart = TransformationHelper.ApplyTransformations(
                orGL.StartPoint, gridFinalMatrix);
            Point newEnd = TransformationHelper.ApplyTransformations(
                orGL.EndPoint, gridFinalMatrix);
            TransformedGridLines.Add(new Segment(newStart, newEnd));
        }

        // -----------------
        // -------Осі-------
        // -----------------
        //Point axisXStart = TransformationHelper.ApplyTransformations(
        //        _originalAxisX.StartPoint, gridFinalMatrix);
        //Point axisXEnd = TransformationHelper.ApplyTransformations(
        //        _originalAxisX.EndPoint, gridFinalMatrix);
        //TransformedAxisX = new Segment(axisXStart, axisXEnd);

        //Point axisYStart = TransformationHelper.ApplyTransformations(
        //        _originalAxisY.StartPoint, gridFinalMatrix);
        //Point axisYEnd = TransformationHelper.ApplyTransformations(
        //        _originalAxisY.EndPoint, gridFinalMatrix);
        //TransformedAxisX = new Segment(axisYStart, axisYEnd);
    }

    #region Command Handlers

    private void ResetTransform(object obj)
    {
        Angle = 0;
        RotationCenter = new Point(150, 150);
        M00 = 1; M01 = 0; M02 = 0;
        M11 = 1; M12 = 0; 
        M20 = 0; M21 = 0;
        IsAffine = false;
        IsProjective = false;
    }
    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
}
