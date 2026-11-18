using StudentGpa.Application.DTOs;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Interfaces;

public interface IGpaService
{
    Task<Result<TranscriptDto>> GetTranscriptAsync(
        int studentId,
        CancellationToken ct = default);
}
