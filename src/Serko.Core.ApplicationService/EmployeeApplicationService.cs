﻿using Serko.Core.Domain.Entities;
using Serko.Core.Domain.Interfaces.ApplicationService;
using Serko.Core.Domain.Interfaces.Service;
using Serko.Core.Domain.Interfaces.UnitOfWork;
using Serko.Core.Domain.Resources;
using Serko.Core.Domain.Scopes;
using Serko.Core.Domain.Util;
using System.Linq;
using System.Threading.Tasks;

namespace Serko.Core.ApplicationService
{
    public class EmployeeApplicationService : BaseApplicationService, IEmployeeApplicationService
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeApplicationService(IEmployeeService employeeService,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _employeeService = employeeService;
        }

        public async Task<Employee> AuthenticationAsync(dynamic request)
        {
            Employee employee = Converter.ConvertTo<Employee>(request);

            var validateUsername = EmployeeScope.ValidateUsername(employee);

            if (!validateUsername.Any())
            {
                var employeeAuth = _employeeService.AuthenticationByUsernameAsync(employee);
                if (employeeAuth?.Result != null) return await employeeAuth;

                AddNotification("Employee", string.Format(Messages.ERROR_INVALID_AUTHENTICATION, "username/password"));
                return null;
            }

            var validateBarcode = EmployeeScope.ValidateBarcode(employee);

            if (!validateBarcode.Any())
            {
                var employeeAuth = _employeeService.AuthenticationByBarcodeAsync(employee);
                if (employeeAuth?.Result != null) return await employeeAuth;

                AddNotification("Employee", string.Format(Messages.ERROR_INVALID_AUTHENTICATION, "barcode/password"));
                return null;
            }

            if (validateUsername.Any() && validateBarcode.Any())
                AddNotification("Employee", Messages.ERROR_AUTHENTICATION_ISNULL);

            else if (validateUsername.Any())
                AddNotifications(validateUsername);

            else if (validateBarcode.Any())
                AddNotifications(validateBarcode);
            
            return null;
        }
    }
}