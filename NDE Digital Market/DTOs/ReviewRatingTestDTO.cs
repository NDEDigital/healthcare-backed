namespace NDE_Digital_Market.DTOs
{
    public class ReviewRatingTestDTO
    {

        public string BuyerName { get; set; }
        public int? RatingValue { get; set; }
        public string ReviewText { get; set; }
        public string ImagePath { get; set; }
        public DateTime? ReviewDate { get; set; }

        public int? ProductId { get; set; }
    }
}
