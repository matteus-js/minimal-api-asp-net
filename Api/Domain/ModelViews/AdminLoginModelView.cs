using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Domain.ModelViews
{
    public record AdminLoginModelView
    {
        public string Token { get; set; } = default!;
    }
}