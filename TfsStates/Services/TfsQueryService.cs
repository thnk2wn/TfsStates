﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Humanizer;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using TfsStates.Extensions.Tfs;
using TfsStates.Models;

namespace TfsStates.Services
{
    public class TfsQueryService : ITfsQueryService
    {
        private readonly ITfsSettingsService tfsSettingsService;
        private readonly IBroadcastService broadcastService;
        private WorkItemTrackingHttpClient workItemClient;
        private object lockObject = new object();
        private int processedCount;

        public TfsQueryService(
            ITfsSettingsService tfsSettingsService,
            IBroadcastService broadCastService)
        {
            this.tfsSettingsService = tfsSettingsService;
            this.broadcastService = broadCastService;
        }

        public async Task<List<TfsInfo>> Query(TfsStatesModel model)
        {
            var sw = Stopwatch.StartNew();
            var connection = await this.tfsSettingsService.GetConnection();
            if (connection == null) throw new InvalidOperationException("no connection");

            this.workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();
            var wiql = TfsQueryBuilder.BuildQuery(model);
            var result = await this.workItemClient.QueryByWiqlAsync(new Wiql { Query = wiql });

            var queue = new ConcurrentQueue<TfsInfo>();
            var asyncOptions = GetAsyncOptions();

            var getRevsBlock = new ActionBlock<WorkItemReference>(
                async workItemRef =>
                {
                    var tfsInfo = await ProcessWorkItemRevisions(workItemRef);

                    if (tfsInfo.TransitionCount > model.MinTransitions) 
                    { 
                        queue.Enqueue(tfsInfo);
                    }
                },
                asyncOptions);

            foreach (var wiRef in result.WorkItems)
            {
                getRevsBlock.Post(wiRef);
            }

            getRevsBlock.Complete();
            await getRevsBlock.Completion;

            var list = queue
                .OrderBy(i => i.Iteration)
                .ThenBy(i => i.Id)
                .ToList();

            sw.Stop();

            return list;
        }

        private bool AsyncTrace = false;
        private ExecutionDataflowBlockOptions GetAsyncOptions()
        {
            #if DEBUG
            if (Debugger.IsAttached && AsyncTrace)
            {
                return new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 };
            }
            #endif

            return new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 6
            };
        }

        private async Task<TfsInfo> ProcessWorkItemRevisions(WorkItemReference workItemRef)
        {
            var revisions = await this.workItemClient.GetRevisionsAsync(
                workItemRef.Id, 
                expand: WorkItemExpand.Fields);

            string lastState = null;
            var revs = new WorkItemRevisionBuilder();

            foreach (var workItemRev in revisions)
            {
                var state = workItemRev.State();

                if (state != null && lastState != state)
                {
                    lastState = state;
                    ProcessWorkItemRevision(workItemRev, revs);
                }            
            }

            if (revs.States.Count > 1)
            {
                var sbTemp = new StringBuilder();

                for (int i = 0; i < revs.States.Count; i++)
                {
                    sbTemp.Append(revs.States[i].Summary);

                    if (i + 1 < revs.States.Count)
                    {
                        sbTemp.AppendLine();
                    }

                    revs.TfsInfo.Transitions = sbTemp.ToString().Trim();
                }

                revs.TfsInfo.TransitionCount = revs.States.Count;
            }

            lock (this.lockObject)
            {
                this.processedCount++;

                var progress = new ReportProgress
                {
                    Message = $"Processed {revs.TfsInfo.Title}",
                    WorkItemsProcessed = this.processedCount
                };

                this.broadcastService.ReportProgress(progress);
            }

            return revs.TfsInfo;
        }

        private void ProcessWorkItemRevision(WorkItem workItemRev, WorkItemRevisionBuilder revs)
        {
            var stateChange = new StateChange
            {
                State = workItemRev.State(),
                Reason = workItemRev.Reason(),
                By = workItemRev.ChangedBy()
            };

            if (revs.TfsInfo == null)
            {
                revs.TfsInfo = new TfsInfo
                {
                    Id = workItemRev.Id.Value,
                    Title = workItemRev.Title(),
                    Type = workItemRev.Type(),
                    State = stateChange.State,
                    Iteration = workItemRev.IterationPath(),
                    Tags = workItemRev.Tags(),
                    Priority = workItemRev.Priority(),
                    ClosedDate = workItemRev.ClosedDate()
                };

                if (workItemRev.IsBug())
                {
                    revs.TfsInfo.Severity = workItemRev.Severity();
                }
            }

            var stateChangeDate = workItemRev.StateChangeDate();

            if (stateChangeDate != null)
            {
                if (revs.LastStateChangeDate != null)
                {
                    stateChange.StateChangeDate = stateChangeDate.Value;

                    var diff = stateChange.StateChangeDate - revs.LastStateChangeDate.Value;
                    stateChange.DurationText = diff.Humanize();
                }
                else
                {
                    stateChange.StateChangeDate = workItemRev.CreatedDate();
                }

                revs.LastStateChangeDate = stateChangeDate;
            }

            revs.States.Add(stateChange);
        }

        class WorkItemRevisionBuilder
        {
            public WorkItemRevisionBuilder()
            {
                this.States = new List<StateChange>();
            }

            public TfsInfo TfsInfo { get; set; }

            public DateTime? LastStateChangeDate { get; set; }

            public List<StateChange> States { get; set; }
        }
    }
}
