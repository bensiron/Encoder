using System;
using System.Security.Cryptography;
using System.Windows;
using UGTS.WPF;

namespace UGTS.Encoder
{
	public partial class SecureStringWindow : UgtsWindow
    {
        public Observable<string> Username { get; set; }
	    public Observable<string> Password { get; set; }
	    public Observable<string> Plaintext { get; set; }
        public Observable<string> Ciphertext { get; set; }
        public Observable<string> Setting { get; set; }
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
            Setting = new Observable<string>("");
            SystemAccount = new Observable<bool>(false);
            ShowPassword = new Observable<bool>(false);
            IsPasswordEnabled = new Computed<bool>(() => !(IsSystemUser() || IsCurrentUser()));
            IsPasswordVisible = new Computed<Visibility>(() => ToVisibility(!ShowPassword));
            IsPasswordTextVisible = new Computed<Visibility>(() => ToVisibility(ShowPassword.Value));
            IsEncodeEnabled = new Computed<bool>(() => HasValidUser && !Plaintext.Value.XIsBlank());
            IsDecodeEnabled = new Computed<bool>(() => HasValidUser && !(Ciphertext.Value.XIsBlank() && Setting.Value.XIsBlank()));
            Username.ValueChanged += HValueChanged;
            Password.ValueChanged += HValueChanged;
            Plaintext.ValueChanged += HValueChanged;
            Ciphertext.ValueChanged += HValueChanged;
            SystemAccount.ValueChanged += HSystemAccountChanged;

            DataContext = this;
            Loaded += HLoad;
            Activated += HActivate;
			InitializeComponent();
		}

        /// <summary>
        /// Converts a boolean into a Windows.Visibility value in the standard way
        /// </summary>
        private static Visibility ToVisibility(bool v)
        {
            return v ? Visibility.Visible : Visibility.Hidden;
        }

        private void HValueChanged(ObservableBase sender, ValueChangedEventArgs<string> e)
        {
            var bSys = IsSystemUser();
            if (bSys != SystemAccount)
                SystemAccount.Value = bSys;

            if (!IsPasswordEnabled) ShowPassword.Value = false;
        }

        private void HEncode(object sender, EventArgs e)
        {
            try
            {
                if (IsPasswordEnabled) Impersonate(true);
                Ciphertext.Value = Plaintext.Value.XEncrypt(ProtectionScope);
                var name = Setting.Value.Trim();
                if (name.StartsWith("<"))
                {
                    var d = MXml.LoadText(name);
                    name = d.X("key");
                }
                var pos = name.IndexOf(':');
                if (pos >= 0) name = name.Substring(0, pos);
                Setting.Value = "<add key=\"" + name + ":" + Username + "\" value=\"" + Ciphertext + "\"/>";
                CopyToClipboard();

            }
            catch (Exception ex)
            {
                ex.Show("encrypting plaintext");
            }
            finally
            {
                Impersonate(false);
            }
        }

        private void HClipboard(object sender, EventArgs e)
        {
            CopyToClipboard();
        }

        private void CopyToClipboard()
        {
            Clipboard.SetText(Setting);
        }

        private DataProtectionScope ProtectionScope => IsSystemUser() ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser;

	    private void HDecode(object sender, EventArgs e)
        {
            try
            {
                if (IsPasswordEnabled) Impersonate(true);
                var ct = Ciphertext.Value;
                if (ct.XIsBlank() && !Setting.Value.XIsBlank())                
                {
                    try 
                    {
                        var xml = MXml.LoadText(Setting);
                        ct = xml.X("value");
                    }
                    catch { throw new Exception("Setting xml tag is not valid."); }
 
                    if (ct.XIsBlank()) throw new Exception("Setting xml tag had no value attribute.");
                }
                Plaintext.Value = ct.XDecrypt();
            }
            catch (Exception ex)
            {
                AnalyzeException(ex).Show("decrypting");
            }
            finally
            {
                Impersonate(false);
            }
        }

        private static Exception AnalyzeException(Exception ex)
        {
            if (ex.Message.StartsWith("Key not valid in specified state")) ex = new Exception(ex.Message + " - please verify that the username matches the user originally used to create this secure string.");
            if (ex.Message.Contains("data is invalid")) ex = new Exception("The ciphertext data blob is invalid.");
            return ex;
        }

        private bool HasValidUser => !(Username.Value.XIsBlank() || (IsPasswordEnabled && Password.Value.XIsBlank()));

	    private void Impersonate(bool bImpersonate)
        {
            if (bImpersonate) MLogin.Impersonate(Username, Password, true);
            else MLogin.Clear();
        }

        private bool IsSystemUser(string user = "")
        {
            if (user.XIsBlank()) user = Username;
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
            if (user.XIsBlank()) user = Username.Value;
            user = ("" + user).Trim().ToLower().Replace(" ", "");
            return (user == CurrentUser().ToLower().Trim());
        }

        private void HActivate(object sender, EventArgs e)
        {
            plaintextEditor.Focus();
        }

        private void HLoad(object sender, EventArgs e)
        {
            Username.Value = CurrentUser();
        }

        private static string CurrentUser()
        {
            return Environment.UserDomainName + "\\" + Environment.UserName;
        }

        private void HSystemAccountChanged(ObservableBase sender, ValueChangedEventArgs<bool> e)
        {
            Username.Value = SystemAccount ? Environment.MachineName + "\\SYSTEM" : CurrentUser();
        }
    }
}
