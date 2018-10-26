using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using TfsStates.Models;

namespace TfsStates.Services
{
    public class TfsSettingsService : ITfsSettingsService
    {
        public async Task<string> GetFilename()
        {
            var filename = await FileUtility.GetFilename("tfs-settings.json");
            return filename;
        }

        public async Task<bool> HasSettings()
        {
            var filename = await GetFilename();
            return File.Exists(filename);
        }

        public async Task<TfsKnownConnections> GetConnections()
        {
            var filename = await GetFilename();

            if (!File.Exists(filename)) return null;

            var json = await File.ReadAllTextAsync(filename);
            var model = JsonConvert.DeserializeObject<TfsKnownConnections>(json);

            foreach (var connection in model.Connections)
            {
                if (connection.ConnectionType == TfsConnectionTypes.TfsNTLM 
                    && !connection.UseDefaultCredentials
                    && connection.Password != null)
                {
                    connection.Password = EncryptionService.DecryptString(
                        connection.Password, 
                        EncryptionSettings.Key);
                }

                if (connection.ConnectionType == TfsConnectionTypes.AzureDevOpsToken
                    && connection.PersonalAccessToken != null)
                {
                    connection.PersonalAccessToken = EncryptionService.DecryptString(
                        connection.PersonalAccessToken, 
                        EncryptionSettings.Key);
                }
            }

            return model;
        }

        public async Task<TfsKnownConnections> GetConnectionsOrDefault()
        {
            var model = (await GetConnections())
                ?? new TfsKnownConnections { Connections = new List<TfsKnownConnection>() };
            return model;
        }

        public async Task<TfsKnownConnection> GetActiveConnection()
        {
            var connections = await GetConnections();
            var connection = connections?.GetActiveConnection();
            return connection;
        }

        public async Task Save(TfsKnownConnection connection)
        {
            string filename = await GetFilename();
            var connections = await GetConnectionsOrDefault();
            var originalPassword = connection.Password;
            var originalToken = connection.PersonalAccessToken;

            if (!string.IsNullOrEmpty(connection.Password))
            { 
                connection.Password = EncryptionService.EncryptString(connection.Password, EncryptionSettings.Key);
            }

            if (!string.IsNullOrEmpty(connection.PersonalAccessToken))
            {
                connection.PersonalAccessToken = EncryptionService.EncryptString(connection.Password, EncryptionSettings.Key);
            }

            var existingConnection = connections.Connections.FirstOrDefault(c => c.Id == connection.Id);

            if (existingConnection == null)
            {
                connection.AddedDate = DateTime.Now;
                connections.Connections.Add(connection);
            }
            else
            {
                connection.UpdatedDate = DateTime.Now;
                var index = connections.Connections.IndexOf(existingConnection);
                connections.Connections[index] = connection;
            }

            var json = JsonConvert.SerializeObject(connections, Formatting.Indented);
            await File.WriteAllTextAsync(filename, json);

            connection.Password = originalPassword;
        }

        public async Task<TfsConnectionValidationResult> Validate(TfsKnownConnection connection)
        {
            try
            {
                var uri = new Uri(connection.Url);
                var creds = TfsCredentialsFactory.Create(connection);
                var vssConnection = new VssConnection(uri, creds);
                vssConnection.Settings.SendTimeout = TimeSpan.FromSeconds(AppSettings.DefaultTimeoutSeconds);

                var projectClient = vssConnection.GetClient<ProjectHttpClient>();

                var projects = await projectClient.GetProjects(ProjectState.All, top: 1);

                if (projects.Count == 0)
                {
                    return new TfsConnectionValidationResult
                    {
                        IsError = true,
                        Message = "TFS connection validation test failed. Didn't find any projects."
                    };
                }
            }
            catch (Exception ex)
            {
                return new TfsConnectionValidationResult
                {
                    IsError = true,
                    Message = $"TFS connection validation test failed. {ex.Message}"
                };
            }            

            return new TfsConnectionValidationResult
            {
                Message = "TFS connection validation test successful."
            };
        }
    }
}
