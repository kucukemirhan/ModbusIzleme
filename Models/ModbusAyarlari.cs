namespace ModbusIzleme.Models;

public class ModbusAyarlari
{
    public string SunucuAdresi { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 502;
    public byte BirimKimligi { get; set; } = 1;
    public int SicaklikAdresi { get; set; } = 1;
    public int NemAdresi { get; set; } = 2;
    public float OlcekFaktoru { get; set; } = 10.0f;
    public int OkumaAraligiMs { get; set; } = 1000;
    public int YenidenBaglanmaMs { get; set; } = 3000;
}
