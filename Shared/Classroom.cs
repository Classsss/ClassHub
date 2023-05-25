namespace ClassHub.Shared {
    public class ClassRoom {
        public int room_id { get; set; }
        public string course_id { get; set; } // ABC12345
        public int section_id { get; set; } // 0810
        public string semester { get; set; } // SPRING, SUMMER, FALL, WINTER         
        public int year { get; set; } // 2023
        public string title { get; set; } // 캡스톤디자인
    }

    public class TimeSlot {
        public DayOfWeek day_of_week { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
    }

    public class ClassRoomDetail : ClassRoom {
        public List<TimeSlot> timeSlots { get; set; }
        public string instructor { get; set; }
        public string buildingId { get; set; }
        public int color { get; set; } = -1;
    }
}
