// Project: aguacongas/Identity.Redis
// Copyright (c) 2025 @Olivier Lefebvre
using System.ComponentModel.DataAnnotations;

namespace IdentitySample.Models.AccountViewModels
{
    public class UseRecoveryCodeViewModel
    {
        [Required]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }
    }
}
