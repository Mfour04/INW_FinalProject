namespace Domain.Enums
{
    public enum BadgeAction
    {
         // --- Đọc truyện ---
        ReadNovel,             // Đọc x truyện khác nhau
        ReadWords,             // Đọc tổng x từ
        ReadDaysStreak,        // Đọc liên tục x ngày
        
        // --- Viết truyện ---
        CreateNovel,           // Tạo truyện đầu tiên hoặc truyện thứ x
        WriteWords,            // Viết tổng x từ
        NovelGetView,          // Truyện đạt x lượt xem
        NovelGetLike,          // Truyện được x lượt thích
        NovelComplete,         // Truyện được hoàn thành

        // --- Hệ thống ---
        InviteUser,            // Mời người dùng mới
        DailyCheckin,          // Điểm danh hằng ngày
    }
}