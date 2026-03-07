using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using ModbusIzleme.Hubs;
using ModbusIzleme.Interfaces;
using ModbusIzleme.Models;

namespace ModbusIzleme.Services;

public class ModbusOkuyucuServis : BackgroundService
{
    private readonly IModbusOkuyucu              _modbusOkuyucu;
    private readonly ICihazVerisiDeposu          _veriDeposu;
    private readonly IHubContext<CihazHub>       _cihazHub;
    private readonly ModbusAyarlari              _ayarlar;
    private readonly ILogger<ModbusOkuyucuServis> _logger;

    public ModbusOkuyucuServis(
        IModbusOkuyucu               modbusOkuyucu,
        ICihazVerisiDeposu           veriDeposu,
        IHubContext<CihazHub>        cihazHub,
        IOptions<ModbusAyarlari>     ayarlar,
        ILogger<ModbusOkuyucuServis> logger)
    {
        _modbusOkuyucu = modbusOkuyucu;
        _veriDeposu    = veriDeposu;
        _cihazHub      = cihazHub;
        _ayarlar       = ayarlar.Value;
        _logger        = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _modbusOkuyucu.Baglan(_ayarlar.SunucuAdresi, _ayarlar.Port);

                _logger.LogInformation(
                    "Modbus TCP baglantisi basariyla kuruldu. Hedef: {Adres}:{Port}, BirimKimligi: {BirimKimligi}",
                    _ayarlar.SunucuAdresi, _ayarlar.Port, _ayarlar.BirimKimligi);

                while (!stoppingToken.IsCancellationRequested)
                {
                    float sicaklik = await _modbusOkuyucu.TekRegisterOkuAsync(
                        _ayarlar.BirimKimligi, _ayarlar.SicaklikAdresi, _ayarlar.OlcekFaktoru, stoppingToken);

                    float nem = await _modbusOkuyucu.TekRegisterOkuAsync(
                        _ayarlar.BirimKimligi, _ayarlar.NemAdresi, _ayarlar.OlcekFaktoru, stoppingToken);

                    var cihazVerisi = new CihazVerisi
                    {
                        Sicaklik       = sicaklik,
                        Nem            = nem,
                        BaglantiDurumu = "Bagli",
                        SonOkumaZamani = DateTime.UtcNow
                    };

                    _veriDeposu.Guncelle(cihazVerisi);

                    await _cihazHub.Clients.All.SendAsync("CihazVerisiGuncellendi", cihazVerisi, stoppingToken);

                    _logger.LogDebug(
                        "SignalR push tamamlandi — Sicaklik: {Sicaklik}°C, Nem: %{Nem}",
                        cihazVerisi.Sicaklik, cihazVerisi.Nem);

                    await Task.Delay(_ayarlar.OkumaAraligiMs, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Modbus okuma servisi durduruldu (uygulama kapanisi).");
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception,
                    "Modbus iletisim hatasi. Otomatik iyilesme (self-healing) baslatildi, " +
                    "{Saniye}sn sonra yeniden baglanilacak. Hata: {HataMesaji}",
                    _ayarlar.YenidenBaglanmaMs / 1000, exception.Message);

                var baglantiKesilenVeri = new CihazVerisi
                {
                    BaglantiDurumu = "Bagli Degil",
                    SonOkumaZamani = DateTime.UtcNow
                };

                _veriDeposu.Guncelle(baglantiKesilenVeri);

                try
                {
                    await _cihazHub.Clients.All.SendAsync("CihazVerisiGuncellendi", baglantiKesilenVeri, stoppingToken);
                }
                catch { }

                await Task.Delay(_ayarlar.YenidenBaglanmaMs, stoppingToken);
            }
        }
    }
}
