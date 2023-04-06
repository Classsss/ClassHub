using System.Text.Json.Serialization;

namespace ClassHub.Client.Models
{
    public class Course
    {
        [JsonPropertyName("number")]
        public int Number { get; set; } // 강좌 번호

        [JsonPropertyName("year")]
        public int Year { get; set; } // 개설 년도

        [JsonPropertyName("semester")]
        public string Semester { get; set; } // 개설 학기

        [JsonPropertyName("name")]
        public string Name { get; set; } // 교과목명

        [JsonPropertyName("instructor")]
        public string Instructor { get; set; } // 담당교수

        [JsonPropertyName("day")]
        public DayOfWeek Day { get; set; } // 강의요일

        [JsonPropertyName("startTime")]
        public int StartTime { get; set; } // 강의 시작시간

        [JsonPropertyName("endTime")]
        public int EndTime { get; set; } // 강의 종료시간

        [JsonPropertyName("classRoom")]
        public string ClassRoom { get; set; } // 강의실
    }
}