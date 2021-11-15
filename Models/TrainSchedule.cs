using System;
using System.Linq;
using System.Collections.Generic;

namespace TrainSchedule.Models
{
    public class TrainSchedule
    {
        public TrainSchedule()
        {
            Times = new();
        }

        public HashSet<string> Times { get; set; }
    }

    public class TrainSchedule_OLD
    {
        public TrainSchedule_OLD()
        {
            Times = new();
            TimeValues = new();
        }

        public HashSet<string> Times
        {
            get
            {
                return (from item in TimeValues select IntToTime(item, false)).ToHashSet();
            }
            set
            {
                TimeValues = new();
                foreach (var item in value) { TimeValues.Add(TimeToInt(item)); }
            }
        }

        private HashSet<int> TimeValues { get; set; }

        public void AddTime(string time)
        {
            var value = TimeToInt(time);

            if (!TimeValues.Contains(value))
            {
                TimeValues.Add(value);
            }
        }

        public void RemoveTime(string time)
        {
            var value = TimeToInt(time);

            if (TimeValues.Contains(value))
            {
                TimeValues.Remove(value);
            }
        }

        private static int TimeToInt(string time)
        {
            time = time ?? throw new NullReferenceException();
            var s = time.Split(new[] { ':', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (s.Length < 2)
                throw new InvalidOperationException("Time format is invalid, should be: ##:## [am|pm]");
            var hourText = s[0];
            var minuteText = s[1];

            if (hourText.Any(c => !char.IsDigit(c)))
                throw new InvalidOperationException("Time values should only contain numbers, like: ##:## [am|pm]");

            if (minuteText.Any(c => !char.IsDigit(c)))
                throw new InvalidOperationException("Time values should only contain numbers, like: ##:## [am|pm]");

            if (!int.TryParse(hourText, out var hourValue))
                throw new InvalidOperationException("Time value appears to be incorrect, should only contain numbers, like: ##:## [am|pm]");

            if (!int.TryParse(minuteText, out var minuteValue))
                throw new InvalidOperationException("Time value appears to be incorrect, should only contain numbers, like: ##:## [am|pm]");

            var ampm = "am";
            if (s.Length == 3)
                ampm = s[2].Trim().ToLower();

            if (ampm == "pm")
            {
                if (hourValue > 12)
                    throw new InvalidCastException("pm passed in but hour value is greater than 12.");

                hourValue += 12;
            }

            if (minuteValue > 59)
                throw new InvalidCastException("Invalid time format, minute value is greater than 59.");

            return hourValue * 60 + minuteValue;
        }

        private static string IntToTime(int value, bool ampm)
        {
            var hour = value / 60;
            var minutes = value % 60;
            var ampmText = "am";

            if (ampm)
            {
                hour -= 12;
                ampmText = "pm";
            }

            return $"{hour}:{minutes}{(ampm ? $" {ampmText}" : "")}";
        }

    }
}
