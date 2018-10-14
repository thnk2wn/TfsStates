﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace TfsStates.Services
{
    public class TfsProjectService : ITfsProjectService
    {
        private readonly ITfsSettingsService tfsSettingsService;

        public TfsProjectService(ITfsSettingsService tfsSettingsService)
        {
            this.tfsSettingsService = tfsSettingsService;
        }

        // TODO: Consider caching such as https://github.com/alastairtree/LazyCache?WT.mc_id=-blog-scottha
        // https://www.hanselman.com/blog/UsingLazyCacheForCleanAndSimpleNETCoreInmemoryCaching.aspx
        // consider to a file as well as memory
        public async Task<List<string>> GetProjectNames()
        {
            var connection = await this.tfsSettingsService.GetConnection();
            if (connection == null) return null;

            var projectClient = connection.GetClient<ProjectHttpClient>();
            var states = ProjectState.Unchanged | ProjectState.WellFormed;
            var projects = await projectClient.GetProjects(states, top: 200);

            var projectNames = projects
                .OrderBy(p => p.Name)
                .Select(p => p.Name)
                .ToList();
            return projectNames;
        }

        public async Task<List<string>> GetIterations(string projectName)
        {
            var connection = await this.tfsSettingsService.GetConnection();
            if (connection == null) return null;
            
            var client = connection.GetClient<WorkItemTrackingHttpClient>();

            var rootIterationNode = await client.GetClassificationNodeAsync(
                projectName,
                TreeStructureGroup.Iterations,
                depth: int.MaxValue);

            var list = new List<IterationItem>();
            GetIterations(list, rootIterationNode);
            
            var sortedIterationNames = list
                .OrderBy(i => i.Path)
                .ThenBy(i => i.StartDate)
                .Select(i => i.Name)
                .ToList();

            return sortedIterationNames;
        }

        private void GetIterations(
            List<IterationItem> list, 
            WorkItemClassificationNode node, 
            string path = "")
        {
            var item = new IterationItem();

            if (path.Length > 0)
            {
                item.Path = path + @"\";
                path = path + @"\" + node.Name;
            }
            else 
            { 
                path = node.Name;
            }

            item.Name = path;

            if (node.Attributes != null && node.Attributes.Any(a => a.Key == "startDate"))
            {
                item.StartDate = Convert.ToDateTime(node.Attributes["startDate"]);
            }            

            list.Add(item);

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetIterations(list, child, path);
                }
            }
        }

        class IterationItem
        {
            public string Path { get; set; }

            public string Name { get; set; }

            public DateTime StartDate { get; set; }
        }
    }
}
