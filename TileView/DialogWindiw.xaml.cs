using System.Windows;

namespace TileView
{
    public partial class DialogWindiw : Window
    {
        public string Text 
        {
            get => ResponseTextBox.Text;
            set => ResponseTextBox.Text = value;
        }

        public DialogWindiw()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
