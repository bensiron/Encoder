using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;

namespace UGTS.Encoder
{
    public static class LoginExtensions
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

            if (!MWinApi.LogonUser(_userName.Username, _userName.Domain, pass, MWinApi.LOGON32_LOGON_INTERACTIVE, MWinApi.LOGON32_PROVIDER_DEFAULT, ref _token))
                throw MWinApi.Win32Exception("impersonating user '" + user + "'");

            if (loadProfile) Load();
            _identity = new WindowsIdentity(_token);
            _impersonation = _identity.Impersonate();
        }

        public static void Clear()
        {
            if (_isProfileLoaded) { MWinApi.UnloadUserProfile(_token, _profile.hProfile); _isProfileLoaded = false; }
            if (_impersonation != null) { _impersonation.Undo(); _impersonation = null; }
            _identity = null;
            if (_token != IntPtr.Zero) { MWinApi.CloseHandle(_token); _token = IntPtr.Zero; }
            _userName = null;
        }

        public static void Load()
        {
            _profile.Size = Marshal.SizeOf(_profile);
            _profile.Flags = 1;
            _profile.UserName = _userName.ToString();

            if (!MWinApi.LoadUserProfile(_token, ref _profile))
                throw new Exception("loading user profile");

            _isProfileLoaded = true;
        }

        public static bool IsValid(string domain, string user, string pass)
        {
            var p = IntPtr.Zero;
            var result = false;
            try
            {
                result = MWinApi.LogonUser(user, domain, pass, MWinApi.LOGON32_LOGON_INTERACTIVE,
                    MWinApi.LOGON32_PROVIDER_DEFAULT, ref p);
            }
            catch
            {

            }
            finally
            {
                if (p != IntPtr.Zero) { MWinApi.CloseHandle(p); p = IntPtr.Zero; }
            }

            return result;
        }
    }

    public enum DataProtectionLevel
    {
        None = 0,
        Machine = 1,
        User = 2, // for backwards compatibility only - DPAPI does not currently support sharing encryped data between machines for a user account
        MachineUser = 3
    }

    public static class MDataProtectionLevel
    {
        public static DataProtectionScope XToDataProtectionScope(this DataProtectionLevel level)
        {
            return level == DataProtectionLevel.Machine ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser;
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
    public struct _PRIVILEGE_SET
    {
        public int PrivilegeCount;
        public int Control;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // ANYSIZE_ARRAY = 1
        public LUID_AND_ATTRIBUTES[] Privileges;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_PRIVILEGES
    {
        public int PrivilegeCount;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //public int[] Privileges;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // ANYSIZE_ARRAY = 1
        public LUID_AND_ATTRIBUTES[] Privileges;

        public TOKEN_PRIVILEGES(LUID luid, int attributes)
        {
            Privileges = null;
            PrivilegeCount = 0;
            Add(luid, attributes);
        }

        public TOKEN_PRIVILEGES(LUID_AND_ATTRIBUTES privilege)
        {
            Privileges = null;
            PrivilegeCount = 0;
            Add(privilege);
        }

        public void Add(LUID_AND_ATTRIBUTES privilege)
        {
            PrivilegeCount++;
            Array.Resize(ref Privileges, PrivilegeCount);
            Privileges[PrivilegeCount - 1] = privilege;
        }

        public void Add(LUID luid, int attributes)
        {
            PrivilegeCount++;
            Array.Resize(ref Privileges, PrivilegeCount);
            Privileges[PrivilegeCount - 1] = new LUID_AND_ATTRIBUTES() { Luid = luid, Attributes = attributes };
        }
    }

    public class WindowsUserName
    {
        public string Username;

        public string Domain;
        public WindowsUserName()
        {
            Username = Environment.UserName;
            Domain = Environment.UserDomainName;
        }

        public WindowsUserName(string user)
        {
            var pos = user.IndexOf('\\');
            if (pos >= 0) { Domain = user.Substring(0, pos).Trim(); Username = user.Substring(pos + 1).Trim(); }
            else { Domain = ""; Username = user; }
            if (Domain.XIsBlank()) Domain = Environment.UserDomainName;
            if (Username.XIsBlank()) Username = Environment.UserName;
        }

        public override string ToString()
        {
            return (!Domain.XIsBlank() ? Domain + "\\" : "") + Username;
        }

        /// <summary>
        /// WindowsUserName objects are equal if they refer to the same user account
        /// </summary>
        public override bool Equals(object obj)
        {
            var o = obj as WindowsUserName;
            if (o == null) return false;
            return o.ToString().ToLower() == ToString().ToLower();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public enum ThreadAccess
    {
        DELETE = 0x10000,
        READ_CONTROL = 0x20000,
        WRITE_DAC = 0x40000,
        WRITE_OWNER = 0x80000,
        SYNCHRONIZE = 0x100000,
        THREAD_DIRECT_IMPERSONATION = 0x200,
        THREAD_GET_CONTEXT = 0x8,
        THREAD_IMPERSONATE = 0x100,
        THREAD_QUERY_INFORMATION = 0x40,
        THREAD_QUERY_LIMITED_INFORMATION = 0x800,
        THREAD_SET_CONTEXT = 0x10,
        THREAD_SET_INFORMATION = 0x20,
        THREAD_SET_LIMITED_INFORMATION = 0x400,
        THREAD_SET_THREAD_TOKEN = 0x80,
        THREAD_SUSPEND_RESUME = 0x2,
        THREAD_TERMINATE = 0x1
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

    public static class MWinApi
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LoadUserProfile(IntPtr hToken, ref PROFILEINFO info);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LogonUser(string user, string domain, string pass, int logonType, int logonProvider, ref IntPtr hToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess desiredAccess, bool inheritHandle, int threadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SuspendThread(IntPtr hThread);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnloadUserProfile(IntPtr hToken, IntPtr hProfile);

        [DllImport("psapi.dll")]
        public static extern bool EmptyWorkingSet(IntPtr hProcess);

        public const int LOGON32_PROVIDER_DEFAULT = 0;

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_LOGON_NETWORK = 3;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_LOGON_SERVICE = 5;
        public const int LOGON32_LOGON_UNLOCK = 7;
        public const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;


        /// <summary>
        /// Uses GetLastWin32Error to determine if an error occurred, and if so, creates an Win32Exception from the error code.
        /// Will return null if there was no error, and alwaysCreate is false.
        /// </summary>
        public static Exception Win32Exception(string action, bool alwaysCreate = true)
        {
            var err = Marshal.GetLastWin32Error();
            if ((err == 0) && !alwaysCreate) return null;
            var ex = new Win32Exception(err);
            return new Exception("Win32 ERROR " + err + (!action.XIsBlank() ? ", " + action : "") + ": " + ex.Message, ex);
        }
    }
}
