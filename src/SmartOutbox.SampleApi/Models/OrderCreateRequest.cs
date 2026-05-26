using System.ComponentModel.DataAnnotations;

namespace SmartOutbox.SampleApi.Models
{
    public sealed class OrderCreateRequest
    {
        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; } = null!;

        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }
    }
}
