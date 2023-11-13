using AuctionService.Entities;
using System.ComponentModel.DataAnnotations;

namespace AuctionService.DTO
{
    public class UpdateAuctionDTO
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int? Year { get; set; }
        public string Colour { get; set; }
        public int? Mileage { get; set; }
    }
}
