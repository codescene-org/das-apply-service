﻿namespace SFA.DAS.ApplyService.Web.ViewModels.Roatp
{
    using Domain.Roatp;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class SelectApplicationRouteViewModel
    {
        public IEnumerable<ApplicationRoute> ApplicationRoutes { get; set; }

        [Required(ErrorMessage = "Select your training provider route")]
        [Range(1, 3, ErrorMessage = "Select your training provider route")]
        public int ApplicationRouteId { get; set; }
    }
}