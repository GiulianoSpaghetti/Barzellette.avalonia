using Avalonia.Controls;
using Avalonia.Interactivity;
using MySqlConnector;
using System;
using System.Data.Common;
using System.Data;
using System.Globalization;

namespace Barzellette.avalonia
{
    public partial class MainWindow : Window
    {

        private MySqlConnectionStringBuilder builder;
        private MySqlConnection conn;
        private MySqlCommand command;
        private MySqlDataReader reader;
        private Random r;
        private ResourceDictionary d;
        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();
            InitializeComponent();
            d = this.FindResource(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) as ResourceDictionary;
            if (d == null)
                d = this.FindResource("it") as ResourceDictionary;
            r = new Random();
            if (args.Length == 4)
            {
                connect(args[1], args[2], args[3]);
                if (btnVisualizza.IsEnabled)
                    this.Closing += (s, e) => conn.Close();
            }
            else
            {
                lblbarzelletta.Content =d["InvalidParameters"] as string;
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;

            }
            mnFile.Header = d["File"];
            mnEsci.Header = d["Exit"];
            mnPI.Header = d["?"];
            mnInfo.Header = d["Informations"];
            btnIndietro.Content = d["Back"];
            btnAvanti.Content = d["Forward"];
            btnVisualizza.Content = d["Show"];
            btnRandom.Content = d["Random"];
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
                lblbarzelletta.Content = ex.Message;
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;

            }
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
                lblbarzelletta.Content = d["IDNotInteger"] as string;
                return;
            }
            catch (System.ArgumentNullException)
            {
                lblbarzelletta.Content = d["IDEmpty"] as string;
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
                lblbarzelletta.Content = ex.Message;
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;
                return;
            }
            catch (InvalidOperationException ex)
            {
                lblbarzelletta.Content = d["IpErrato"];
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
                lblbarzelletta.Content = d["IDNotFound"] as string;
            }
            reader.Close();
        }

        private void btnIndietro_Clicked(object sender, RoutedEventArgs e)
        {
            ulong id;
            lblbarzelletta.Content = "";
            try
            {
                id = ulong.Parse(txtid.Text);
            }
            catch (System.FormatException)
            {
                lblbarzelletta.Content = d["IDNotFound"] as string;
                return;
            }
            txtid.Text = $"{--id}";
        }

        private void btnAvanti_Clicked(object sender, RoutedEventArgs e)
        {
            ulong id;
            lblbarzelletta.Content = "";
            try
            {
                id = ulong.Parse(txtid.Text);
            }
            catch (System.FormatException)
            {
                lblbarzelletta.Content = d["IDNotFound"] as string;
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
                lblbarzelletta.Content = ex.Message;
                btnVisualizza.IsEnabled = false;
                btnAvanti.IsEnabled = false;
                btnIndietro.IsEnabled = false;
                btnRandom.IsEnabled = false;
                return;
            }
            catch (InvalidOperationException ex)
            {
                lblbarzelletta.Content = d["IpErrato"];
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
                lblbarzelletta.Content = d["IDNotFound"];
            }
            reader.Close();
        }

        private void mnEsci_click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void mnInfo_click(object sender, RoutedEventArgs e)
        {
            lblbarzelletta.Content = $"Autore: Giulio Sorrentino © 2023\nVersione: 0.2\nUn semplice fortune personale basato su Avalonia e MariaDB\nLicenza: GPL 3.0 o, secondo la tua opinione, qualsiasi versione successiva.";
        }
    }
}
