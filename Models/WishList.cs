namespace MTKPM_FE.Models
{
    public class WishList
    {
        public int WishListId { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<WishListItem> Items { get; set; } = new List<WishListItem>();
    }
}
