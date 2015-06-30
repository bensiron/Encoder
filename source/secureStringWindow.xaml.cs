using JetBrains.Annotations;
using System;
using System.Security.Cryptography;
using System.Windows;
using UGTS.Exceptions;
using UGTS.UI;

namespace UGTS.Encoder
{
	public partial class SecureStringWindow
	{
        [UsedImplicitly] public Observable<string> Username { get; set; }
	    [UsedImplicitly] public Observable<string> Password { get; set; }
	    [UsedImplicitly] public Observable<string> Plaintext { get; set; }
        [UsedImplicitly] public Observable<string> Ciphertext { get; set; }
        [UsedImplicitly] public Observable<string> Setting { get; set; }
        [UsedImplicitly] public Observable<bool> SystemAccount { get; set; }
        [UsedImplicitly] public Observable<bool> ShowPassword { get; set; }
        [UsedImplicitly] public Computed<bool> IsPasswordEnabled { get; set; }
        [UsedImplicitly] public Computed<Visibility> IsPasswordVisible { get; set; }
        [UsedImplicitly] public Computed<Visibility> IsPasswordTextVisible { get; set; }
        [UsedImplicitly] public Computed<bool> IsEncodeEnabled { get; set; }
        [UsedImplicitly] public Computed<bool> IsDecodeEnabled { get; set; }

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
            IsPasswordVisible = new Computed<Visibility>(() => (!ShowPassword).XToVisibility());
            IsPasswordTextVisible = new Computed<Visibility>(() => ShowPassword.Value.XToVisibility());
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
                ex.XHandle("encrypting plaintext");
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

        private DataProtectionScope ProtectionScope
        {
            get { return IsSystemUser() ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser; }
        }

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
                    catch { throw MException.MessageException("Setting xml tag is not valid."); }
 
                    if (ct.XIsBlank()) throw MException.MessageException("Setting xml tag had no value attribute.");
                }
                Plaintext.Value = ct.XDecrypt();
            }
            catch (Exception ex)
            {
                ex = AnalyzeException(ex);
                ex.XHandle("decrypting");
            }
            finally
            {
                Impersonate(false);
            }
        }

        private static Exception AnalyzeException(Exception ex)
        {
            if (ex.Message.StartsWith("Key not valid in specified state")) ex = MException.MessageException(ex.Message + " - please verify that the username matches the user originally used to create this secure string.");
            if (ex.Message.Contains("data is invalid")) ex = MException.MessageException("The ciphertext data blob is invalid.");
            return ex;
        }

        private bool HasValidUser
        {
            get { return !(Username.Value.XIsBlank() || (IsPasswordEnabled && Password.Value.XIsBlank())); }
        }
        
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
            }
            return false;
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
