using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace CRC_32.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    private Timer _timer;

    public MainViewModel() 
    {
        for (int i = 0; i < 32; i++)
        {
            PolyValue.Add(false);
        }

        _timer = new Timer(ChagePolynom, null, 0, 250);
    }

    public ObservableCollection<bool> PolyValue { get; set; } = new ObservableCollection<bool>();

    private string _pol;
    public string Polynom
    {
        get => _pol;
        set
        {
            this.RaiseAndSetIfChanged(ref _pol, value);
        }
    }

    public void Generate()
    {

    }

    private void ChagePolynom(object? state)
    {
        Polynom = string.Empty;

        for (int i = 31; i >= 0; i--)
        {
            if (PolyValue[i]) Polynom += "X^" + (i + 1).ToString() + "+";
        }

        //if (Polynom.Length > 0 ) Polynom = Polynom.TrimEnd('+');
        if (Polynom.Length > 0) Polynom += "1"; 
    }
}
