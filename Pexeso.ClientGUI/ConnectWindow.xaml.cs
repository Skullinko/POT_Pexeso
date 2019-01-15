using System.Windows;
using System.Windows.Input;

namespace Pexeso.ClientGUI
{
    public partial class ConnectWindow
    {
        public ConnectWindow()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, NicknameBox);

            Closing += (sender, args) =>
            {
                if (string.IsNullOrEmpty(NicknameBox.Text.Trim()))
                    args.Cancel = true;
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NicknameBox.Text.Trim()))
                return;

            Close();
        }
    }
}