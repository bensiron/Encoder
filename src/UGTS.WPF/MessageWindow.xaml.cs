using System;
using System.Windows;
using System.Windows.Controls;

namespace UGTS.WPF
{
	public partial class MessageWindow : UgtsWindow
	{
        private Button myChoice;        

		public MessageWindow() : base()
		{
            Activated += HActivated;
			InitializeComponent();
		}

        private void HActivated(object sender, System.EventArgs e)
        {
            inputEditor.Focus();
        }

        private void HButtonClick(object sender, EventArgs e)
        {
            CloseForm(sender, e);
        }

        public string MessageText
        {
            get { return messageText.Text; }
            set { messageText.Text = value; }
        }

        public string InputText
        {
            get { return inputEditor.Text; }
            set { inputEditor.Text = value; }
        }

        public bool ShowInput
        {
            get { return inputEditor.Visibility == Visibility.Visible; }
            set
            {
                inputEditor.Visibility = value.XToVisibility();
                messageText.Height = Height - (value ? 112 : 86);  // Todo: adjust
            }
        }

        public void SetButtonText(string text)
        {
            var list = text.XSplit(",");
            for (var i = 0; i <= 2; i++)
            {
                var b = Button(i);
                b.Content = i < list.Count ? list[i] : "";
                b.Visibility = (!b.Content.XToString().XIsBlank()).XToVisibility();
            }
        }

        public Button Button(int index)
        {
            return index <= 0 ? button0 : (index == 1 ? button1 : button2);
        }

        public Button ButtonChosen
        {
            get { return myChoice; }
        }

        private void CloseForm(object sender, EventArgs e)
        {
            myChoice = sender as Button;
            Close();
        }

        public static MessageWindowResult AskQuestion(string messageText, string title, string buttonList = "Cancel,OK", string initialValue = null, bool showInput = true)
        {
            var m = new MessageWindow();
            m.MessageText = messageText;
            m.InputText = initialValue;
            m.ShowInput = showInput;
            m.Title = title;
            m.Topmost = true;
            m.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            m.SetButtonText(buttonList);
            m.ShowDialog();
            return m.Result;
        }

        public MessageWindowResult Result
        {
            get 
            {
                var result = new MessageWindowResult();
                result.InputText = InputText;
                result.ButtonChosen = ButtonChosen == null ? "" : ButtonChosen.Content.XToString();
                return result;
            }
        }

	}

    public class MessageWindowResult
    {
        public string ButtonChosen;
        public string InputText;
    }
}
