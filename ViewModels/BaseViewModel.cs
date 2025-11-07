using GraphicModelling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphicModelling.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    protected const int PIXELS_IN_SANTIMETER = 35;

    protected readonly ObservableCollection<Segment> _originalGridLines = new ObservableCollection<Segment>();
    public ObservableCollection<Segment> TransformedGridLines { get; set; }

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
        protected set { _transformedRotationCenter = value; OnPropertyChanged(); }
    }

    // Матричні параметри для афінної та проєктивної трансформацій
    private double _xx = 1, _xy = 0, _wx = 0;
    private double _yx = 0, _yy = 1, _wy = 0;
    private double _dx = 0, _dy = 0, _w0 = 1;

    public double Xx { get => _xx; set { _xx = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // xx
    public double Xy { get => _xy; set { _xy = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // xy
    public double Wx { get => _wx; set { _wx = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // wx (projective)
    public double Yx { get => _yx; set { _yx = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // yx
    public double Yy { get => _yy; set { _yy = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // yy
    public double Wy { get => _wy; set { _wy = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // wy (projective)
    public double Dx { get => _dx; set { _dx = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // dx (offset)
    public double Dy { get => _dy; set { _dy = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // dy (offset)
    public double W0 { get => _w0; set { _w0 = value; OnPropertyChanged(); UpdateAndApplyTransforms(); } } // w0 - вага для початку координат

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

    public BaseViewModel()
    {
        TransformedGridLines = new ObservableCollection<Segment>();
    }

    public void UpdateCanvasSize(double width, double height)
    {
        CreateOriginalGrid(width, height, PIXELS_IN_SANTIMETER);
        UpdateAndApplyTransforms();
    }

    private void CreateOriginalGrid(double width, double height, int step)
    {
        _originalGridLines.Clear();
        for (int x = 0; x <= width; x += step)
            _originalGridLines.Add(new Segment(new Point(x, 0), new Point(x, height)));
        for (int y = 0; y <= height; y += step)
            _originalGridLines.Add(new Segment(new Point(0, y), new Point(width, y)));
    }

    protected Matrix3x3 BuildTransformationMatrix(bool isRotate)
    {
        Matrix3x3 userMatrix = Matrix3x3.Identity;
        if (IsAffine || IsProjective)
        {
            userMatrix = new Matrix3x3(
                IsProjective ? Xx * Wx : Xx, IsProjective ? Xy * Wx : Xy, IsProjective ? Wx : 0,
                IsProjective ? Yx * Wy : Yx, IsProjective ? Yy * Wy : Yy, IsProjective ? Wy : 0,
                IsProjective ? Dx * W0 : Dx, IsProjective ? Dy * W0 : Dy, IsProjective ? W0 : 1);
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

    public abstract void UpdateAndApplyTransforms();

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
}
