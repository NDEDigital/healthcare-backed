using System.ComponentModel.DataAnnotations;

namespace NDE_Digital_Market.Model
{
    public class ReviewsAndRatings
    {

        public int ReviewId { get; set; }

        public int? OrderDetailId { get; set; }

        public string? ReviewText { get; set; }

        public int? RatingValue { get; set; }

        public string? BuyerId { get; set; }
        public string? BuyerCode { get; set; }

        public string? GroupCode { get; set; }

        public int? GoodsId { get; set; }

        public string? GroupName { get; set; }

        public string? SellerId { get; set; }

        public DateTime? DateTime { get; set; }
        public IFormFile? Image { get; set; }

        public string? ImageName { get; set; }
        
        public string? ImagePath { get; set; }

        // Added by Marufa
        public string? RatingArray { get; set; }
        public string? EmptyRatingArray { get; set; }
        public string? BuyerName { get; set; }
        public string? SellerName { get; set; }
    }
}
