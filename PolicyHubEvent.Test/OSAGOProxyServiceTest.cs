using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PolicyEventHub.Applications.DTOs;
using PolicyEventHub.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyHubEvent.Test
{
    [TestFixture]
    public class OSAGOProxyServiceTest
    {
        private Mock<ILogger<OSAGOProxyService>> _logger;
        private Mock<ILegacyOSAGODataRetriver> _legacyOSAGODataRetriver;

        private OSAGOProxyService _service;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<OSAGOProxyService>>();
            _legacyOSAGODataRetriver = new Mock<ILegacyOSAGODataRetriver>();

            _service = new OSAGOProxyService(
                _logger.Object,
                _legacyOSAGODataRetriver.Object
            );
        }

        [Test]
        public async Task GetCancelledCompulsoryPolicyByIdAsync_ShouldReturnPolicy_WhenIdIsValid()
        {

            int policyId = 1144740;
            var expectedPolicy = new OSAGOResponseDto
            {
                PIN = "123456",
                Plate = "10-AA-001"
            };

            _legacyOSAGODataRetriver
                .Setup(x => x.GetCancelledCompulsoryPolicyByIdAsync(policyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPolicy);


            var result = await _service.GetCancelledCompulsoryPolicyByIdAsync(policyId, CancellationToken.None);


            Assert.That(result, Is.Not.Null);
            Assert.That(result.PIN, Is.EqualTo(expectedPolicy.PIN));
            Assert.That(result.Plate, Is.EqualTo(expectedPolicy.Plate));


            _legacyOSAGODataRetriver.Verify(
                x => x.GetCancelledCompulsoryPolicyByIdAsync(policyId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        [Test]
        public async Task GetIframeUrlUnregisteredAsync_ShouldReturnUrl_WhenIdIsValid()
        {

            int policyId = 1144740;
            string expectedUrl = "https://iframe.example.com/?id=1144740";

            _legacyOSAGODataRetriver
                .Setup(x => x.GetIframeUrlUnregisteredAsync(policyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUrl);


            var result = await _service.GetIframeUrlUnregisteredAsync(policyId, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedUrl));

            _legacyOSAGODataRetriver.Verify(
                x => x.GetIframeUrlUnregisteredAsync(policyId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        [Test]
        public async Task UpdateCancelledCompulsoryPolicyAsync_ShouldCallLegacyService_WhenDataIsValid()
        {

            int policyId = 1144740;
            var updateDto = new CancelledCompulsoryPolicyUpdateDto
            {
                InsuredFullName = "Test User",
                PIN = "123456",
                PhoneNumber = "0501234567",
                Email = "test@mail.com"
            };

            _legacyOSAGODataRetriver
                .Setup(x => x.UpdateCancelledCompulsoryPolicyAsync(
                    policyId,
                    updateDto,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            await _service.UpdateCancelledCompulsoryPolicyAsync(
                policyId,
                updateDto,
                CancellationToken.None);


            _legacyOSAGODataRetriver.Verify(
                x => x.UpdateCancelledCompulsoryPolicyAsync(
                    policyId,
                    updateDto,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Test]
        public async Task GetFilteredCancelledCompulsoryPoliciesAsync_ShouldReturnPagedResult_WhenRequestIsValid()
        {
            string sessionId = "9b5f6edf-9a9d-432b-869d-2b432f9b2e1a";
            var request = new OSAGORequestDto
            {
                Page = 1,
                PageSize = 10,
                PIN = null,
                Plate = null,
                StartDate = new DateTime(2025, 7, 4),
                EndDate = new DateTime(2025, 7, 9)
            };

            var expectedResult = new PagedResultDto<OSAGOResponseDto>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 1,
                Items = new List<OSAGOResponseDto>
        {
            new OSAGOResponseDto
            {
                PIN = "123456",
                Plate = "10-AA-001"
            }
        }
            };

            _legacyOSAGODataRetriver
                .Setup(x => x.GetFilteredCancelledCompulsoryPoliciesAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);


            var result = await _service.GetFilteredCancelledCompulsoryPoliciesAsync(
                request,
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Is.Not.Null);
            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Page, Is.EqualTo(request.Page));
            Assert.That(result.PageSize, Is.EqualTo(request.PageSize));
            Assert.That(result.TotalCount, Is.EqualTo(1));

            _legacyOSAGODataRetriver.Verify(
                x => x.GetFilteredCancelledCompulsoryPoliciesAsync(request,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
