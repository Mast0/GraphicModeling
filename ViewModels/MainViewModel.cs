using GraphicModelling.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GraphicModelling.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Segment> Segments { get; set; }

    private Segment _selectedSegment;

    public Segment SelectedSegment
    {
        get => _selectedSegment;
        set { _selectedSegment = value; OnPropertyChanged(); }
    }

    private double _angle;
    public double Angle
    {
        get => _angle;
        set { _angle = value; OnPropertyChanged(); UpdateTransform(); }
    }

    private Point _rotationCenter = new Point(5,5);
    public Point RotationCenter
    {
        get => _rotationCenter;
        set { _rotationCenter = value; OnPropertyChanged(); UpdateTransform(); }
    }

    // Матричні параметри для афінної трансформації
    private double _m11 = 1, _m12 = 0, _m21 = 0, _m22 = 1, _offsetX = 0, _offsetY = 0;
    public double M11 { get => _m11; set { _m11 = value; OnPropertyChanged(); UpdateTransform(); } }
    public double M12 { get => _m12; set { _m12 = value; OnPropertyChanged(); UpdateTransform(); } }
    public double M21 { get => _m21; set { _m21 = value; OnPropertyChanged(); UpdateTransform(); } }
    public double M22 { get => _m22; set { _m22 = value; OnPropertyChanged(); UpdateTransform(); } }
    public double OffsetX { get => _offsetX; set { _offsetX = value; OnPropertyChanged(); UpdateTransform(); } }
    public double OffsetY { get => _offsetY; set { _offsetY = value; OnPropertyChanged(); UpdateTransform(); } }

    // Додаткові параметри для проективної трансформації
    private double _m31 = 0, _m32 = 0;
    public double M31 { get => _m31; set { _m31 = value; OnPropertyChanged(); UpdateTransform(); } } // wx
    public double M32 { get => _m32; set { _m32 = value; OnPropertyChanged(); UpdateTransform(); } } // wy

    private bool _isAffine;
    public bool IsAffine
    {
        get => _isAffine;
        set { _isAffine = value; OnPropertyChanged(); UpdateTransform(); }
    }

    private bool _isProjective;
    public bool IsProjective
    {
        get => _isProjective;
        set { _isProjective = value; OnPropertyChanged(); UpdateTransform(); }
    }

    private MatrixTransform _transform;
    public MatrixTransform Transform
    {
        get => _transform;
        set { _transform = value; OnPropertyChanged(); }
    }

    // Команди
    public ICommand AddSegmentCommand { get; }
    public ICommand RemoveSegmentCommand { get; }
    public ICommand ResetTransformCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    public MainViewModel()
    {
        Segments = new ObservableCollection<Segment>();
        LoadDefaultShape();

        Transform = new MatrixTransform();

        AddSegmentCommand = new MainViewModelCommand(AddSegment);
        RemoveSegmentCommand = new MainViewModelCommand(RemoveSegment, CanRemoveSegment);
        ResetTransformCommand = new MainViewModelCommand(ResetTransform);
        SaveCommand = new MainViewModelCommand(SaveToFile);
        LoadCommand = new MainViewModelCommand(LoadFromFile);

        UpdateTransform();
    }

    private void LoadDefaultShape()
    {
        Segments.Clear();
        Segments.Add(new Segment(new Point(10, 10), new Point(20, 10)));
        Segments.Add(new Segment(new Point(20, 10), new Point(20, 20)));
        Segments.Add(new Segment(new Point(20, 20), new Point(10, 20)));
        Segments.Add(new Segment(new Point(10, 20), new Point(10, 10)));
    }

    private void UpdateTransform()
    {
        Matrix m = new Matrix();

        // Обертання навколо центру
        m.RotateAt(Angle, RotationCenter.X, RotationCenter.Y);

        // Афінна/Проективна трансформація
        if (IsAffine || IsProjective)
        {
            // Поки тіки афінна
            var affineMatrix = new Matrix(M11, M12, M21, M22, OffsetX, OffsetY);
            m = Matrix.Multiply(m, affineMatrix);
        }

        Transform = new MatrixTransform(m);
    }

    private void AddSegment(object obj)
    {
        var newSegment = new Segment(new Point(5, 5), new Point(10, 5));
        Segments.Add(newSegment);
        SelectedSegment = newSegment;
    }

    private void RemoveSegment(object obj)
    {
        if (SelectedSegment != null)
        {
            Segments.Remove(SelectedSegment);
        }
    }

    private bool CanRemoveSegment(object obj) => SelectedSegment != null;

    private void ResetTransform(object obj)
    {
        Angle = 0;
        RotationCenter = new Point(5, 5);
        M11 = 1; M12 = 0;
        M21 = 0; M22 = 1;
        OffsetX = 0; OffsetY = 0;
        M31 = 0; M32 = 0;
        IsAffine = false;
        IsProjective = false;

        UpdateTransform();
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
            var json = JsonConvert.SerializeObject(Segments, Formatting.Indented);
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
            Segments.Clear();
            foreach (var seg in loadedSegments)
            {
                Segments.Add(seg);
            }
        }
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
}
