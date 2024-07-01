using SWD.F_LocalBrand.Business.DTO.Cart;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Cart
{
    public class UpdateCartRequest
    {
        public List<CartProduct> Products { get; set; }
        public ListCartUpdateModel MapToModel()
        {
            return new ListCartUpdateModel
            {
                Products = Products.Select(p => new CartProductModel
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity
                }).ToList()
            };
        }
    }
}
