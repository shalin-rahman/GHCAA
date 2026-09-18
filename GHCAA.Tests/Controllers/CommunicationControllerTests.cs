using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // Template CRUD (Create/Update/Delete) had zero coverage until now (47.13.6).
    [TestFixture]
    public class CommunicationControllerTests
    {
        private Mock<ICommunicationService> _commServiceMock;
        private Mock<INotificationService> _notificationServiceMock;
        private CommunicationController _controller;

        [SetUp]
        public void Setup()
        {
            _commServiceMock = new Mock<ICommunicationService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _controller = new CommunicationController(_commServiceMock.Object, _notificationServiceMock.Object);
        }

        [Test]
        public async Task CreateTemplate_ReturnsOk_OnSuccess()
        {
            var template = new EmailTemplate { Code = "WELCOME_EMAIL", Subject = "Welcome", Body = "Hi", Description = "Sent on approval" };
            var created = new EmailTemplate { Id = 1, Code = "WELCOME_EMAIL", Subject = "Welcome", Body = "Hi", Description = "Sent on approval" };
            _commServiceMock.Setup(x => x.CreateTemplateAsync(template, It.IsAny<CancellationToken>())).ReturnsAsync(created);

            var result = await _controller.CreateTemplate(template, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(created));
        }

        [Test]
        public void CreateTemplate_PropagatesException_WhenCodeAlreadyExists()
        {
            var template = new EmailTemplate { Code = "WELCOME_EMAIL", Subject = "Welcome", Body = "Hi", Description = "Sent on approval" };
            _commServiceMock.Setup(x => x.CreateTemplateAsync(template, It.IsAny<CancellationToken>()))
                            .ThrowsAsync(new InvalidOperationException("Template code already exists."));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.CreateTemplate(template, CancellationToken.None));
        }

        [Test]
        public async Task UpdateTemplate_ReturnsOk_OnSuccess()
        {
            var template = new EmailTemplate { Code = "WELCOME_EMAIL", Subject = "Welcome v2", Body = "Hi", Description = "Sent on approval" };
            var updated = new EmailTemplate { Id = 1, Code = "WELCOME_EMAIL", Subject = "Welcome v2", Body = "Hi", Description = "Sent on approval" };
            _commServiceMock.Setup(x => x.UpdateTemplateAsync(It.Is<EmailTemplate>(t => t.Id == 1), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(updated);

            var result = await _controller.UpdateTemplate(1, template, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(updated));
        }

        [Test]
        public void UpdateTemplate_PropagatesException_WhenTemplateMissing()
        {
            var template = new EmailTemplate { Code = "MISSING", Subject = "x", Body = "y", Description = "z" };
            _commServiceMock.Setup(x => x.UpdateTemplateAsync(It.Is<EmailTemplate>(t => t.Id == 99), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(new KeyNotFoundException("Template not found."));

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _controller.UpdateTemplate(99, template, CancellationToken.None));
        }

        [Test]
        public async Task DeleteTemplate_ReturnsNoContent_OnSuccess()
        {
            _commServiceMock.Setup(x => x.DeleteTemplateAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.DeleteTemplate(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public void DeleteTemplate_PropagatesException_WhenTemplateMissing()
        {
            _commServiceMock.Setup(x => x.DeleteTemplateAsync(99, It.IsAny<CancellationToken>()))
                            .ThrowsAsync(new KeyNotFoundException("Template not found."));

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _controller.DeleteTemplate(99, CancellationToken.None));
        }

        [Test]
        public async Task SendBatch_ReturnsOk_OnSuccess()
        {
            var dto = new BulkEmailDto { TemplateCode = "REUNION", PassingYears = new List<int> { 2010, 2012 } };
            _commServiceMock.Setup(x => x.SendBatchEmailAsync(dto.PassingYears, dto.TemplateCode, dto.CustomVars, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var result = await _controller.SendBatch(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task SendBatch_ReturnsBadRequest_WhenNoPassingYearGiven()
        {
            var dto = new BulkEmailDto { TemplateCode = "REUNION" };

            var result = await _controller.SendBatch(dto, CancellationToken.None);

            var problem = result as ObjectResult;
            Assert.That(problem?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task SendType_ReturnsOk_OnSuccess()
        {
            var dto = new BulkEmailDto { TemplateCode = "DUES_REMINDER", MembershipTypes = new List<string> { "Alumni" } };
            _commServiceMock.Setup(x => x.SendTypeEmailAsync(dto.MembershipTypes, dto.TemplateCode, dto.CustomVars, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var result = await _controller.SendType(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task SendType_ReturnsBadRequest_WhenNoMembershipTypeGiven()
        {
            var dto = new BulkEmailDto { TemplateCode = "DUES_REMINDER" };

            var result = await _controller.SendType(dto, CancellationToken.None);

            var problem = result as ObjectResult;
            Assert.That(problem?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task SendCustom_ReturnsOk_OnSuccess_ForEmailList()
        {
            var dto = new CustomEmailDto { Emails = new List<string> { "a@ghc.edu" }, Subject = "Update", Body = "Body text", Channel = "email" };
            _commServiceMock.Setup(x => x.SendCustomEmailAsync(dto.Emails, dto.TemplateCode, dto.Subject, dto.Body, null, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var result = await _controller.SendCustom(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _commServiceMock.Verify(x => x.SendCustomEmailAsync(dto.Emails, dto.TemplateCode, dto.Subject, dto.Body, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendCustom_ReturnsBadRequest_WhenBatchTargetHasNoValues()
        {
            var dto = new CustomEmailDto { TargetMethod = "batch", Channel = "email" };

            var result = await _controller.SendCustom(dto, CancellationToken.None);

            var problem = result as ObjectResult;
            Assert.That(problem?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}
