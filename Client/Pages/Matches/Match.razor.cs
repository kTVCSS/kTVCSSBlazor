using kTVCSSBlazor.Db.Models.Matches;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Http.Json;
using System.Threading.Tasks;
using kTVCSS.Models.Db.Models.Matches;

namespace kTVCSSBlazor.Client.Pages.Matches
{
    public partial class Match
    {
        [Parameter]
        public int MatchID { get; set; }

        private string text = "";

        bool ready = false;

        private DotNetObjectReference<Match>? dotNetRef;

        private MatchInfo match = new MatchInfo();
        private bool isMobile { get; set; }
        private string headerStyle = string.Empty;
        private bool dataLoadFinished = false;
        private string matchLength = string.Empty;
        HttpClient httpClient = null!;
        private bool setIntervalShouldStop = false;
        private string matchLog { get; set; } = string.Empty;

        private string _aMMRColor;
        private string _aAVGColor;
        private string _aKDRColor;
        private string _aHSRColor;
        private string _aWinrateColor;
        private string _bMMRColor;
        private string _bAVGColor;
        private string _bKDRColor;
        private string _bHSRColor;
        private string _bWinrateColor;

        private string moreColor = "#00ffae";
        private string lessColor = "#ff2225";

        private async Task Send()
        {
            var com = new Commentary()
            {
                Id = MatchID,
                User = AuthProvider.CurrentUser.Username,
                Text = text
            };

            var response = await http.PostAsJsonAsync("/api/matches/addcommentary", com);

            match = await response.Content.ReadFromJsonAsync<MatchInfo>();

            text = "";

            InvokeAsync(StateHasChanged);
        }

        private void ConnectToSourceTV()
        {
            DialogService.Alert($"connect {match.SourceTV}", "SourceTV", new AlertOptions() { OkButtonText = "Ок" });
        }

        private bool demoDownloadButtonDisabled = false;

        private async Task DemoDownload()
        {
            var filePath = Path.Combine("wwwroot", "demos", match.DemoUrl + ".dem.zip");

            if (File.Exists(filePath))
            {
                demoDownloadButtonDisabled = true;

                NotifyService.Notify(summary: "Скачивание демо-записи", detail: "Подождите, мы готовим файл к загрузке...");

                byte[] fileBytes = File.ReadAllBytes(filePath);

                using (var outputStream = new MemoryStream(fileBytes))
                {
                    using var streamRef = new DotNetStreamReference(stream: outputStream);

                    await JS.InvokeVoidAsync("downloadFileFromStream", $"ktvcss-match-id-{MatchID}.zip", streamRef);
                }
            }
            else
            {
                NotifyService.Notify(NotificationSeverity.Error, "Ошибка", "Извините, мы не смогли найти демо-запись этого матча 😭");
            }
        }

        private bool isTeamStuffsVisible = true;

        void ShowTooltip(ElementReference elementReference, string text, TooltipOptions options = null) => tooltipService.Open(elementReference, text, options);

        private void ParseMatchLogAsTicks(bool mobile)
        {
            StringBuilder sb = new StringBuilder();
            string rowStyle = mobile ? "display: flex;height: 60px;text-wrap: wrap;text-align: center;align-items: center;" : "display: flex;height:40px";
            string weaponStyle = mobile ? "display: flex;align-items: center;" : "";
            string weaponHeight = mobile ? "16px" : "24px";

            if (match.IsFinished)
            {
                sb.Append($"<div style='{rowStyle}'><div style='color: #cddc39'>Матч окончен! {matchLength}!</div></div>");
            }

            foreach (var log in match.MatchLog)
            {
                if (log.Record.StartsWith("<Round Start>"))
                {
                    Regex regex = new Regex(@"<(.*?)>\s+\[(.*?)\]");
                    var match = regex.Match(log.Record);
                    if (match.Success)
                    {
                        string round = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #0dde9c'>Начало раунда&nbsp;</div></div>");
                    }
                    continue;
                }
                if (log.Record.StartsWith("<Round End>"))
                {
                    Regex regex = new Regex(@"<(.*?)> (.*?) \[(\d+)-(\d+)\] (.*?) \[(.*?)\]");
                    var match = regex.Match(log.Record);
                    if (match.Success)
                    {
                        try
                        {
                            string aName = match.Groups[2].Value;
                            string bName = match.Groups[5].Value;
                            string aScore = match.Groups[3].Value;
                            string bScore = match.Groups[4].Value;
                            var timestamp = match.Groups[6].Value;
                            var timespan = TimeSpan.Parse(timestamp);
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #0dde9c'>Раунд закончился&nbsp;</div><div style='color: #f42f46'>{aName}&nbsp;</div><div style='color: white'>[{aScore}&nbsp-&nbsp{bScore}]&nbsp</div><div style='color: #3399ff'>{bName}&nbsp;</div></div>");
                        }
                        catch (Exception)
                        {
                            //
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("said:"))
                {
                    var match = Regex.Match(log.Record, @"^(.*?) <([\w_:\d]+)> said: (.*?) \[([\d:.]+)\]$");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var steamId = match.Groups[2].Value;
                        var message = match.Groups[3].Value;
                        var timestamp = match.Groups[4].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{nickname}:&nbsp;</div><div>{message}</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{nickname}:&nbsp;</div><div>{message}</div></div>");
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("> killed "))
                {
                    var match = Regex.Match(log.Record, @"(.*?)<(.+?)>\s+killed\s+(.*?)<(.+?)>\s+with\s+weapon\s+<(.*?)>\s+(\<.+?\>) \[([\d:.]+)\]$");
                    if (match.Success)
                    {
                        var timespan = TimeSpan.Parse(match.Groups[7].Value);
                        var killer = match.Groups[1].Value;
                        var loser = match.Groups[3].Value;
                        var weapon = match.Groups[5].Value;
                        var headshot = match.Groups[6].Value;
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(killer)).Any())
                        {
                            if (headshot != "<True>")
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='color: #3399ff'>{loser}&nbsp;</div></div>");
                            }
                            else
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/headshot.svg'>&nbsp;&nbsp;</div><div style='color: #3399ff'>{loser}&nbsp;</div></div>");
                            }
                        }
                        else
                        {
                            if (headshot != "<True>")
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='color: #f42f46'>{loser}&nbsp;</div></div>");
                            }
                            else
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/headshot.svg'>&nbsp;&nbsp;</div><div style='color: #f42f46'>{loser}&nbsp;</div></div>");
                            }
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("made a triple kill!"))
                {
                    var match = Regex.Match(log.Record, @"(.*?) made a triple kill! \[(.*?)\]");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ffbf00'>ДЕЛАЕТ ТРИПЛ КИЛЛ!</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ffbf00'>ДЕЛАЕТ ТРИПЛ КИЛЛ!</div></div>");
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("made a quad kill!"))
                {
                    var match = Regex.Match(log.Record, @"(.*?) made a quad kill! \[(.*?)\]");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff3a95'>ДЕЛАЕТ КВАДРО КИЛЛ!</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff3a95'>ДЕЛАЕТ КВАДРО КИЛЛ!</div></div>");
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("MADE A RAMPAGE!!!"))
                {
                    var match = Regex.Match(log.Record, @"(.*?) MADE A RAMPAGE!!! \[(.*?)\]");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{(Math.Round(timespan.TotalSeconds) * 100)}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                        }
                    }
                    continue;
                }
            }

            sb.Append($"<div style='{rowStyle}'><div style='color: #cddc39'>{match.AName} (~{match.ATeamAVG}) vs {match.BName} (~{match.BTeamAVG}) на сервере {match.ServerName}!</div></div>");
            sb.Append($"<div style='{rowStyle}'><div style='color: #cddc39'>Матч начался!</div></div>");

            matchLog = sb.ToString();
        }

        private void ParseMatchLog(bool mobile)
        {
            StringBuilder sb = new StringBuilder();
            string rowStyle = mobile ? "display: flex;height: 60px;text-wrap: wrap;text-align: center;align-items: center;" : "display: flex;height:40px";
            string weaponStyle = mobile ? "display: flex;align-items: center;" : "";
            string weaponHeight = mobile ? "16px" : "24px";

            if (match.IsFinished)
            {
                sb.Append($"<div style='{rowStyle}'><div style='color: #cddc39'>Матч окончен! {matchLength}!</div></div>");
            }

            foreach (var log in match.MatchLog)
            {
                if (log.Record.StartsWith("<Round Start>"))
                {
                    Regex regex = new Regex(@"<(.*?)>\s+\[(.*?)\]");
                    var match = regex.Match(log.Record);
                    if (match.Success)
                    {
                        string round = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #0dde9c'>Начало раунда&nbsp;</div></div>");
                    }
                    continue;
                }
                if (log.Record.StartsWith("<Round End>"))
                {
                    Regex regex = new Regex(@"<(.*?)> (.*?) \[(\d+)-(\d+)\] (.*?) \[(.*?)\]");
                    var match = regex.Match(log.Record);
                    if (match.Success)
                    {
                        try
                        {
                            string aName = match.Groups[2].Value;
                            string bName = match.Groups[5].Value;
                            string aScore = match.Groups[3].Value;
                            string bScore = match.Groups[4].Value;
                            var timestamp = match.Groups[6].Value;
                            var timespan = TimeSpan.Parse(timestamp);
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #0dde9c'>Раунд закончился&nbsp;</div><div style='color: #f42f46'>{aName}&nbsp;</div><div style='color: white'>[{aScore}&nbsp-&nbsp{bScore}]&nbsp</div><div style='color: #3399ff'>{bName}&nbsp;</div></div>");
                        }
                        catch (Exception)
                        {
                            //
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("said:"))
                {
                    var match = Regex.Match(log.Record, @"^(.*?) <([\w_:\d]+)> said: (.*?) \[([\d:.]+)\]$");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var steamId = match.Groups[2].Value;
                        var message = match.Groups[3].Value;
                        var timestamp = match.Groups[4].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{nickname}:&nbsp;</div><div>{message}</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{nickname}:&nbsp;</div><div>{message}</div></div>");
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("> killed "))
                {
                    var match = Regex.Match(log.Record, @"(.*?)<(.+?)>\s+killed\s+(.*?)<(.+?)>\s+with\s+weapon\s+<(.*?)>\s+(\<.+?\>) \[([\d:.]+)\]$");
                    if (match.Success)
                    {
                        var timespan = TimeSpan.Parse(match.Groups[7].Value);
                        var killer = match.Groups[1].Value;
                        var loser = match.Groups[3].Value;
                        var weapon = match.Groups[5].Value;
                        var headshot = match.Groups[6].Value;
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(killer)).Any())
                        {
                            if (headshot != "<True>")
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='color: #3399ff'>{loser}&nbsp;</div></div>");
                            }
                            else
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/headshot.svg'>&nbsp;&nbsp;</div><div style='color: #3399ff'>{loser}&nbsp;</div></div>");
                            }
                        }
                        else
                        {
                            if (headshot != "<True>")
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='color: #f42f46'>{loser}&nbsp;</div></div>");
                            }
                            else
                            {
                                sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{killer}&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/weapons/{weapon}.svg'>&nbsp;&nbsp;&nbsp;</div><div style='{weaponStyle}'><img style='height: {weaponHeight}' src='/images/headshot.svg'>&nbsp;&nbsp;</div><div style='color: #f42f46'>{loser}&nbsp;</div></div>");
                            }
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("made a triple kill!"))
                {
                    var match = Regex.Match(log.Record, @"(.*?) made a triple kill! \[(.*?)\]");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ffbf00'>ДЕЛАЕТ ТРИПЛ КИЛЛ!</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ffbf00'>ДЕЛАЕТ ТРИПЛ КИЛЛ!</div></div>");
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("made a quad kill!"))
                {
                    var match = Regex.Match(log.Record, @"(.*?) made a quad kill! \[(.*?)\]");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff3a95'>ДЕЛАЕТ КВАДРО КИЛЛ!</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff3a95'>ДЕЛАЕТ КВАДРО КИЛЛ!</div></div>");
                        }
                    }
                    continue;
                }
                if (log.Record.Contains("MADE A RAMPAGE!!!"))
                {
                    var match = Regex.Match(log.Record, @"(.*?) MADE A RAMPAGE!!! \[(.*?)\]");
                    if (match.Success)
                    {
                        var nickname = match.Groups[1].Value;
                        var timestamp = match.Groups[2].Value;
                        var timespan = TimeSpan.Parse(timestamp);
                        if (this.match.ATeamGrid.Where(x => x.Name.Contains(nickname)).Any())
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #f42f46'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                        }
                        else
                        {
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                            sb.Append($"<div style='{rowStyle}'><div style='color: #ffb624'>[{timespan.ToString(@"hh\:mm\:ss")}]&nbsp;</div><div style='color: #3399ff'>{nickname}&nbsp;</div><div style='color:#ff0018'>ОФОРМЛЯЕТ ЭЙС!!!</div></div>");
                        }
                    }
                    continue;
                }
            }

            sb.Append($"<div style='{rowStyle}'><div style='color: #cddc39'>{match.AName} (~{match.ATeamAVG}) vs {match.BName} (~{match.BTeamAVG}) на сервере {match.ServerName}!</div></div>");
            sb.Append($"<div style='{rowStyle}'><div style='color: #cddc39'>Матч начался!</div></div>");

            matchLog = sb.ToString();
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            setIntervalShouldStop = true;
        }

        private async Task Updater()
        {
            while (!match.IsFinished)
            {
                if (setIntervalShouldStop) break;

                await Task.Delay(20000);

                match = await http.GetFromJsonAsync<MatchInfo>($"/api/matches/getmatch?id={MatchID}");

                matchLength = "Длительность матча: " + match.MatchLength;

                if (AuthProvider.CurrentUser is not null)
                {
                    if (AuthProvider.CurrentUser.IsAdmin || AuthProvider.CurrentUser.IsVip || AuthProvider.CurrentUser.IsPremiumVip)
                    {
                        ParseMatchLogAsTicks(isMobile);
                    }
                    else
                    {
                        ParseMatchLog(isMobile);
                    }
                }
                else
                {
                    ParseMatchLog(isMobile);
                }

                StateHasChanged();
            }
        }

        [JSInvokable] // Этот метод может быть вызван из JavaScript
        public async Task OnResize(int width, int height)
        {
            if (width < 1800)
            {
                isTeamStuffsVisible = false;
            }
            else
            {
                isTeamStuffsVisible = true;
            }

            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            // Удаляем обработчик через JS
            JS.InvokeVoidAsync("resizeInterop.removeResizeListener");

            // Уничтожаем ссылку на объект .NET
            dotNetRef?.Dispose();

            match = null;
            matchLog = null;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    dotNetRef = DotNetObjectReference.Create(this);

                    // Вызываем JS-функцию для инициализации
                    await JS.InvokeVoidAsync("resizeInterop.initResizeListener", dotNetRef);

                    match = await http.GetFromJsonAsync<MatchInfo>($"/api/matches/getmatch?id={MatchID}");

                    Console.Write(match);

                    matchLength = "Длительность матча: " + match.MatchLength;

                    _aAVGColor = match.ABestPlayer.AVG > match.BBestPlayer.AVG ? moreColor : lessColor;
                    _aHSRColor = match.ABestPlayer.HSR > match.BBestPlayer.HSR ? moreColor : lessColor;
                    _aKDRColor = match.ABestPlayer.KDR > match.BBestPlayer.KDR ? moreColor : lessColor;
                    _aWinrateColor = match.ABestPlayer.Winrate > match.BBestPlayer.Winrate ? moreColor : lessColor;
                    _aMMRColor = match.ABestPlayer.MMR > match.BBestPlayer.MMR ? moreColor : lessColor;

                    _bAVGColor = match.ABestPlayer.AVG < match.BBestPlayer.AVG ? moreColor : lessColor;
                    _bHSRColor = match.ABestPlayer.HSR < match.BBestPlayer.HSR ? moreColor : lessColor;
                    _bKDRColor = match.ABestPlayer.KDR < match.BBestPlayer.KDR ? moreColor : lessColor;
                    _bWinrateColor = match.ABestPlayer.Winrate < match.BBestPlayer.Winrate ? moreColor : lessColor;
                    _bMMRColor = match.ABestPlayer.MMR < match.BBestPlayer.MMR ? moreColor : lessColor;

                    dataLoadFinished = true;

                    NavigationManager.LocationChanged += OnLocationChanged;

                    headerStyle = isMobile ? "background-image: url(/images/mapsbackgrs/" + match.MapName + ".jpg);height: 300px;background-size: 200% auto;background-color: rgb(0 0 0 / 65%); background-position: center; background-blend-mode: multiply;" :
            "background-image: url(/images/mapsbackgrs/" + match.MapName + ".jpg);height: 250px;background-size: 100% auto;background-color: rgb(0 0 0 / 65%); background-position: center; background-blend-mode: multiply;";

                    while (!dataLoadFinished)
                    {
                        // waiting for loading match data...
                    }

                    if (AuthProvider.CurrentUser is not null)
                    {
                        if (AuthProvider.CurrentUser.IsAdmin || AuthProvider.CurrentUser.IsVip || AuthProvider.CurrentUser.IsPremiumVip)
                        {
                            ParseMatchLogAsTicks(isMobile);
                        }
                        else
                        {
                            ParseMatchLog(isMobile);
                        }
                    }
                    else
                    {
                        ParseMatchLog(isMobile);
                    }

                    if (!match.IsFinished)
                    {
                        Updater();
                    }

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
