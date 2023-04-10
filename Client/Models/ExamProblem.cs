﻿namespace ClassHub.Client.Models {
public class ExamProblem {
    public int ProblemId { get; set; }
    public string Description { get; set; }
}

// 객관식
public class MultipleChoiceProblem : ExamProblem {
    public string[] Questions { get; set; }
    public int? Answer { get; set; } = null;
}

// 단답형
public class ShortAnswerProblem : ExamProblem {
    public string Answer { get; set; } = "";
}

// 코드형
public class CodeProblem : ExamProblem {
    public string Example { get; set; } = "";
    public string Answer { get; set; } = "";
}
}