using GHCAA.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface IForumService
    {
        Task<List<ForumCategoryDto>> GetCategoriesAsync();
        
        Task<List<ForumTopicDto>> GetTopicsByCategoryAsync(int categoryId, int page, int pageSize);
        
        Task<ForumTopicDto?> GetTopicByIdAsync(int topicId);
        
        Task<List<ForumPostDto>> GetPostsByTopicAsync(int topicId, int page, int pageSize);
        
        Task<ForumTopicDto> CreateTopicAsync(CreateForumTopicDto dto, int authorId);
        
        Task<ForumPostDto> CreatePostAsync(CreateForumPostDto dto, int authorId);
        
        Task DeleteTopicAsync(int topicId, int memberId, bool isSuperAdmin);
        
        Task DeletePostAsync(int postId, int memberId, bool isSuperAdmin);
    }
}
