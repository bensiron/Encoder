using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace UGTS.Encoder.Windows
{
    public static class WindowsLogin
    {
        [ThreadStatic]
        private static WindowsImpersonationContext _impersonation;
        [ThreadStatic]
        private static WindowsIdentity _identity;
        [ThreadStatic]
        private static IntPtr _token;
        [ThreadStatic]
        private static WindowsUserName _userName;
        [ThreadStatic]
        private static PROFILEINFO _profile;
        [ThreadStatic]
        private static bool _isProfileLoaded;

        public static void Impersonate(string user, string pass, bool loadProfile = false, bool alwaysImpersonate = false)
        {
            var username = new WindowsUserName(user);
            var current = new WindowsUserName();
            if (!alwaysImpersonate && username.Equals(current)) { return; }  // no need to impersonate the currently logged on user

            Clear();
            _userName = username;

            if (!WinApi.LogonUser(_userName.Username, _userName.Domain, pass, WinApi.LOGON32_LOGON_INTERACTIVE, WinApi.LOGON32_PROVIDER_DEFAULT, ref _token))
                throw WinApi.Win32Exception("impersonating user '" + user + "'");

            if (loadProfile) Load();
            _identity = new WindowsIdentity(_token);
            _impersonation = _identity.Impersonate();
        }

        public static void Clear()
        {
            if (_isProfileLoaded) { WinApi.UnloadUserProfile(_token, _profile.hProfile); _isProfileLoaded = false; }
            if (_impersonation != null) { _impersonation.Undo(); _impersonation = null; }
            _identity = null;
            if (_token != IntPtr.Zero) { WinApi.CloseHandle(_token); _token = IntPtr.Zero; }
            _userName = null;
        }

        private static void Load()
        {
            _profile.Size = Marshal.SizeOf(_profile);
            _profile.Flags = 1;
            _profile.UserName = _userName.ToString();

            if (!WinApi.LoadUserProfile(_token, ref _profile))
            {
                throw WinApi.Win32Exception("loading user profile");
            }

            _isProfileLoaded = true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public int LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public int Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROFILEINFO
    {
        public int Size;
        public int Flags;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string UserName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string ProfilePath;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string DefaultPath;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string ServerName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string PolicyPath;
        public IntPtr hProfile;
    }

    public static class WinApi
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LoadUserProfile(IntPtr hToken, ref PROFILEINFO info);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LogonUser(string user, string domain, string pass, int logonType, int logonProvider, ref IntPtr hToken);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnloadUserProfile(IntPtr hToken, IntPtr hProfile);

        public const int LOGON32_PROVIDER_DEFAULT = 0;
        public const int LOGON32_LOGON_INTERACTIVE = 2;

        /// <summary>
        /// Uses GetLastWin32Error to determine if an error occurred, and if so, creates an Win32Exception from the error code.
        /// Will return null if there was no error, and alwaysCreate is false.
        /// </summary>
        public static Exception Win32Exception(string action, bool alwaysCreate = true)
        {
            var err = Marshal.GetLastWin32Error();
            if ((err == 0) && !alwaysCreate) return null;
            var ex = new Win32Exception(err);
            return new Exception("Win32 ERROR " + err + (!action.IsBlank() ? ", " + action : "") + ": " + ex.Message, ex);
        }
    }
}
