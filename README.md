# Modbus İzleme Sistemi

Gerçek zamanlı SignalR iletişimine dayalı, endüstriyel Modbus TCP sensör verilerini (Sıcaklık ve Nem) okuyan, otonom ve hata toleranslı (fault-tolerant) bir .NET 8 Web API projesi.

## Mimari Özet

Sistem, "Server-Push" mantığıyla tasarlanmıştır. Frontend'den istek beklemeden, arka plandaki servis Modbus'tan veriyi okur ve SignalR üzerinden tüm istemcilere anlık olarak yayınlar (broadcast).

```mermaid
graph TD
    A[Modbus Simulator (ModRSsim2)] <-->|TCP (Port: 502) FC:03| B(ModbusOkuyucuServis<br>BackgroundService)
    B -->|Singleton Depolama| C[(CihazVerisiDeposu)]
    B -->|Push (CihazVerisiGuncellendi)| D[CihazHub<br>SignalR]
    D -->|WebSocket| E(Müşteri Tarayıcısı<br>index.html)
    F[CihazController API] -.->|Son veriyi okur| C
```

**Temel Bileşenler (Clean Architecture & DI):**
- **IModbusOkuyucu / ModbusTCP:** Modbus haberleşmesini, Endianness dönüşümlerini ve ölçeklemeyi (Scaling) yönetir. (FluentModbus kütüphanesi).
- **ModbusOkuyucuServis:** Otonom çalışan arka plan işçisidir (BackgroundService). Belirlenen periyotlarla ModbusTCP üzerinden okuma yapar.
- **CihazVerisiDeposu:** Thread-safe singleton yapıda son okunan veriyi saklar.
- **CihazController:** Dış sistemlerin son veriye REST üzerinden ulaşmasını (Polling) sağlayan destekleyici endpoint.

## Modbus Yapılandırması (`appsettings.json`)

Bağlantı parametreleri ve adres yapılandırmaları tamamen konfigürasyon üzerinden esnek yapıdadır (Hard-coded magic number'lar kullanılmaz):

```json
"ModbusAyarlari": {
  "SunucuAdresi": "127.0.0.1",
  "Port": 502,
  "BirimKimligi": 1,
  "SicaklikAdresi": 1,   // Holding Register PDU Adresi
  "NemAdresi": 2,        // Holding Register PDU Adresi
  "OlcekFaktoru": 10.0,
  "OkumaAraligiMs": 1000,
  "YenidenBaglanmaMs": 3000
}
```

## Veri İşleme Pipeline

Modbus üzerinden okunan ham değerlerin (Raw Values) fiziksel birime çevrilmesi şu adımlarla gerçekleşir:

1. **Count: 1 Okuma:** `ReadHoldingRegistersAsync` ile sıcaklık ve nem registerleri 16-bit unsigned/signed formatta bağımsız olarak okunur (FC 0x03).
2. **Byte Swapping (Endianness Düzeltmesi):** Modbus standardı verileri Big-Endian formatta iletir. x86/x64 PC mimarisi ise Little-Endian kullanır. Bu farklılıktan doğacak sorunlar `BinaryPrimitives.ReverseEndianness` ile çözülür.
3. **Scaling (Ölçekleme):** Endüstriyel sensörler ondalık sayıları tam sayı halinde gönderir (Örn: `24.5 °C` değeri Modbus'tan `245` olarak gelir). Konfigürasyondan alınan `OlcekFaktoru` (10.0f) değerine bölünerek gerçek fiziksel değer hesaplanır.

## Hata Yönetimi (Resilience & Self-Healing)

Sistem, geçici bağlantı kopmalarına ve iletişim hatalarına karşı tamamen **çökme korumalıdır (Crash-proof)**.

BackgroundService içerisinde "Çift Döngü (Double Loop)" yapısı kullanılmıştır:
- **Dış Döngü:** Modbus sunucusuna bağlanmayı ve oluşan TCP/Modbus hatalarında tanımlı `YenidenBaglanmaMs` süresi kadar bekleyip, kendi kendini yeniden başlatmayı (Self-Healing) sağlar.
- **İç Döngü:** Bağlantı sağlıklı olduğu sürece `OkumaAraligiMs` sıklığında periyodik okuma yapar.

Olası bir kilitlenmede, SignalR tarafına `BaglantiDurumu = "Bagli Degil"` bilgisi anlık olarak iletilir. Arayüzde veri akışı durur ve kırmızı uyarı yanar, bağlantı geldiğinde sistem otomatik toparlanır.

## Simülatör Kullanımı (ModRSsim2)

Test senaryoları için **ModRSsim2** simülatörü kullanılabilir:
1. Simülatörü kurun ve başlatın.
2. Bağlantı tipini **TCP/IP** (Port: `502`) olarak seçin.
3. **Holding Registers (4xxxx)** sekmesine geçin.
4. `40002` (Adres 1 - Sıcaklık) değerine `245`, `40003` (Adres 2 - Nem) değerine `550` girin. Uygulamada anlık olarak `24.5 °C` ve `%55.0` göreceksiniz.
5. Değerlerin değiştiğini daha dinamik görmek için simülatör ekranında ilgili hücrelere sağ tıklayıp "Increment" simülasyonunu başlatabilirsiniz.

*(Not: Modbus adreslemelerinde PDU 1 değeri, mantıksal olarak 40002. adrese denk gelmektedir.)*

## Çalıştırma

Projeyi derlemek ve çalıştırmak için:

```bash
dotnet build
dotnet run
```

Uygulama kalktığında, herhangi bir tarayıcıdan http://localhost:5051 adresine (Log ekranında belirtilen URL) gidildiğinde, modern web arayüzü SignalR ile otomatik olarak veri akışını sunmaya başlayacaktır.
