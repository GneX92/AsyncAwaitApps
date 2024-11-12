﻿using System.Text;
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

namespace DnsHtml_WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public Dictionary<string , Dictionary<string , int>> dic = new();

    public MainWindow()
    {
        InitializeComponent();
        List<string> hostlist = new();
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

    public List<string> ResolveEntry( string host )
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
        List<string> listHost = new();
        List<string> listIP = new();
        List<Task<string>> tasks = new();
        List<Task<Dictionary<string , int>>> tasks2 = new();
        Dictionary<string , Task<string>> dic2 = new();

        foreach ( var host in hosts )
        {
            string hostName = host.Trim();
            Task<string> t = Task.Run( () => GetHtml( hostName ) );
            dic2.Add( hostName , t );
            tasks.Add( t );
        }

        try
        {
            Task.WhenAll( tasks ).Wait();
        }
        catch ( AggregateException ex )
        {
            string log = "";
            foreach ( var item in ex.InnerExceptions )
                log += item.Message + "\n";

            MessageBox.Show( log , "Error" , MessageBoxButton.OK );
        }

        foreach ( var task in tasks )
        {
            Task<Dictionary<string , int>> t = Task.Run( () => CountTags( task.Result ) );
            tasks2.Add( t );
        }

        Task.WhenAll( tasks2 ).Wait();

        foreach ( var task in tasks )

            lbHosts.ItemsSource = hosts;
    }

    private void lbHost_SelectionChanged( object sender , SelectionChangedEventArgs e )
    {
        if ( lbHosts.SelectedIndex != -1 )
        {
            lbIP.ItemsSource = ResolveEntry( lbHosts.SelectedItem as string );
        }
    }

    private void lbIP_SelectionChanged( object sender , SelectionChangedEventArgs e )
    {
        if ( lbIP.SelectedIndex != -1 )
        {
        }
    }

    public string GetHtml( string url )
    {
        // Anfrage an die übergebene URL starten
        HttpClient httpClient = new HttpClient();
        Task<string> t = httpClient.GetStringAsync( url );
        string result = "";
        try
        {
            result = t.Result;
        }
        catch ( AggregateException ex )
        {
            string log = "";
            foreach ( var item in ex.InnerExceptions )
                log += item.Message + "\n";

            MessageBox.Show( log , "Error" , MessageBoxButton.OK );
        }

        return result;
    }

    public Dictionary<string , int> CountTags( string html )
    {
        Dictionary<string , int> tagCounts = new Dictionary<string , int>();

        // Regular expression to match HTML tags
        Regex tagRegex = new Regex( @"<\s*([a-z][a-z0-9]*)\b[^>]*>" , RegexOptions.IgnoreCase );

        // Find all matches
        MatchCollection matches = tagRegex.Matches( html );

        // Count occurrences of each tag
        foreach ( Match match in matches )
        {
            string tagName = match.Groups [ 1 ].Value.ToLower(); // Convert to lowercase for consistency
            if ( tagCounts.ContainsKey( tagName ) )
            {
                tagCounts [ tagName ]++;
            }
            else
            {
                tagCounts [ tagName ] = 1;
            }
        }

        return tagCounts;
    }
}

//BeginInvoke((MethodInvoker)delegate {
//textBox1.Text += "Text";
//});