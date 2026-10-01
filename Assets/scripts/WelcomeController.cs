// Simpan di: Assets/Scripts/WelcomeController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomeController : MonoBehaviour
{
    [Tooltip("Nama scene tujuan saat tombol 'Mulai Sekarang' ditekan")]
    public string getStartedScene = "MainMenu";

    [Tooltip("Nama scene tujuan saat tombol 'Saya Sudah Punya Akun' ditekan")]
    public string loginScene = "Login";

    public void OnGetStarted() => Load(getStartedScene);

    public void OnLogin() => Load(loginScene);

    private void Load(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning($"Scene '{sceneName}' belum ada di Build Settings.");
    }
}
