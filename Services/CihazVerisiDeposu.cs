using ModbusIzleme.Interfaces;
using ModbusIzleme.Models;

namespace ModbusIzleme.Services;

public class CihazVerisiDeposu : ICihazVerisiDeposu
{
    private readonly object _kilit = new();
    private CihazVerisi? _sonVeri;

    public CihazVerisi? SonVeri
    {
        get { lock (_kilit) { return _sonVeri; } }
    }

    public void Guncelle(CihazVerisi veri)
    {
        lock (_kilit) { _sonVeri = veri; }
    }
}
