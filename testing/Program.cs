using System.Net;

namespace testing;

internal class Program
{
    static void Main( string [] args )
    {
        ResolveHosts( @"D:\Texte\hosts.txt" );
    }

    static List<IPAddress> ResolveHosts( string path )
    {
        string [] hosts = File.ReadAllLines( path );
        List<IPAddress> iplist = new();

        foreach ( string host in hosts )
        {
            IPHostEntry entry = Dns.GetHostEntry( host );

            Console.WriteLine( "# " + host );

            foreach ( var item in entry.AddressList )
            {
                Console.WriteLine( item );
                iplist.Add( item );
            }
        }

        return iplist;
    }

    static void GetHtml( List<IPAddress> list )
    {
        HttpClient client = new();

        foreach ( IPAddress ip in list )
        {
            string html = client.GetStringAsync( ip.ToString() );
        }
    }
}