using System;

namespace TicketManagementSystem.Services
{
    public class PaymentService
    {
        public bool ProcessPayment(decimal amount, Payment paymentMethod)
        {
            // Mock payment processing logic
            Console.WriteLine($"Processing payment of {amount:C} using {paymentMethod}...");
            
            // Simulate success for demonstration purposes
            bool isSuccess = true;

            if (isSuccess)
            {
                Console.WriteLine("Payment processed successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Payment failed.");
                return false;
            }
        }
    }
}