namespace Domain.Enums
{
    public enum ReportScope
    {
        Novel = 0,
        Chapter = 1,
        Comment = 2,
        ForumPost = 3,
        ForumComment = 4,
        User = 5
    }

    public enum ReportReason
    {
        Nudity,        // Ảnh khỏa thân, nội dung gợi dục
        Violence,      // Bạo lực
        Hate,          // Thù ghét, phân biệt
        Spam,          // Spam, quảng cáo rác
        Copyright,     // Vi phạm bản quyền
        Scam,          // Lừa đảo
        Illegal,       // Nội dung phi pháp
        Other,         // Lý do khác
        Harassment,    // Quấy rối (cho comment/post)
        Doxxing,       // Tiết lộ thông tin cá nhân (cho comment/post)
        Offtopic,      // Lệch chủ đề (cho comment/post)
        Misinfo,       // Thông tin sai lệch (cho post)
        Spoiler,       // Tiết lộ nội dung (spoiler) (cho comment/post)
        Impersonation, // Giả mạo danh tính (cho user)
        InappropriateUsername, // Username không phù hợp
        Underage,      // Người dùng dưới tuổi cho phép
    }

    public enum ReportStatus
    {
        Pending = 0,
        Resolved = 1,  // đã xử lý (ẩn/xóa/cảnh cáo…)
        Rejected = 2,  // từ chối báo cáo
        Ignored = 3   // bỏ qua (không đủ bằng chứng…)
    }

    public enum ModerationAction
    {
        None = 0,            // Không làm gì
        HideResource = 1,    // Ẩn tài nguyên 
        DeleteResource = 2,  // Xóa tài nguyên 
        WarnUser = 3,        // Cảnh cáo user
        SuspendUser = 4,     // Tạm khóa tài khoản user
        BanUser = 5          // Cấm vĩnh viễn tài khoản user
    }
}