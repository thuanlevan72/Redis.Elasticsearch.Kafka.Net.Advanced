using FluentValidation;

namespace TodoApp.Application.Todos.Commands.UpdateTodo;

/// <summary>
/// Validator cho command cập nhật Todo
/// </summary>
public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    /// <summary>
    /// Khởi tạo validator với các quy tắc validation
    /// </summary>
    public UpdateTodoCommandValidator()
    {
        // Quy tắc cho ID: không được rỗng
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID không được để trống.");

        // Quy tắc cho tiêu đề: không được rỗng và độ dài từ 3-100 ký tự
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(100).WithMessage("Tiêu đề không được vượt quá 100 ký tự.")
            .MinimumLength(3).WithMessage("Tiêu đề phải có ít nhất 3 ký tự.");

        // Quy tắc cho mô tả: có thể rỗng, tối đa 500 ký tự
        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự.");

        // Quy tắc cho mức độ ưu tiên: giá trị từ 0-2
        RuleFor(v => v.Priority)
            .InclusiveBetween(0, 2).WithMessage("Mức độ ưu tiên phải có giá trị từ 0 đến 2 (0: Thấp, 1: Trung bình, 2: Cao).");

        // Quy tắc cho ngày đến hạn: không được sớm hơn ngày hiện tại
        RuleFor(v => v.DueDate)
            .Must(BeAValidDueDate).WithMessage("Ngày đến hạn không được là ngày trong quá khứ.")
            .When(v => v.DueDate.HasValue);
    }

    /// <summary>
    /// Kiểm tra ngày đến hạn hợp lệ
    /// </summary>
    /// <param name="dueDate">Ngày đến hạn cần kiểm tra</param>
    /// <returns>True nếu ngày hợp lệ, ngược lại là False</returns>
    private bool BeAValidDueDate(DateTime? dueDate)
    {
        // Nếu không có ngày đến hạn, coi là hợp lệ
        if (!dueDate.HasValue)
            return true;

        // Ngày đến hạn phải lớn hơn hoặc bằng ngày hiện tại
        return dueDate.Value.Date >= DateTime.UtcNow.Date;
    }
}
