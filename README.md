# Wirama Nusantara - Splash Page (Unity 6.3 LTS)

Splash interaktif: logo + nama aplikasi, peta Indonesia (7 kelompok pulau dari id.svg) dengan 21 pin destinasi
yang bisa diketuk, popup destinasi unggulan, dan tombol **Mulai** yang pindah ke scene Home (tidak otomatis).
Palet "Tropis Laut": #0B3D4F, #F2B134, #2EC4B6, #FFF8E7. Target: iPhone 13 Pro Max (1284x2778), portrait.

## Langkah pakai (urut)

1. **Buat / buka project Unity 6.3 LTS** (template 2D atau 3D, bebas).
2. **Install DOTween** (gratis):
   - Buka Asset Store: cari **"DOTween (HOTween v2)"** oleh Demigiant, klik *Add to My Assets*.
   - Di Unity: *Window > Package Manager > My Assets* > cari DOTween > *Download* > *Import*.
   - Setelah import muncul panel setup: klik **Setup DOTween...** > **Apply**.
     (Kalau tidak muncul: *Tools > Demigiant > DOTween Utility Panel > Setup DOTween...*)
   - Langkah ini wajib, tanpa itu muncul error `DG.Tweening` tidak ditemukan.
3. **Import TextMeshPro Essentials**: *Window > TextMeshPro > Import TMP Essential Resources* > Import.
4. **Copy folder `Assets/Wirama`** dari zip ini ke folder `Assets` project kamu. Tunggu compile selesai.
5. Klik menu **Tools > Wirama > 1. Build Splash Scene**. Menu ini otomatis membuat:
   `Assets/Wirama/Scenes/Splash.unity` dan `Home.unity` (placeholder), mengatur Build Settings (Splash index 0),
   orientasi portrait, dan nama produk.
6. Buka `Splash.unity`, set **Game view** ke 1284x2778 (tombol + di dropdown resolusi, Fixed Resolution),
   atau pakai *Device Simulator* > iPhone 13 Pro Max. Tekan **Play**.

Kalau gambar terlihat salah / ada warning "tidak readable": jalankan **Tools > Wirama > 2. Reimport Art**, lalu ulangi langkah 5.

## Alur & fitur

- Intro (+-4 detik): fade-in, logo muncul + gong, nama & tagline slide, pulau muncul barat ke timur, pin jatuh,
  tombol Mulai muncul. Ketuk layar saat intro untuk melewati.
- Ketuk **pulau** atau **pin**: peta zoom ke pulau itu. Ketuk **pin**: popup nama, daerah, deskripsi. Tombol X menutup.
- Ketuk laut kosong atau tombol panah kiri atas: kembali ke peta penuh.
- Tombol **Mulai**: fade lalu load scene `Home`. Scene Home asli tim: ganti `Home.unity` (nama scene tetap "Home")
  atau ubah `Home Scene Name` di komponen SplashController pada `SplashCanvas`.
- Responsif: Canvas Scaler (ref 1284x2778, match 0.5), Safe Area (notch / home indicator), peta menyesuaikan lebar layar.

## Kustomisasi

| Yang diubah | Caranya |
|---|---|
| Nama aplikasi / tagline | `Scripts/Editor/SplashSceneBuilder.cs`: `AppName`, `Tagline`, lalu jalankan menu Build lagi |
| Destinasi (nama, deskripsi, koordinat) | `Scripts/Runtime/WiramaCatalog.cs`, lalu jalankan menu Build lagi |
| Logo | Ganti `Art/Sprites/logo.png` (512x512, transparan). Sumber vektor: `Source/logo.svg` |
| Warna | Konstanta warna di `SplashSceneBuilder.cs` + build ulang |
| Kecepatan / zoom | Field publik di komponen `SplashController` (mis. `Max Focus Zoom`) |
| Suara | Ganti file di `Audio/` (gong, ting, click) |

Folder `Source/` (opsional, di luar Assets) berisi skrip Python pembuat aset peta dan sprite.

## Catatan jujur

- Peta bergaya (hasil dari id.svg), posisi pin **perkiraan** (sudah dipastikan jatuh di daratan).
- Logo adalah logo vektor sederhana buatan Claude, silakan diganti kalau tim punya desain sendiri.
- Kode ditulis dan di-review manual, **belum dikompilasi di Unity** oleh pembuatnya. Kalau ada error, kirim pesan error-nya.
