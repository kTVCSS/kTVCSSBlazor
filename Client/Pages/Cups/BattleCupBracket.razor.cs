using kTVCSS.Models.Db.Models.BattleCup.DTOs;
using kTVCSS.Models.Db.Models.BattleCup.Enums;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Cups
{
    public partial class BattleCupBracket
    {
        [Parameter]
        public CupDto Cup { get; set; }

        private List<MatchDto> GetMatchesByStage(MatchStage stage)
        {
            var matches = Cup?.Matches?.Where(m => m.Stage == stage).ToList();

            if (matches.Any())
            {
                return matches;
            }
            else
            {
                if (stage == MatchStage.SemiFinal)
                {
                    return new List<MatchDto>()
                {
                    new MatchDto()
                    {
                        CupId = Cup.Id,
                        Stage = stage,
                        StartTime = Cup.Matches.First().StartTime.Date.AddHours(21),
                        Status = MatchStatus.Pending
                    },
                    new MatchDto()
                    {
                        CupId = Cup.Id,
                        Stage = stage,
                        StartTime = Cup.Matches.First().StartTime.Date.AddHours(21),
                        Status = MatchStatus.Pending
                    },
                };
                }
                else if (stage == MatchStage.Final)
                {
                    return new List<MatchDto>()
                {
                    new MatchDto()
                    {
                        CupId = Cup.Id,
                        Stage = stage,
                        StartTime = Cup.Matches.First().StartTime.Date.AddHours(22),
                        Status = MatchStatus.Pending
                    },
                };
                }
            }

            return default;
        }

        private string GetMatchStatusText(MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Pending => "Ожидание",
                MatchStatus.InProgress => "В процессе",
                MatchStatus.Completed => "Завершён",
                _ => "Неизвестно"
            };
        }

        private bool IsWinner(MatchDto match, TeamDto team)
        {
            if (match.Status != MatchStatus.Completed || team == null) return false;

            if (match.TeamA?.Id == team.Id && match.ScoreA > match.ScoreB) return true;
            if (match.TeamB?.Id == team.Id && match.ScoreB > match.ScoreA) return true;

            return false;
        }
    }
}
