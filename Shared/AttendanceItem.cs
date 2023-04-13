namespace ClassHub.Shared {
    public class AttendanceItem {
        public int Week { get; set; }
        public int Lesson { get; set; }
        public string Title { get; set; } //학습 제목
        public string LearningType { get; set; } // 학습유형
        public string LearningProgress { get; set; } // 학습현황
        public string AttendProgress { get; set; } // 출결현황
        public string DetailLink { get; set; } // 대상 자료 링크
    }
}
