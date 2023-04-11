using ClassHub.Client.Models;
using Microsoft.AspNetCore.Components;

namespace ClassHub.Client.Shared.ExamProblem {
    public class CodeProblemDisplayBase : ComponentBase {
        [Parameter]
        public CodeProblem Problem { get; set; }
    }

}
