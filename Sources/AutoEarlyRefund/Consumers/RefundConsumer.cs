using System.Text.Json;
using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>, IConsumer<ResultsRefundConfirmedSdt>, IConsumer<ResultsRefundConfirmed>, IConsumer<MatchCalculated>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetResourcesInfoHandler resourcesInfoHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly GetRefundMatchInfoHandler getRefundMatchInfoHandler;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetResourcesInfoHandler resourcesInfoHandler,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
        XmlConvertService xmlConvertService,
        GetRefundMatchInfoHandler getRefundMatchInfoHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
        this.xmlConvertService = xmlConvertService;
        this.getRefundMatchInfoHandler = getRefundMatchInfoHandler;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation("Refund result received: {MatchId}", context.Message.MatchId);

        try
        {
            var criteria = new GetResourcesTriggerCriteria
            {
                Action = context.Message.Action,
                SportType = context.Message.ShortType,
                ChildMatchType = context.Message.ChildMatchType,
                ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
            };

            var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(criteria);
            if (triggerResources.Count == 0)
            {
                logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
                return;
            }
            var resourceConditions = await getResourcesConditionsHandler.GetConditionsAsync(triggerResources);
            var sourceResources = await resourcesInfoHandler.HandleAsync(context.Message.MatchId, resourceConditions);

            //var validResults = sourceResources
            //    .Where(x => x.Resource is not null)
            //    .GroupBy(x => x.ResourceId)
            //    .SelectMany(group =>
            //        group.GroupBy(x => x.RuleName)
            //            .Where(ruleGroup => ruleGroup.All(sc =>
            //                SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc)))
            //            .Select(ruleGroup => new
            //            {
            //                ResourceId = group.Key,
            //                RuleName = ruleGroup.Key,
            //            })
            //            .Take(1)
            //    )
            //    .ToList();


            var srByResourceId = sourceResources
            //.Where(x => x.Resource is not null)
            .GroupBy(x => new { x.ResourceId })
            .ToDictionary(g => g.Key, g => g.ToList());

            var validResults = new List<(string, int, int)>();
            foreach (var (resourceId, sourceCondition) in srByResourceId)
            {
                var ruleGroup = sourceCondition
                     .GroupBy(x => new { x.RuleId, x.RuleName })
                     .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var (rule, condition) in ruleGroup)
                {
                    var valid = condition.All(sc =>
                    {
                        var spec = SpecificationFactory.CreateSpecification(sc);
                        var isSatisfiedBy = spec.IsSatisfiedBy(sc);
                        return isSatisfiedBy;
                    });
                    if (valid)
                    {
                        validResults.Add((resourceId.ResourceId, rule.RuleId, condition.First().MatchType));
                        break;
                    }

                }
            }

            Console.WriteLine(validResults);

            var match = new Match
            {
                MatchId = context.Message.MatchId,
                ResultDetails = validResults.Select(x => new ResultDetail
                {
                    ResourceId = x.Item1,
                    HomeValue = "",
                    AwayValue = "",
                    OtherInfo = JsonSerializer.Serialize(new { matchType = x.Item3, ruleId = x.Item2 }),
                }).ToList()
            }.WithResourcesXml(xmlConvertService);

            //await this.insOrUpdAutoPayoutResultHandler.Handle(match, 0);

        }
        catch (Exception ex)
        {

            throw;
        }



        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmedSdt> context)
    {
        logger.LogInformation("Refund result received: {MatchId}", context.Message.MatchId);
        var criteria = new GetResourcesTriggerCriteria
        {
            Action = context.Message.Action,
            SportType = context.Message.ShortType,
            ChildMatchType = context.Message.ChildMatchType,
            ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        };

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(criteria);
        if (triggerResources.Count == 0)
        {
            return;
        }
        var conditions = await getResourcesConditionsHandler.GetConditionsAsync(triggerResources);
        var sourceResources = await resourcesInfoHandler.HandleAsync(context.Message.MatchId, conditions);

        var z = sourceResources
            .GroupBy(x => new { x.ResourceId, x.RuleName })
            .ToDictionary(g => g.Key, g => g.ToList());

        var validResults = new List<string>();
        foreach (var (ruleKey, sourceCondition) in z)
        {
            var valid = sourceCondition.All(sc =>
            {
                var spec = SpecificationFactory.CreateSpecification(sc);
                var isSatisfiedBy = spec.IsSatisfiedBy(sc);
                return isSatisfiedBy;
            });

            if (valid)
            {
                validResults.Add(ruleKey.ResourceId);
                break;
            }
        }

        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);

    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmed> context)
    {
        var criteria = new GetResourcesTriggerCriteria
        {
            Action = context.Message.Action,
            SportType = context.Message.ShortType,
            ChildMatchType = context.Message.ChildMatchType,
            ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        };

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(criteria);
        if (triggerResources.Count == 0)
        {
            logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
            return;
        }

        var resourceConditions = await getResourcesConditionsHandler.GetConditionsAsync(triggerResources);
        var sourceResources = await resourcesInfoHandler.HandleAsync(context.Message.MatchId, resourceConditions);

        var srByResourceId = sourceResources
            .Where(x => x.Resource is not null)
            .GroupBy(x => new { x.ResourceId })
            .ToDictionary(g => g.Key, g => g.ToList());

        var validResults = new List<(string, int)>();
        foreach (var (resourceId, sourceCondition) in srByResourceId)
        {
            var ruleGroup = sourceCondition
                 .GroupBy(x => new { x.RuleId, x.RuleName })
                 .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (rule, condition) in ruleGroup)
            {
                var valid = condition.All(sc =>
                {
                    var spec = SpecificationFactory.CreateSpecification(sc);
                    var isSatisfiedBy = spec.IsSatisfiedBy(sc);
                    return isSatisfiedBy;
                });
                if (valid)
                {
                    validResults.Add((resourceId.ResourceId, rule.RuleId));
                    break;
                }

            }
        }

        var match = new Match
        {
            MatchId = context.Message.MatchId,
            ResultDetails = validResults.Select(x => new ResultDetail
            {
                ResourceId = x.Item1,
                HomeValue = "",
                AwayValue = "",
                SourceId = x.Item2.ToString(),
            }).ToList()
        }.WithResourcesXml(xmlConvertService);


        //await this.insOrUpdAutoPayoutResultHandler.Handle(match, 0);


        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);


        //var validResults = sourceResources
        //    .Where(x => x.Resource is not null)
        //    .GroupBy(x => x.ResourceId)
        //    .SelectMany(group =>
        //        group.GroupBy(x => x.RuleName)
        //            .Where(ruleGroup => ruleGroup.All(sc =>
        //                SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc)))
        //            .Select(ruleGroup => new
        //            {
        //                ResourceId = group.Key,
        //                RuleName = ruleGroup.Key,
        //            })
        //            .Take(1)
        //    )
        //    .ToList();
    }

    public async Task Consume(ConsumeContext<MatchCalculated> context)
    {
        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);

        var criteria = new GetResourcesTriggerCriteria
        {
            Action = context.Message.Action,
            SportType = context.Message.ShortType,
            ChildMatchType = context.Message.ChildMatchType,
            ResourceIds = "00"

        };

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(criteria);
        var currentTriggerResources = triggerResources.Where(x => x.TargetIsCrrMatch == 1).ToList();
        var relatedTriggerResources = triggerResources.Where(x => x.TargetIsCrrMatch != 1).ToList();

        var matchInfo = await getRefundMatchInfoHandler.HandleAsync(relatedTriggerResources.Select(x => new GetRelatedMatchItemRequest
        {
            MatchId = context.Message.MatchId,
            ChildMatchType = x.ChildMatchType,
            TargetChildMatchType = x.TargetResourceChildMatchType
        }).ToList());
        // GET MATCHES

        if (triggerResources.Count == 0)
        {
            logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
            return;
        }

        var resourceConditions = await getResourcesConditionsHandler.GetConditionsAsync(triggerResources);

        var sourceResources = await resourcesInfoHandler.HandleAsync(context.Message.MatchId, resourceConditions);



        var srByResourceId = sourceResources
            .Where(x => x.Resource is not null)
            .GroupBy(x => new { x.ResourceId })
            .ToDictionary(g => g.Key, g => g.ToList());

        var validResults = new List<(string, int)>();
        foreach (var (resourceId, sourceCondition) in srByResourceId)
        {
            var ruleGroup = sourceCondition
                 .GroupBy(x => new { x.RuleId, x.RuleName })
                 .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (rule, condition) in ruleGroup)
            {
                var valid = condition.All(sc =>
                {
                    var spec = SpecificationFactory.CreateSpecification(sc);
                    var isSatisfiedBy = spec.IsSatisfiedBy(sc);
                    return isSatisfiedBy;
                });
                if (valid)
                {
                    validResults.Add((resourceId.ResourceId, rule.RuleId));
                    break;
                }

            }
        }
        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
    }


    private int GetMatchType(int matchType, int childMatchType)
    {
        // not current match ==> related match, parent match
        // 
        return matchType == 1 ? 1 : 2;
    }
}