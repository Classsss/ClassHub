using ClassHub.Client.Models;
using Microsoft.AspNetCore.Components;

namespace ClassHub.Client.Shared.ExamProblem {
    public class ShortAnswerDisplayBase : ComponentBase {
        [Parameter]
        public ShortAnswerProblem Problem { get; set; }
    }
}
