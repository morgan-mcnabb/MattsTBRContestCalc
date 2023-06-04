using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Reflection.Metadata.BlobBuilder;

namespace TBRContestCalc
{
    /// <summary>
    /// Interaction logic for MattBookScoreEntry.xaml
    /// </summary>
    public partial class MattBookScoreEntry : Window
    {
        private List<BookScore> _MattScores { get; set; }
        public List<BookScore> MattScores { get
            {
                return _MattScores;
            }
        }

        public MattBookScoreEntry(List<string> books)
        {
            _MattScores = new List<BookScore>();
            foreach(var book in books)
            {
                _MattScores.Add(new BookScore()
                {
                    Title = book,
                    Score = 0,
                });
            }
            InitializeComponent();
            GridOfData.ItemsSource = _MattScores;
            //GridOfData.DataContext = this._MattScores;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
           _MattScores = GridOfData.Items.OfType<BookScore>().ToList();
            DialogResult = true;
            this.Close();
        }
    }

    public class BookScore
    {
        public string Title { get; set; }
        public double Score { get; set; }
    }
}
