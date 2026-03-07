using ModbusIzleme.Models;

namespace ModbusIzleme.Interfaces;

public interface ICihazVerisiDeposu
{
    CihazVerisi? SonVeri { get; }

    void Guncelle(CihazVerisi veri);
}
