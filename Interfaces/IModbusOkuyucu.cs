using ModbusIzleme.Models;

namespace ModbusIzleme.Interfaces;

public interface IModbusOkuyucu : IDisposable
{
    void Baglan(string sunucuAdresi, int port);

    Task<float> TekRegisterOkuAsync(byte birimKimligi, int registerAdresi, float olcekFaktoru, CancellationToken cancellationToken = default);
}
