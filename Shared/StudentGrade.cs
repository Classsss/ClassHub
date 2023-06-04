namespace ClassHub.Shared {
    public class StudentGrade {
        private GradeRatio grade_ratio;
        private double _attendance_score;
        private double _assignment_score;
        private double _practice_score;
        private double _exam_score;

        public StudentGrade() {
            grade_ratio = new GradeRatio();
        }

        public StudentGrade(GradeRatio gradeRatio) {
            grade_ratio = gradeRatio;
        }

        public int student_id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public double attendance_score {
            get { return _attendance_score * grade_ratio.attendance_ratio; }
            set { _attendance_score = value; }
        }
        public double assignment_score {
            get { return _assignment_score * grade_ratio.assignment_ratio; }
            set { _assignment_score = value; }
        }
        public double practice_score {
            get { return _practice_score * grade_ratio.practice_ratio; }
            set { _practice_score = value; }
        }
        public double exam_score {
            get { return _exam_score * grade_ratio.exam_ratio; }
            set { _exam_score = value; }
        }
        public double final_score {
            get { return attendance_score + assignment_score + practice_score + exam_score; }
        }
    }
}
