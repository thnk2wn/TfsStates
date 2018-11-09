using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using TfsStates.Extensions;
using TfsStates.Models;

namespace TfsStates.Services
{
    public class TfsSettingsService : ITfsSettingsService
    {
        private readonly IDataProtector dataProtector;

        public TfsSettingsService(IDataProtectionProvider dataProtection)
        {
            this.dataProtector = dataProtection.CreateProtector(this.GetType().FullName); ;
        }

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
            var model = await GetRawConnections() ?? new TfsKnownConnections();

            foreach (var connection in model.Connections)
            {
                if (connection.ConnectionType == TfsConnectionTypes.TfsNTLM 
                    && !connection.UseDefaultCredentials
                    && connection.Password != null)
                {
                    connection.Password = this.dataProtector.Unprotect(connection.Password);
                }

                if (connection.ConnectionType == TfsConnectionTypes.AzureDevOpsToken
                    && connection.PersonalAccessToken != null)
                {
                    connection.PersonalAccessToken = this.dataProtector.Unprotect(
                        connection.PersonalAccessToken);
                }
            }

            return model;
        }

        public async Task<TfsKnownConnections> GetConnectionsOrDefault()
        {
            var model = (await GetConnections()) ?? new TfsKnownConnections();
            return model;
        }

        public async Task<TfsKnownConnection> GetConnection(Guid connectionId)
        {
            var connections = await GetConnections();
            if (!connections.Any()) return null;

            var conn = connections.Connection(connectionId);
            return conn;
        }

        public async Task<bool> Remove(Guid connectionId)
        {
            string filename = await GetFilename();
            if (!File.Exists(filename)) return false;

            var connections = await GetRawConnections() ?? new TfsKnownConnections();
            var connection = connections.Connections.FirstOrDefault(c => c.Id == connectionId);

            if (connection == null) return false;

            connections.Connections.Remove(connection);
            await SaveConnections(filename, connections);

            return true;
        }

        public async Task Save(TfsKnownConnection connection)
        {
            string filename = await GetFilename();
            var connections = await GetRawConnections() ?? new TfsKnownConnections();
            var originalPassword = connection.Password;
            var originalToken = connection.PersonalAccessToken;

            if (!string.IsNullOrEmpty(connection.Password))
            {
                connection.Password = this.dataProtector.Protect(connection.Password);
            }

            if (!string.IsNullOrEmpty(connection.PersonalAccessToken))
            {
                connection.PersonalAccessToken = this.dataProtector.Protect(connection.PersonalAccessToken);
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

            await SaveConnections(filename, connections);

            connection.Password = originalPassword;
            connection.PersonalAccessToken = originalToken;
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
                Message = $"Successfully connected to {connection.Url}."
            };
        }

        private async Task<TfsKnownConnections> GetRawConnections()
        {
            var filename = await GetFilename();

            if (!File.Exists(filename)) return null;

            var json = await File.ReadAllTextAsync(filename);
            var model = JsonConvert.DeserializeObject<TfsKnownConnections>(json);

            return model;
        }

        private static async Task SaveConnections(string filename, TfsKnownConnections connections)
        {
            var json = JsonConvert.SerializeObject(connections, Formatting.Indented);
            await File.WriteAllTextAsync(filename, json);
        }
    }
}
