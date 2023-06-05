using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace TBRContestCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContestantData MattData { get; set; }
        private List<ContestantData> ContestantData { get; set; }
        private List<string> PotentialNonBookHeaders = new List<string>() { "Place", "Name", "Average Guessed", "Absolute Off Average", "Off Average", "Total Correct", "% Correct" };
        private List<string> AllHeaders { get; set; }
        private List<string> CurrentNonBookHeaders { get; set; }

        private List<string> Books { get; set; }
        private bool LoadedCSVData { get; set; }
        private bool MattEnteredScores { get; set; }
        private bool Calculated { get; set; }


        public MainWindow()
        {
            MattData = new ContestantData();
            ContestantData = new List<ContestantData>();
            Books = new List<string>();
            AllHeaders = new List<string>();
            CurrentNonBookHeaders = new List<string>();
            LoadedCSVData = false;
            MattEnteredScores = false;
            Calculated = false;
            InitializeComponent();
        }

        private void CsvSelector_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var labelText = string.Empty;
            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                ContestantData = GetContestantData(path);
                LoadedCSVData = ContestantData.Count > 0;
                labelText = $"{ContestantData.Count} rows successfully loaded";
                RowsAdded.Content = labelText;
            }
            CheckForEnabledProperties();
        }

        private void EnterScores_Click(object sender, RoutedEventArgs e)
        {
            MattBookScoreEntry entryWindow = new MattBookScoreEntry(Books);
            if (entryWindow.ShowDialog() == true)
            {
                var mattScores = entryWindow.MattScores;
                foreach (var score in mattScores)
                {
                    MattData.Predictions[score.Title] = score.Score;
                }
                MattEnteredScores = true;
            }
            CheckForEnabledProperties();
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            ContestantData = CalculateScores(ContestantData);
            Calculated = true;
            CheckForEnabledProperties();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var reOrderedContestants = ContestantData.OrderByDescending(a => a.TotalCorrect)
                                                     .ThenByDescending(b => b.PercentCorrect)
                                                     .ThenBy(c => c.AbsoluteOffAverage)
                                                     .ThenBy(d => d.OffAverage).ToList();

            for (int i = 0; i < reOrderedContestants.Count; i++)
                reOrderedContestants[i].Place = i + 1;
            WriteToCSV(reOrderedContestants);
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MattData = new ContestantData();
            ContestantData = new List<ContestantData>();
            Books = new List<string>();
            CurrentNonBookHeaders = new List<string>();
            LoadedCSVData = false;
            MattEnteredScores = false;
            Calculated = false;
            CheckForEnabledProperties();
            RowsAdded.Content = "0 Number of rows added";
            ExportStatusLabel.Content = string.Empty;
            ExportStatusLabel.Visibility = Visibility.Hidden;
        }

        private List<ContestantData> GetContestantData(string path)
        {
            var listOfContestantData = new List<ContestantData>();

            try
            {
                //Getting Contestant data
                using (TextFieldParser parser = new TextFieldParser(path))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Grab all Books in order
                    if (!parser.EndOfData)
                    {
                        AllHeaders = parser.ReadFields().ToList();
                        var books = AllHeaders?.Where(x => !PotentialNonBookHeaders.Contains(x)).ToList();
                        var nonBookHeaders = AllHeaders?.Where(x => !books.Contains(x)).ToList();
                        Books.AddRange(books);
                        CurrentNonBookHeaders.AddRange(nonBookHeaders);
                    }

                    //Get Contestant Data
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        var successFullyParsed = int.TryParse(fields[0], out var place);
                        if(!successFullyParsed)
                        {
                            var errorPopup = new ErrorPopup($"Expected integer. Found {fields[0]} instead");
                            errorPopup.ShowDialog();
                            ResetButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                            return new List<ContestantData>();
                        }

                        ContestantData contestant = new ContestantData()
                        {
                            Place = place,
                            Name = fields[1],
                            Predictions = new Dictionary<string, double>(),
                        };
                        for (int i = 2; i < fields.Length; i++)
                        {
                            if (string.IsNullOrEmpty(fields[i]))
                                continue;
                            foreach (var book in Books)
                            {
                                var score = fields[i];
                                if (!string.IsNullOrEmpty(score))
                                {
                                    contestant.Predictions.Add(book, double.Parse(score));
                                }
                                i++;
                            }
                        }
                        listOfContestantData.Add(contestant);
                    }
                }
            }
            catch (Exception e)
            {

            }
            return listOfContestantData;
        }

        private List<ContestantData> CalculateScores(List<ContestantData> contestantData)
        {
            // math calculations

            foreach (var contestant in contestantData)
            {
                double contestantScore = 0;
                double mattScore = 0;
                double numBooks = Books.Count;
                double totalDifference = 0;
                var totalCorrect = 0;
                var mattAverageScore = (double)MattData.Predictions.Sum(x => x.Value) / (double)numBooks;
                foreach (var book in Books)
                {
                    contestantScore = contestant.Predictions[book];
                    mattScore = MattData.Predictions[book];
                    var currentDifference = Math.Abs(contestantScore - mattScore);
                    // if Matt gives a half score (e.g. 4.5), he gives those who 
                    // predicted a 4 or 5 +1 to total correct
                    if (currentDifference == 0 || (currentDifference > 0 && currentDifference < 1))
                        totalCorrect++;

                    totalDifference += currentDifference;

                }
                var contestantAverageScore = (double)contestant.Predictions.Sum(x => x.Value) / (double)numBooks;
                contestant.AverageGuessed = Math.Round(contestantAverageScore, 2);
                contestant.OffAverage = Math.Round(Math.Abs(mattAverageScore - contestantAverageScore), 2);
                double absoluteOffAverage = Math.Round((double)totalDifference / (double)numBooks, 2);
                contestant.AbsoluteOffAverage = absoluteOffAverage;
                contestant.TotalCorrect = totalCorrect;
                contestant.PercentCorrect = Math.Round((double)totalCorrect / (double)numBooks, 2);
            }
            return contestantData;
        }

        private bool WriteToCSV(List<ContestantData> reOrderedContestants)
        {
            var csvData = new StringBuilder();
            csvData.AppendLine(string.Join(",", AllHeaders));
            foreach (var contestant in reOrderedContestants)
            {
                var place = contestant.Place;
                var name = contestant.Name;
                var averageGuessed = contestant.AverageGuessed;
                var offAverage = contestant.OffAverage;
                var absoluteOffAverage = contestant.AbsoluteOffAverage;
                var totalCorrect = contestant.TotalCorrect;
                var percentCorrect = contestant.PercentCorrect;
                var bookScores = new StringBuilder();
                for (int i = 0; i < Books.Count; i++)
                {
                    bookScores.Append(contestant.Predictions[Books[i]]);
                    if (i != contestant.Predictions.Count - 1)
                    {
                        bookScores.Append(",");
                    }
                }
                var strBookScores = bookScores.ToString();
                var entry = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", place, name, bookScores, averageGuessed, absoluteOffAverage, offAverage, totalCorrect, percentCorrect);
                csvData.AppendLine(entry);
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Data Files (*.csv)|*.csv";
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.AddExtension = true;
            var successfullyExported = false;
            var path = string.Empty;
            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
                File.WriteAllText(path, csvData.ToString());
                successfullyExported = true;
            }

            ExportStatusLabel.Content = successfullyExported ? $"Saved CSV to {path}" : $"Problem saving CSV to {path}";
            ExportStatusLabel.Visibility = Visibility.Visible;
            return true;
        }

        private void CheckForEnabledProperties()
        {
            var readyToCalc = LoadedCSVData && MattEnteredScores;
            CalculateButton.IsEnabled = readyToCalc;
            CalculateReady.IsChecked = readyToCalc;

            var readyToExport = Calculated;
            ExportButton.IsEnabled = readyToExport;
            ExportReady.IsChecked = readyToExport;

            CsvSelectorButton.IsEnabled = !LoadedCSVData;
        }
    }
}
