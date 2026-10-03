using Microsoft.AspNetCore.Http;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Helpers
{
    public class UserSession
    {
        public short AccountID { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
        public int Role { get; set; } // 0: Admin, 1: Staff, 2: Lecturer
        public string RoleName { get; set; } = string.Empty;
        public bool IsLoggedIn => !string.IsNullOrEmpty(AccountEmail);
        public bool IsAdmin => Role == 0;
        public bool IsStaff => Role == 1;

        public static UserSession FromHttpContext(HttpContext context)
        {
            var session = context.Session;
            var email = session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return new UserSession();
            }

            return new UserSession
            {
                AccountID = (short)(session.GetInt32("AccountID") ?? 0),
                AccountName = session.GetString("UserName") ?? "User",
                AccountEmail = email,
                Role = session.GetInt32("UserRole") ?? 2,
                RoleName = session.GetString("RoleName") ?? "User"
            };
        }

        public void SaveToSession(HttpContext context)
        {
            var session = context.Session;
            session.SetInt32("AccountID", AccountID);
            session.SetString("UserName", AccountName);
            session.SetString("UserEmail", AccountEmail);
            session.SetInt32("UserRole", Role);
            session.SetString("RoleName", RoleName);
        }

        public static void ClearSession(HttpContext context)
        {
            context.Session.Clear();
        }
    }
}
