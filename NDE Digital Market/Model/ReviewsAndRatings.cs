using System.ComponentModel.DataAnnotations;

namespace NDE_Digital_Market.Model
{
    public class ReviewsAndRatings
    {
        
   
 
        public string? BuyerName { get; set; }

        public string? SellerName { get; set; }
   
        public DateTime? DateTime { get; set; }

 

        public string? EmptyRatingArray { get; set; }
        public string? RatingArray { get; set; }
        public string? BuyerCode { get; set; }
        public int? ReviewId { get; set; }

        public int? OrderDetailId { get; set; }

        public string? ReviewText { get; set; }

        public int? RatingValue { get; set; }

        public int? BuyerId { get; set; }


        public int? ProductGroupID { get; set; }

        public int? ProductId { get; set; }

        public int? SellerId { get; set; }

        public DateTime? ReviewDate { get; set; }

        public IFormFile? ImageFile { get; set; }

        //public byte[]? ImageFileBite { get; set; }

        public string? ImagePath { get; set; }

        public DateTime? AddedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? AddedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? AddedPc { get; set; }
        public string? UpdatedPC { get; set; }
    }
}
