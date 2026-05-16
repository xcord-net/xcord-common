namespace Xcord.Exceptions;

/// <summary>
/// Thrown for known Stripe / billing surface failures: charge declined, webhook
/// signature mismatch, missing customer, etc.
/// </summary>
public sealed class BillingException : XcordException
{
    public string? StripeCode { get; init; }

    public BillingException(string message, Exception? innerException = null)
        : base(message, "billing_error", innerException)
    {
    }

    public BillingException(string message, string stripeCode, Exception? innerException = null)
        : base(message, "billing_error", innerException)
    {
        StripeCode = stripeCode;
    }
}
