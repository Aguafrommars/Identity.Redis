// Project: aguacongas/Identity.Redis
// Copyright (c) 2018 @Olivier Lefebvre
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
