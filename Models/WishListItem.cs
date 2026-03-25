namespace MTKPM_FE.Models
{
    public class WishListItem
    {
        public int WishListItemId { get; set; }
        public int WishListId { get; set; }
        public WishList WishList { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
