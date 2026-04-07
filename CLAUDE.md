# Mobile Game Collection — Unity 2D Android Projesi

## Proje Durumu
5/15 oyun tamamlandı, iyileştirme aşamasındayız.

## Tamamlanan Oyunlar
- ✅ 01_Snake — çalışıyor
- ✅ 02_BrickBreaker — çalışıyor  
- ✅ 03_FlappyBird — çalışıyor
- ✅ 04_ColorSwitch — çalışıyor
- ✅ 05_BubbleShooter — çalışıyor

## Bekleyen Oyunlar
- ⏳ 06_EndlessRunner
- ⏳ 07_SpaceShooter
- ⏳ 08_PlatformJumper
- ⏳ 09_BallDrop
- ⏳ 10_TopDownRacer
- ⏳ 11_ZombieShooter
- ⏳ 12_FruitNinja
- ⏳ 13_ShapePuzzle
- ⏳ 14_Labirent
- ⏳ 15_StickmanFighter

## Şu An Yapılanlar: Ortak Altyapı + İyileştirmeler
1. PauseManager.cs — _Core/Scripts/ → tüm oyunlara ekle
2. CameraShake.cs — _Core/Scripts/ → Main Camera'ya ekle
3. GameAudio.cs — _Core/Scripts/ → procedural ses sistemi

## Bilinen Sorunlar
- Flappy Bird: sürekli ses çalıyor (BirdController Update'te)
- Snake: CameraShake sonrası düz ekran
- Pause butonu: Canvas çakışması sorunu

## Klasör Yapısı
Assets/
├── _Core/Scripts/     ← PauseManager, CameraShake, GameAudio
├── Games/
│   ├── 01_Snake/
│   ├── 02_BrickBreaker/
│   ├── 03_FlappyBird/
│   ├── 04_ColorSwitch/
│   └── 05_BubbleShooter/
├── UI/
└── Audio/

## Teknik Kararlar
- Tüm UI koddan oluşturuluyor (Inspector'a bağımlı değil)
- Fizik: Manuel bounds kontrolü (Unity physics değil)
- Platform: Android öncelikli, iOS sonra
- Unity: 2022 LTS, 2D URP template
- Renk paleti: BubbleGameManager.BubbleColors[]

## Oyun Başına Notlar

### Snake
- Grid: -10 ile +10 arası
- Hız: moveInterval 0.2f'den başlar, yem yedikçe azalır
- Sınır çizgisi: DrawBorders() kameraya göre dinamik

### Brick Breaker  
- Top manuel hareket (transform.position)
- Çarpışma: BallOverlapsBounds() ile bounds kontrolü
- Powerup sistemi henüz yok

### Flappy Bird
- Boru çarpışma: OverlapsObject() bounds kontrolü
- Skor: ScoreZone tag'i ile, geçince Untagged yapılır
- Ses sorunu: PlayBounce her frame tetikleniyor olabilir

### Color Switch
- Çember: Mesh ile oluşturuluyor (4 segment)
- Renk kontrolü: GetColorAtAngle() local rotation ile
- Kamera topu takip ediyor (cameraFollowY)

### Bubble Shooter
- Grid: BFS ile eşleşme bulma
- Snap: SnapToGrid() en yakın boş hücreyi bulur
- Akıllı renk: GetAvailableColors() ile sadece grid'deki renkler
- Patlama: 3+ aynı renk = patlama, +10*sayi puan
- Patlama yok: -5 puan ceza