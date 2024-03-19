using Avalonia.Controls.Shapes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        OpenOrCreateFile();


    }

    private async void OpenOrCreateFile()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        path = System.IO.Path.Combine(path, "CRC-32_Module.v");

        FileInfo file = new FileInfo(path);

        if (!file.Exists)
        {
            file.Create();
        }

        await WriteInfo(path);
    }

    private async Task WriteInfo(string path)
    {
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            await writer.WriteLineAsync();
            await writer.WriteAsync("// CRC polynomial coefficients: ");
            await writer.WriteLineAsync(Polynom);
            await writer.WriteLineAsync("// CRC width:                   32 bits");
            await writer.WriteLineAsync("// CRC shift direction:         right (little endian)");
            await writer.WriteLineAsync("// Input word width:            8 bits");

            await writer.WriteLineAsync();
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("module CRC_32 (");
            await writer.WriteLineAsync("\tinput [31:0]     crcIn,");
            await writer.WriteLineAsync("\tinput [7:0]      data,");
            await writer.WriteLineAsync("\toutput [31:0]    crcOut");
            await writer.WriteLineAsync(");");


            await writer.WriteLineAsync("endmodule");
        }
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
