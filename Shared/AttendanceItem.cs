namespace ClassHub.Shared {
    public class AttendanceItem {
        public int Id { get; set; } //출석 아이템의 고유코드 (강의자료 번호, 공지사항 번호 등등)
        public int Week { get; set; } //주차
        public int Chapter { get; set; } //차시
        public string Title { get; set; } //학습 제목
        public string LearningType { get; set; } // 학습유형
        public string AttendProgress { get; set; } // 출결현황
        public string DetailLink { get; set; } // 대상 자료 링크
        public int Status { get; set; } // 오프라인 출결상태
        public DateTime startDate { get; set; } //시작일
    }
}
