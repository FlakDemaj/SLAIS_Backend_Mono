using Domain.Base;
using Domain.Public.Users;

namespace Domain.Simulations.NightShift;

public abstract class NightShiftSessionNavigationPropertyEntity : BaseCreatedByEntity
{
    public ICollection<NightShiftMessageEntity> Messages { get; private set; } = new List<NightShiftMessageEntity>();

    public NightShiftFeedbackEntity? Feedback { get; private set; }

    public UserEntity? User { get; private set; }

    protected NightShiftSessionNavigationPropertyEntity(
        Guid userGuid)
        : base(userGuid)
    {
    }
}
