using FluentValidation;

namespace TodoApp.Application.Todos.Commands.DeleteTodo;

/// <summary>
/// Validator cho command xóa Todo
/// </summary>
public class DeleteTodoCommandValidator : AbstractValidator<DeleteTodoCommand>
{
    /// <summary>
    /// Khởi tạo validator với các quy tắc validation
    /// </summary>
    public DeleteTodoCommandValidator()
    {
        // Quy tắc cho ID: không được rỗng
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID không được để trống.");
    }
}
