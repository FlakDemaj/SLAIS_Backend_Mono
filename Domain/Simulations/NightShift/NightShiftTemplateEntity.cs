using System.Text.RegularExpressions;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Domain.Simulations.NightShift;

public class NightShiftTemplateEntity : NightShiftTemplateNavigationPropertyEntity
{
    public string Key { get; private set; }

    public States State { get; private set; }

    public int Version { get; private set; }

    public short TensionStart { get; private set; }

    public int SortOrder { get; private set; }

    private NightShiftTemplateEntity(
        Guid? createdByUserGuid,
        string key,
        short tensionStart,
        int sortOrder)
        : base(createdByUserGuid)
    {
        Key = key;
        State = States.Pending;
        Version = 1;
        TensionStart = tensionStart;
        SortOrder = sortOrder;
    }

    #region Create

    public static NightShiftTemplateEntity CreateDraft(
        Guid? createdByUserGuid,
        string key,
        short tensionStart,
        int sortOrder)
    {
        CheckInputs(key, tensionStart, sortOrder);

        return new NightShiftTemplateEntity(
            createdByUserGuid,
            key,
            tensionStart,
            sortOrder);
    }

    #endregion

    #region Behaviour

    // Audit fields of the base classes have no protected mutators; the maintainer decides whether to add them (see docs/night-shift/PROGRESS-phase1.md).
    public void UpdateHead(
        Guid? updatedByUserGuid,
        string key,
        short tensionStart,
        int sortOrder)
    {
        EnsureEditable();
        CheckInputs(key, tensionStart, sortOrder);

        Key = key;
        TensionStart = tensionStart;
        SortOrder = sortOrder;
        Version++;
    }

    public void AddOrReplaceText(NightShiftTemplateTextEntity text)
    {
        EnsureEditable();

        var existingText = Texts.FirstOrDefault(x => x.Language == text.Language);
        if (existingText != null)
        {
            Texts.Remove(existingText);
        }

        Texts.Add(text);
        Version++;
    }

    // Audit fields of the base classes have no protected mutators; the maintainer decides whether to add them (see docs/night-shift/PROGRESS-phase1.md).
    public void Activate(Guid? updatedByUserGuid)
    {
        EnsureEditable();

        if (State != States.Pending && State != States.Deactived)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateStateInvalid);
        }

        if (GetText(Language.German) == null || GetText(Language.English) == null)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateActivationNeedsBothLanguages);
        }

        State = States.Active;
        Version++;
    }

    // Audit fields of the base classes have no protected mutators; the maintainer decides whether to add them (see docs/night-shift/PROGRESS-phase1.md).
    public void Archive(Guid? updatedByUserGuid)
    {
        EnsureEditable();

        if (State != States.Active && State != States.Pending)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateStateInvalid);
        }

        State = States.Deactived;
        Version++;
    }

    // Audit fields of the base classes have no protected mutators; the maintainer decides whether to add them (see docs/night-shift/PROGRESS-phase1.md).
    public void Reopen(Guid? updatedByUserGuid)
    {
        EnsureEditable();

        if (State != States.Active && State != States.Deactived)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateStateInvalid);
        }

        State = States.Pending;
        Version++;
    }

    // Audit fields of the base classes have no protected mutators; the maintainer decides whether to add them (see docs/night-shift/PROGRESS-phase1.md).
    public void MarkDeleted(Guid deletedByUserGuid)
    {
        EnsureEditable();

        State = States.Deleted;
        Version++;
    }

    public NightShiftTemplateTextEntity? GetText(Language language)
    {
        return Texts.FirstOrDefault(x => x.Language == language);
    }

    public NightShiftTemplateTextResolution GetTextOrFallback(Language language)
    {
        var text = GetText(language);
        if (text != null)
        {
            return new NightShiftTemplateTextResolution(text, false);
        }

        var germanText = GetText(Language.German);
        if (germanText != null)
        {
            return new NightShiftTemplateTextResolution(germanText, true);
        }

        throw new SlaisException(NightShiftErrorCodes.InvalidInput);
    }

    #endregion

    #region Checks

    private void EnsureEditable()
    {
        if (State == States.Deleted)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateStateInvalid);
        }
    }

    private static void CheckInputs(
        string key,
        short tensionStart,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(key) || !Regex.IsMatch(key, "^[a-z0-9-]{2,40}$"))
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateKeyInvalid);
        }

        if (tensionStart < 0 || tensionStart > 10 || sortOrder < 0)
        {
            throw new SlaisException(NightShiftErrorCodes.InvalidInput);
        }
    }

    #endregion
}
