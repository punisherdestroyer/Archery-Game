ARCHERY GAME - DEVLOG

1- PROJE AMACI
- Bu proje, temel mekaniklerin akıcılığını ve fizik tabanlı etkileşimleri merkeze alan bir okçuluk deneyimi olarak tasarlandı. Ancak sıradan bir deneme olmakla kalmayıp eklemelerim ve inisiyatiflerim sayesinde kapsamlı ve oynanabilir hale geldi.

2- PROJE SORUNLARI & ÇÖZÜMLER
- S: User Interface ekranlarının, her ekranda aynı göründüğü için her cihazla (özellikle mobile) uyumlu olmaması sorunu.
- Ç: UI ögeleri Rect Transform ile değiştirildi, yerleri tekrardan ayarlandı ve simulatörler ile test yapıldı.
- S: Karakter animasyonları ve Mesh ögeleri, benim deneyimsizliğim sonucu başıma gereğinden fazla sorun açtı.
- Ç: Yapay zekadan yardım alarak animasyonları ve mesh kısımlarını daha stabil hale getirdim ve deneyim kazandım.
- S: Başta seviye ve yetenekleri Player.cs dosyasında kontrol ettiğim için son derece karışık ve uğraştırıcı olması sorunu.
- Ç: LevelManager.cs ve AbilityManager.cs ile beraber bu yoğunluğu azaltarak kodları böldüm ve işleri dağıttım.

2.1- HALA VAR OLAN SORUNLAR
- Bazı animasyonlar (özellikle hareket hızı ve saldırı hızı artışlarında) bu haldeyken bozulabiliyor ve net bir tutarlılık yok.
- Kodun çalışma hızına (cihazın performansına) göre Boss çağırma kodu birden fazla kez tetiklenip fazladan Boss çağırabiliyor.
- Player.cs içinde gereksiz yük yapacak seviye elemanları hala bulunuyor, detaylı bir temizlik yapamadım.
- Düşmanlar anlık ve direkt hasar vermek yerine temas süresine oranla hasar veriyor, bu durum başlangıçta oyunu son derece kolay bir hale getiriyor. Hata değil bir tasarım tercihi ancak ekleme gereksinimi duydum.
- Aynı şekilde; User Interface, Enemy, Map vb. gibi bir çok bileşen için düzgün bir görsel, asset, bulmakla uğraşmadım bu yüzden son derece basit görünüyor.
- MainMenuManager.cs dosyası içinde gereksiz yük yapacak fonksiyonlar var.
- Oyun başladığında kamera oyuncuya yakınlaştırma yapıp çekilmek yerine doğrudan başlıyor.
- Uygulamada çıkış (quit.application) tuşu yok, görevi sonlandır yapmadan çıkış yapılamıyor.

2.2- PROJE EKLEMELERİ
- WASD, 12345 ve Space entegrasyonları sayesinde windows uyumluluğunu geliştirip cross platform hale getirdim.
- Yetenek mekanikleri ile oynayarak açılır kapanır yetenekler yerine aktif kullanım süresi ve bekleme süresi ekledim. Ardından yetenek değerlerini kendime göre değiştirdim, bu süreçte birkaç yetenekte yenilemeye gittim. Açılır kapanır bir yetenek penceresi yerine doğrudan ekranda bulunan tuşlarla kullanılabilir hale getirdim.
- Deneyim puanı ve seviye atlama mekaniği ekledim. 4 adet istatistik ve 15 adet yetenek geliştirmesi olmak üzere toplam 19 adet geliştirme bulunuyor.
- Hareketsiz duran ya daöylece belirlenmiş güzergahta hareket eden düşmanlar yerine oyuncuyu algılayan, oyuncuya yönelen ve gerektiğinde hasar veren daha zeki düşmanlar ekledim.
- Ek olarak; farklı düşman tipleri ekledim ve zamana oranla doğma şanslarını artırdım. Belirlenen dakikalarda "Boss" türü son derece güçlü düşmanlar çıkabiliyor, çıkıyor.
- Düşmanların öldüğü vakit yeniden tam canla doğması yerine zamana oranla doğan düşman sayısını artıran, diğer tipteki düşmanları doğurma şansı olan ve bu doğurmaları karakterin mevcut pozisyonuna göre hesaplayan EnemySpawner.cs yazdım.
- Oyunda belli bir amaç yoktu bu yüzden süre, skor, ve en iyi süre kavramlarını ekleyerek basit bir PlayerPrefs ile tutulmasını sağladım, yeterli olmasa bile oyunda artık bir amaç var.
- Bu amaç doğrultusunda oyuna ek olarak durdurma ve bitiş ekranı ekledim. Bu ekranların yanı sıra, ne kadar oyunda müzik olmasa da, ayarlar ve nasıl oynanır kısımlarıyla bilgilendirmeler ekledim.
- Haritayı 16:9 ekranda tamamen görünecek kadar kısıtlı yapmak yerine daha büyük bir hale getirip oynanılabilirliği arttırdım, haritaya sınırlar ekledim.

3- ÖZET
- Bu çalışmada isteneni yapmak yerine kendimi geliştirmek adına hem öğrendim hem de eklemeler yaptım. Özellikle oyunun temelini aynı bırakıp oynanışı farklı bir hale getirerek hem isteneni yapıp hem de kendimi geliştirmeye çalıştım.

