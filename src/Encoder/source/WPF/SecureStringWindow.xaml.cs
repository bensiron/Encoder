using System;
using System.Security.Cryptography;
using System.Windows;
using UGTS.Encoder.Windows;
using UGTS.WPF;

namespace UGTS.Encoder.WPF
{
	public partial class SecureStringWindow : UgtsWindow
    {
        public Observable<string> Username { get; set; }
	    public Observable<string> Password { get; set; }
	    public Observable<string> Plaintext { get; set; }
        public Observable<string> Ciphertext { get; set; }
        public Observable<bool> SystemAccount { get; set; }
        public Observable<bool> ShowPassword { get; set; }
        public Computed<bool> IsPasswordEnabled { get; set; }
        public Computed<Visibility> IsPasswordVisible { get; set; }
        public Computed<Visibility> IsPasswordTextVisible { get; set; }
        public Computed<bool> IsEncodeEnabled { get; set; }
        public Computed<bool> IsDecodeEnabled { get; set; }

        public SecureStringWindow()
        {
            Username = new Observable<string>("");
            Password = new Observable<string>("");
            Plaintext = new Observable<string>("");
            Ciphertext = new Observable<string>("");
            SystemAccount = new Observable<bool>(false);
            ShowPassword = new Observable<bool>(false);
            IsPasswordEnabled = new Computed<bool>(() => !(IsSystemUser() || IsCurrentUser()));
            IsPasswordVisible = new Computed<Visibility>(() => ToVisibility(!ShowPassword));
            IsPasswordTextVisible = new Computed<Visibility>(() => ToVisibility(ShowPassword.Value));
            IsEncodeEnabled = new Computed<bool>(() => HasValidUser && !Plaintext.Value.IsBlank());
            IsDecodeEnabled = new Computed<bool>(() => HasValidUser && !Ciphertext.Value.IsBlank());
            Username.ValueChanged += ValueChanged;
            Password.ValueChanged += ValueChanged;
            Plaintext.ValueChanged += ValueChanged;
            Ciphertext.ValueChanged += ValueChanged;
            SystemAccount.ValueChanged += SystemAccountChanged;

            DataContext = this;
            Loaded += WindowLoaded;
            Activated += WindowActivated;
			InitializeComponent();
		}

        /// <summary>
        /// Converts a boolean into a Windows.Visibility value in the standard way
        /// </summary>
        private static Visibility ToVisibility(bool v)
        {
            return v ? Visibility.Visible : Visibility.Hidden;
        }

        private void ValueChanged(ObservableBase sender, ValueChangedEventArgs<string> e)
        {
            var isSystemUser = IsSystemUser();
            if (isSystemUser != SystemAccount)
            {
                SystemAccount.Value = isSystemUser;
            }

            if (!IsPasswordEnabled)
            {
                ShowPassword.Value = false;
            }
        }

        private void EncodeClicked(object sender, EventArgs e)
        {
            RunWithImpersonation(() =>
            {
                Ciphertext.Value = Plaintext.Value.EncryptWithDpapi(ProtectionScope);
                CopyToClipboard();
            }, "encrypting plaintext");
        }

        private void DecodeClicked(object sender, EventArgs e)
        {
            RunWithImpersonation(() =>
            {
                var ct = Ciphertext.Value;
                Plaintext.Value = ct.DecryptWithDpapi();
            }, "decrypting");
        }

	    private void RunWithImpersonation(Action action, string description)
	    {
            try
            {
                if (IsPasswordEnabled)
                {
                    WindowsLogin.Impersonate(Username, Password, true);
                }

                action();
            }
            catch (Exception ex)
            {
                AnalyzeException(ex).Show(description);
            }
            finally
            {
                WindowsLogin.Clear();
            }
        }

        private void CopyToClipboardClicked(object sender, EventArgs e)
        {
            CopyToClipboard();
        }

        private void CopyToClipboard()
        {
            Clipboard.SetText(Ciphertext.Value);
        }

        private DataProtectionScope ProtectionScope => IsSystemUser() ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser;

        private static Exception AnalyzeException(Exception ex)
        {
            if (ex.Message.Contains("Key not valid for use in specified state")) ex = new Exception(ex.Message + "Please verify that the username matches the user that was originally used to create this secure string.");
            if (ex.Message.Contains("data is invalid")) ex = new Exception("The ciphertext data blob is invalid.");
            if (ex.Message.Contains("privilege is not held")) ex = new Exception(ex.Message + " - try exiting and re-running Encoder with elevated privileges.");
            return ex;
        }

        private bool HasValidUser => !(Username.Value.IsBlank() || (IsPasswordEnabled && Password.Value.IsBlank()));

        private bool IsSystemUser(string user = "")
        {
            if (user.IsBlank()) user = Username;
            var u = new WindowsUserName(user);
            switch (u.Username.Replace(" ", "").ToLower())
            {
                case "system":
                case "localsystem":
                case "networkservice":
                case "localservice":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsCurrentUser(string user = "")
        {
            if (user.IsBlank()) user = Username.Value;
            user = ("" + user).Trim().ToLower().Replace(" ", "");
            return (user == CurrentUser().ToLower().Trim());
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            plaintextEditor.Focus();
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            Username.Value = CurrentUser();
        }

        private static string CurrentUser()
        {
            return Environment.UserDomainName + "\\" + Environment.UserName;
        }

        private void SystemAccountChanged(ObservableBase sender, ValueChangedEventArgs<bool> e)
        {
            Username.Value = SystemAccount ? Environment.MachineName + "\\SYSTEM" : CurrentUser();
        }
    }
}
