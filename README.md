# 📚 KPSS Takip Paneli 2026

KPSS ve AGS adaylarının çalışma süreçlerini disiplin altına almak, ders sürelerini kaydetmek, soru sayılarını analiz etmek ve deneme netlerini otomatik hesaplamak amacıyla geliştirilmiş modern bir masaüstü takip uygulamasıdır.

Bu uygulama hem doğrudan bilgisayarında kullanmak isteyen adaylar için hazır bir programa sahiptir hem de kaynak kodları açık bir şekilde geliştiricilerin incelemesine sunulmuştur.

---

## 🎯 Temel Özellikler

- **Dinamik Sınav Geri Sayımı:** İlk açılışta seçtiğiniz sınav türüne göre (Lisans, Önlisans, Ortaöğretim, AGS) kalan gün sayısını anlık olarak hesaplar.
- **Hedef Puan Belirleme:** Sınav hedefinizi kaydeder ve ana ekranda motivasyon kaynağı olarak sürekli gösterir.
- **Ders Çalışma Kronometresi:** Çalışma yaparken sürenizi başlatıp durdurabilir, tek tıkla ilgili dersin hanesine çalışma süresi olarak kaydedebilirsiniz.
- **Soru Sayısı & Zaman Dağılımı:** Türkçe, Matematik, Tarih, Coğrafya ve Vatandaşlık derslerinde çözülen soru sayılarını ve harcanan süreleri görsel grafik barlarıyla gösterir.
- **Genel & Branş Deneme Takibi:** Doğru, yanlış ve boş sayılarınızı girdiğinizde 4 yanlış 1 doğruyu götürecek şekilde netlerinizi ve tahmini puanınızı anında hesaplar.
- **Kişisel Veri Tabanı (SQLite):** İnternet bağlantısı gerektirmez; tüm çalışma verileriniz bilgisayarınızda güvenli bir şekilde yerel veritabanında saklanır.
- **Fabrika Ayarlarına Dönüş:** Dilediğiniz an tek butonla tüm kayıtları sıfırlayıp yeni bir çalışma dönemine başlayabilirsiniz.

---

## ⚡ Hızlı Başlangıç (Doğrudan Kullanmak İsteyenler İçin)

Kodlarla uğraşmadan uygulamayı hemen bilgisayarınızda çalıştırmak için:

1. Bu sayfanın sağ tarafındaki **[Releases](../../releases)** bölümünden veya dosyalar arasındaki `KPSS-Setup.zip` paketini indirin.
2. İndirdiğiniz zip dosyasını bir klasöre çıkartın.
3. Klasör içindeki **`KPSS.exe`** dosyasına çift tıklayarak uygulamayı başlatın.
4. İlk açılışta hazırlanmakta olduğunuz sınav türünü ve hedef puanınızı girin; paneliniz hemen hazır hale gelecektir!

> **Sistem Gereksinimi:** Windows 10 veya Windows 11 işletim sistemi (.NET 8.0 Masaüstü Çalışma Zamanı).

---

## 🛠️ Kaynak Kodları İnceleme ve Geliştirici Kurulumu

Projeyi yerel makinenizde açmak, kodları incelemek veya geliştirmek için:

### Gereksinimler
- **Visual Studio 2022** (veya üzeri)
- **.NET 8.0 SDK** (veya `.NET Masaüstü Geliştirme` iş yükü)
- **System.Data.SQLite** NuGet paketi

### Adım Adım Kurulum

1. Repoyu bilgisayarınıza klonlayın:
   ```bash
   git clone [https://github.com/Emrecanm/KPSS-Takip-Paneli-2026.git](https://github.com/Emrecanm/KPSS-Takip-Paneli-2026.git)
   cd KPSS-Takip-Paneli-2026
