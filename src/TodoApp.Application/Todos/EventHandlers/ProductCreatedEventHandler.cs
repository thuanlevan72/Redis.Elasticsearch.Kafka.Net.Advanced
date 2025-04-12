// using MediatR;
// using TodoApp.Domain.Entities;
// using TodoApp.Domain.Interfaces;
// using TodoApp.Application.Todos.Commands.CreateTodo;
//
// namespace TodoApp.Application.Todos.EventHandlers;
//
// /// <summary>
// /// Handler xử lý sự kiện khi Todo được tạo
// /// </summary>
// public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedNotification>
// {
//     private readonly ITodoSearchRepository _todoSearchRepository;
//
//     /// <summary>
//     /// Khởi tạo handler với repository tìm kiếm
//     /// </summary>
//     /// <param name="todoSearchRepository">Repository tìm kiếm Todo</param>
//     public ProductCreatedEventHandler(ITodoSearchRepository todoSearchRepository)
//     {
//         // Lưu trữ repository tìm kiếm
//         _todoSearchRepository = todoSearchRepository;
//     }
//
//     /// <summary>
//     /// Xử lý thông báo sự kiện Todo được tạo
//     /// </summary>
//     /// <param name="notification">Thông báo sự kiện</param>
//     /// <param name="cancellationToken">Token hủy</param>
//     public async Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
//     {
//         // Log thông tin về Todo đã được tạo
//         Console.WriteLine($"Đang xử lý sự kiện Todo đã tạo: {notification.ProductCreatedEvent.Id}");
//
//         try
//         {
//             // Tạo entity Todo từ sự kiện
//             var todo = new Product(
//                 notification.TodoCreatedEvent.Title,
//                 notification.TodoCreatedEvent.Description,
//                 notification.TodoCreatedEvent.Priority,
//                 notification.TodoCreatedEvent.DueDate
//             );
//
//             // Phản chiếu ID và CreatedAt từ sự kiện
//             // Lưu ý: Trong môi trường thực tế, bạn nên sử dụng Reflection hoặc tạo constructor đặc biệt
//             // Đây là cách tiếp cận đơn giản để minh họa
//             var todoType = typeof(Todo);
//             var idProperty = todoType.GetProperty("Id");
//             var createdAtProperty = todoType.GetProperty("CreatedAt");
//
//             if (idProperty != null && createdAtProperty != null)
//             {
//                 idProperty.SetValue(todo, notification.TodoCreatedEvent.Id);
//                 createdAtProperty.SetValue(todo, notification.TodoCreatedEvent.CreatedAt);
//             }
//
//             // Đồng bộ Todo vào Elasticsearch
//             await _todoSearchRepository.IndexAsync(todo);
//
//             // Log thông tin thành công
//             Console.WriteLine($"Đã đồng bộ Todo {notification.TodoCreatedEvent.Id} vào Elasticsearch");
//         }
//         catch (Exception ex)
//         {
//             // Log lỗi nếu có
//             Console.WriteLine($"Lỗi khi đồng bộ Todo vào Elasticsearch: {ex.Message}");
//             throw;
//         }
//     }
// }
//
// /// <summary>
// /// Thông báo sự kiện khi Todo được tạo
// /// </summary>
// public class ProductCreatedNotification : INotification
// {
//     /// <summary>
//     /// Sự kiện Todo đã tạo
//     /// </summary>
//     public ProductCreatedEvent ProductCreatedEvent { get; }
//
//     /// <summary>
//     /// Khởi tạo thông báo với sự kiện
//     /// </summary>
//     /// <param name="todoCreatedEvent">Sự kiện Todo đã tạo</param>
//     public ProductCreatedNotification(ProductCreatedEvent productCreatedEvent)
//     {
//         // Lưu trữ sự kiện
//         ProductCreatedEvent = productCreatedEvent;
//     }
// }
