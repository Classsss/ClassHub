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

    public class AttendanceSummary {
        public int total_attendance; //모든 출석 차시
        public int attend; //총 출석
        public int late; //총 지각
        public int absent; //총 결석
        public int no_target_attendance; //총 대상아님
        public int total_todo; //총 할일
        public int finished; //총 완료
        public int not_solved; //총 미완료
        public int lecturematerials; //총 강의자료

        public AttendanceSummary() {
            total_attendance = 0;
            attend = 0;
            late = 0;
            absent = 0;
            no_target_attendance = 0;
            total_todo = 0;
            finished = 0;
            not_solved = 0;
            lecturematerials = 0;
        }
    }
}
