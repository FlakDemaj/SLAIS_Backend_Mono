using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Tests.Domain.Shared.Builders;

public class NightShiftTemplateEntityBuilder
{
    private string _key = "keller";
    private short _tensionStart = 8;
    private int _sortOrder = 10;
    private bool _withTexts = true;
    private bool _withEnglishText;
    private States _state = States.Pending;

    public NightShiftTemplateEntityBuilder WithKey(string key)
    {
        _key = key;
        return this;
    }

    public NightShiftTemplateEntityBuilder WithTensionStart(short tensionStart)
    {
        _tensionStart = tensionStart;
        return this;
    }

    public NightShiftTemplateEntityBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public NightShiftTemplateEntityBuilder WithoutTexts()
    {
        _withTexts = false;
        return this;
    }

    public NightShiftTemplateEntityBuilder WithEnglishText()
    {
        _withEnglishText = true;
        return this;
    }

    public NightShiftTemplateEntityBuilder WithState(States state)
    {
        _state = state;
        return this;
    }

    public NightShiftTemplateEntity Build()
    {
        var template = NightShiftTemplateEntity.CreateDraft(null, _key, _tensionStart, _sortOrder);
        if (_withTexts)
        {
            template.AddOrReplaceText(CreateGermanText(template.Guid));
        }

        if (_withEnglishText)
        {
            template.AddOrReplaceText(CreateEnglishText(template.Guid));
        }

        if (_state == States.Active)
        {
            template.Activate(null);
        }
        else if (_state == States.Deactived)
        {
            template.Archive(null);
        }
        else if (_state == States.Deleted)
        {
            template.MarkDeleted(Guid.Empty);
        }

        return template;
    }

    private static NightShiftTemplateTextEntity CreateGermanText(Guid templateGuid)
    {
        return NightShiftTemplateTextEntity.Create(
            null,
            templateGuid,
            Language.German,
            "Herr Keller, 58",
            "Akute Schmerzkrise nach OP, fuehlt sich nicht ernst genommen, kurz vorm Ausrasten.",
            "wuetend, verzweifelt, misstrauisch",
            "Deeskalation und Schmerz ernst nehmen ohne falsche Versprechen.",
            "Seit zwei Stunden klingle ich und keiner kommt. Wollen Sie mich hier verrecken lassen?",
            "14.03.1968",
            "maennlich",
            "Station Chirurgie, post-OP (Hueft-TEP)",
            "Z.n. Hueft-TEP rechts; arterielle Hypertonie",
            "Penicillin",
            "Analgesie nach Schema; niedermolekulares Heparin",
            "2",
            "Sturzrisiko erhoeht; Schmerzexazerbation",
            "Ja (kein DNR); keine Patientenverfuegung",
            "Ehefrau hinterlegt; keine Betreuung");
    }

    private static NightShiftTemplateTextEntity CreateEnglishText(Guid templateGuid)
    {
        return NightShiftTemplateTextEntity.Create(
            null,
            templateGuid,
            Language.English,
            "Mr Keller, 58",
            "Acute pain crisis after surgery, feels he is not being taken seriously, about to lose it.",
            "angry, desperate, distrustful",
            "De-escalate and take the pain seriously without making false promises.",
            "I've been ringing for two hours and nobody comes. Are you going to let me die in here?",
            "14/03/1968",
            "male",
            "Surgical ward, post-op (total hip replacement)",
            "Status post right total hip replacement; arterial hypertension",
            "Penicillin",
            "Analgesia per protocol; low-molecular-weight heparin",
            "2",
            "Increased fall risk; pain exacerbation",
            "Yes (no DNR); no advance directive",
            "Wife on file; no legal guardian");
    }
}
