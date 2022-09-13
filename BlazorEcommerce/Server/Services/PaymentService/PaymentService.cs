using Stripe;
using Stripe.Checkout;

namespace BlazorEcommerce.Server.Services.PaymentService;

public class PaymentService : IPaymentService
{
    private readonly IAuthService _authService;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    private const string secret = "whsec_b54f37a3f2340aa78fd54f6e1fede0a35554b3c1aace1ac695abec1a16375378";

    public PaymentService(IAuthService authService, ICartService cartService, IOrderService orderService)
    {
        StripeConfiguration.ApiKey = "sk_test_51LbANHKZXMz6jcNcCZtliktilONWQ6cyx9EhRPJXMw5VNIedyJd2xUVcqFiZOIRJtEpwNhrmmebY09FOjKogzabl00liiIrtxS";

        _authService = authService;
        _cartService = cartService;
        _orderService = orderService;
    }
    public async Task<Session> CreateCheckoutSession()
    {
        var products = (await _cartService.GetDbCartProducts()).Data;
        var lineItems = new List<SessionLineItemOptions>();

        foreach (var product in products)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = product.Price * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Title, 
                        Images = new List<string>{ product.ImageUrl }
                    }
                },
                Quantity = product.Quantity
            });
        }

        var options = new SessionCreateOptions
        {
            CustomerEmail = _authService.GetUserEmail(),
            ShippingAddressCollection = new SessionShippingAddressCollectionOptions
            {
                AllowedCountries = new List<string>
                {
                    "US", "CA"
                }
            },
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = "https://localhost:7267/order-success",
            CancelUrl = "https://localhost:7267/cart",
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session;
    }

    public async Task<ServiceResponse<bool>> FulfillOrder(HttpRequest request)
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json, 
                request.Headers["Stripe-Signature"], 
                secret
            );

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                var user = await _authService.GetUserByEmail(session.CustomerEmail);
                await _orderService.PlaceOrder(user.Id);
            }

            return new ServiceResponse<bool> { Data = true };
        }
        catch
        {
            return new ServiceResponse<bool> { Data = false, Success = false, Message = "" };
        }
    }
}