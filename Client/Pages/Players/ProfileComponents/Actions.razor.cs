﻿using kTVCSS.Models.Db.Models.Chat.DTOs;
using kTVCSS.Models.Db.Models.Common;
using kTVCSS.Models.Db.Models.Statuses;
using kTVCSS.Models.Db.Models.Teams;
using kTVCSS.Models.Db.Models.Tickets;
using kTVCSS.Models.Models;
using kTVCSSBlazor.Client.Extensions;
using kTVCSSBlazor.Db.Models.Players;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players.ProfileComponents
{
    public partial class Actions
    {
        [Parameter] public PlayerInfo player { get; set; }
        [Parameter] public bool isOnline { get; set; }
        [Parameter] public bool isMeAFriend { get; set; }
        [Parameter] public kTVCSS.Models.Db.Models.Players.FriendRequest? FriendRequest { get; set; }

        RadzenButton? clrBtn;
        RadzenButton? fullClrBtn;
        RadzenButton? itBtn;

        bool sndMsgButton = false;
        private string _selectedReason = string.Empty;
        private string _unbanRequestString = string.Empty;

        private void OpenEditProfile()
        {
            if (player.MainInfo.Block == 1)
            {
                ns.Notify(NotificationSeverity.Error, "Бан", "Редактирования профиля при наличии активного бана запрещено!");
                return;
            }
            nm.NavigateTo($"/editprofile/{player.MainInfo.Id}");
        }

        void ShowTooltip(ElementReference elementReference, string text, TooltipOptions options = null) => tooltipService.Open(elementReference, text, options);

        private async Task ClearProfile()
        {
            clrBtn.Disabled = true;

            ns.Notify(NotificationSeverity.Info, "Подождите", "Не закрывайте страницу, идет обнуление статистики...");

            ResetStatsResult result = await http.GetFromJsonAsync<ResetStatsResult>($"/api/players/ClearPlayerById?id={player.MainInfo.Id}");

            switch (result)
            {
                case ResetStatsResult.NotVip:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "У Вас нет VIP!");
                        break;
                    }
                case ResetStatsResult.NotEnoughMatches:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Чтобы обнулять статистику необходимо иметь минимум 10 матчей!");
                        break;
                    }
                case ResetStatsResult.Live:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя обнулять статистику во время матча!");
                        break;
                    }
                case ResetStatsResult.NotPremium:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "У Вас нет PREMIUM VIP!");
                        break;
                    }
                case ResetStatsResult.Fail:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Произошла непредвиденная ошибка, попробуйте позже!");
                        break;
                    }
                case ResetStatsResult.Success:
                    {
                        ns.Notify(NotificationSeverity.Success, "Успех", "Статистика обнулена!");

                        await Task.Delay(2000);

                        nm.Refresh(true);

                        return;
                    }
            }

            clrBtn.Disabled = false;
        }

        private async Task FullClearProfile()
        {
            clrBtn.Disabled = true;
            fullClrBtn.Disabled = true;

            ns.Notify(NotificationSeverity.Info, "Подождите", "Не закрывайте страницу, идет обнуление статистики...");

            ResetStatsResult result = await http.GetFromJsonAsync<ResetStatsResult>($"/api/players/ClearFullPlayerById?id={player.MainInfo.Id}");

            switch (result)
            {
                case ResetStatsResult.NotVip:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "У Вас нет VIP!");
                        break;
                    }
                case ResetStatsResult.NotEnoughMatches:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Чтобы обнулять статистику необходимо иметь минимум 10 матчей!");
                        break;
                    }
                case ResetStatsResult.Live:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя обнулять статистику во время матча!");
                        break;
                    }
                case ResetStatsResult.NotPremium:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "У Вас нет PREMIUM VIP!");
                        break;
                    }
                case ResetStatsResult.Fail:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Произошла непредвиденная ошибка, попробуйте позже!");
                        break;
                    }
                case ResetStatsResult.Success:
                    {
                        ns.Notify(NotificationSeverity.Success, "Успех", "Статистика обнулена!");

                        await Task.Delay(2000);

                        nm.Refresh(true);

                        return;
                    }
            }

            clrBtn.Disabled = false;
            fullClrBtn.Disabled = false;
        }

        private async Task SendMessage()
        {
            sndMsgButton = true;

            string baseUri = "http://localhost:4050";

#if DEBUG

            //Console.WriteLine("asd");

#endif

#if RELEASE

        baseUri = "https://chat.ktvcss.ru";

#endif

            ns.Notify(NotificationSeverity.Info, "Проверяем наличие диалога...", payload: new Random().Next(0, int.MaxValue));

            var dialogId = await http.GetFromJsonAsync<int>($"{baseUri}/api/chat/dialogexist?from={AuthProvider.CurrentUser.Id}&to={player.MainInfo.Id}");

            if (dialogId == -1)
            {
                // диалога нет, создаем

                ns.Notify(NotificationSeverity.Info, "Создаем диалог...", payload: new Random().Next(0, int.MaxValue));

                var request = new CreateDialogRequest
                {
                    ParticipantUserIds = new List<int> { player.MainInfo.Id },
                    IsGroup = false,
                    Name = null
                };

                try
                {
                    var dialog = await http.PostAsJsonAsync($"{baseUri}/api/chat/dialogs?userId={AuthProvider.CurrentUser.Id}", request);
                    if (dialog.IsSuccessStatusCode)
                    {
                        ns.Notify(NotificationSeverity.Success, "Направляем в мессенджер...", payload: new Random().Next(0, int.MaxValue));

                        var newDialog = await dialog.Content.ReadFromJsonAsync<DialogDto>();

                        nm.NavigateTo("/chat/" + newDialog.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating dialog: {ex.Message}");
                }
            }
            else
            {
                ns.Notify(NotificationSeverity.Success, "Направляем в мессенджер...", payload: new Random().Next(0, int.MaxValue));
                nm.NavigateTo("/chat/" + dialogId);
            }
        }

        private async Task CancelFriendRequest()
        {
            await http.GetAsync($"/api/friendsengine/CancelFriendRequest?requesterId={FriendRequest.Requester.PlayerID}&addresseeId={FriendRequest.Addressee.PlayerID}");

            ns.Notify(NotificationSeverity.Info, "kTVCSS", "Заявка отменена!");

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private async Task AcceptFriendRequest()
        {
            var result = await http.GetFromJsonAsync<FriendsEngineStatus>($"/api/friendsengine/AcceptFriendRequest?requesterId={FriendRequest.Requester.PlayerID}&addresseeId={FriendRequest.Addressee.PlayerID}");

            ns.Notify(result < FriendsEngineStatus.Ok ? NotificationSeverity.Error : NotificationSeverity.Success, "kTVCSS", EnumDescriptor.GetFriendStatus(result));

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private async Task RejectFriendRequest()
        {
            var result = await http.GetFromJsonAsync<FriendsEngineStatus>($"/api/friendsengine/RejectFriendRequest?requesterId={FriendRequest.Requester.PlayerID}&addresseeId={FriendRequest.Addressee.PlayerID}");

            ns.Notify(result < FriendsEngineStatus.Ok ? NotificationSeverity.Error : NotificationSeverity.Success, "kTVCSS", EnumDescriptor.GetFriendStatus(result));

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private async Task SendInviteToTeam()
        {
            itBtn.Disabled = true;

            InviteResult result =
                await http.GetFromJsonAsync<InviteResult>($"/api/teams/makeinvite?captain={AuthProvider.CurrentUser.Id}&player={player.MainInfo.Id}");

            switch (result)
            {
                case InviteResult.AlreadyInTeam:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Игрок уже есть в какой-то команде!");
                        break;
                    }
                case InviteResult.BlockEditing:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Редактирование составов для Вашей команды сейчас запрещено!");
                        break;
                    }
                case InviteResult.NoPlayer:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Такого игрока не существует!"); // lol
                        break;
                    }
                case InviteResult.FullStack:
                    {
                        ns.Notify(NotificationSeverity.Error, "Ошибка", "Состав Вашей команды уже достиг максимума!");
                        break;
                    }
                case InviteResult.Ok:
                    {
                        ns.Notify(NotificationSeverity.Info, "kTVCSS", "Приглашение отправлено!");
                        break;
                    }
            }
        }

        private async Task AddFriend()
        {
            var result = await http.GetFromJsonAsync<FriendsEngineStatus>($"/api/friendsengine/SendFriendRequest?requesterId={AuthProvider.CurrentUser.Id}&addresseeId={player.MainInfo.Id}");

            ns.Notify(result < FriendsEngineStatus.Ok ? NotificationSeverity.Error : NotificationSeverity.Success, "kTVCSS", EnumDescriptor.GetFriendStatus(result));

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private async Task RemoveFriend()
        {
            var result = await http.GetFromJsonAsync<FriendsEngineStatus>($"/api/friendsengine/RemoveFriend?playerId={AuthProvider.CurrentUser.Id}&friendId={player.MainInfo.Id}");

            ns.Notify(result < FriendsEngineStatus.Ok ? NotificationSeverity.Error : NotificationSeverity.Success, "kTVCSS", EnumDescriptor.GetFriendStatus(result));

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private async Task SendReport()
        {
            var report = new Report()
            {
                FromID = AuthProvider.CurrentUser.Id,
                FromPlayerName = AuthProvider.CurrentUser.Username,
                Reason = _selectedReason,
                ToID = player.MainInfo.Id,
                ToPlayerName = player.MainInfo.Name
            };

            var response = await http.PostAsJsonAsync("/api/actions/sendreport", report);

            try
            {
                var ticket = await response.Content.ReadFromJsonAsync<Ticket?>();

                if (ticket is not null)
                {
                    ns.Notify(NotificationSeverity.Success, "Система репортов", "Репорт успешно отправлен, мы создали для Вас тикет, на который сейчас перенаправим!", TimeSpan.FromSeconds(5));

                    await Task.Delay(3000);

                    nm.NavigateTo($"/ticket/{ticket.TicketId}");
                }
                else
                {
                    ns.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя создавать несколько жалоб подряд, дождитесь рассмотрения хотя бы одной!");
                }
            }
            catch (Exception)
            {
                ns.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя создавать несколько жалоб подряд, дождитесь рассмотрения хотя бы одной!");
            }
        }

        private async Task UnbanRequest()
        {
            var response = await http.PostAsJsonAsync("/api/players/unbanrequest", new InitialUnbanRequest()
            {
                Message = _unbanRequestString,
                PlayerID = AuthProvider.CurrentUser.Id,
                PlayerName = AuthProvider.CurrentUser.Username
            });

            try
            {
                var ticket = await response.Content.ReadFromJsonAsync<Ticket?>();

                if (ticket is not null)
                {
                    ns.Notify(NotificationSeverity.Success, "Успех", "Ваша заявка на разбан успешно создана! Сейчас мы Вас перенаправим в созданный тикет", TimeSpan.FromSeconds(3));

                    await Task.Delay(3000);

                    nm.NavigateTo($"/ticket/{ticket.TicketId}");
                }
                else
                {
                    ns.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя создавать несколько обращений подряд, дождитесь рассмотрения хотя бы одной!");
                }
            }
            catch (Exception)
            {
                ns.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя создавать несколько обращений подряд, дождитесь рассмотрения хотя бы одной!");
            }
        }
    }
}
