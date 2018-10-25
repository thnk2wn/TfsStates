using System.Collections.Generic;
using TfsStates.Models;

namespace TfsStates.ViewModels
{
    public class TfsStatesViewModel
    {
        public TfsStatesViewModel()
        {
            this.Projects = new List<string>();
            this.Iterations = new List<string>();

            this.RunReadyState = new RunReadyStateModel
            {
                IsReady = false
            };

            this.MinTransitions = 1;
        }

        public List<string> Projects { get; set; }

        public List<string> Iterations { get; set; }

        public string Project { get; set; }

        public string Iteration { get; set; }

        public int MinTransitions { get; set; }

        public RunReadyStateModel RunReadyState { get; set; }

        public string ResultFilename { get; set; }

        public ReportProgress FinalProgress { get; set; }

        public class RunReadyStateModel
        {
            public RunReadyStateModel()
            {
                this.State = RunStates.Unknown;
            }

            public bool IsReady { get; set; }

            public string Message { get; set; }

            public void NotReady(string message)
            {
                this.IsReady = false;
                this.Message = message;
                this.State = RunStates.NotReady;
            }

            public void Ready(string message = null)
            {
                this.IsReady = true;
                this.Message = message;
                this.State = RunStates.Ready;
            }

            public RunStates State { get; private set; }
        }

        public enum RunStates
        {
            Unknown,
            NotReady,
            Ready
        }
    }
}
