using FluentValidation;
using MediatR;

namespace TodoApp.Application.Common.Behaviors;

/// <summary>
/// Behavior xử lý validation tự động cho các request trước khi được xử lý
/// </summary>
/// <typeparam name="TRequest">Kiểu của request</typeparam>
/// <typeparam name="TResponse">Kiểu của response</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Khởi tạo behavior với danh sách validators
    /// </summary>
    /// <param name="validators">Danh sách validators cho request</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        // Lưu trữ danh sách validators
        _validators = validators;
    }

    /// <summary>
    /// Xử lý request, thực hiện validation trước khi chuyển tiếp
    /// </summary>
    /// <param name="request">Request cần xử lý</param>
    /// <param name="next">Handler tiếp theo trong pipeline</param>
    /// <param name="cancellationToken">Token hủy thao tác</param>
    /// <returns>Response từ handler</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Kiểm tra xem có validators nào không
        if (_validators.Any())
        {
            // Tạo context validation
            var context = new ValidationContext<TRequest>(request);

            // Thực hiện tất cả các validators và gom kết quả lại
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Lọc ra các lỗi từ kết quả validation
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // Nếu có lỗi validation, ném ra exception
            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        // Nếu không có lỗi, tiếp tục xử lý request
        return await next();
    }
}
