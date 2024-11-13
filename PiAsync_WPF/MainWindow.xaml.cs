using System.CodeDom;
using System.Drawing;
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
using static System.Net.Mime.MediaTypeNames;

namespace PiAsync_WPF;

public partial class MainWindow : Window
{
    private CancellationTokenSource? cts;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void btnStart_Click( object sender , RoutedEventArgs e )
    {
        cts = new();
        tbResult.Text = "Calculating ...";

        Progress<int> progress = new( value =>
        {
            pbStatus.Value = value;
            labelStatus.Content = value.ToString( "N0" ) + " / 1.000.000.000";
        } );

        await DoWorkAsync( cts.Token , progress );
    }

    public Task<double> ComputePiAsync( CancellationToken token , IProgress<int> progress )
    {
        return Task.Run( () =>
        {
            double sum = 0.0;
            const double step = 1e-9;
            const int iterations = 1_000_000_000;

            for ( int i = 0 ; i < iterations ; i++ )
            {
                token.ThrowIfCancellationRequested();
                double x = ( i + 0.5 ) * step;
                sum += 4.0 / ( 1.0 + x * x );

                if ( i % 1000000 == 0 )
                    progress.Report( i );
            }
            return sum * step;
        } , token );
    }

    public async Task DoWorkAsync( CancellationToken token , IProgress<int> progress )
    {
        try
        {
            double result = await ComputePiAsync( token , progress );
            labelStatus.Content = pbStatus.Maximum.ToString( "N0" ) + " / 1.000.000.000";
            tbResult.Text = result.ToString();
        }
        catch ( OperationCanceledException )
        {
            tbResult.Text = "Calculation cancelled";
        }
    }

    private void btnCancel_Click( object sender , RoutedEventArgs e ) => cts?.Cancel();
}