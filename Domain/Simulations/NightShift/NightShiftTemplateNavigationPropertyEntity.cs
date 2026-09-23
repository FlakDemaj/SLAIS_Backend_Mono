using SLAIS.Domain.Commom;

namespace Domain.Simulations.NightShift;

public abstract class NightShiftTemplateNavigationPropertyEntity : BaseDeleteByEntity
{
    public ICollection<NightShiftTemplateTextEntity> Texts { get; private set; } = new List<NightShiftTemplateTextEntity>();

    protected NightShiftTemplateNavigationPropertyEntity(
        Guid? createdByUserGuid)
        : base(createdByUserGuid)
    {
    }
}
