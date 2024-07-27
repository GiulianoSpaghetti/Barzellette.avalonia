using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using System.Globalization;
using System.Runtime.InteropServices;
using System;
using Avalonia.Interactivity;
using MySqlConnector;
using DesktopNotifications.FreeDesktop;
using INotificationManager = DesktopNotifications.INotificationManager;
using Notification = DesktopNotifications.Notification;
namespace Barzellette
{
    public partial class MainWindow : Window
    {
        private static INotificationManager notification = CreateManager();
        private static Notification not;
        public static ResourceDictionary d;
        private static MySqlConnectionStringBuilder builder;
        public static MySqlConnection conn;
        private static MySqlCommand command;
        private static MySqlDataReader reader;
        private static Random r;
        public static bool isConnected = false;

        private static INotificationManager CreateManager()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new FreeDesktopNotificationManager();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new DesktopNotifications.Windows.WindowsNotificationManager(null);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new DesktopNotifications.Apple.AppleNotificationManager();

            throw new PlatformNotSupportedException();
        }
        public MainWindow()
        {
            InitializeComponent();
            notification.Initialize();
            d = this.FindResource(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) as ResourceDictionary;
            if (d == null)
                d = this.FindResource("it") as ResourceDictionary;
            r = new Random();

            mnFile.Header = d["File"];
            mnEsci.Header = d["Exit"];
            mnPI.Header = d["?"];
            mnInfo.Header = d["Informations"];
            btnConnetti.Content = d["Connetti"];
            btnIndietro.Content = d["Back"];
            btnAvanti.Content = d["Forward"];
            btnVisualizza.Content = d["Show"];
            btnRandom.Content = d["Random"];
        }

	public static void MakeNotification(String title, String body)
        {
            not = new Notification
            {
                Title = title,
                Body = body
            };
            notification.ShowNotification(not);
        }
        private async void connect(string ip, string user, string passwd)
        {
            builder = new MySqlConnectionStringBuilder
            {
                Server = ip,
                Database = "barzellette",
                UserID = user,
                Password = passwd,
                SslMode = MySqlSslMode.Disabled,
            };
            conn = new MySqlConnection(builder.ConnectionString);
            try
            {
                await conn.OpenAsync();
            }
            catch (MySqlException ex)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, ex.Message);
		return;
            }
            btnVisualizza.IsEnabled=true;
        }
	
	private void btnConnetti_Clicked(object sender, RoutedEventArgs e)
        {
	            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 4)
            {
                connect(args[1], args[2], args[3]);
                isConnected = btnVisualizza.IsEnabled;
                btnVisualizza.IsEnabled = isConnected;
                btnAvanti.IsEnabled = isConnected;
                btnIndietro.IsEnabled = isConnected;
                btnRandom.IsEnabled = isConnected;
            }
            if (!isConnected)
                MakeNotification(d["Error"] as string, d["InvalidParameters"] as string);
	}
        private void btn_Clicked(object sender, RoutedEventArgs e)
        {
            ulong id = 0;
            try
            {
                id = ulong.Parse(txtid.Text);
            }
            catch (System.FormatException)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IDNotInteger"] as string);
                return;
            }
            catch (System.ArgumentNullException)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IDEmpty"] as string);
                return;
            }
            command = conn.CreateCommand();
            command.CommandText = $"SELECT Testo FROM Barzellette WHERE ID={id}";
            try
            {
                reader = command.ExecuteReader();
            }
            catch (MySqlException ex)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, ex.Message);
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;
                return;
            }
            catch (InvalidOperationException ex)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IpErrato"] as string);
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;
                return;

            }
            if (reader.HasRows)
            {
                reader.Read();
                lblbarzelletta.Content = reader.GetString(0);
            }
            else
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IDNotFound"] as string);
            }
            reader.Close();
        }

        private void btnIndietro_Clicked(object sender, RoutedEventArgs e)
        {
            ulong id;
            try
            {
                id = ulong.Parse(txtid.Text);
            }
            catch (System.FormatException)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IDNotFound"] as string);
                return;
            }
            txtid.Text = $"{--id}";
        }

        private void btnAvanti_Clicked(object sender, RoutedEventArgs e)
        {
            ulong id;
            try
            {
                id = ulong.Parse(txtid.Text);
            }
            catch (System.FormatException ex)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, ex.Message);
                return;
            }
            txtid.Text = $"{++id}";

        }

        private void btnRandom_Clicked(object sender, RoutedEventArgs e)
        {
            command = conn.CreateCommand();
            command.CommandText = $"Select ID FROM Barzellette order by ID desc LIMIT 1";
            try
            {
                reader = command.ExecuteReader();
            }
            catch (MySqlException ex)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IDNotFound"] as string);
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;
                return;
            }
            catch (InvalidOperationException ex)
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IpErrato"] as string);
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;
                return;

            }
            if (reader.HasRows)
            {
                reader.Read();
                int id = reader.GetInt32(0);
                txtid.Text = $"{r.Next(1, id)}";
            }
            else
            {
                MainWindow.MakeNotification(MainWindow.d["Error"] as string, MainWindow.d["IDNotFound"] as string);
            }
            reader.Close();
        }


        public static bool IsConnected()
        {
            return isConnected;
        }
        private void mnEsci_click(object sender, RoutedEventArgs e)
        {
		Close();
        }
        private void mnInfo_click(object sender, RoutedEventArgs e)
        {
            lblbarzelletta.Content = $"Giulio Sorrentino ⓒ  2023-2024\nVersione: 0.3\nUn semplice fortune personale basato su Avalonia e MariaDB\nLicenza: GPL 3.0 o, secondo la tua opinione, qualsiasi versione successiva.";
        }
    }
}
