// Project: aguacongas/Identity.Redis
// Copyright (c) 2025 @Olivier Lefebvre
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentitySample.Models.ManageViewModels
{
    public class DisplayRecoveryCodesViewModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }

    }
}
