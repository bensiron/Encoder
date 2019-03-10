using System;
using System.Windows;
using System.Windows.Controls;
using UGTS.Dictionaries;

namespace UGTS.WPF
{
	public partial class UserLoginWindow : Window
	{
        [Flags]
		public enum Element
		{
			None = 0,
			Account = 1,
			Domain = 2,
			Username = 4,
			Password = 8,
			Back = 0x10,
			OK = 0x20,
			Cancel = 0x40,
			Warning = 0x80,
			All = -1
		}

        public Element ButtonPressed;

		private Element myVisibleElements;
		private Element myEnabledElements;
		private SafeStringDictionary<Element> myAccounts = new SafeStringDictionary<Element>();


		public UserLoginWindow() : base()
		{
			MouseUp += HKeyDown;
			SizeChanged += HKeyDown;
			Activated += HKeyDown;
			KeyDown += HKeyDown;
			InitializeComponent();
            accountText.SelectionChanged += HSelectionChanged;
            myVisibleElements = Element.All;
			myEnabledElements = Element.All;
		}

		public UserLoginWindow.Element VisibleElements 
        {
			get { return myVisibleElements; }
			set 
            {
				myVisibleElements = value;
				UpdateVisiblility();
			}
		}

		public UserLoginWindow.Element EnabledElements 
        {
			get { return myEnabledElements; }
			set 
            {
				myEnabledElements = value;
				UpdateInputs(accountText.Text);
			}
		}

		private void UpdateVisiblility()
		{
			var ca = new Control[]{accountLabel, accountText, domainLabel, domainText, userLabel, userText, passwordLabel, passwordText};
			bool v;

		    var pos = 10;
			for (var i = 0; i <= 3; i++) 
            {
				var l = (Label)ca[i * 2];
				var t = ca[i * 2 + 1];
				var e = ToElement(t);
				v = myVisibleElements.HasFlag(e);
				var vs = v.XToVisibility();
				l.Visibility = vs;
				t.Visibility = vs;
				if (!v)	continue;
				new [] {l, t}.XUpdateMargin(top:pos);
				pos += 26;
			}

			pos++;
			new Control[] {backButton, okButton, cancelButton, warningLabel}.XUpdateMargin(top:pos);
			pos += 50;

			var rpos = 12;
			foreach (var button in new []{cancelButton, okButton, backButton}) 
            {
				var eb = ToElement(button);
				v = myVisibleElements.HasFlag(eb);
				if (v) { button.XUpdateMargin(right:rpos); rpos += 81; }
				button.Visibility = v.XToVisibility();
			}

			v = myVisibleElements.HasFlag(Element.Warning);
			if (v) warningLabel.XUpdateMargin(right:rpos);
			warningLabel.Visibility = v.XToVisibility();

			if (myVisibleElements.HasFlag(Element.Back | Element.OK | Element.Cancel | Element.Warning)) pos += 19;

			MinHeight = pos;
			Height = pos;
			MaxHeight = pos;
		}

		public void AddAccounts(Element applicableInputs, params string[] accounts)
		{
			foreach (string a in accounts) 
            {
				myAccounts[a] = applicableInputs;
				accountText.Items.Add(a);
				if (!a.XIsBlank() && accountText.Text.XIsBlank())	accountText.Text = a;
			}
		}

		public void AddDomains(params string[] domains)
		{
			foreach (string d in domains) 
            {
				domainText.Items.Add(d);
				if (!d.XIsBlank() && domainText.Text.XIsBlank()) domainText.Text = d;
			}
		}

		public string Domain 
        {
			get { return domainText.Text; }
			set { domainText.Text = value; }
		}

		public string Account 
        {
			get { return accountText.Text; }
			set { accountText.Text = value; }
		}

		public string Username 
        {
			get { return userText.Text; }
			set { userText.Text = value; }
		}

		public string Password 
        {
			get { return passwordText.Password; }
			set { passwordText.Password = value; }
		}

		private void HButton(System.Object sender, RoutedEventArgs e)
		{
			DialogResult = !object.ReferenceEquals(sender, cancelButton);
			ButtonPressed = ToElement(sender);
			Hide();
		}

		private Element ToElement(object c)
		{
			if (object.ReferenceEquals(c, accountText))
				return Element.Account;
			if (object.ReferenceEquals(c, domainText))
				return Element.Domain;
			if (object.ReferenceEquals(c, userText))
				return Element.Username;
			if (object.ReferenceEquals(c, passwordText))
				return Element.Password;
			if (object.ReferenceEquals(c, backButton))
				return Element.Back;
			if (object.ReferenceEquals(c, okButton))
				return Element.OK;
			if (object.ReferenceEquals(c, cancelButton))
				return Element.Cancel;
			if (object.ReferenceEquals(c, warningLabel))
				return Element.Warning;
			return Element.None;
		}

		public Element ElementFocus 
        {
			get { return ToElement(Focus()); }
			set { ToControl(value).XFocus(); }
		}

		private Control ToControl(Element e)
		{
			foreach (var c in this.XChildren<Control>()) { if (ToElement(c) == e) return c; }
			return null;
		}

		private void HSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateInputs(e.XNewSelection());
		}

		private void UpdateInputs(string acct)
		{
			var e = Element.All;
			if (myAccounts.ContainsKey(acct)) e = myAccounts[acct];
			domainText.IsEnabled = (e & myEnabledElements).HasFlag(Element.Domain);
			userText.IsEnabled = (e & myEnabledElements).HasFlag(Element.Username);
			passwordText.IsEnabled = (e & myEnabledElements).HasFlag(Element.Password);
		}

		private void HKeyDown(object sender, EventArgs e)
		{
			UpdateWarning();
		}

		private void UpdateWarning()
		{
			var key = new Microsoft.VisualBasic.Devices.Keyboard();
			var s = "";
            if (key.NumLock) s = "NUM LOCK is ON";
            if (key.CapsLock) s = "CAPS LOCK is ON";
			warningLabel.Content = s;
		}
	}
}
