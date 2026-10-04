using Evently.Modules.Ticketing.Application.Orders;
using Microsoft.Extensions.Options;
using Quartz;

namespace Evently.Modules.Ticketing.Infrastructure.Orders;

internal sealed class ConfigureProcessOrderExpirationsJob(IOptions<OrdersOptions> ordersOptions)
    : IConfigureOptions<QuartzOptions>
{
    private readonly OrdersOptions _ordersOptions = ordersOptions.Value;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ProcessOrderExpirationsJob).FullName!;

        options
            .AddJob<ProcessOrderExpirationsJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(_ordersOptions.ExpirationIntervalInSeconds).RepeatForever()));
    }
}
