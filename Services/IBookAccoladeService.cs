using Backend.Dtos.BookAccolade;
using Backend.enums;

namespace Backend.Services;

public interface IBookAccoladeService
{
    Task<IEnumerable<BookAccoladeResponse>> GetAllAccolades();
    Task<IEnumerable<BookAccoladeResponse>> GetAccoladesByType(AccoladeType type);
    Task<IEnumerable<BookAccoladeResponse>> GetAccoladesByBookId(Guid bookId);
    Task<BookAccoladeResponse> GetAccoladeById(Guid id);
    Task<BookAccoladeResponse> CreateAccolade(CreateBookAccoladeRequest request);
    Task<BookAccoladeResponse> UpdateAccolade(Guid id, UpdateBookAccoladeRequest request);
    Task<bool> DeleteAccolade(Guid id);
}
