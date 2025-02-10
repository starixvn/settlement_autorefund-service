using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesInfo;

public class GetResourcesInfoHandler
{
    private readonly IDynamicRepository repository;

    public GetResourcesInfoHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }
    public async Task<List<SourcesConditions>> HandleAsync(int matchId, List<SourcesConditions> sourcesConditions)
    {
        var groupBySources = sourcesConditions
            .GroupBy(x => new
            {
                x.SportType,
                x.SourceMatchType,
                x.SourceChildMatchType,
                x.SourceResource
            }).Select(x => new
            {
                SourceResource = x.Key.SourceResource,
                SportType = x.Key.SportType,
                SourceMatchType = x.Key.SourceMatchType,
                sourceChildMatchType = x.Key.SourceChildMatchType,
                MatchId = matchId
            }).ToList();

        var json = JsonSerializer.Serialize(groupBySources, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var resources = await repository.FetchAsync<Resource>(new GetResourcesInfoCriteria(json));

        var newResources = sourcesConditions.Join(
            resources,
            sc => sc.SourceResource,
            s => s.SourceResource,
            (sc, s) => new SourcesConditions
            {
                Id = sc.Id,
                SportType = sc.SportType,
                ResourceId = sc.ResourceId,
                RuleName = sc.RuleName,
                SourceAction = sc.SourceAction,
                MatchType = sc.MatchType,
                ChildMatchType = sc.ChildMatchType,
                SourceMatchType = sc.SourceMatchType,
                SourceChildMatchType = sc.SourceChildMatchType,
                SourceResource = sc.SourceResource,
                Conditions = sc.Conditions,

                MatchId = s.MatchId,
                ParentId = s.ParentId,
                ResultTable = s.ResultTable,
                HomeValue = s.HomeValue,
                AwayValue = s.AwayValue,
                ResultStatusId = s.ResultStatusId,
                SettlementStatusId = s.SettlementStatusId,
                IsConfirmedRefund = s.IsConfirmedRefund,
                IsConfirmedRefundSdt = s.IsConfirmedRefundSdt
            }).ToList();

        return newResources;
    }
}
