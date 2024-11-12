using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Net;
using System;
using Microsoft.Win32;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace DnsHtml_WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public Dictionary<string , ObservableCollection<KeyValuePair<string , int>>> dic = new();

    public MainWindow()
    {
        InitializeComponent();        
    }

    private void btnOpenFile_Click( object sender , RoutedEventArgs e )
    {
        string path = string.Empty;
        OpenFileDialog openFile = new();

        //openFile.Filter = "*.txt";

        var result = openFile.ShowDialog();

        try
        {
            if ( result == true )
                path = openFile.FileName;
        }
        catch ( Exception ex )
        {
            MessageBox.Show( ex.Message , "Error" , MessageBoxButton.OK );
        }

        tbFilePath.Text = path;
    }

    public static List<string> ResolveEntry( string host )
    {
        List<string> listIP = new List<string>();

        IPHostEntry entry = Dns.GetHostEntry( host );

        foreach ( var item in entry.AddressList )
        {
            listIP.Add( item.ToString() );
        }

        return listIP;
    }

    private void btnStart_Click( object sender , RoutedEventArgs e )
    {
        dic.Clear();
        string [] hosts = File.ReadAllLines( tbFilePath.Text );
        List<Task> tasks2 = new();
        Dictionary<string , Task<string>> dic2 = new();

        foreach ( var host in hosts )
        {
            string hostName = host.Trim();
            Task<string> t = Task.Run( () => GetHtml( hostName ) );
            dic2.Add( hostName , t );
        }

        try
        {
            Task.WhenAll( dic2.Values ).Wait();
        }
        catch ( AggregateException ex )
        {
            string log = "";
            foreach ( var item in ex.InnerExceptions )
                log += item.Message + "\n";

            MessageBox.Show( log , "Error" , MessageBoxButton.OK );
        }

        foreach ( var task in dic2 )
        {
            Task t = Task.Run( () => CountTags( task.Key, task.Value.Result ) );
            tasks2.Add( t );
        }

        Task.WhenAll( tasks2 ).Wait();

        

        lbHosts.ItemsSource = hosts;
    }

    private void lbHost_SelectionChanged( object sender , SelectionChangedEventArgs e )
    {
        if ( lbHosts.SelectedIndex != -1 )
        {
            string selectedHost = lbHosts.SelectedItem as string;
            lbIP.ItemsSource = ResolveEntry( selectedHost );
            lbHTML.ItemsSource = dic [ selectedHost ];
        }
    }

    private void lbIP_SelectionChanged( object sender , SelectionChangedEventArgs e )
    {
        //Might not be needed
    }

    public static string GetHtml( string url )
    {
        if ( !url.StartsWith( "http://" ) && !url.StartsWith( "https://" ) )
        {
            url = "http://" + url;
        }

        // Anfrage an die übergebene URL starten
        using ( HttpClient httpClient = new HttpClient() )
        {
            try
            {
                // Use async/await pattern instead of .Result to avoid potential deadlocks
                Task<string> t = httpClient.GetStringAsync( url );
                return t.GetAwaiter().GetResult();
            }
            catch ( HttpRequestException ex )
            {
                MessageBox.Show( $"Error fetching HTML for {url}: {ex.Message}" , "Error" , MessageBoxButton.OK );
                return string.Empty;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( $"Unexpected error for {url}: {ex.Message}" , "Error" , MessageBoxButton.OK );
                return string.Empty;
            }
        }
    }

    public void CountTags( string host, string html )
    {
        var tagCounts = new ObservableCollection<KeyValuePair<string , int>>();

        // Regular expression to match HTML tags
        Regex tagRegex = new Regex( @"<\s*([a-z][a-z0-9]*)\b[^>]*>" , RegexOptions.IgnoreCase );

        // Find all matches
        MatchCollection matches = tagRegex.Matches( html );

        // Count occurrences of each tag
        var tempDict = new Dictionary<string , int>();
        foreach ( Match match in matches )
        {
            string tagName = "<" + match.Groups [ 1 ].Value.ToLower() + ">"; // Convert to lowercase for consistency
            if ( tempDict.ContainsKey( tagName ) )
            {
                tempDict [ tagName ]++;
            }
            else
            {
                tempDict [ tagName ] = 1;
            }
        }

        // Convert to ObservableCollection
        foreach ( var kvp in tempDict.OrderByDescending( x => x.Value ) )
        {
            tagCounts.Add( kvp );
        }

        dic [ host ] = tagCounts;
    }
}

//BeginInvoke((MethodInvoker)delegate {
//textBox1.Text += "Text";
//});