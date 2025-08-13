using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum NotificationType
    {
        // ============================
        // 📘 Novel
        // ============================
        NovelReportNofitication,       // Báo cáo tiểu thuyết
        CommentNovelNotification,      // Bình luận vào tiểu thuyết
        RelyCommentNovel,              // Trả lời bình luận trong tiểu thuyết
        LikeNovelComment,              // Thích bình luận trong tiểu thuyết
        NovelFollow,                   // Theo dõi tiểu thuyết
        LockNovel,                     // Khoá tiểu thuyết
        UnLockNovel,                   // Mở khoá tiểu thuyết

        // ============================
        // 📄 Chapter
        // ============================
        ChapterReportNotification,     // Báo cáo chương
        CommentChapterNotification,    // Bình luận vào chương
        RelyCommentChapter,            // Trả lời bình luận trong chương
        LikeChapterComment,            // Thích bình luận trong chương
        LockChapter,                   // Khoá chương
        UnLockChapter,                 // Mở khoá chương
        CreateChapter,                  // Tạo chương mới
        // ============================
        // 💬 Comment (cả novel & chapter)
        // ============================
        ReportComment,                 // Báo cáo bình luận

        // ============================
        // 👤 User
        // ============================
        UserReport,                    // Báo cáo người dùng
        BanUser,                       // Khoá tài khoản người dùng
        UnBanUser                      // Mở khoá tài khoản người dùng
    }

}
