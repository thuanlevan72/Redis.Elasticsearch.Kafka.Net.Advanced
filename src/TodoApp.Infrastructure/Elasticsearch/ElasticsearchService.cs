using Elasticsearch.Net;
using Nest;
using System.Text.Json;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Infrastructure.Elasticsearch;

/// <summary>
/// Dịch vụ tương tác với Elasticsearch
/// </summary>
public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticClient _client;

    /// <summary>
    /// Khởi tạo dịch vụ với cấu hình Elasticsearch
    /// </summary>
    /// <param name="elasticsearchSettings">Cài đặt kết nối</param>
    public ElasticsearchService(ElasticsearchSettings elasticsearchSettings)
    {
        // Tạo cấu hình kết nối đến máy chủ Elasticsearch
        var settings = new ConnectionSettings(new Uri(elasticsearchSettings.Url))
            .DefaultIndex("todos");

        // Thêm thông tin xác thực nếu cần
        if (!string.IsNullOrEmpty(elasticsearchSettings.Username) && !string.IsNullOrEmpty(elasticsearchSettings.Password))
        {
            settings.BasicAuthentication(elasticsearchSettings.Username, elasticsearchSettings.Password);
        }

        // Bật logging cho phát triển
        settings.EnableDebugMode();

        // Khởi tạo client
        _client = new ElasticClient(settings);
    }

    /// <summary>
    /// Tạo chỉ mục mới trong Elasticsearch
    /// </summary>
    /// <typeparam name="T">Kiểu của tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <returns>True nếu tạo thành công</returns>
    public async Task<bool> CreateIndexAsync<T>(string indexName) where T : class
    {
        // Xây dựng mô tả chỉ mục
        var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Map<T>(m => m.AutoMap())
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(1)
                .RefreshInterval(new Time(TimeSpan.FromSeconds(5)))
            )
        );

        // Kiểm tra kết quả
        return createIndexResponse.IsValid;
    }

    /// <summary>
    /// Kiểm tra chỉ mục đã tồn tại chưa
    /// </summary>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <returns>True nếu chỉ mục đã tồn tại</returns>
    public async Task<bool> IndexExistsAsync(string indexName)
    {
        // Kiểm tra chỉ mục tồn tại
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        
        // Trả về kết quả
        return existsResponse.Exists;
    }

    /// <summary>
    /// Thêm hoặc cập nhật tài liệu vào chỉ mục
    /// </summary>
    /// <typeparam name="T">Kiểu của tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="document">Tài liệu cần thêm/cập nhật</param>
    /// <param name="id">ID của tài liệu</param>
    /// <returns>True nếu thao tác thành công</returns>
    public async Task<bool> IndexDocumentAsync<T>(string indexName, T document, string id) where T : class
    {
        // Index tài liệu vào Elasticsearch
        var indexResponse = await _client.IndexAsync(document, idx => idx
            .Index(indexName)
            .Id(id)
            .Refresh(Refresh.True)
        );  

        // Kiểm tra kết quả
        return indexResponse.IsValid;
    }


    public async Task<bool> IndexAllDocumentAsync<T>(string indexName, List<T> documents) where T : class
    {
        // Index tài liệu vào Elasticsearch
        var indexResponse = await _client.BulkAsync(b => b.Index(indexName).IndexMany(documents).Refresh(Refresh.True));

        // Kiểm tra kết quả
        return indexResponse.IsValid;
    }

    /// <summary>
    /// Xóa tài liệu khỏi chỉ mục
    /// </summary>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="id">ID của tài liệu cần xóa</param>
    /// <returns>True nếu xóa thành công</returns>
    public async Task<bool> DeleteDocumentAsync(string indexName, string id)
    {
        // Xóa tài liệu từ Elasticsearch
        var deleteResponse = await _client.DeleteAsync(new DeleteRequest(indexName, id)
        {
            Refresh = Refresh.True
        });

        // Kiểm tra kết quả
        return deleteResponse.IsValid;
    }

    /// <summary>
    /// Tìm kiếm tài liệu trong chỉ mục
    /// </summary>
    /// <typeparam name="T">Kiểu của tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="page">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách tài liệu và tổng số tài liệu</returns>
    public async Task<(IEnumerable<T> Items, long TotalCount)> SearchAsync<T>(
        string indexName, 
        string searchTerm, 
        int page = 1, 
        int pageSize = 10) where T : class
    {
        // Tính toán vị trí bắt đầu cho phân trang
        var from = (page - 1) * pageSize;

        // Xây dựng truy vấn tìm kiếm
        SearchResponse<T>? searchResponse;

        // Nếu tìm kiếm tất cả
        if (searchTerm == "*")
        {
            var response = await _client.SearchAsync<T>(s => s
                .Index(indexName)
                .From(from)
                .Size(pageSize)
                .MatchAll()
                .Sort(sort => sort.Descending("createdAt"))
            );
            searchResponse = (SearchResponse<T>)response;
        }
        // Nếu là truy vấn Filter (query đã được xây dựng với định dạng KQL)
        else if (searchTerm.Contains(":"))
        {
            // Sử dụng QueryString query để xử lý truy vấn KQL
            var response = await _client.SearchAsync<T>(s => s
                .Index(indexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(searchTerm)
                    )
                )
                .Sort(sort => sort.Descending("createdAt"))
            );
            searchResponse = (SearchResponse<T>)response;
        }
        // Nếu là tìm kiếm thông thường
        else
        {
            var response = await _client.SearchAsync<T>(s => s
                .Index(indexName)
                .From(from)
                .Size(pageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(f => f
                            .Field("title", 3.0) // Tăng trọng số cho tiêu đề
                            .Field("description")
                        )
                        .Query(searchTerm)
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(Fuzziness.Auto) // Cho phép tìm kiếm mờ
                    )
                )
                .Sort(sort => sort.Descending("createdAt"))
            );
            searchResponse = (SearchResponse<T>)response;
        }

        // Kiểm tra kết quả
        if (!searchResponse.IsValid)
        {
            throw new Exception($"Lỗi khi tìm kiếm trong Elasticsearch: {searchResponse.DebugInformation}");
        }

        // Trả về kết quả
        return (searchResponse.Documents, searchResponse.Total);
    }

    /// <summary>
    /// Lấy tài liệu theo ID
    /// </summary>
    /// <typeparam name="T">Kiểu của tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="id">ID của tài liệu</param>
    /// <returns>Tài liệu nếu tìm thấy, null nếu không tìm thấy</returns>
    public async Task<T?> GetDocumentAsync<T>(string indexName, string id) where T : class
    {
        // Lấy tài liệu từ Elasticsearch
        var getResponse = await _client.GetAsync<T>(id, g => g.Index(indexName));
        
        // Nếu không tìm thấy hoặc không hợp lệ
        if (!getResponse.IsValid || !getResponse.Found)
        {
            return null;
        }
        
        // Trả về tài liệu
        return getResponse.Source;
    }
}

/// <summary>
/// Cài đặt kết nối Elasticsearch
/// </summary>
public class ElasticsearchSettings
{
    /// <summary>
    /// URL của máy chủ Elasticsearch
    /// </summary>
    public string Url { get; set; } = "http://localhost:9200";
    
    /// <summary>
    /// Tên người dùng nếu cần xác thực
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Mật khẩu nếu cần xác thực
    /// </summary>
    public string? Password { get; set; }
}
