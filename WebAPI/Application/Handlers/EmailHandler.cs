using Application.Notifications;
using Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    internal sealed class EmailHandler : INotificationHandler<CompanyDeleteNotification>
    {
        private readonly ILoggerManager _logger;

        public EmailHandler(ILoggerManager logger)
        {
            _logger = logger;
        }

        public async Task Handle(CompanyDeleteNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogWarn($"Delete action for the company with id: {notification.Id} has occurred.");

            await Task.CompletedTask;
        }
    }
}
