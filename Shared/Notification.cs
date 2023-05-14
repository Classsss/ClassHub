namespace ClassHub.Shared {
    public class ClassRoomNotification {
        public int room_id { get; set; }
        public int student_id { get; set; }
        public int notification_id { get; set; }
        public string message { get; set; }
        public string uri { get; set; }
        public DateTime notify_date { get; set; }
    }

    public class StudentNotification {
        public int room_id { get; set; }
        public int student_id { get; set; }
        public int notification_id { get; set; }
        public bool is_read { get; set; }
    }

    public class DisplayStudentNotification : ClassRoomNotification {
        public bool is_read { get; set; }
        public string title { get; set; }
    }
}