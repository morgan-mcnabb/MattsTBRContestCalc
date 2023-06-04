using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBRContestCalc
{
    public class ContestantData
    {
        public int? Place { get; set; }
        public string? Name { get; set; }

        // Set up like, {Hyperion Cantos, 4}, {Jade City, 5} etc.
        public Dictionary<string, double>? Predictions { get; set; }
        public double? AverageGuessed { get; set; }
        public double? OffAverage { get; set; }
        public double? AbsoluteOffAverage { get; set; }
        public int TotalCorrect { get; set; }
        public double PercentCorrect { get; set; }

        public ContestantData()
        {
            Predictions = new Dictionary<string, double>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Name: {Name}");
            sb.AppendLine();
            foreach (var prediction in Predictions)
            {
                sb.Append($"Book: {prediction.Key} Score: {prediction.Value}");
                sb.AppendLine();
            }
            sb.Append($"Average Guessed: {AverageGuessed}");
            sb.AppendLine();
            sb.Append($"Off Average: {OffAverage}");
            sb.AppendLine();
            sb.Append($"Absolute Off Average: {AbsoluteOffAverage}");
            sb.AppendLine();
            sb.Append($"Total Correct: {TotalCorrect}");
            sb.AppendLine();
            sb.Append($"Percent Correct: {PercentCorrect}");
            sb.AppendLine();
            sb.Append("-------------------------------------------------------------");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
