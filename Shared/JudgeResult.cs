namespace ClassHub.Shared
{
    public class JudgeResult
    {
        public bool IsCorrect { get; set; }
        public double ExecutionTime { get; set; }
        public long MemoryUsage { get; set; }
        public string? CompileErrorMsg { get; set; }
        public string? RuntimeErrorMsg { get; set; }
        public bool IsTimeOut { get; set; } = false;
        public bool IsExceedMemory { get; set; } = false;
    }
}
