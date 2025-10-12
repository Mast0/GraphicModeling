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
    private readonly Segment _originalAxisX;
    private readonly Segment _originalAxisY;

    public ObservableCollection<Segment> OriginalSegments
    {
        get => _originalSegments;
        set { _originalSegments = value; OnPropertyChanged(); }
    }

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

    #region SelectedSegment
    private Segment _selectedSegment;
    public Segment SelectedSegment
    {
        get => _selectedSegment;
        set { _selectedSegment = value; OnPropertyChanged(); }
    }

    public double SelectedSegmentStartX
    {
        get 
        { 
            if (_selectedSegment != null)
                return _selectedSegment.StartPoint.X;
            return 0;
        }
        set 
        { 
            var newPoint = _selectedSegment.StartPoint;
            newPoint.X = value;
            _selectedSegment.StartPoint = newPoint;
            OnPropertyChanged(); 
            UpdateAndApplyTransforms(); 
        }
    }

    public double SelectedSegmentStartY
    {
        get
        {
            if (_selectedSegment != null)
                return _selectedSegment.StartPoint.Y;
            return 0;
        }
        set
        {
            var newPoint = _selectedSegment.StartPoint;
            newPoint.Y = value;
            _selectedSegment.StartPoint = newPoint;
            OnPropertyChanged();
            UpdateAndApplyTransforms();
        }
    }

    public double SelectedSegmentEndX
    {
        get
        {
            if (_selectedSegment != null)
                return _selectedSegment.EndPoint.X;
            return 0;
        }
        set
        {
            var newPoint = _selectedSegment.EndPoint;
            newPoint.X = value;
            _selectedSegment.EndPoint = newPoint;
            OnPropertyChanged();
            UpdateAndApplyTransforms();
        }
    }

    public double SelectedSegmentEndY
    {
        get
        {
            if (_selectedSegment != null)
                return _selectedSegment.EndPoint.Y;
            return 0;
        }
        set
        {
            var newPoint = _selectedSegment.EndPoint;
            newPoint.Y = value;
            _selectedSegment.EndPoint = newPoint;
            OnPropertyChanged();
            UpdateAndApplyTransforms();
        }
    }
    #endregion

    #region Transformation props
    private double _angle;
    public double Angle
    {
        get => _angle;
        set { _angle = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    #region RotationCenter
    private Point _rotationCenter = new Point(150, 150);
    public Point RotationCenter
    {
        get => _rotationCenter;
        set { _rotationCenter = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    public double RotationCenterX
    {
        get => _rotationCenter.X;
        set { _rotationCenter.X = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }

    public double RotationCenterY
    {
        get => _rotationCenter.Y;
        set { _rotationCenter.Y = value; OnPropertyChanged(); UpdateAndApplyTransforms(); }
    }
    #endregion

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
    public ICommand AddSegmentCommand { get; }
    public ICommand RemoveSegmentCommand { get; }
    public ICommand ResetTransformCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }
    #endregion

    public MainViewModel()
    {
        TransformedSegments = new ObservableCollection<Segment>();
        TransformedGridLines = new ObservableCollection<Segment>();

        CreateOriginalShape();
        _originalAxisX = new Segment(new Point(0, 300), new Point(600, 300));
        _originalAxisY = new Segment(new Point(300, 0), new Point(300, 600));
        CreateOriginalGrid(600, 600, 20);

        AddSegmentCommand = new MainViewModelCommand(AddSegment);
        RemoveSegmentCommand = new MainViewModelCommand(RemoveSegment, CanRemoveSegment);
        ResetTransformCommand = new MainViewModelCommand(ResetTransform);
        SaveCommand = new MainViewModelCommand(SaveToFile);
        LoadCommand = new MainViewModelCommand(LoadFromFile);

        UpdateAndApplyTransforms();
    }

    private void CreateOriginalShape()
    {
        _originalSegments = new ObservableCollection<Segment>
        {
            new Segment(new Point(100, 100), new Point(200, 100)),
            new Segment(new Point(200, 100), new Point(200, 200)),
            new Segment(new Point(200, 200), new Point(100, 200)),
            new Segment(new Point(100, 200), new Point(100, 100))
        };
    }

    private void CreateOriginalGrid(int width, int height, int step)
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
            Point newStart = TransformationHelper.ApplyTransformations(
                orSeg.StartPoint, finalMatrix);
            Point newEnd = TransformationHelper.ApplyTransformations(
                orSeg.EndPoint, finalMatrix);
            TransformedSegments.Add(new Segment(newStart, newEnd));
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
        Point axisXStart = TransformationHelper.ApplyTransformations(
                _originalAxisX.StartPoint, gridFinalMatrix);
        Point axisXEnd = TransformationHelper.ApplyTransformations(
                _originalAxisX.EndPoint, gridFinalMatrix);
        TransformedAxisX = new Segment(axisXStart, axisXEnd);

        Point axisYStart = TransformationHelper.ApplyTransformations(
                _originalAxisY.StartPoint, gridFinalMatrix);
        Point axisYEnd = TransformationHelper.ApplyTransformations(
                _originalAxisY.EndPoint, gridFinalMatrix);
        TransformedAxisX = new Segment(axisYStart, axisYEnd);
    }

    #region Command Handlers
    private void AddSegment(object obj)
    {
        var newSegment = new Segment(new Point(50, 50), new Point(100, 50));
        _originalSegments.Add(newSegment);
        SelectedSegment = newSegment;
        UpdateAndApplyTransforms();
    }

    private void RemoveSegment(object obj)
    {
        if (SelectedSegment != null)
        {
            _originalSegments.Remove(SelectedSegment);
            UpdateAndApplyTransforms();
        }
    }

    private bool CanRemoveSegment(object obj) => SelectedSegment != null;

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

    private void SaveToFile(object obj)
    {
        var sfd = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Зберегти фігуру"
        };

        if (sfd.ShowDialog() == true)
        {
            var json = JsonConvert.SerializeObject(_originalSegments, Formatting.Indented);
            File.WriteAllText(sfd.FileName, json);
        }
    }

    private void LoadFromFile(object obj)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Завантажити фігуру"
        };

        if (ofd.ShowDialog() == true)
        {
            var json = File.ReadAllText(ofd.FileName);
            var loadedSegments = JsonConvert.DeserializeObject<ObservableCollection<Segment>>(json);
            _originalSegments.Clear();
            foreach (var seg in loadedSegments)
            {
                _originalSegments.Add(seg);
            }
        }
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
