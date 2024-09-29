using Octokit;
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using Android.App;
using Application = Android.App.Application;

namespace MauiGameOfLife.Platforms.Android.Services
{
    public class GitHubUploadService
    {
        private readonly string _owner;
        private readonly string _repo;
        private readonly string _branch;
        private readonly string _personalAccessToken;

        public GitHubUploadService(string owner, string repo, string branch, string personalAccessToken)
        {
            _owner = owner;
            _repo = repo;
            _branch = branch;
            _personalAccessToken = personalAccessToken;
        }

        public async Task UploadFileAsync(string contentUri, string commitMessage)
        {
            var client = new GitHubClient(new ProductHeaderValue("MauiGameOfLife"))
            {
                Credentials = new Credentials(_personalAccessToken)
            };

            try
            {
                // Get the current commit SHA
                var reference = await client.Git.Reference.Get(_owner, _repo, $"heads/{_branch}");
                var latestCommit = await client.Git.Commit.Get(_owner, _repo, reference.Object.Sha);

                // Read file content from content URI
                byte[] fileContent = await ReadContentFromUri(contentUri);

                // Create blob with file content
                var blob = new NewBlob
                {
                    Content = Convert.ToBase64String(fileContent),
                    Encoding = EncodingType.Base64
                };
                var blobReference = await client.Git.Blob.Create(_owner, _repo, blob);

                // Create tree
                var newTree = new NewTree();
                newTree.Tree.Add(new NewTreeItem
                {
                    Path = GetFileNameFromUri(contentUri),
                    Mode = Octokit.FileMode.File,
                    Type = TreeType.Blob,
                    Sha = blobReference.Sha
                });

                var tree = await client.Git.Tree.Create(_owner, _repo, newTree);

                // Create commit
                var newCommit = new NewCommit(commitMessage, tree.Sha, latestCommit.Sha);
                var commit = await client.Git.Commit.Create(_owner, _repo, newCommit);

                // Update branch reference
                await client.Git.Reference.Update(_owner, _repo, $"heads/{_branch}", new ReferenceUpdate(commit.Sha));

                Console.WriteLine("File uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private async Task<byte[]> ReadContentFromUri(string contentUri)
        {
            var context = Application.Context;
            var contentResolver = context.ContentResolver;

            using (var inputStream = contentResolver.OpenInputStream(global::Android.Net.Uri.Parse(contentUri)))
            using (var memoryStream = new MemoryStream())
            {
                await inputStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private string GetFileNameFromUri(string contentUri)
        {
            var context = Application.Context;
            var contentResolver = context.ContentResolver;
            var uri = global::Android.Net.Uri.Parse(contentUri);

            string fileName = "unknown_file";
            var cursor = contentResolver.Query(uri, null, null, null, null);
            if (cursor != null && cursor.MoveToFirst())
            {
                int nameIndex = cursor.GetColumnIndex(MediaStore.IMediaColumns.DisplayName);
                if (nameIndex >= 0)
                {
                    fileName = cursor.GetString(nameIndex);
                }
                cursor.Close();
            }
            return fileName;
        }
    }
}