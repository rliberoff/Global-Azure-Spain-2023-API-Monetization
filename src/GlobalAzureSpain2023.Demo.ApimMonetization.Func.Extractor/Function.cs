using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;

using Microsoft.Azure.WebJobs;

using Microsoft.Extensions.Logging;

using Stripe;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Func.Extractor
{
    public class Function
    {
        private readonly ArmClient armClient;

        private readonly string apimResourceGroupName;
        private readonly string apimServiceName;
        private readonly string apimSubsctiptionId;

        public Function()
        {
            armClient = new ArmClient(new DefaultAzureCredential());

            apimResourceGroupName = Environment.GetEnvironmentVariable(@"ApimResourceGroupName");
            apimServiceName = Environment.GetEnvironmentVariable(@"ApimServiceName");
            apimSubsctiptionId = Environment.GetEnvironmentVariable(@"ApimSubscriptionId");

            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable(@"StripeApplicationKey");
        }

        [FunctionName(@"FunctionExtractor")]
        public async Task Run([TimerTrigger(@"0 */1 * * * *")] TimerInfo _, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation($@"Extractor started at {DateTime.Now}");

            var subscriptionService = new SubscriptionService();
            var usageRecordService = new UsageRecordService();

            StripeList<Subscription> stripeSubscriptions;

            string startingAfter = null;

            var now = DateTime.UtcNow;

            var options = new SubscriptionListOptions
            {
                Status = @"active",
            };

            do
            {
                if (!string.IsNullOrWhiteSpace(startingAfter))
                {
                    options.StartingAfter = startingAfter;
                }

                stripeSubscriptions = await subscriptionService.ListAsync(options, cancellationToken: cancellationToken);

                foreach (var stripeSubscription in stripeSubscriptions)
                {
                    var stripeSubscriptionItem = stripeSubscription.Items.Data[0];

                    if (stripeSubscriptionItem.Price.Recurring.UsageType.ToUpperInvariant() != @"METERED")
                    {
                        continue;
                    }

                    if (!stripeSubscription.Metadata.TryGetValue(@"apim-subscription-id", out var subscriptionId) || string.IsNullOrWhiteSpace(subscriptionId))
                    {
                        continue;
                    }

                    var apiManagementServiceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(apimSubsctiptionId, apimResourceGroupName, apimServiceName);
                    var apiManagementService = armClient.GetApiManagementServiceResource(apiManagementServiceResourceId);

                    var startOfBillingPeriod = stripeSubscription.CurrentPeriodStart;

                    if (stripeSubscription.Metadata.TryGetValue(@"last-usage-update", out var lastUsageUpdateVale)
                        && DateTime.TryParse(lastUsageUpdateVale, out var lastUsageUpdate)
                        && lastUsageUpdate < startOfBillingPeriod)
                    {
                        var priorUsageReport = await GetUsageReportAsync(subscriptionId, lastUsageUpdate, startOfBillingPeriod, apiManagementService, cancellationToken);

                        await ProcessReportAsync(priorUsageReport, subscriptionService, usageRecordService, stripeSubscriptionItem.Id, startOfBillingPeriod.AddMilliseconds(1), startOfBillingPeriod, cancellationToken);
                    }

                    var report = await GetUsageReportAsync(subscriptionId, startOfBillingPeriod, now, apiManagementService, cancellationToken);

                    await ProcessReportAsync(report, subscriptionService, usageRecordService, stripeSubscriptionItem.Id, startOfBillingPeriod, now, cancellationToken);

                    startingAfter = stripeSubscription.Id;
                }
            }
            while (stripeSubscriptions.HasMore);
        }

        private static async Task ProcessReportAsync(ReportRecordContract report, SubscriptionService stripeSubscriptionService, UsageRecordService stripeUsageRecordService, string stripeSubscriptionId, DateTime timeStamp, DateTime lastUsageUpdate, CancellationToken cancellationToken)
        {
            if (report.CallCountTotal.HasValue)
            {
                var usageUnits = report.CallCountTotal.Value;

                if (usageUnits != 0)
                {
                    await stripeUsageRecordService.CreateAsync(stripeSubscriptionId, new UsageRecordCreateOptions()
                    {
                        Quantity = usageUnits,
                        Timestamp = timeStamp,
                        Action = @"set",
                    }, cancellationToken: cancellationToken);

                    await stripeSubscriptionService.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions()
                    {
                        Metadata = new Dictionary<string, string>()
                        {
                            { @"last-usage-update", lastUsageUpdate.ToString(@"o", CultureInfo.InvariantCulture) }
                        }
                    }, cancellationToken: cancellationToken);
                }
            }
        }

        private async Task<ReportRecordContract> GetUsageReportAsync(string subscriptionId, DateTime from, DateTime to, ApiManagementServiceResource apiManagementService, CancellationToken cancellationToken)
        {
            var filter = $@"timestamp ge datetime'{from.ToString(@"o", CultureInfo.InvariantCulture)}' and timestamp le datetime'{to.ToString(@"o", CultureInfo.InvariantCulture)}' and subscriptionId eq '{subscriptionId}'";

            return await apiManagementService.GetReportsBySubscriptionAsync(filter, cancellationToken: cancellationToken).FirstAsync(cancellationToken);
        }
    }
}
