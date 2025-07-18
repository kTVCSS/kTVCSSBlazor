using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.UserFeed;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Drawing;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class Wall
    {
        private List<Post> Posts = [];
        private string _newPostContent = "";
        private string newPostContent
        {
            get
            {
                return _newPostContent;
            }
            set
            {
                _newPostContent = value;
                InvokeAsync(StateHasChanged);
            }
        }
        private string uploadedFilePath;
        private IBrowserFile uploadedFile;
        private Post post;
        private string currentFile;
        private int windowWidth = 0;
        private int windowHeight = 0;
        private bool isMobile = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                WindowSize.OnResized += (w, h) =>
                {
                    windowWidth = w;
                    windowHeight = h;
                    InvokeAsync(StateHasChanged);
                };

                windowWidth = WindowSize.GetWidth();
                windowHeight = WindowSize.GetHeight();

                isMobile = await mds.IsMobileDeviceAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Posts = await http.GetFromJsonAsync<List<Post>>("/api/userposts/getposts");
        }

        private async Task PostNews()
        {
            if (!string.IsNullOrWhiteSpace(newPostContent))
            {
                DateTime now = DateTime.UtcNow.AddHours(3);

                post = new Post()
                {
                    AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl ?? "/images/logo_ktv.png",
                    AuthorId = AuthProvider.CurrentUser.Id,
                    AuthorName = AuthProvider.CurrentUser.Username,
                    Content = newPostContent,
                    MediaType = uploadedFile?.ContentType == "video/mp4" ? MediaType.Video : MediaType.Image,
                    MediaUrl = uploadedFile != null ? $"{uploadedFilePath}" : null,
                    PostDate = now,
                };

                Console.Write(post);

                if (AuthProvider.CurrentUser.IsVip)
                {
                    post.IsVip = true;
                }

                if (AuthProvider.CurrentUser.Role >= kTVCSS.Models.Db.Models.Roles.RoleType.Admin)
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
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Для публикации поста нужно хотя бы что-то написать!");
            }
        }

        private async Task DeleteComment(Post post, Comment comment)
        {
            await http.PostAsJsonAsync("/api/userposts/removecomment", comment);

            Posts.FirstOrDefault(x => x.PostId == post.PostId).Comments.RemoveAll(x => x.CommentId == comment.CommentId);

            await InvokeAsync(StateHasChanged);
        }

        private async Task LikePost(Post post)
        {
            await http.PostAsJsonAsync($"/api/userposts/likepost?id={AuthProvider.CurrentUser.Id}", post);
            var p = await http.GetFromJsonAsync<Post>($"/api/userposts/getpost?id={post.PostId}");
            Posts.First(x => x.PostId == post.PostId).Likes = p.Likes;

            await InvokeAsync(StateHasChanged);
        }

        private async Task DislikePost(Post post)
        {
            await http.PostAsJsonAsync($"/api/userposts/dislikepost?id={AuthProvider.CurrentUser.Id}", post);
            var p = await http.GetFromJsonAsync<Post>($"/api/userposts/getpost?id={post.PostId}");
            Posts.First(x => x.PostId == post.PostId).Likes = p.Likes;

            await InvokeAsync(StateHasChanged);
        }

        private async Task AddComment(Post post)
        {
            if (!string.IsNullOrEmpty(post.NewComment))
            {
                var com = new Comment
                {
                    AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl ?? "/images/logo_ktv.png",
                    AuthorId = AuthProvider.CurrentUser.Id,
                    AuthorName = AuthProvider.CurrentUser.Username,
                    Content = post.NewComment,
                    PostId = post.PostId
                };

                if (AuthProvider.CurrentUser.IsVip)
                {
                    com.IsVip = true;
                }

                if (AuthProvider.CurrentUser.Role >= kTVCSS.Models.Db.Models.Roles.RoleType.Admin)
                {
                    com.IsVip = false;
                    com.IsAdmin = true;
                }

                await http.PostAsJsonAsync($"/api/userposts/addcomment?postId={post.PostId}", com);

                var p = await http.GetFromJsonAsync<Post>($"/api/userposts/getpost?id={post.PostId}");

                Posts.First(x => x.PostId == post.PostId).Comments = p.Comments;

                post.NewComment = string.Empty;

                await InvokeAsync(StateHasChanged);
            }
        }

        async Task OnClientChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                uploadedFile = file;

                using var content = new MultipartFormDataContent();
                var fileStream = uploadedFile.OpenReadStream(maxAllowedSize: 512 * 1024 * 1024);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new(uploadedFile.ContentType);
                content.Add(streamContent, "file", uploadedFile.Name);

                var httpRequest = await http.PostAsync("/api/userposts/UploadAttachment", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    Console.WriteLine(result.Message);
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

            await InvokeAsync(StateHasChanged);
        }

        public async ValueTask DisposeAsync()
        {
            Posts = null;
            WindowSize.OnResized -= (w, h) => InvokeAsync(StateHasChanged); 
            //await WindowSize.DisposeAsync();
        }
    }
}
