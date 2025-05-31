using FoodTour.API.DTOs;
using System.Text.RegularExpressions;

namespace FoodTour.API.Helpers
{
    public static class AIResponseParser
    {
        public static List<RoutePlaceDto> ParseRouteFromAiResponse(string response)
        {
            var result = new List<RoutePlaceDto>();
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var regex = new Regex(
                @"^\d+\.\s*(?:(?<time>\d{1,2}:\d{2})\s*[-–]\s*)?(?<name>.+?)\s*\((?<address>.+?)\)\s*[-–]\s*(?<role>.+?)\s*[-–]\s*(?<note>.+)$",
                RegexOptions.Compiled);

            foreach (var line in lines)
            {
                var match = regex.Match(line.Trim());
                if (match.Success)
                {
                    result.Add(new RoutePlaceDto
                    {
                        Name = match.Groups["name"].Value.Trim(),
                        Address = match.Groups["address"].Value.Trim(),
                        Role = match.Groups["role"].Value.Trim(),
                        Note = match.Groups["note"].Value.Trim(),
                        TimeSlot = ConvertTimeToSlot(match.Groups["time"].Value),
                        Lat = 0,
                        Lng = 0
                    });
                }
            }

            return result;
        }

        private static string ConvertTimeToSlot(string time)
        {
            if (string.IsNullOrWhiteSpace(time)) return "Không xác định";

            if (time.StartsWith("06") || time.StartsWith("07") || time.StartsWith("08") || time.StartsWith("09")) return "Sáng";
            if (time.StartsWith("11") || time.StartsWith("12") || time.StartsWith("13") || time.StartsWith("14")) return "Trưa";
            if (time.StartsWith("17") || time.StartsWith("18") || time.StartsWith("19") || time.StartsWith("20")) return "Tối";

            return "Không xác định";
        }
    }
}
