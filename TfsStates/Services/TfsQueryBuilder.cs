using System;
using TfsStates.Models;

namespace TfsStates.Services
{
    public static class TfsQueryBuilder
    {
        public static string BuildQuery(TfsStatesModel model)
        {
            // TODO: Make work item type configurable
            var wiql = @"
SELECT [System.Id], [System.Title]
FROM WorkItems
WHERE [System.TeamProject] = '@project'
AND  [Work Item Type] IN ('User Story', 'Bug')";

            // Note: @CurrentIteration doesn't work in API, only TFS portal

            if (!string.IsNullOrEmpty(model.Iteration))
            {
                wiql = wiql + Environment.NewLine +
                     $"AND[System.IterationPath] UNDER '{model.Iteration}'";
            }

            wiql = wiql.Replace("@project", model.Project);

            return wiql;
        }
    }
}
