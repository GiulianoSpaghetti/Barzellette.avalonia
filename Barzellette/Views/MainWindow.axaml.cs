using Avalonia.Controls;

namespace Barzellette.Views;

public partial class MainWindow : Window
{

    public static MainWindow window = null;
    public MainWindow()
    {
        window = this;
        InitializeComponent();
        if (MainView.isConnected)
            this.Closing += (s, e) => MainView.conn.Close();

    }

}
