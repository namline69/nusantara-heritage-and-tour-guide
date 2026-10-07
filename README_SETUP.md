# Nusantara Splash – Setup (Unity 6.3 LTS + DOTween)

1. Copy folder `Assets/NusantaraSplash` ke folder `Assets` project kamu.
2. Pastikan DOTween sudah di-setup (Tools > Demigiant > DOTween Utility Panel > Setup DOTween...) dan
   TMP Essentials sudah di-import (Window > TextMeshPro > Import TMP Essential Resources).
3. Menu: **Nusantara > Build Splash Scene**. Otomatis membuat `Assets/Scenes/Splash.unity`,
   `Home.unity` (placeholder, jika belum ada) dan mendaftarkannya ke Build Settings (Splash = index 0).
4. Game view > pilih **Simulator** > iPhone 13 Pro Max (atau Resolution 1284x2778) > Play.

Opsional: isi `Click Clip` pada komponen SplashController untuk suara tombol.
Scene tujuan bisa diganti di field `Next Scene Name` (default: Home).
