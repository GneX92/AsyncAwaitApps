using SkiaSharp;
using System.Drawing;

namespace AsyncGrayscalePicture;

internal class Program
{
    static void Main( string [] args )
    {
        FileSystemWatcher watcher = new( @"D:\Bilder" );
        //List<Task> tasks = new List<Task>();

        Console.WriteLine( @"Watching D:\Bilder ..." );
        Console.WriteLine( "\nLog:" );

        watcher.Created += OnCreated;
        watcher.Filter = "*.jpg";

        watcher.EnableRaisingEvents = true;

        Console.ReadLine();
    }

    static void OnCreated( object sender , FileSystemEventArgs e )
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine( "New File detected! " );
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( "Start processing ..." );
        Console.ResetColor();

        Task.Run( () => WaitForFile( e.FullPath ) )
            .ContinueWith( t => Console.WriteLine( "Processing completed for: " + e.FullPath ) );
    }

    static void WaitForFile( string path )
    {
        while ( true )
        {
            try
            {
                using ( FileStream stream = File.Open( path , FileMode.Open , FileAccess.Read , FileShare.None ) )
                    if ( stream.Length > 0 )
                        break;
            }
            catch ( IOException )
            {
                Thread.Sleep( 500 );
            }
        }

        GetBitmap( path );
    }

    static void GetBitmap( string path )
    {
        using ( var image = SKBitmap.Decode( path ) )
            if ( image != null )
            {
                string grayFilename = @"D:\BilderGrau\" + "gray_" + Path.GetFileName( path );
                ColorToGray( image , grayFilename );
            }
    }

    static void ColorToGray( SKBitmap image , string grayFilename )
    {
        for ( int x = 0 ; x < image.Width ; x++ )
            for ( int y = 0 ; y < image.Height ; y++ )
            {
                SKColor color = image.GetPixel( x , y );
                byte gray = (byte) ( 0.2126f * color.Red + 0.7152f * color.Green + 0.0722f *
               color.Blue );
                SKColor sKColor = new SKColor( gray , gray , gray , color.Alpha );
                image.SetPixel( x , y , sKColor );
            }

        try
        {
            using ( FileStream grayFileStream = File.Create( grayFilename ) )
            {
                image.Encode( grayFileStream , SKEncodedImageFormat.Jpeg , 100 );
            }
        }
        catch ( Exception e )
        {
            Console.WriteLine( e.Message );
        }
        image.Dispose();
    }
}