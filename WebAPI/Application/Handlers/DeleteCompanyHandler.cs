using Application.Commands;
using Application.Notifications;
using AutoMapper;
using Contracts;
using Entities.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    internal sealed class DeleteCompanyHandler : INotificationHandler<CompanyDeleteNotification>
    {
        private readonly IRepositoryManager _repository;

        public DeleteCompanyHandler(IRepositoryManager repository)
        {
            _repository = repository;
        }


        public async Task Handle(CompanyDeleteNotification notification, CancellationToken cancellationToken)
        {
            var company = await _repository.Company.GetCompany(notification.Id, false);

            if (company is null)
                throw new CompanyNotFoundException(notification.Id);

            _repository.Company.DeleteCompany(company);
            await _repository.SaveASync();
        }
    }
}
