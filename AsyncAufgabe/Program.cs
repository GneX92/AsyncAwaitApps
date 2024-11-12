using static System.Net.Mime.MediaTypeNames;

var files = Directory.GetFiles( @"D:\Texte\" ).ToArray();

List<Task> tasks = new();

foreach ( var file in files )
{
    string localpath = file;
    Task t = Task.Run( () => DistinctLetterCount( localpath ) );
    tasks.Add( t );
}

Console.WriteLine( "Tasks running ..." );

await Task.WhenAll( tasks );

Console.WriteLine( "All Tasks finished ..." );
Console.ReadLine();

static async Task DistinctLetterCount( string path )
{
    string localpath = path;
    string content = await File.ReadAllTextAsync( localpath );

    Dictionary<char , int> counters = new();
    counters = await FillDictionary( content );

    foreach ( char c in content )
        if ( char.IsAscii( c ) )
            counters [ char.ToLower( c ) ]++;

    var output = counters.OrderByDescending( x => x.Value ).Select( x => x.Key + " : " + x.Value ).ToArray();

    string outputpath = Path.ChangeExtension( localpath , ".freq" );

    await File.WriteAllLinesAsync( outputpath , output );
}

static async Task<Dictionary<char , int>> FillDictionary( string source )
{
    Dictionary<char , int> d = new();

    var characters = source.ToLower().Distinct();

    foreach ( var c in characters )
        if ( char.IsAscii( c ) )
            d [ c ] = 0;

    return d;
}

static void PrintDictionary( Dictionary<char , int> d , string? header = null )
{
    if ( header != null )
        Console.WriteLine( header );

    foreach ( var kvp in d )
        Console.WriteLine( $"{kvp.Key}: {kvp.Value}" );
}