﻿// Project: aguacongas/Identity.Redis
// Copyright (c) 2018 @Olivier Lefebvre
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySample.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
