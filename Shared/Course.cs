namespace ClassHub.Shared
{
    public class SectionRequest
    {
        private string AccessToken;
    }
    public class SectionResponse
    {
        public int SectionId { get; set; } // ���� ��ȣ
        public int Year { get; set; } // ���� �⵵
        public int Semester { get; set; } // ���� �б�
        public string Name { get; set; } // �������
        public string Instructor { get; set; } // ��米��
        public DayOfWeek Day { get; set; } // ���ǿ���
        public int StartTime { get; set; } // ���� ���۽ð�
        public int EndTime { get; set; } // ���� ����ð�
        public string BuildingId { get; set; } // ���ǽ�
    }
}