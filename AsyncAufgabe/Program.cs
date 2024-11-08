using static System.Net.Mime.MediaTypeNames;

var files = Directory.GetFiles( @"D:\Texte\" ).ToArray();

List<Task> tasks = new();

foreach ( var file in files )
{
    string localpath = file;
    Task t = Task.Factory.StartNew( () => DistinctLetterCount( localpath ) );
    tasks.Add( t );
}

Task.WaitAll( tasks.ToArray() );
//Console.ReadLine();

static void DistinctLetterCount( string path )
{
    string localpath = path;
    string content = File.ReadAllText( localpath );
    Dictionary<char , int> counters = new();
    FillDictionary( ref counters , content );

    foreach ( char c in content )
        if ( char.IsAscii( c ) )
            counters [ char.ToLower( c ) ]++;

    var output = counters.OrderByDescending( x => x.Value ).Select( x => x.Key + " : " + x.Value ).ToArray();

    string outputpath = Path.ChangeExtension( localpath , ".freq" );

    File.WriteAllLines( outputpath , output );
}

static void FillDictionary( ref Dictionary<char , int> d , string source )
{
    var characters = source.ToLower().Distinct();

    foreach ( var c in characters )
        if ( char.IsAscii( c ) )
            d [ c ] = 0;
}

static void PrintDictionary( Dictionary<char , int> d , string? header = null )
{
    if ( header != null )
        Console.WriteLine( header );

    foreach ( var kvp in d )
        Console.WriteLine( $"{kvp.Key}: {kvp.Value}" );
}