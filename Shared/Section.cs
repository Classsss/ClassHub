using System.Text.Json.Serialization;

namespace ClassHub.Shared
{
    public class SectionRequest
    {
        private string AccessToken;
    }
    public class Section
    {
        [JsonPropertyName("sectionId")]
        public int SectionId { get; set; } // 강좌 번호 (SectionId)

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

        [JsonPropertyName("buildingId")]
        public string BuildingId { get; set; } // 강의실 (BuildingId)

        public int Color { get; set; } = -1;
    }
}