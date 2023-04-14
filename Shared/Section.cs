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
        public int SectionId { get; set; } // ���� ��ȣ (SectionId)

        [JsonPropertyName("year")]
        public int Year { get; set; } // ���� �⵵

        [JsonPropertyName("semester")]
        public string Semester { get; set; } // ���� �б�

        [JsonPropertyName("name")]
        public string Name { get; set; } // �������

        [JsonPropertyName("instructor")]
        public string Instructor { get; set; } // ��米��

        [JsonPropertyName("day")]
        public DayOfWeek Day { get; set; } // ���ǿ���

        [JsonPropertyName("startTime")]
        public int StartTime { get; set; } // ���� ���۽ð�

        [JsonPropertyName("endTime")]
        public int EndTime { get; set; } // ���� ����ð�

        [JsonPropertyName("buildingId")]
        public string BuildingId { get; set; } // ���ǽ� (BuildingId)

        public int Color { get; set; } = -1;
    }
}