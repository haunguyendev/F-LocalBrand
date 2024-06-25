using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SWD.F_LocalBrand.Business.Settings;
using Firebase.Auth;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace SWD.F_LocalBrand.Business.Services
{
    public class FirebaseService
    {
        
        private readonly FirebaseSettings _firebaseSettings;
        private readonly IConfiguration _configuration;

        public FirebaseService(IOptions<FirebaseSettings> options, IConfiguration configuration)
        {
            _firebaseSettings = options.Value;
            _configuration = configuration;
        }

        public async Task<bool> DeleteFileFromFirebase(string pathFileName)
        {
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseSettings.ApiKey));
                var account = await auth.SignInWithEmailAndPasswordAsync(_firebaseSettings.AuthEmail, _firebaseSettings.AuthPassword);
                var storage = new FirebaseStorage(
                    _firebaseSettings.Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(account.FirebaseToken),
                        ThrowOnCancel = true
                    });
                await storage
                    .Child(pathFileName)
                    .DeleteAsync();

                return true;
            }
            catch (FirebaseAuthException ex)
            {
                // Wrap ngoại lệ FirebaseAuthException và throw lại
                throw new InvalidOperationException("Authentication failed with Firebase", ex);
            }
            catch (FirebaseStorageException ex)
            {
                // Wrap ngoại lệ FirebaseStorageException và throw lại
                throw new InvalidOperationException("Error occurred while deleting file from Firebase Storage", ex);
            }
            catch (Exception ex)
            {
                // Wrap tất cả các ngoại lệ khác và throw lại
                throw new InvalidOperationException("An unexpected error occurred while deleting file", ex);
            }
        }

        public async Task<string> GetUrlImageFromFirebase(string pathFileName)
        {
            var a = pathFileName.Split("/o/")[1];
            var api = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseSettings.Bucket}/o?name={a}";
            if (string.IsNullOrEmpty(pathFileName))
            {
                return string.Empty;
            }

            var client = new RestClient();
            var request = new RestRequest(api);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jmessage = JObject.Parse(response.Content);
                var downloadToken = jmessage.GetValue("downloadTokens").ToString();
                return $"https://firebasestorage.googleapis.com/v0/b/{_firebaseSettings.Bucket}/o/{pathFileName}?alt=media&token={downloadToken}";
            }

            return string.Empty;
        }

        //public async Task<IActionResult> UploadFileToFirebase(IFormFile file, string pathFileName)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return new BadRequestObjectResult(new { Message = "The file is empty", IsSuccess = false });
        //    }

        //    var stream = file.OpenReadStream();
        //    var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseSettings.ApiKey));
        //    var account = await auth.SignInWithEmailAndPasswordAsync(_firebaseSettings.AuthEmail, _firebaseSettings.AuthPassword);
        //    var storage = new FirebaseStorage(
        //        _firebaseSettings.Bucket,
        //        new FirebaseStorageOptions
        //        {
        //            AuthTokenAsyncFactory = () => Task.FromResult(account.FirebaseToken),
        //            ThrowOnCancel = true
        //        });

        //    var task = storage.Child(pathFileName).PutAsync(stream);
        //    var downloadUrl = await task;

        //    return new OkObjectResult(new { Result = downloadUrl, IsSuccess = true });
        //}
        public async Task<string> UploadFileToFirebase(IFormFile file, string pathFileName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("The file is empty");
            }

            try
            {
                var stream = file.OpenReadStream();
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseSettings.ApiKey));
                var account = await auth.SignInWithEmailAndPasswordAsync(_firebaseSettings.AuthEmail, _firebaseSettings.AuthPassword);

                var storage = new FirebaseStorage(
                    _firebaseSettings.Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(account.FirebaseToken),
                        ThrowOnCancel = true
                    });

                var task = storage.Child(pathFileName).PutAsync(stream);
                var downloadUrl = await task;

                return downloadUrl;
            }
            catch (FirebaseAuthException ex)
            {
                throw new InvalidOperationException("Authentication failed with Firebase", ex);
            }
            catch (FirebaseStorageException ex)
            {
                throw new InvalidOperationException("Error occurred while uploading file to Firebase Storage", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred", ex);
            }
        }

        public async Task<IActionResult> UploadFilesToFirebase(List<IFormFile> files, string basePath)
        {
            var uploadResults = new List<string>();
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseSettings.ApiKey));
            var account = await auth.SignInWithEmailAndPasswordAsync(_firebaseSettings.AuthEmail, _firebaseSettings.AuthPassword);
            var storage = new FirebaseStorage(
                _firebaseSettings.Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(account.FirebaseToken),
                    ThrowOnCancel = true
                });

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var stream = file.OpenReadStream();
                string destinationPath = $"{basePath}/{file.FileName}";
                var task = storage.Child(destinationPath).PutAsync(stream);
                var downloadUrl = await task;
                uploadResults.Add(downloadUrl);
            }

            return new OkObjectResult(new { Result = uploadResults, IsSuccess = uploadResults.Count == files.Count, Message = uploadResults.Count == files.Count ? "All files uploaded successfully" : "Some files failed to upload" });
        }
    }
}
