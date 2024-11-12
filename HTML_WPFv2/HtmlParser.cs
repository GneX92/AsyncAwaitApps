using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace HTML_WPFv2;
public class HtmlParser
{
    public async Task<string> OpenFileDialogAsync()
    {
        var openFile = new OpenFileDialog();
        var result = openFile.ShowDialog();
        return result == true ? openFile.FileName : string.Empty;
    }

    public async Task<string []> ReadHostsFromFileAsync( string filePath )
    {
        return File.ReadAllLines( filePath );
    }

    public async Task<Dictionary<string , string>> FetchHtmlAsync( string [] hosts , CancellationToken token )
    {
        var hostHtmlDictionary = new Dictionary<string , string>();

        foreach ( var host in hosts )
        {
            token.ThrowIfCancellationRequested();
            string hostName = host.Trim();
            string html = await GetHtmlAsync( hostName , token );
            hostHtmlDictionary [ hostName ] = html;
        }

        return hostHtmlDictionary;
    }

    public List<string> ResolveHostIpAddresses( string host )
    {
        var listIP = new List<string>();
        var entry = Dns.GetHostEntry( host );

        foreach ( var item in entry.AddressList )
        {
            listIP.Add( item.ToString() );
        }

        return listIP;
    }

    private async Task<string> GetHtmlAsync( string url , CancellationToken token )
    {
        if ( !url.StartsWith( "http://" ) && !url.StartsWith( "https://" ) )
        {
            url = "http://" + url;
        }

        using ( var httpClient = new HttpClient() )
        {
            try
            {
                return await httpClient.GetStringAsync( url, token );
            }
            catch ( HttpRequestException ex )
            {
                MessageBox.Show( $"Error fetching HTML for {url}: {ex.Message}" , "Error" , MessageBoxButton.OK );
                return string.Empty;
            }
            catch ( OperationCanceledException )
            {
                // Rethrow the cancellation exception
                throw;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( $"Unexpected error for {url}: {ex.Message}" , "Error" , MessageBoxButton.OK );
                return string.Empty;
            }
        }
    }
}