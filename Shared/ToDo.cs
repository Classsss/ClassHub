namespace ClassHub.Shared {
    public enum Kind {
        실습, 과제, 온라인강의, 온라인시험
    }

    public class ToDo {
        public string RoomTitle { get; set; }
        public string Title { get; set; }
        public Kind Kind { get; set; }
        public DateTime EndTime { get; set; }
        public string Uri { get; set; }

        public string GetDDay() {
            if(DateTime.Now > EndTime) return "종료";
            TimeSpan remainPeriod = EndTime - DateTime.Now;
            if(remainPeriod.Days > 0) return $"D-{remainPeriod.Days}";
            else if(remainPeriod.Hours > 0) return $"{remainPeriod.Hours}시간 남음";
            else if(remainPeriod.Minutes > 0) return $"{remainPeriod.Minutes}분 남음";
            else return $"{remainPeriod.Seconds}초 남음";
        }

        public string GetTitle() {
            return $"[{Kind}] {Title}";
        }
    }
}