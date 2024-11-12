using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace HTML_WPFv2;

public partial class MainWindow : Window
{
    private readonly HtmlParser _urlHtmlProcessor;
    private readonly HtmlTagCounter _htmlTagCounter;
    private CancellationTokenSource _cts;
    private Dictionary<string , ObservableCollection<KeyValuePair<string , int>>>? _hostTagCountDictionary;

    public MainWindow()
    {
        InitializeComponent();
        _urlHtmlProcessor = new HtmlParser();
        _htmlTagCounter = new HtmlTagCounter();
    }

    private async void btnOpenFile_Click( object sender , RoutedEventArgs e )
    {
        string path = await _urlHtmlProcessor.OpenFileDialogAsync();
        tbFilePath.Text = path;
    }

    private async void btnStart_Click( object sender , RoutedEventArgs e )
    {
        await Application.Current.Dispatcher.InvokeAsync( () =>
        {
            Mouse.OverrideCursor = Cursors.Wait;
        } );

        btnCancel.IsEnabled = true;
        btnStart.IsEnabled = false;

        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        try
        {

            string [] hosts = await _urlHtmlProcessor.ReadHostsFromFileAsync( tbFilePath.Text );
            var hostHtmlDictionary = await _urlHtmlProcessor.FetchHtmlAsync( hosts, token );
            var hostTagCountDictionary = await _htmlTagCounter.AnalyzeTagCountsAsync( hostHtmlDictionary, token );           

            lbHosts.ItemsSource = hosts;
            _hostTagCountDictionary = hostTagCountDictionary;

        }
        catch ( OperationCanceledException )
        {
            MessageBox.Show( "Processing cancelled." , "Cancelled" , MessageBoxButton.OK );
        }
        catch ( Exception ex )
        {
            MessageBox.Show( $"Error: {ex.Message}" , "Error" , MessageBoxButton.OK );
        }
        finally
        {
            _cts.Dispose();
            btnCancel.IsEnabled = false;
            btnStart.IsEnabled = true;
        }

        await Application.Current.Dispatcher.InvokeAsync( () =>
        {
            Mouse.OverrideCursor = null;
        } );
    }

    private void lbHost_SelectionChanged( object sender , SelectionChangedEventArgs e )
    {
        if ( lbHosts.SelectedIndex != -1 )
        {
            string? selectedHost = lbHosts.SelectedItem as string;
            lbIP.ItemsSource = _urlHtmlProcessor.ResolveHostIpAddresses( selectedHost );
            lbHTML.ItemsSource = _hostTagCountDictionary [ selectedHost ];
        }
    }

    private void btnCancel_Click( object sender , RoutedEventArgs e )
    {
        _cts.Cancel();
    }
}



