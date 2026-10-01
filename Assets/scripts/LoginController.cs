// Simpan di: Assets/Scripts/LoginController.cs
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Pesan")]
    public TMP_Text emailError;
    public TMP_Text passwordError;
    public TMP_Text statusText;

    [Header("Tombol")]
    public Button loginButton;
    public Button passwordToggleButton;
    public Button forgotPasswordButton;
    public Button signUpButton;
    public Button googleButton;
    public Button appleButton;
    public Button githubButton;

    [Header("Visual")]
    public TMP_Text loginLabel;
    public GameObject loginArrow;
    public Image passwordToggleIcon;
    public Sprite eyeSprite;      // password terlihat
    public Sprite eyeOffSprite;   // password tersembunyi

    [Header("Scene tujuan (harus ada di Build Settings)")]
    public string successScene = "Home";
    public string signUpScene = "SignUp";
    public string forgotPasswordScene = "ForgotPassword";

    [Header("Autentikasi")]
    [Tooltip("Aktif = login pura-pura (terima semua input valid). Matikan kalau API sudah siap.")]
    public bool demoMode = true;
    public float demoDelay = 1.2f;
    [Tooltip("Endpoint POST login kamu. Body: {\"email\":\"...\",\"password\":\"...\"}")]
    public string apiUrl = "https://example.com/api/login";
    public int minPasswordLength = 6;

    static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    static readonly Color ErrorColor = new Color(1f, 0.42f, 0.42f);
    static readonly Color SuccessColor = new Color(0.48f, 0.85f, 0.56f);

    bool isBusy;
    bool passwordVisible;

    [System.Serializable]
    class LoginRequest { public string email; public string password; }

    // ============ Setup ============

    void Awake()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        passwordToggleButton.onClick.AddListener(TogglePassword);
        forgotPasswordButton.onClick.AddListener(OnForgotPassword);
        signUpButton.onClick.AddListener(OnSignUp);
        googleButton.onClick.AddListener(() => OnSocialLogin("Google"));
        appleButton.onClick.AddListener(() => OnSocialLogin("Apple"));
        githubButton.onClick.AddListener(() => OnSocialLogin("GitHub"));

        // Enter di email -> pindah ke password; Enter di password -> login
        emailInput.onSubmit.AddListener(_ => passwordInput.ActivateInputField());
        passwordInput.onSubmit.AddListener(_ => OnLoginClicked());

        // Hapus pesan error saat user mulai mengetik lagi
        emailInput.onValueChanged.AddListener(_ => { emailError.text = ""; statusText.text = ""; });
        passwordInput.onValueChanged.AddListener(_ => { passwordError.text = ""; statusText.text = ""; });

        ClearMessages();
        ApplyPasswordVisibility();
    }

    // ============ Login ============

    public void OnLoginClicked()
    {
        if (isBusy) return;
        ClearMessages();

        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        bool valid = true;

        if (string.IsNullOrEmpty(email))
        {
            ShowError(emailError, "Email is required.");
            valid = false;
        }
        else if (!EmailRegex.IsMatch(email))
        {
            ShowError(emailError, "Please enter a valid email address.");
            valid = false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError(passwordError, "Password is required.");
            valid = false;
        }
        else if (password.Length < minPasswordLength)
        {
            ShowError(passwordError, $"Minimum {minPasswordLength} characters.");
            valid = false;
        }

        if (!valid) return;
        StartCoroutine(LoginRoutine(email, password));
    }

    IEnumerator LoginRoutine(string email, string password)
    {
        SetBusy(true);

        bool success = false;
        string message = "";
        yield return StartCoroutine(Authenticate(email, password, (ok, msg) => { success = ok; message = msg; }));

        SetBusy(false);

        if (success)
        {
            SetStatus("Login successful!", false);
            yield return new WaitForSeconds(0.5f);
            LoadScene(successScene);
        }
        else
        {
            SetStatus(string.IsNullOrEmpty(message) ? "Login failed. Please try again." : message, true);
        }
    }

    /// <summary>
    /// Ganti isi method ini kalau pakai Firebase / PlayFab / backend lain.
    /// Panggil done(true, "") kalau berhasil, done(false, "pesan error") kalau gagal.
    /// </summary>
    protected virtual IEnumerator Authenticate(string email, string password, System.Action<bool, string> done)
    {
        if (demoMode)
        {
            yield return new WaitForSeconds(demoDelay);
            OnLoginSuccess("{}");
            done(true, "");
            yield break;
        }

        string body = JsonUtility.ToJson(new LoginRequest { email = email, password = password });
        using (var req = new UnityWebRequest(apiUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                OnLoginSuccess(req.downloadHandler.text);
                done(true, "");
            }
            else if (req.responseCode == 401 || req.responseCode == 403)
            {
                done(false, "Incorrect email or password.");
            }
            else
            {
                done(false, "Connection error. Please try again.");
            }
        }
    }

    /// <summary>Dipanggil saat login sukses. Simpan token/sesi di sini (pakai penyimpanan aman, bukan PlayerPrefs polos).</summary>
    protected virtual void OnLoginSuccess(string responseBody)
    {
        Debug.Log("Login success. Response: " + responseBody);
    }

    // ============ Password ============

    public void TogglePassword()
    {
        passwordVisible = !passwordVisible;
        ApplyPasswordVisibility();
    }

    void ApplyPasswordVisibility()
    {
        passwordInput.contentType = passwordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate();
        if (passwordToggleIcon != null)
            passwordToggleIcon.sprite = passwordVisible ? eyeSprite : eyeOffSprite;
    }

    // ============ Navigasi ============

    public void OnForgotPassword()
    {
        if (Application.CanStreamedLevelBeLoaded(forgotPasswordScene))
            SceneManager.LoadScene(forgotPasswordScene);
        else
            SetStatus("Forgot password page isn't connected yet.", true);
    }

    public void OnSignUp()
    {
        if (Application.CanStreamedLevelBeLoaded(signUpScene))
            SceneManager.LoadScene(signUpScene);
        else
            SetStatus("Sign up page isn't connected yet.", true);
    }

    /// <summary>
    /// Login Google/Apple/GitHub butuh SDK/OAuth masing-masing.
    /// Override method ini setelah SDK-nya terpasang.
    /// </summary>
    public virtual void OnSocialLogin(string provider)
    {
        Debug.Log($"Social login requested: {provider}");
        SetStatus($"{provider} login isn't connected yet.", true);
    }

    void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning($"Scene '{sceneName}' belum ada di Build Settings.");
    }

    // ============ UI helpers ============

    void SetBusy(bool busy)
    {
        isBusy = busy;
        loginButton.interactable = !busy;
        emailInput.interactable = !busy;
        passwordInput.interactable = !busy;
        if (loginLabel != null) loginLabel.text = busy ? "Logging in..." : "Login";
        if (loginArrow != null) loginArrow.SetActive(!busy);
    }

    void ClearMessages()
    {
        emailError.text = "";
        passwordError.text = "";
        statusText.text = "";
    }

    void ShowError(TMP_Text target, string message)
    {
        target.color = ErrorColor;
        target.text = message;
    }

    void SetStatus(string message, bool isError)
    {
        statusText.color = isError ? ErrorColor : SuccessColor;
        statusText.text = message;
    }
}
