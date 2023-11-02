﻿using Backend_Project.Application.Entity;
using Backend_Project.Application.Notifications.Services;
using Backend_Project.Domain.Entities;
using Backend_Project.Persistence.DataContexts;
using Backend_Project.Persistence.SeedData;

namespace Backend_Project.Infrastructure.CompositionServices
{
    public class EmailManagementService : IEmailManagementService
    {
        private readonly IEntityBaseService<EmailTemplate> _emailTemplateService;
        private readonly IEmailPlaceholderService _emailPlaceholderService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IEmailMessageService _emailMessageService;
        private readonly IEntityBaseService<Email> _emailService;
        private readonly IDataContext _appDataContext;

        public EmailManagementService(
            IEntityBaseService<EmailTemplate> emailTemplateService,
            IEmailPlaceholderService emailPlaceholderService,
            IEmailSenderService emailSenderService,
            IEmailMessageService emailMessageService,
            IEntityBaseService<Email> emailService,
            IDataContext dataContext
        )
        {
            _emailTemplateService = emailTemplateService;
            _emailPlaceholderService = emailPlaceholderService;
            _emailSenderService = emailSenderService;
            _emailMessageService = emailMessageService;
            _emailService = emailService;
            _appDataContext = dataContext;
        }
        public async ValueTask<bool> SendEmailAsync(Guid userId, Guid templateId)
        {
            var template = await _emailTemplateService.GetByIdAsync(templateId);

            var placeholders = await _emailPlaceholderService.GetTemplateValues(userId, template);

            var message = await _emailMessageService.ConvertToMessage(template, placeholders, _appDataContext.GetUserSystem().Id, userId);

            var result = await _emailSenderService.SendEmailAsync(message);

            var email = ToEmail(message);
            email.IsSent = result;

            await _emailService.CreateAsync(email);
            return result;
        }

        private Email ToEmail(EmailMessage message)
        {
            return new Email()
            {
                SendUserId = message.SenderUserId,
                ReceiverUserId = message.ReceiverUserId,
                SenderEmailAddress = message.SenderAddress,
                ReceiverEmailAddress = message.ReceiverAddress,
                Subject = message.Subject,
                Body = message.Body,
            };
        }
    }
}