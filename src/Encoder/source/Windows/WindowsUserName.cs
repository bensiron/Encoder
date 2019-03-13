using System;

namespace UGTS.Encoder.Windows
{
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
            if (Domain.IsBlank()) Domain = Environment.UserDomainName;
            if (Username.IsBlank()) Username = Environment.UserName;
        }

        public override string ToString()
        {
            return (!Domain.IsBlank() ? Domain + "\\" : "") + Username;
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
}