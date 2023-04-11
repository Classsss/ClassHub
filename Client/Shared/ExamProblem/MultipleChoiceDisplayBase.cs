using ClassHub.Client.Models;
using Microsoft.AspNetCore.Components;

namespace ClassHub.Client.Shared.ExamProblem {
    public class MultipleChoiceDisplayBase : ComponentBase {
        [Parameter]
        public MultipleChoiceProblem Problem { get; set; }

        // 객관식 답안이 선택되면 Problem의 멤버변수 Answer에 선택한 정답을 저장
        public void OnAnswerSelected(ChangeEventArgs e) {
            if(int.TryParse(e.Value.ToString(), out int selectedAnswer)) {
                Problem.Answer = selectedAnswer;
            }
        }
    }
}
