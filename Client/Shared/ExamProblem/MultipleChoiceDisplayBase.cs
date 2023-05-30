﻿using ClassHub.Client.Models;
using Microsoft.AspNetCore.Components;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClassHub.Client.Shared.ExamProblem {
    public class MultipleChoiceDisplayBase : ComponentBase {
        [Parameter]
        public MultipleChoiceProblem Problem { get; set; }

        [Parameter]
        public bool IsRandomChoice { get; set; }

        protected override void OnInitialized() {
            // 처음 객관식 문제가 생성될 때 보기 랜덤 순서 여부에 따라 셔플
            if (IsRandomChoice) {
                // 보기 순서 무작위 셔플
                var rnd = new Random();
                Problem.Questions = Problem.Questions.OrderBy(item => rnd.Next()).ToArray();
            }
        }

        // 객관식 답안이 선택되면 Problem의 멤버변수 Answer에 선택한 정답을 저장
        public void OnAnswerSelected(ChangeEventArgs e) {
            if(int.TryParse(e.Value.ToString(), out int selectedAnswer)) {
                Problem.Answer = selectedAnswer;
            }
        }   
    }
}
