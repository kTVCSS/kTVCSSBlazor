using kTVCSS.Models.Db.Models.BattleCup;
using kTVCSS.Models.Db.Models.BattleCup.DTOs;
using kTVCSS.Models.Db.Models.BattleCup.Enums;
using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;

namespace kTVCSSBlazor.Client.Pages.Cups
{
    public partial class BattleCupSchedule
    {
        [Parameter]
        public CupDto Cup { get; set; }
        [Parameter]
        public List<GameServer> GameServers { get; set; } = [];

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
                        TeamA = new TeamDto(),
                        TeamB = new TeamDto(),
                        StartTime = Cup.Matches.First().StartTime.Date.AddHours(21),
                        Status = MatchStatus.Pending
                    },
                    new MatchDto()
                    {
                        CupId = Cup.Id,
                        Stage = stage,
                        TeamA = new TeamDto(),
                        TeamB = new TeamDto(),
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
                        TeamA = new TeamDto(),
                        TeamB = new TeamDto(),
                        StartTime = Cup.Matches.First().StartTime.Date.AddHours(22),
                        Status = MatchStatus.Pending
                    },
                };
                }
            }

            return default;
        }
    }
}
