using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphicModelling.Models;

public class FergusonSegment : INotifyPropertyChanged
{
    private Point _p0; // Початкова точка
    private Point _p1; // Кінцева точка
    private Point _c0; // Керуюча точка для дотичної початку
    private Point _c1; // Керуюча точка для дотичної кінця

    #region Properties
    public Point P0
    {
        get => _p0;
        set { _p0 = value; OnPropertyChanged(); OnPropertyChanged(nameof(Tangent0)); }
    }

    public Point P1
    {
        get => _p1;
        set { _p1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(Tangent1)); }
    }

    public Point C0
    {
        get => _c0;
        set { _c0 = value; OnPropertyChanged(); OnPropertyChanged(nameof(Tangent0)); }
    }

    public Point C1
    {
        get => _c1;
        set { _c1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(Tangent1)); }
    }

    public Vector Tangent0 => C0 - P0;
    public Vector Tangent1 => P1 - C1;

    #endregion

    public FergusonSegment(Point p0, Point c0, Point c1, Point p1)
    {
        _p0 = p0;
        _c0 = c0;
        _c1 = c1;
        _p1 = p1;
    }

    #region OnPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}
