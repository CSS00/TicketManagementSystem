namespace TicketManagementSystem.Utils
{
    public static class Utils
    {

        /// <summary>
        /// Validates if a string is a valid email address.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a random alphanumeric string of the specified length.
        /// </summary>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Checks if a reservation time is still valid by comparing it with the current UTC time.
        /// </summary>
        public static bool IsReservationTimeValid(DateTime reservationTime)
        {
            return reservationTime > DateTime.UtcNow;
        }
    }
}