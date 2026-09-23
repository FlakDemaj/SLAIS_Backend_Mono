using Domain.Base;
using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Domain.Simulations.NightShift;

public class NightShiftMessageEntity : BaseGuidEntity
{
    public Guid SessionGuid { get; private set; }

    public NightShiftMessageRole Role { get; private set; }

    public string Content { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public NightShiftSessionEntity? Session { get; private set; }

    private NightShiftMessageEntity(
        Guid sessionGuid,
        NightShiftMessageRole role,
        string content,
        int sortOrder)
    {
        SessionGuid = sessionGuid;
        Role = role;
        Content = content;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public static NightShiftMessageEntity Create(
        Guid sessionGuid,
        NightShiftMessageRole role,
        string content,
        int sortOrder)
    {
        CheckInputs(content, sortOrder);

        return new NightShiftMessageEntity(
            sessionGuid,
            role,
            content,
            sortOrder);
    }

    private static void CheckInputs(
        string content,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidInput);
        }

        if (content.Length > 4000)
        {
            throw new SlaisException(NightShiftErrorCodes.MessageTooLong);
        }

        if (sortOrder < 0)
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidSortOrder);
        }
    }
}
