namespace ModbusIzleme.Models;

public class CihazVerisi
{
    public float Sicaklik { get; set; }
    public float Nem { get; set; }
    public string BaglantiDurumu { get; set; } = "Bagli Degil";
    public DateTime SonOkumaZamani { get; set; }
}
