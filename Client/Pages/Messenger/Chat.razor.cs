using kTVCSS.Models.Db.Models.Chat.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Messenger
{
    public partial class Chat
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int? DialogueId { get; set; }

        private List<DialogDto>? dialogs;
        private List<MessageDto>? messages;
        private DialogDto? selectedDialog;
        private List<UserDto> typingUsers = new();
        private MessageDto? replyToMessage;

        private string messageText = string.Empty;
        private int currentUserId = 0;

        private bool showCreateDialog = false;
        private int newDialogUserId = 0;
        private bool isGroupDialog = false;
        private string groupName = string.Empty;

        private ElementReference messagesContainer;
        private Timer? typingTimer;
        private bool ready = false;
        private string currentLoadText = "";
        private bool disposed = false;

        protected override void OnInitialized()
        {
#if DEBUG

            //Console.WriteLine("asd");

#endif

#if RELEASE

        baseUri = "https://chat.ktvcss.ru";

#endif

            base.OnInitialized();
        }

        private string baseUri = "http://localhost:4050";

        void OnSpeechCaptured(string speechValue, bool updateTextArea, string name)
        {
            if (updateTextArea)
            {
                messageText += speechValue;
            }
        }

        List<IDisposable> disposables = [];

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            while (AuthProvider.CurrentUser is null)
            {
                currentLoadText = "Авторизуемся...";
                await InvokeAsync(StateHasChanged);
                await Task.Delay(100);
            }

            currentUserId = AuthProvider.CurrentUser.Id;

            while (ChatHub.Connection.State != HubConnectionState.Connected)
            {
                currentLoadText = "Подключаемся к сети проекта...";
                await InvokeAsync(StateHasChanged);
                await Task.Delay(100);
            }

            currentLoadText = "Загружаем чат...";
            await InvokeAsync(StateHasChanged);

            // Обработчики событий
            var rm = ChatHub.Connection.On<MessageDto>("ReceiveMessage", async (message) =>
            {
                try
                {
                    await LoadDialogs(); // Обновляем список диалогов
                    await InvokeAsync(StateHasChanged);
                    Console.WriteLine(message.DialogId);
                    if (selectedDialog?.Id != message.DialogId)
                    {
                        var needed = dialogs.FirstOrDefault(x => x.Id == message.DialogId);
                        if (needed is not null)
                        {
                            needed.UnreadCount += 1;
                        }
                        JSRuntime.InvokeVoidAsync("showNotification", message.Sender.Login, message.Content.Length > 20 ? message.Content[0..20] : message.Content, message.Sender.AvatarUrl);
                        await InvokeAsync(StateHasChanged);
                    }
                    if (selectedDialog?.Id == message.DialogId)
                    {
                        messages?.Add(message);
                        selectedDialog.UnreadCount = 0;
                        ChatHub.Connection.InvokeAsync("MarkAsRead", message.DialogId, AuthProvider.CurrentUser.Id);
                        // Обновляем UI для отображения прочитанных сообщений
                        await InvokeAsync(StateHasChanged);
                        await ScrollToBottom();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            });

            var du = ChatHub.Connection.On<int>("DialogUpdated", async (dialogId) =>
            {
                Console.WriteLine(dialogId);
                await LoadDialogs();
                await InvokeAsync(StateHasChanged);
            });

            var dr = ChatHub.Connection.On<int, string>("MessagesRead", async (dialogId, userId) =>
            {
                if (selectedDialog?.Id == dialogId)
                {
                    selectedDialog.UnreadCount = 0;
                    ChatHub.Connection.InvokeAsync("MarkAsRead", dialogId, userId);
                    // Обновляем UI для отображения прочитанных сообщений
                    await InvokeAsync(StateHasChanged);
                }
            });

            var ust = ChatHub.Connection.On<int, int>("UserStartedTyping", async (dialogId, userId) =>
            {
                if (selectedDialog?.Id == dialogId && userId != currentUserId)
                {
                    var user = await GetUser(userId);
                    if (user != null && !typingUsers.Any(u => u.UserId == userId))
                    {
                        typingUsers.Add(user);
                        await InvokeAsync(StateHasChanged);
                    }
                }
            });

            var ustt = ChatHub.Connection.On<int, int>("UserStoppedTyping", async (dialogId, userId) =>
            {
                if (selectedDialog?.Id == dialogId)
                {
                    typingUsers.RemoveAll(u => u.UserId == userId);
                    await InvokeAsync(StateHasChanged);
                }
            });

            var uosc = ChatHub.Connection.On<int, bool>("UserOnlineStatusChanged", async (userId, isOnline) =>
            {
                // Обновляем статус пользователей в диалогах
                if (dialogs != null)
                {
                    TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                    DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);

                    foreach (var dialog in dialogs)
                    {
                        var participant = dialog.Participants.FirstOrDefault(p => p.UserId == userId);
                        if (participant != null)
                        {
                            participant.IsOnline = isOnline;
                            if (!isOnline)
                            {
                                participant.LastSeenAt = now;
                            }
                        }
                    }
                    await InvokeAsync(StateHasChanged);
                }
            });

            disposables.Add(rm);
            disposables.Add(du);
            disposables.Add(dr);
            disposables.Add(ust);
            disposables.Add(ustt);
            disposables.Add(uosc);

            await LoadDialogs();

            await InvokeAsync(StateHasChanged);

            ready = true;

            await InvokeAsync(StateHasChanged);

            if (DialogueId.HasValue)
            {
                var dialog = dialogs.FirstOrDefault(x => x.Id == DialogueId.Value);

                if (dialog is not null)
                {
                    await SelectDialog(dialog);
                }
            }
        }

        private async Task LoadDialogs()
        {
            try
            {
                dialogs = await Http.GetFromJsonAsync<List<DialogDto>>($"{baseUri}/api/chat/dialogs?userId={AuthProvider.CurrentUser.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dialogs: {ex.Message}");
            }
        }

        private async Task SelectDialog(DialogDto dialog)
        {
            if (selectedDialog?.Id == dialog.Id) return;

            Console.WriteLine(dialog.Id);

            // Покидаем предыдущий диалог
            if (selectedDialog != null && ChatHub.Connection != null)
            {
                await ChatHub.Connection.SendAsync("LeaveDialog", selectedDialog.Id);
            }
            // сука
            selectedDialog = dialog;
            messages = null;
            replyToMessage = null;
            typingUsers.Clear();

            await LoadMessages();

            await InvokeAsync(StateHasChanged);

            // Присоединяемся к новому диалогу
            if (ChatHub.Connection != null)
            {
                await ChatHub.Connection.SendAsync("JoinDialog", dialog.Id, AuthProvider.CurrentUser.Id);
                ChatHub.Connection.InvokeAsync("MarkAsRead", dialog.Id, AuthProvider.CurrentUser.Id);
                selectedDialog.UnreadCount = 0;
            }

            await JSRuntime.InvokeVoidAsync("setAnimateHook");

            await ScrollToBottom();

            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadMessages()
        {
            if (selectedDialog == null) return;

            try
            {
                messages = await Http.GetFromJsonAsync<List<MessageDto>>($"{baseUri}/api/chat/dialogs/{selectedDialog.Id}/messages/{AuthProvider.CurrentUser.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
            }

            await ScrollToBottom();
        }

        bool isWaiting = false;

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(messageText) || selectedDialog == null || ChatHub.Connection == null || isWaiting)
                return;

            var request = new SendMessageRequest
            {
                DialogId = selectedDialog.Id,
                Content = messageText.Trim(),
                ReplyToMessageId = replyToMessage?.Id
            };

            try
            {
                isWaiting = true;
                await ChatHub.Connection.SendAsync("SendMessage", request, AuthProvider.CurrentUser.Id);
                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    isWaiting = false;
                    await InvokeAsync(StateHasChanged);
                });
                messageText = string.Empty;
                replyToMessage = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
        }

        private async Task HandleTyping()
        {
            if (selectedDialog == null || ChatHub.Connection == null) return;

            await ChatHub.Connection.SendAsync("StartTyping", selectedDialog.Id, AuthProvider.CurrentUser.Id);

            // Отменяем предыдущий таймер
            typingTimer?.Dispose();

            // Устанавливаем новый таймер для остановки печати
            typingTimer = new Timer(async _ =>
            {
                if (ChatHub.Connection != null && selectedDialog != null)
                {
                    await ChatHub.Connection.SendAsync("StopTyping", selectedDialog.Id, AuthProvider.CurrentUser.Id);
                }
            }, null, 2000, Timeout.Infinite);
        }

        private void SetReplyMessage(MessageDto message)
        {
            replyToMessage = message;
        }

        private void CancelReply()
        {
            replyToMessage = null;
        }

        private void ShowCreateDialogModal()
        {
            showCreateDialog = true;
            newDialogUserId = 0;
            isGroupDialog = false;
            groupName = string.Empty;
        }

        private void HideCreateDialogModal()
        {
            showCreateDialog = false;
        }

        private async Task CreateDialog()
        {
            if (newDialogUserId == 0)
                return;

            var request = new CreateDialogRequest
            {
                ParticipantUserIds = new List<int> { newDialogUserId },
                IsGroup = isGroupDialog,
                Name = isGroupDialog ? groupName : null
            };

            try
            {
                var dialog = await Http.PostAsJsonAsync($"{baseUri}/api/chat/dialogs?userId={AuthProvider.CurrentUser.Id}", request);
                if (dialog.IsSuccessStatusCode)
                {
                    await LoadDialogs();
                    HideCreateDialogModal();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating dialog: {ex.Message}");
            }
        }

        private async Task<UserDto?> GetUser(int userId)
        {
            try
            {
                return await Http.GetFromJsonAsync<UserDto>($"{baseUri}/api/chat/users/{userId}");
            }
            catch
            {
                return null;
            }
        }

        private async Task ScrollToBottom()
        {
            await Task.Delay(100); // Небольшая задержка для рендеринга
            try
            {
                await JSRuntime.InvokeVoidAsync("scrollToBottom", messagesContainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling to bottom: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var item in disposables)
            {
                item.Dispose();
            }
            typingTimer?.Dispose();
        }
    }
}
