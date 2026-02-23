using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Application.DTOs
{
    public class LookupDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
