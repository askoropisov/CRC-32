using Avalonia.Controls.Shapes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRC_32.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Timer _timer;

    private uint[] Crc32Table;

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

    private string _inData;
    public string InputData
    {
        get => _inData;
        set
        {
            this.RaiseAndSetIfChanged(ref _inData, value);
        }
    }

    private string _outData;
    public string OutputData
    {
        get => _outData;
        set
        {
            this.RaiseAndSetIfChanged(ref _outData, value);
        }
    }

    private bool _isReverse;
    public bool IsReverse
    {
        get => _isReverse;
        set => this.RaiseAndSetIfChanged(ref _isReverse, value);
    }

    public void Generate()
    {
        if (!string.IsNullOrEmpty(Polynom))
        {
            CreateTable();

            byte[] data = Encoding.UTF8.GetBytes(InputData);
            uint crc32 = CalculateCRC32(data);

            OutputData = crc32.ToString("X");
        }
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

    public uint BitsToUInt(bool[] bits)
    {
        uint result = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                result |= (uint)(1 << i); // Устанавливаем i-тый бит в единицу
            }
        }
        return result;
    }

    private void CreateTable()
    {
        uint polynom = 0;

        //string test = "00000100110000010001110110110111";
        //string tesn = "11101101101110001000001100100000";

        if (IsReverse)
        {
            var array = PolyValue.ToArray();
            Array.Reverse(array);
            polynom = BitsToUInt(array);
        }
        else polynom = BitsToUInt(PolyValue.ToArray());

        Crc32Table = new uint[256];

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 8; j > 0; j--)
            {
                if ((crc & 1) == 1)
                {
                    crc = (crc >> 1) ^ polynom;
                }
                else
                {
                    crc >>= 1;
                }
            }
            Crc32Table[i] = crc;
        }
    }

    public uint CalculateCRC32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
        {
            byte index = (byte)(((crc) & 0xFF) ^ b);
            crc = (crc >> 8) ^ Crc32Table[index];
        }
        return ~crc;
    }

    private void ChagePolynom(object? state)
    {
        Polynom = string.Empty;

        for (int i = 31; i >= 0; i--)
        {
            if (PolyValue[i])
            {
                if (i == 0) Polynom += "1"; 
                else Polynom += "X^" + (i).ToString() + "+";
            }
        }
    }
}
