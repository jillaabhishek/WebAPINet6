using Contracts;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logggerManager;

        public CompanyService(IRepositoryManager repositoryManager, ILoggerManager logggerManager)
        {
            _repositoryManager = repositoryManager;
            _logggerManager = logggerManager;
        }
    }
}
