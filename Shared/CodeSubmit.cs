namespace ClassHub.Shared
{
    public class CodeSubmit
    {
        public int room_id { get; set; }
        public int week { get; set; } 
        public int assignment_id { get; set; } 
        public int submit_id { get; set; }
        public DateTime submit_date { get; set; }
        public string status { get; set; }
        public int student_id { get; set; }
        public double exec_time { get; set; }          // 실행 시간(ms)    // TODO : 기준 정립 필요 (ex. 평균)
        public long mem_usage { get; set; }              // 메모리 사용량(?) // TODO : 단위 지정 필요, 기준 정립 필요 (ex. 평균)
        public string code { get; set; }
        public string message { get; set; }
        public string language { get; set; }
    }
}
