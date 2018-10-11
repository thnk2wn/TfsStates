﻿using System;
using System.IO;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
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
            var appDataPath = await Electron.App.GetPathAsync(PathName.appData);
            var path = Path.Combine(appDataPath, this.GetType().Assembly.GetName().Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filename = Path.Combine(path, "settings.json");
            return filename;
        }

        public async Task<bool> HasSettings()
        {
            var filename = await GetFilename();
            return File.Exists(filename);
        }

        public async Task<TfsConnectionModel> GetSettings()
        {
            var filename = await GetFilename();

            if (File.Exists(filename))
            {
                var json = await File.ReadAllTextAsync(filename);
                var model = JsonConvert.DeserializeObject<TfsConnectionModel>(json);

                if (!model.UseWindowsIdentity)
                {
                    model.Password = EncryptionService.DecryptString(model.Password, EncryptionSettings.Key);
                }

                return model;
            }

            return null;
        }

        public async Task<TfsConnectionModel> GetSettingsOrDefault()
        {
            var model = (await GetSettings())
                ?? new TfsConnectionModel
                {
                    UseWindowsIdentity = true
                };
            return model;
        }

        public async Task<TfsConnectionModel> Save(TfsConnectionModel model, bool validate = true)
        {
            string filename = await GetFilename();
            var originalPassword = model.Password;

            if (!model.UseWindowsIdentity)
            {
                model.Password = EncryptionService.EncryptString(model.Password, EncryptionSettings.Key);
            }
            else
            {
                model.Username = string.Empty;
                model.Password = string.Empty;
            }

            var json = JsonConvert.SerializeObject(model);
            await File.WriteAllTextAsync(filename, json);

            if (!model.UseWindowsIdentity)
            {
                model.Password = originalPassword;
            }

            if (validate)
            {
                model.ValidationResult = (await Validate(model)).ValidationResult;
                model.ValidationResult.Message = $"Settings saved. {model.ValidationResult.Message}";
            }
            else
            {
                model.ValidationResult = new TfsConnectionValidationResult
                {
                    Message = "Settings saved."
                };
            }

            return model;
        }

        public async Task<TfsConnectionModel> Validate(TfsConnectionModel model)
        {
            try
            {
                var uri = new Uri(model.Url);
                var creds = TfsCredentialsFactory.Create(model);
                var connection = new VssConnection(uri, creds);

                var projectClient = connection.GetClient<ProjectHttpClient>();
                var projects = await projectClient.GetProjects(ProjectState.All, top: 1);

                if (projects.Count == 0)
                {
                    model.ValidationResult = new TfsConnectionValidationResult
                    {
                        IsError = true,
                        Message = "TFS connection validation test failed. Didn't find any projects."
                    };
                }
            }
            catch (Exception ex)
            {
                model.ValidationResult = new TfsConnectionValidationResult
                {
                    IsError = true,
                    Message = $"TFS connection validation test failed. {ex.Message}"
                };
            }
            
           
            if (model.ValidationResult == null)
            {
                model.ValidationResult = new TfsConnectionValidationResult
                {
                    Message = "TFS connection validation test successful."
                };
            }

            return model;
        }
    }
}