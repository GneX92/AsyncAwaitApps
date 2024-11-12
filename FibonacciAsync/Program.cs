namespace FibonacciAsync;

internal class Program
{
    static void Main( string [] args )
    {
        List<Task<long>> tasks = new();
        CancellationTokenSource cts = new();

        Task loadingTask = Task.Run( () => ShowActivityIndicator( cts.Token ) );

        for ( int i = 30 ; i <= 45 ; i++ )
        {
            int current = i;
            Task<long> t = Task.Run( () => Fib( current ) );
            tasks.Add( t );
        }

        long [] completed = Task.WhenAll( tasks ).Result.ToArray();

        cts.Cancel();

        Console.Write( "\rDone!                \n\n" );

        Console.WriteLine( "Fibonacci:" );

        foreach ( long t in completed )
            Console.WriteLine( t.ToString( "N0" ) );

        Console.ReadLine();
    }

    static void ShowActivityIndicator( CancellationToken token )
    {
        string [] s = { "/" , "-" , "\\" , "|" };
        int counter = 0;

        while ( !token.IsCancellationRequested )
        {
            Console.Write( $"\rCalculating {s [ counter++ % s.Length ]}" );
            Thread.Sleep( 100 );
        }
    }

    static long Fib( long x )
    {
        return x <= 2 ? 1 : Fib( x - 1 ) + Fib( x - 2 );
    }
}