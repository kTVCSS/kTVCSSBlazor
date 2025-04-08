using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Arcticles;
using kTVCSSBlazor.Db.Models.Home;
using kTVCSSBlazor.Db.Models.UserFeed;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages
{
    public partial class Index
    {
        private bool ready = false;
        private int selectedIndex = 0;
        private string newPostContent;
        private string uploadedFilePath;
        private IBrowserFile uploadedFile;
        private List<Article> News = new();
        private List<Post> Posts = [];
        private List<WorkSuggestion> workSuggestions = [];

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //string result = await AuthProvider.LoginAsync("kurwanator1337", "Tk16y1rh");

                News = await http.GetFromJsonAsync<List<Article>>("/api/articles");

                Posts = await http.GetFromJsonAsync<List<Post>>("/api/userposts/getposts");

                workSuggestions = await http.GetFromJsonAsync<List<WorkSuggestion>>("/api/worksuggestions");

                ready = true;

                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task DeleteWorkEntry(WorkSuggestion suggestion)
        {
            await http.DeleteAsync($"/api/worksuggestions/{suggestion.Id}");

            workSuggestions.Remove(suggestion);

            NotificationService.Notify(NotificationSeverity.Warning, "Успех", "Вакансия была удалена!");
        }

        private async Task MakeWorkEntry(WorkSuggestion suggestion)
        {
            await http.PostAsJsonAsync("/api/MakeEntry2Work", 
                new WorkEntry() 
                { 
                    UserName = AuthProvider.CurrentUser.Username, 
                    WorkSuggestion = suggestion 
                });

            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Ваше желание помочь проекту было доставлено администрации, спасибо!");
        }

        private async Task PostNews()
        {
            if (!string.IsNullOrWhiteSpace(newPostContent))
            {
                var post = new Post()
                {
                    AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl,
                    AuthorId = AuthProvider.CurrentUser.Id,
                    AuthorName = AuthProvider.CurrentUser.Username,
                    Content = newPostContent,
                    MediaType = uploadedFile?.ContentType == "video/mp4" ? MediaType.Video : MediaType.Image,
                    MediaUrl = uploadedFile != null ? $"{uploadedFilePath}" : null,
                    PostDate = DateTime.Now,
                };

                Console.Write(post);

                if (AuthProvider.CurrentUser.IsVip)
                {
                    post.IsVip = true;
                }

                if (AuthProvider.CurrentUser.IsAdmin)
                {
                    post.IsVip = false;
                    post.IsAdmin = true;
                }

                await http.PostAsJsonAsync("/api/userposts/addpost", post, new System.Text.Json.JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                newPostContent = string.Empty;
                uploadedFile = null;
                uploadedFilePath = string.Empty;

                Posts.Clear();

                Posts = await http.GetFromJsonAsync<List<Post>>("/api/userposts/getposts");

                NotificationService.Notify(NotificationSeverity.Success, "Успех", "Ваш пост был опубликован!");
            }
        }

        private async Task DeleteComment(Post post, Comment comment)
        {
            await http.PostAsJsonAsync("/api/userposts/removecomment", comment);

            Posts.FirstOrDefault(x => x.PostId == post.PostId).Comments.RemoveAll(x => x.CommentId == comment.CommentId);
        }

        private async Task LikePost(Post post)
        {
            await http.PostAsJsonAsync($"/api/userposts/likepost?id={AuthProvider.CurrentUser.Id}", post);
            var p = await http.GetFromJsonAsync<Post>($"/api/userposts/getpost?id={post.PostId}");
            Posts.First(x => x.PostId == post.PostId).Likes = p.Likes;
        }

        private async Task DislikePost(Post post)
        {
            await http.PostAsJsonAsync($"/api/userposts/dislikepost?id={AuthProvider.CurrentUser.Id}", post);
            var p = await http.GetFromJsonAsync<Post>($"/api/userposts/getpost?id={post.PostId}");
            Posts.First(x => x.PostId == post.PostId).Likes = p.Likes;
        }

        private async Task AddComment(Post post)
        {
            if (!string.IsNullOrEmpty(post.NewComment))
            {
                var com = new Comment
                {
                    AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl,
                    AuthorId = AuthProvider.CurrentUser.Id,
                    AuthorName = AuthProvider.CurrentUser.Username,
                    Content = post.NewComment,
                    PostId = post.PostId
                };

                if (AuthProvider.CurrentUser.IsVip)
                {
                    com.IsVip = true;
                }

                if (AuthProvider.CurrentUser.IsAdmin)
                {
                    com.IsVip = false;
                    com.IsAdmin = true;
                }

                await http.PostAsJsonAsync($"/api/userposts/addcomment?postId={post.PostId}", com);

                var p = await http.GetFromJsonAsync<Post>($"/api/userposts/getpost?id={post.PostId}");

                Posts.First(x => x.PostId == post.PostId).Comments = p.Comments;

                post.NewComment = string.Empty;
            }
        }

        async Task OnClientChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                uploadedFile = file;

                using var content = new MultipartFormDataContent();
                var fileStream = uploadedFile.OpenReadStream(maxAllowedSize: 10_000_000);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new(uploadedFile.ContentType);
                content.Add(streamContent, "file", uploadedFile.Name);

                var httpRequest = await http.PostAsync("/api/userposts/UploadAttachment", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    uploadedFilePath = result.Message;
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Файл успешно загружен, можете теперь публиковать пост!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", result.Message);
                }
            }
        }

        private async Task RemovePost(int postId)
        {
            await http.GetAsync($"/api/userposts/removepost?id={postId}");
            Posts.RemoveAll(x => x.PostId == postId);
        }

        public void Dispose()
        {
            News = null;
            Posts = null;
        }
    }
}
