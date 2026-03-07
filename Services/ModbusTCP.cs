using FluentModbus;
using ModbusIzleme.Interfaces;
using System.Buffers.Binary;
using System.Net;

namespace ModbusIzleme.Services;

public class ModbusTCP : IModbusOkuyucu
{
    private ModbusTcpClient? _istemci;

    public void Baglan(string sunucuAdresi, int port)
    {
        _istemci?.Dispose();
        _istemci = new ModbusTcpClient();
        _istemci.Connect(new IPEndPoint(IPAddress.Parse(sunucuAdresi), port));
    }

    public async Task<float> TekRegisterOkuAsync(byte birimKimligi, int registerAdresi, float olcekFaktoru, CancellationToken cancellationToken = default)
    {
        if (_istemci is null)
            throw new InvalidOperationException("Modbus TCP baglantisi kurulmamis. Once Baglan() metodu cagrilmalidir.");

        var okunanDeger = await _istemci.ReadHoldingRegistersAsync<ushort>(birimKimligi, registerAdresi, 1, cancellationToken);
        var dizi = okunanDeger.ToArray();

        short duzeltilmisDeger = (short)BinaryPrimitives.ReverseEndianness(dizi[0]);

        return duzeltilmisDeger / olcekFaktoru;
    }

    /// <inheritdoc />
    public void Dispose() => _istemci?.Dispose();
}
