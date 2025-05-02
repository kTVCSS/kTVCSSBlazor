using kTVCSS.Models.Db.Models.Statuses;

namespace kTVCSSBlazor.Client.Extensions
{
    public class EnumDescriptor
    {
        public static string GetFriendStatus(FriendsEngineStatus status)
        {
            switch (status)
            {
                case FriendsEngineStatus.ExistingRequest:
                    {
                        return "Заявка уже существует";
                    }
                case FriendsEngineStatus.FriendshipExists:
                    {
                        return "Игрок уже является Вашим другом";
                    }
                case FriendsEngineStatus.FriendshipNotFound:
                    {
                        return "Дружба не найдена";
                    }
                case FriendsEngineStatus.RequestNotFound:
                    {
                        return "Заявка не найдена";
                    }
                case FriendsEngineStatus.Ok:
                    {
                        return "OK";
                    }
            }

            return string.Empty;
        }
    }
}
