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

public class Lab1ViewModel : BaseViewModel
{
    private ObservableCollection<Segment> _originalSegments;
    private ObservableCollection<CircleShape> _originalCircles;

    public ObservableCollection<Segment> OriginalSegments { get; set; }
    public ObservableCollection<CircleShape> OriginalCircles { get; set; }

    public ObservableCollection<Segment> TransformedSegments { get; set; }

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

    #region Commands
    public ICommand ResetTransformCommand { get; }
    #endregion

    public Lab1ViewModel() : base()
    {
        OriginalSegments = new ObservableCollection<Segment>();
        OriginalCircles = new ObservableCollection<CircleShape>();

        TransformedSegments = new ObservableCollection<Segment>();
        CreateModel();
        //CreateOriginalShape();
        ResetTransformCommand = new MainViewModelCommand(ResetTransform);

        UpdateAndApplyTransforms();
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

    public override void UpdateAndApplyTransforms()
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
        Xx = 1; Xy = 0; Wx = 0;
        Yx = 0; Yy = 1; Wy = 0; 
        Dx = 0; Dy = 0; W0 = 1;
        IsAffine = false;
        IsProjective = false;
    }
    #endregion
}
