using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HTML_WPFv2;
public class HtmlTagCounter
{
    public async Task<Dictionary<string , ObservableCollection<KeyValuePair<string , int>>>> AnalyzeTagCountsAsync( Dictionary<string , string> hostHtmlDictionary, CancellationToken token )
    {
        var hostTagCountDictionary = new Dictionary<string , ObservableCollection<KeyValuePair<string , int>>>();

        foreach ( var (host, html) in hostHtmlDictionary )
        {
            token.ThrowIfCancellationRequested();
            var tagCounts = await CountTagsAsync( html );
            hostTagCountDictionary [ host ] = tagCounts;
        }

        return hostTagCountDictionary;
    }

    private async Task<ObservableCollection<KeyValuePair<string , int>>> CountTagsAsync( string html )
    {
        var tagCounts = new ObservableCollection<KeyValuePair<string , int>>();

        // Regular expression to match HTML tags
        var tagRegex = new Regex( @"<\s*([a-z][a-z0-9]*)\b[^>]*>" , RegexOptions.IgnoreCase );

        // Find all matches
        var matches = tagRegex.Matches( html );

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

        return tagCounts;
    }
}
