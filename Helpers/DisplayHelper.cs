namespace RaceDayMvc.Helpers
{
    // Small helpers that only change how values are shown on screen.
    public static class DisplayHelper
    {
        // Turns 1 into "1st", 2 into "2nd", 47 into "47th" and so on.
        public static string Ordinal(int number)
        {
            int lastTwo = number % 100;

            if (lastTwo >= 11 && lastTwo <= 13)
            {
                return number + "th";
            }

            return (number % 10) switch
            {
                1 => number + "st",
                2 => number + "nd",
                3 => number + "rd",
                _ => number + "th"
            };
        }

        // Shows a finish time as hh:mm:ss.
        public static string FinishTime(TimeSpan time)
        {
            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        // The Bootstrap/RaceDay colour class used for a status badge.
        public static string StatusClass(string? status)
        {
            return (status ?? string.Empty).ToLowerInvariant() switch
            {
                "confirmed" => "status-confirmed",
                "pending" => "status-pending",
                "cancelled" => "status-cancelled",
                "upcoming" => "status-upcoming",
                "completed" => "status-completed",
                _ => "status-other"
            };
        }

        // Event types offered in drop-down lists.
        public static readonly string[] EventTypes =
        {
            "Road Race", "Trail Run", "Marathon", "Fun Run", "Cycling", "Triathlon"
        };

        // Event statuses an organiser can choose from.
        public static readonly string[] EventStatuses =
        {
            "Upcoming", "Completed", "Cancelled"
        };
    }
}