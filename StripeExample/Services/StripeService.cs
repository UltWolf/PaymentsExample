using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using StripeExample.Data;
using StripeExample.Models;

namespace StripeExample.Services
{
    public interface IStripeService
    {
        Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct);
        Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct);

    }

    public class StripeService : IStripeService
    {
        private readonly StripeConfig _stripeSettings;
        private readonly ChargeService _chargeService;
        private readonly CustomerService _customerService;
        private readonly TokenService _tokenService;

        public StripeService(
            ChargeService chargeService,
            CustomerService customerService,
            TokenService tokenService)
        {
            _chargeService = chargeService;
            _customerService = customerService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Create a new customer at Stripe through API using customer and card details from records.
        /// </summary>
        /// <param name="customer">Stripe Customer</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Stripe Customer</returns>
        public async Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct)
        {
            TokenCreateOptions tokenOptions = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Name = customer.Name,
                    Number = customer.CreditCard.CardNumber,
                    ExpYear = customer.CreditCard.ExpirationYear,
                    ExpMonth = customer.CreditCard.ExpirationMonth,
                    Cvc = customer.CreditCard.Cvc
                }
            };

            // Create new Stripe Token
            Token stripeToken = await _tokenService.CreateAsync(tokenOptions, null, ct);

            // Set Customer options using
            CustomerCreateOptions customerOptions = new CustomerCreateOptions
            {
                Name = customer.Name,
                Email = customer.Email,
                Source = stripeToken.Id
            };

            // Create customer at Stripe
            Customer createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

            // Return the created customer at stripe
            return new StripeCustomer(createdCustomer.Name, createdCustomer.Email, createdCustomer.Id);

        }
        [HttpPost]
        public ActionResult Create()
        {
            var domain = "http://localhost:4242";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = "{{PRICE_ID}}",
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = domain + "/success.html",
                CancelUrl = domain + "/cancel.html",
            };
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        /// <summary>
        /// Add a new payment at Stripe using Customer and Payment details.
        /// Customer has to exist at Stripe already.
        /// </summary>
        /// <param name="payment">Stripe Payment</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns><Stripe Payment/returns>
        public async Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct)
        {
            // Set the options for the payment we would like to create at Stripe
            ChargeCreateOptions paymentOptions = new ChargeCreateOptions
            {
                Customer = payment.CustomerId,
                ReceiptEmail = payment.ReceiptEmail,
                Description = payment.Description,
                Currency = payment.Currency,
                Amount = payment.Amount,
            };

            // Create the payment
            var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);
            // Return the payment to requesting method
            return new StripePayment(
              createdPayment.CustomerId,
              createdPayment.ReceiptEmail,
              createdPayment.Description,
              createdPayment.Currency,
              createdPayment.Amount,
              createdPayment.Id);
        }  /// <summary>
           /// Add a new payment at Stripe using Customer and Payment details.
           /// Customer has to exist at Stripe already.
           /// </summary>
           /// <param name="payment">Stripe Payment</param>
           /// <param name="ct">Cancellation Token</param>
           /// <returns><Stripe Payment/returns>
        public async Task<StripePayment> AddStripeCheckout(AddStripePayment payment, CancellationToken ct)
        {
            // Set the options for the payment we would like to create at Stripe
            ChargeCreateOptions paymentOptions = new ChargeCreateOptions
            {
                Customer = payment.CustomerId,
                ReceiptEmail = payment.ReceiptEmail,
                Description = payment.Description,
                Currency = payment.Currency,
                Amount = payment.Amount,
            };

            // Create the payment
            var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);
            // Return the payment to requesting method
            return new StripePayment(
              createdPayment.CustomerId,
              createdPayment.ReceiptEmail,
              createdPayment.Description,
              createdPayment.Currency,
              createdPayment.Amount,
              createdPayment.Id);
        }
        public async Task<Customer> CreateResource()
        {
            var options = new CustomerCreateOptions
            {
                Email = "customer@example.com"
            };

            var service = new CustomerService();
            return await service.CreateAsync(options);

        }
        public async Task<Customer> GetResource(string id)
        {
            var service = new CustomerService();
            return await service.GetAsync(id);
        }
        public async Task<StripeCharge> CreateChargeAsync(string cardNumber, string cardCvc, int cardExpMonth, int cardExpYear, decimal amount, string currency)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = 10,
                Currency = "UAH",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId",Guid.NewGuid().ToString()}
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);
            return await service.CreateAsync(options);
        }
    }
}
