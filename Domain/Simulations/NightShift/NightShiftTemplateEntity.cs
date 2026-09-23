using System.Text.RegularExpressions;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Domain.Simulations.NightShift;

public class NightShiftTemplateEntity : NightShiftTemplateNavigationPropertyEntity
{
    public string Key { get; private set; }

    public States State { get; private set; }

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

    // BaseUpdatedByEntity provides no protected mutator for the updated audit fields.
    public void UpdateHead(
        Guid? updatedByUserGuid,
        string key,
        short tensionStart,
        int sortOrder)
    {
        CheckInputs(key, tensionStart, sortOrder);

        Key = key;
        TensionStart = tensionStart;
        SortOrder = sortOrder;
    }

    public void AddOrReplaceText(NightShiftTemplateTextEntity text)
    {
        var existingText = Texts.FirstOrDefault(x => x.Language == text.Language);
        if (existingText != null)
        {
            Texts.Remove(existingText);
        }

        Texts.Add(text);
    }

    // BaseUpdatedByEntity provides no protected mutator for the updated audit fields.
    public void Activate(Guid? updatedByUserGuid)
    {
        if (GetText(Language.German) == null || GetText(Language.English) == null)
        {
            throw new SlaisException(NightShiftErrorCodes.TemplateActivationNeedsBothLanguages);
        }

        State = States.Active;
    }

    // BaseUpdatedByEntity provides no protected mutator for the updated audit fields.
    public void Archive(Guid? updatedByUserGuid)
    {
        State = States.Deactived;
    }

    // BaseDeleteByEntity provides no protected mutator for the deleted audit fields.
    public void MarkDeleted(Guid deletedByUserGuid)
    {
        State = States.Deleted;
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
