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

namespace GraphicModelling.ViewModels;

public class Lab7ViewModel : BaseViewModel
{
    public ObservableCollection<Segment> FractalSegments { get; set; }

    #region L-System Parameters
    private string _axiom = "X";
    public string Axiom
    {
        get => _axiom;
        set { _axiom = value; OnPropertyChanged(); }
    }

    private string _rulesText = "X->+FXF-FXF-FXF+\nF->FXF";
    public string RulesText
    {
        get => _rulesText;
        set { _rulesText = value; OnPropertyChanged(); }
    }

    private int _iterations = 4;
    public int Iterations
    {
        get => _iterations;
        set
        {
            if (value < 0) value = 0;
            if (value > 4) value = 4;
            _iterations = value;
            OnPropertyChanged();
            GenerateFractal();
        }
    }

    private double _angle = 60.0;
    public double Angle
    {
        get => _angle;
        set { _angle = value; OnPropertyChanged(); GenerateFractal(); }
    }

    private double _startLength = 10.0;
    #endregion

    #region Visualization Parameters
    private bool _autoScale = true;
    public bool AutoScale
    {
        get => _autoScale;
        set { _autoScale = value; OnPropertyChanged(); GenerateFractal(); }
    }
    #endregion

    public ICommand GenerateCommand { get; }

    public Lab7ViewModel()
    {
        FractalSegments = new ObservableCollection<Segment>();
        GenerateCommand = new MainViewModelCommand(obj => GenerateFractal());

        GenerateFractal();
    }

    private void GenerateFractal()
    {
        FractalSegments.Clear();

        // 1. Парсинг правил
        var rules = ParseRules(RulesText);

        // 2. Генерація рядка L-системи
        string currentString = Axiom;
        for (int i = 0; i < Iterations; i++)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in currentString)
            {
                if (rules.ContainsKey(c))
                {
                    sb.Append(rules[c]);
                }
                else
                {
                    sb.Append(c);
                }
            }
            currentString = sb.ToString();
        }

        // 3. Інтерпретація
        var points = InterpretString(currentString);

        // 4. Масштабування та відображення
        if (points.Count < 2) return;

        // Знаходимо межі
        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;

        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X;
            if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.Y > maxY) maxY = p.Y;
        }

        double width = maxX - minX;
        double height = maxY - minY;
        if (width == 0) width = 1;
        if (height == 0) height = 1;

        double canvasWidth = 450;
        double canvasHeight = 800;
        double padding = 20;

        double scaleX = (canvasWidth - 2 * padding) / width;
        double scaleY = (canvasHeight - 2 * padding) / height;
        double scale = _autoScale ? Math.Min(scaleX, scaleY) : 1.0;

        double offsetX = padding - minX * scale + (canvasWidth - 2 * padding - width * scale) / 2;
        double offsetY = padding - minY * scale + (canvasHeight - 2 * padding - height * scale) / 2;

        Point p1 = points[0];
        Point screenP1 = new Point(p1.X * scale + offsetX, p1.Y * scale + offsetY);

        // Створюємо сегменти
        for (int i = 1; i < points.Count; i++)
        {
            Point p2 = points[i];
            Point screenP2 = new Point(p2.X * scale + offsetX, p2.Y * scale + offsetY);

            // Градієнт від синього до фіолетового
            byte r = (byte)(255 * i / points.Count);
            byte b = (byte)(255 - r);
            Brush color = new SolidColorBrush(Color.FromRgb(r, 0, b));
            color.Freeze();

            FractalSegments.Add(new Segment(screenP1, screenP2, color));
            screenP1 = screenP2;
        }
    }

    private Dictionary<char, string> ParseRules(string text)
    {
        var dict = new Dictionary<char, string>();
        if (string.IsNullOrWhiteSpace(text)) return dict;

        var lines = text.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { "->", "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                char key = parts[0].Trim()[0];
                string val = parts[1].Trim();
                if (!dict.ContainsKey(key))
                    dict.Add(key, val);
            }
        }
        return dict;
    }

    private List<Point> InterpretString(string lSystemString)
    {
        var points = new List<Point>();
        double x = 0, y = 0;
        double currentAngle = 0;

        // Стек для збереження стану
        Stack<(double x, double y, double angle)> stateStack = new Stack<(double, double, double)>();

        points.Add(new Point(x, y));

        // Наконечник Серпінського:
        // F - малює, X - ігнорується (допоміжна змінна), +, - повороти

        foreach (char c in lSystemString)
        {
            switch (c)
            {
                case 'F': // Draw forward
                case 'A': // Cиноніми F
                case 'B':
                    double rad = currentAngle * Math.PI / 180.0;
                    x += _startLength * Math.Cos(rad);
                    y += _startLength * Math.Sin(rad);
                    points.Add(new Point(x, y));
                    break;
                case '+': // Turn right
                    currentAngle += Angle;
                    break;
                case '-': // Turn left
                    currentAngle -= Angle;
                    break;
                case '[': // Save state
                    stateStack.Push((x, y, currentAngle));
                    break;
                case ']': // Restore state
                    if (stateStack.Count > 0)
                    {
                        var state = stateStack.Pop();
                        x = state.x;
                        y = state.y;
                        currentAngle = state.angle;
                        points.Add(new Point(x, y));
                    }
                    break;
            }
        }

        return points;
    }

    public override void UpdateAndApplyTransforms() { }
}
