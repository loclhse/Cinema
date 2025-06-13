using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    /// <summary>
    /// OperationResult đại diện cho kết quả của một thao tác (thành công hoặc thất bại kèm thông điệp).
    /// Thay thế cho IdentityResult khi trao đổi giữa Application và Infrastructure.
    /// </summary>
    public class OperationResult
    {
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Danh sách lỗi khi thất bại.
        /// </summary>
        public string[] Errors { get; private set; } = Array.Empty<string>();

        /// <summary>
        /// Danh sách thông điệp khi thành công.
        /// </summary>
        public string[] Messages { get; private set; } = Array.Empty<string>();

        // ✅ Constructor riêng cho trường hợp thất bại
        public OperationResult(IEnumerable<string> errors)
        {
            Succeeded = false;
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        // ✅ Constructor riêng cho trường hợp thành công
        public OperationResult(IEnumerable<string> messages, bool dummy = true)
        {
            Succeeded = true;
            Messages = messages?.ToArray() ?? Array.Empty<string>();
        }

        // ✅ Static helper cho thành công
        public static OperationResult Success(IEnumerable<string> messages)
            => new OperationResult(messages, true);

        // ✅ Static helper cho thất bại
        public static OperationResult Failed(IEnumerable<string> errors)
            => new OperationResult(errors);
    }
}
