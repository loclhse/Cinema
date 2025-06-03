using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    /// <summary>
    /// OperationResult đại diện cho kết quả của một thao tác (thành công hoặc thất bại kèm danh sách lỗi).
    /// Thay thế cho IdentityResult khi trao đổi giữa Application và Infrastructure.
    /// </summary>
    public class OperationResult
    {
        public bool Succeeded { get; private set; }
        public string[] Errors { get; private set; } = Array.Empty<string>();

        private OperationResult(bool succeeded, string[]? errors = null)
        {
            Succeeded = succeeded;
            Errors = errors ?? Array.Empty<string>();
        }

        public static OperationResult Success()
            => new OperationResult(true);

        public static OperationResult Failed(IEnumerable<string> errors)
            => new OperationResult(false, errors.ToArray());
    }
}
