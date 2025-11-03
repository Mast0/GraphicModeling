using GraphicModelling.Helpers;
using GraphicModelling.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GraphicModelling.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const int PIXELS_IN_SANTIMETER = 35;

    private ObservableCollection<Segment> _originalSegments;
    private ObservableCollection<CircleShape> _originalCircles;
    private readonly ObservableCollection<Segment> _originalGridLines = new ObservableCollection<Segment>();

    public ObservableCollection<Segment> OriginalSegments { get; set; }
    public ObservableCollection<CircleShape> OriginalCircles { get; set; }

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

    private double _scale = 1.0;
    public double Scale
    {
        get => _scale;
        set { _scale = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    private Point _rotationCenter = new Point(4.7 * PIXELS_IN_SANTIMETER, 12.5 * PIXELS_IN_SANTIMETER);
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

    private Point _transformedRotationCenter;
    public Point TransformedRotationCenter
    {
        get => _transformedRotationCenter;
        private set { _transformedRotationCenter = value; OnPropertyChanged(); }
    }

    // Матричні параметри для афінної трансформації
    private double _m00 = 1, _m01 = 0, _m02 = 0;
    private double _m10 = 0, _m11 = 1, _m12 = 0;
    private double _m20 = 0, _m21 = 0, _m22 = 1;

    public double M00 { get => _m00; set { _m00 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // xx
    public double M01 { get => _m01; set { _m01 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // xy
    public double M02 { get => _m02; set { _m02 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // wx (projective)
    public double M10 { get => _m10; set { _m10 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // yx
    public double M11 { get => _m11; set { _m11 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // yy
    public double M12 { get => _m12; set { _m12 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // wy (projective)
    public double M20 { get => _m20; set { _m20 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // dx (offset)
    public double M21 { get => _m21; set { _m21 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // dy (offset)
    public double M22 { get => _m22; set { _m22 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // w0 - вага для початку координат

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
        OriginalSegments = new ObservableCollection<Segment>();
        OriginalCircles = new ObservableCollection<CircleShape>();

        TransformedSegments = new ObservableCollection<Segment>();
        TransformedGridLines = new ObservableCollection<Segment>();

        CreateModel();
        //CreateOriginalShape();
        ResetTransformCommand = new MainViewModelCommand(ResetTransform);

        UpdateAndApplyTransforms();
    }

    public void UpdateCanvasSize(double width, double height)
    {
        CreateOriginalGrid(width, height, PIXELS_IN_SANTIMETER);
        UpdateAndApplyTransforms();
    }

    private void CreateOriginalShape()
    {
        OriginalSegments = new ObservableCollection<Segment>
        {
            new Segment(1, 1, 2, 1),
            new Segment(2, 1, 2, 2),
            new Segment(2, 2, 1, 2),
            new Segment(1, 2, 1, 1)
        };
    }

    private void CreateModel()
    {
        OriginalSegments = new ObservableCollection<Segment>
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
            new Segment(8, 4, 7, 4)
        };

        foreach (var seg in OriginalSegments)
        {
            seg.PropertyChanged += (s, e) => UpdateAndApplyTransforms();
        }

        OriginalCircles = new ObservableCollection<CircleShape>
        {
            new CircleShape(4.7, 12.5, 0.6)
        };

        foreach (var circle in OriginalCircles)
        {
            circle.PropertyChanged += (s, e) => UpdateAndApplyTransforms();
        }
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
                M20, M21, IsProjective ? M22 : 1);
        }

        Matrix3x3 tempMatrix = new();
        if (isRotate)
        {
            Matrix3x3 rotationMatrix = Matrix3x3.CreateRotation(Angle, RotationCenter);
            Matrix3x3 scalingMatrix = Matrix3x3.CreateScaling(Scale, RotationCenter);

            tempMatrix = Matrix3x3.Multiply(scalingMatrix, rotationMatrix);

            return Matrix3x3.Multiply(tempMatrix, userMatrix);
        }

        else return userMatrix;
    }

    private void UpdateAndApplyTransforms(bool withRotationCenter = true)
    {
        Matrix3x3 finalMatrix = BuildTransformationMatrix(true);

        // -----------------
        // ------Фігура-----
        // -----------------
        TransformedSegments.Clear();
        foreach (var orSeg in OriginalSegments)
        {
            Point newStart = TransformationHelper.ApplyTransformations(orSeg.StartPoint, finalMatrix);
            Point newEnd = TransformationHelper.ApplyTransformations(orSeg.EndPoint, finalMatrix);
            TransformedSegments.Add(new Segment(newStart, newEnd));
        }

        // -----------------
        // -------Кола------
        // -----------------
        foreach (var orCircle in OriginalCircles)
        {
            List<Point> circlePoints = GeometryHelper.TessellateCircle(orCircle);

            for (int i = 0; i < circlePoints.Count - 1; i++)
            {
                Point transformedStart = TransformationHelper.ApplyTransformations(circlePoints[i], finalMatrix);
                Point transformedEnd = TransformationHelper.ApplyTransformations(circlePoints[i + 1], finalMatrix);
                TransformedSegments.Add(new Segment(transformedStart, transformedEnd));
            }
        }

        Matrix3x3 gridFinalMatrix = BuildTransformationMatrix(false);
        // -----------------
        // ------Сітка------
        // -----------------
        TransformedGridLines.Clear();
        foreach (var orGL in _originalGridLines)
        {
            Point newStart = TransformationHelper.ApplyTransformations(orGL.StartPoint, gridFinalMatrix);
            Point newEnd = TransformationHelper.ApplyTransformations(orGL.EndPoint, gridFinalMatrix);
            TransformedGridLines.Add(new Segment(newStart, newEnd));
        }

        TransformedRotationCenter = TransformationHelper.ApplyTransformations(RotationCenter, finalMatrix);
    }

    #region Command Handlers

    private void ResetTransform(object obj)
    {
        Angle = 0; Scale = 1.0;
        RotationCenter = new Point(4.7 * PIXELS_IN_SANTIMETER, 12.5 * PIXELS_IN_SANTIMETER); ;
        M00 = 1; M01 = 0; M02 = 0;
        M10 = 0; M11 = 1; M12 = 0; 
        M20 = 0; M21 = 0; M22 = 1;
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
