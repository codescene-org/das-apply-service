﻿using AutoMapper;

namespace SFA.DAS.ApplyService.InternalApi.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Controllers;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models.Roatp;
    using Moq;
    using NUnit.Framework;
    using SFA.DAS.ApplyService.Domain.Roatp;

    [TestFixture]
    public class RoatpApplicationControllerTests
    {
        private Mock<ILogger<RoatpApplicationController>> _logger;
        private Mock<IRoatpApiClient> _apiClient;

        private RoatpApplicationController _controller;

        [SetUp]
        public void Before_each_test()
        {
            _logger = new Mock<ILogger<RoatpApplicationController>>();
            _apiClient = new Mock<IRoatpApiClient>();

            _controller = new RoatpApplicationController(_logger.Object, _apiClient.Object);

            Mapper.Reset();

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<RoatpProfile>();
            });
            
            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void Get_application_routes_returns_mapped_provider_types()
        {
            var providerTypes = new List<ProviderType>
            {
                new ProviderType { Id = 1, Type = "Main provider" },
                new ProviderType { Id = 2, Type = "Employer provider" },
                new ProviderType { Id = 3, Type = "Supporting provider" }
            };

            _apiClient.Setup(x => x.GetProviderTypes()).ReturnsAsync(providerTypes);

            var result = _controller.GetApplicationRoutes().GetAwaiter().GetResult();

            result.Should().BeOfType<OkObjectResult>();

            var applicationRoutes = ((OkObjectResult)result).Value as IEnumerable<ApplicationRoute>;

            applicationRoutes.Should().NotBeNull();

            applicationRoutes.Count().Should().Be(3);
            var applicationRoutesList = applicationRoutes.ToList();
            applicationRoutesList[0].Id.Should().Be(providerTypes[0].Id);
            applicationRoutesList[0].RouteName.Should().Be(providerTypes[0].Type);
            applicationRoutesList[1].Id.Should().Be(providerTypes[1].Id);
            applicationRoutesList[1].RouteName.Should().Be(providerTypes[1].Type);
            applicationRoutesList[2].Id.Should().Be(providerTypes[2].Id);
            applicationRoutesList[2].RouteName.Should().Be(providerTypes[2].Type);
        }

        [Test]
        public void Ukprn_on_register_performs_register_status_lookup_if_already_exists()
        {
            var matchedResponse = new DuplicateCheckResponse
            {
                DuplicateFound = true,
                DuplicateOrganisationId = Guid.NewGuid(),
                DuplicateOrganisationName = "Duplicate Org"
            };

            _apiClient.Setup(x => x.DuplicateUKPRNCheck(It.IsAny<Guid>(), It.IsAny<long>())).ReturnsAsync(matchedResponse);

            var registerStatus = new OrganisationRegisterStatus
            {
                ExistingUKPRN = true,
                StatusId = OrganisationRegisterStatus.ActiveStatus,
                ProviderTypeId = ProviderType.EmployerProvider
            };

            _apiClient.Setup(x => x.GetOrganisationRegisterStatus(It.IsAny<Guid>())).ReturnsAsync(registerStatus).Verifiable();

            var result = _controller.UkprnOnRegister(10002000).GetAwaiter().GetResult();

            result.Should().BeOfType<OkObjectResult>();

            var registerStatusResult = ((OkObjectResult)result).Value as OrganisationRegisterStatus;

            registerStatusResult.Should().NotBeNull();

            registerStatusResult.ExistingUKPRN.Should().BeTrue();
            registerStatusResult.StatusId.Should().Be(registerStatus.StatusId);
            registerStatusResult.ProviderTypeId.Should().Be(registerStatus.ProviderTypeId);

            _apiClient.Verify(x => x.GetOrganisationRegisterStatus(It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public void Ukprn_on_register_does_not_lookup_status_if_not_already_on_register()
        {
            var matchedResponse = new DuplicateCheckResponse
            {
                DuplicateFound = false,
                DuplicateOrganisationId = Guid.Empty,
                DuplicateOrganisationName = null
            };

            _apiClient.Setup(x => x.DuplicateUKPRNCheck(It.IsAny<Guid>(), It.IsAny<long>())).ReturnsAsync(matchedResponse);

            var registerStatus = new OrganisationRegisterStatus
            {
                ExistingUKPRN = true,
                StatusId = OrganisationRegisterStatus.ActiveStatus,
                ProviderTypeId = ProviderType.EmployerProvider
            };

            _apiClient.Setup(x => x.GetOrganisationRegisterStatus(It.IsAny<Guid>())).ReturnsAsync(registerStatus).Verifiable();

            var result = _controller.UkprnOnRegister(10002000).GetAwaiter().GetResult();

            result.Should().BeOfType<OkObjectResult>();

            var registerStatusResult = ((OkObjectResult)result).Value as OrganisationRegisterStatus;

            registerStatusResult.Should().NotBeNull();

            registerStatusResult.ExistingUKPRN.Should().BeFalse();

            _apiClient.Verify(x => x.GetOrganisationRegisterStatus(It.IsAny<Guid>()), Times.Never);
        }
    }
}