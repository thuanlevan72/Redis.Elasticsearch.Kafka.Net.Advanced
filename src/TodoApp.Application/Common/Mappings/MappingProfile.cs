using AutoMapper;
using System.Reflection;
using TodoApp.Domain.Entities;
using TodoApp.Application.Todos.Commands.CreateTodo;
using TodoApp.Application.Todos.Commands.UpdateTodo;
using TodoApp.Application.Todos.Queries.GetTodoById;
using TodoApp.Application.Todos.Queries.GetTodosList;
using TodoApp.Application.Todos.Queries.SearchTodos;

namespace TodoApp.Application.Common.Mappings;

/// <summary>
/// Lớp cấu hình mapping giữa các đối tượng
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Khởi tạo cấu hình mapping
    /// </summary>
    public MappingProfile()
    {
        // Áp dụng các mapping được định nghĩa trong từng lớp
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());

        // Định nghĩa các mapping cụ thể
        
        // Mapping từ Todo sang TodoDto
        CreateMap<Todo, TodoDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.IsCompleted, opt => opt.MapFrom(s => s.IsCompleted))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority))
            .ForMember(d => d.DueDate, opt => opt.MapFrom(s => s.DueDate))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt));

        // Mapping từ Todo sang TodoDetailDto
        CreateMap<Todo, TodoDetailDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.IsCompleted, opt => opt.MapFrom(s => s.IsCompleted))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority))
            .ForMember(d => d.DueDate, opt => opt.MapFrom(s => s.DueDate))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt));

        // Mapping từ Todo sang TodoSearchResultDto
        CreateMap<Todo, TodoSearchResultDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.IsCompleted, opt => opt.MapFrom(s => s.IsCompleted))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority))
            .ForMember(d => d.DueDate, opt => opt.MapFrom(s => s.DueDate));
    }

    /// <summary>
    /// Tự động áp dụng các mapping từ các lớp có triển khai IMapFrom
    /// </summary>
    /// <param name="assembly">Assembly chứa các lớp mapping</param>
    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        // Pattern để tìm các phương thức mapping
        var mapFromType = typeof(IMapFrom<>);
        
        // Tìm tất cả các type có triển khai IMapFrom
        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        // Lặp qua tất cả các types trong assembly
        foreach (var type in assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType)))
        {
            // Tạo instance của type
            var instance = Activator.CreateInstance(type);

            // Tìm phương thức Mapping
            var methodInfo = type.GetMethod(mappingMethodName) 
                          ?? type.GetInterface(mapFromType.Name)?.GetMethod(mappingMethodName);

            // Gọi phương thức Mapping để thiết lập mapping
            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
}

/// <summary>
/// Interface cho các lớp muốn định nghĩa mapping
/// </summary>
/// <typeparam name="T">Type nguồn cần mapping</typeparam>
public interface IMapFrom<T>
{
    /// <summary>
    /// Phương thức thiết lập mapping
    /// </summary>
    /// <param name="profile">Profile mapping</param>
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
