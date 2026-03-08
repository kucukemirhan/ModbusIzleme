# Modbus İzleme Sistemi

Gerçek zamanlı SignalR iletişimine dayalı, endüstriyel Modbus TCP sensör verilerini (Sıcaklık ve Nem) okuyan bir .NET 8 Web API projesi.

## Simülatör Kullanımı

Test senaryoları için **Modbus Slave** simülatörü kullanılabilir:
1. Simülatörü kurun ve başlatın.
2. Bağlantı tipini **TCP/IP** (Port: `502`) olarak seçin.
3. **Holding Registers (4xxxx)** seçin.
4. (Adres 1 - Sıcaklık) değerine `245`, (Adres 2 - Nem) değerine `550` girin. Uygulamada anlık olarak `24.5 °C` ve `%55.0` göreceksiniz.
5. Değerlerin değiştiğini daha dinamik görmek için simülatör ekranında ilgili hücrelere sağ tıklayıp "Increment" simülasyonunu başlatabilirsiniz.


## Çalıştırma

Projeyi derlemek ve çalıştırmak için:

```bash
dotnet build
dotnet run
```

Uygulama kalktığında, herhangi bir tarayıcıdan http://localhost:5051 adresine (Log ekranında belirtilen URL) gidildiğinde, modern web arayüzü SignalR ile otomatik olarak veri akışını sunmaya başlayacaktır.
