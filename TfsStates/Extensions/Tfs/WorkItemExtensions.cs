﻿using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace TfsStates.Extensions.Tfs
{
    public static class WorkItemExtensions
    {
        public static string GetFieldValue(this WorkItem workItem, string field)
        {
            var value = workItem.Fields.ContainsKey(field)
                ? Convert.ToString(workItem.Fields[field])
                : null;
            return value;
        }

        public static DateTime? GetFieldDateValue(this WorkItem workItem, string field)
        {
            var value = workItem.GetFieldValue(field);

            if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out DateTime dateValue))
            {                
                 return dateValue;                
            }

            return null;
        }

        public static IdentityRef GetFieldIdentity(this WorkItem workItem, string field)
        {
            var value = workItem.Fields.ContainsKey(field)
                ? workItem.Fields[field] as IdentityRef
                : null;
            return value;
        }

        public static string State(this WorkItem workItem)
        {
            return workItem.GetFieldValue("System.State");
        }

        public static string Reason(this WorkItem workItem)
        {
            return workItem.GetFieldValue("System.Reason");
        }

        public static DateTime? StateChangeDate(this WorkItem workItem)
        {
            return workItem.GetFieldDateValue("Microsoft.VSTS.Common.StateChangeDate");
        }

        public static DateTime CreatedDate(this WorkItem workItem)
        {
            return workItem.GetFieldDateValue("System.CreatedDate").Value;
        }

        public static DateTime? ClosedDate(this WorkItem workItem)
        {
            return workItem.GetFieldDateValue("Microsoft.VSTS.Common.ClosedDate");
        }

        public static string Title(this WorkItem workItem)
        {
            return workItem.GetFieldValue("System.Title");
        }

        public static string Type(this WorkItem workItem)
        {
            return workItem.GetFieldValue("System.WorkItemType");
        }

        public static bool IsBug(this WorkItem workItem)
        {
            return workItem.Type() == "Bug";
        }

        public static string IterationPath(this WorkItem workItem)
        {
            return workItem.GetFieldValue("System.IterationPath");
        }

        public static string Tags(this WorkItem workItem)
        {
            return workItem.GetFieldValue("System.Tags");
        }

        public static string Priority(this WorkItem workItem)
        {
            return workItem.GetFieldValue("Microsoft.VSTS.Common.Priority");
        }

        public static string Severity(this WorkItem workItem)
        {
            return workItem.GetFieldValue("Microsoft.VSTS.Common.Severity");
        }

        public static string ChangedBy(this WorkItem workItem)
        {
            const string key = "System.ChangedBy";
            var identity = workItem.GetFieldIdentity(key);
            var name = identity?.DisplayName ?? workItem.GetFieldValue(key);
            return name;
        }

        public static string ChangedByNameOnly(this WorkItem workItem)
        {
            var changedBy = workItem.ChangedBy();

            if (changedBy == null) return null;

            var pos = changedBy.IndexOf('<');
            var pos2 = changedBy.IndexOf('>', pos+1);

            if (pos > -1 && pos2 > pos)
            {
                var username = changedBy.Substring(pos, pos2 - pos + 1);
                var name = changedBy.Replace($" {username}", string.Empty);
                return name;
            }

            return changedBy;
        }
    }
}
