using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
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
            var settings = await this.tfsSettingsService.GetSettings();
            if (settings == null) return null;

            var uri = new Uri(settings.Url);
            var creds = TfsCredentialsFactory.Create(settings);
            var connection = new VssConnection(uri, creds);

            var projectClient = connection.GetClient<ProjectHttpClient>();
            var states = ProjectState.Unchanged | ProjectState.WellFormed;
            var projects = await projectClient.GetProjects(states, top: 200);

            var projectNames = projects
                .OrderBy(p => p.Name)
                .Select(p => p.Name)
                .ToList();
            return projectNames;
        }
    }
}
