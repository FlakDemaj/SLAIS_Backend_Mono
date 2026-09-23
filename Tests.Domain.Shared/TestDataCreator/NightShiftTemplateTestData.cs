using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Tests.Domain.Shared.TestDataCreator;

public static class NightShiftTemplateTestData
{
    public static List<NightShiftTemplateEntity> CreateCuratedTemplates()
    {
        return new List<NightShiftTemplateEntity>
        {
            CreateTemplate("keller", 8, 10, _kellerGerman, _kellerEnglish),
            CreateTemplate("schmidt", 6, 20, _schmidtGerman, _schmidtEnglish),
            CreateTemplate("yilmaz", 9, 30, _yilmazGerman, _yilmazEnglish),
            CreateTemplate("entzug", 9, 40, _entzugGerman, _entzugEnglish),
            CreateTemplate("vogel", 8, 50, _vogelGerman, _vogelEnglish),
        };
    }

    private static readonly string[] _kellerGerman =
    [
        "Herr Keller, 58", "Akute Schmerzkrise nach OP, fuehlt sich nicht ernst genommen, kurz vorm Ausrasten.", "wuetend, verzweifelt, misstrauisch", "Deeskalation und Schmerz ernst nehmen ohne falsche Versprechen.", "Seit zwei Stunden klingle ich und keiner kommt. Wollen Sie mich hier verrecken lassen?", "14.03.1968", "maennlich", "Station Chirurgie, post-OP (Hueft-TEP)", "Z.n. Hueft-TEP rechts; arterielle Hypertonie", "Penicillin", "Analgesie nach Schema; niedermolekulares Heparin", "2", "Sturzrisiko erhoeht; Schmerzexazerbation", "Ja (kein DNR); keine Patientenverfuegung", "Ehefrau hinterlegt; keine Betreuung"
    ];

    private static readonly string[] _kellerEnglish =
    [
        "Mr Keller, 58", "Acute pain crisis after surgery, feels he is not being taken seriously, about to lose it.", "angry, desperate, distrustful", "De-escalate and take the pain seriously without making false promises.", "I've been ringing for two hours and nobody comes. Are you going to let me die in here?", "14/03/1968", "male", "Surgical ward, post-op (total hip replacement)", "Status post right total hip replacement; arterial hypertension", "Penicillin", "Analgesia per protocol; low-molecular-weight heparin", "2", "Increased fall risk; pain exacerbation", "Yes (no DNR); no advance directive", "Wife on file; no legal guardian"
    ];

    private static readonly string[] _schmidtGerman =
    [
        "Frau Schmidt, 71", "Hat gerade eine Krebsdiagnose erhalten, sitzt allein im Zimmer.", "geschockt, weinend, klammert sich an Hoffnung", "Schlechte Nachricht begleiten, Gefuehle aushalten, nicht beschwichtigen.", "Sagen Sie mir... das stimmt doch nicht, oder? Das muss ein Fehler sein.", "02.09.1954", "weiblich", "Station Onkologie; Diagnoseeroeffnung", "Neu diagnostiziertes Mamma-Ca; Osteoporose", "keine bekannt", "Bisphosphonat; Bedarfsmedikation", "1", "psychische Krise; Schlafstoerung", "Ja; Patientenverfuegung vorhanden", "Tochter hinterlegt"
    ];

    private static readonly string[] _schmidtEnglish =
    [
        "Mrs Schmidt, 71", "Has just received a cancer diagnosis, sitting alone in her room.", "shocked, crying, clinging to hope", "Accompany bad news, tolerate the feelings, do not placate.", "Tell me... that's not true, is it? It has to be a mistake.", "02/09/1954", "female", "Oncology ward; diagnosis disclosure", "Newly diagnosed breast cancer; osteoporosis", "none known", "Bisphosphonate; medication as needed", "1", "Psychological crisis; sleep disturbance", "Yes; advance directive in place", "Daughter on file"
    ];

    private static readonly string[] _yilmazGerman =
    [
        "Herr Yilmaz, 34", "Panikattacke auf Station, Atemnot, Angst zu sterben.", "panisch, hyperventilierend, kann kaum sprechen", "Beruhigen, Atmung fuehren, Sicherheit vermitteln.", "Ich... ich krieg keine Luft... ich glaub ich sterbe... helfen Sie mir!", "21.11.1991", "maennlich", "Notaufnahme/Innere; akute Atemnot", "Panikattacke; V.a. Angststoerung; somatisch unauffaellig", "keine bekannt", "keine Dauermedikation", "kein", "Hyperventilation; Wiederholungsattacken", "Ja (kein DNR); keine Patientenverfuegung", "Lebensgefaehrtin erreichbar"
    ];

    private static readonly string[] _yilmazEnglish =
    [
        "Mr Yilmaz, 34", "Panic attack on the ward, shortness of breath, afraid of dying.", "panicking, hyperventilating, can barely speak", "Calm him down, guide his breathing, convey safety.", "I... I can't breathe... I think I'm dying... help me!", "21/11/1991", "male", "Emergency department / internal medicine; acute shortness of breath", "Panic attack; suspected anxiety disorder; no somatic findings", "none known", "no regular medication", "none", "Hyperventilation; recurrent attacks", "Yes (no DNR); no advance directive", "Partner reachable"
    ];

    private static readonly string[] _entzugGerman =
    [
        "Herr Brandt, 41", "Stationaerer Entzug, koerperlich unruhig und schwitzend, fordert sofort sein Medikament und droht, die Station auf eigene Faust zu verlassen.", "gereizt, fordernd, misstrauisch, getrieben und unruhig, schwankt zwischen Druck machen und Verzweiflung", "Klare Grenzen setzen und gleichzeitig empathisch bleiben, ruhig deeskalieren, Sicherheit gewaehrleisten, nicht moralisieren und sich nicht unter Druck setzen lassen.", "Jetzt hoeren Sie mal zu: Ich brauche sofort was, mir geht's beschissen. Und wenn keiner spurt, bin ich hier gleich weg.", "05.06.1984", "maennlich", "Station; stationaerer Entzug", "Alkoholabhaengigkeit; Entzugssyndrom", "keine bekannt", "Entzugsmedikation nach Schema; Thiamin (Vitamin B)", "kein", "Weglaufgefahr; Aggression; Entzugskomplikationen (Krampf/Delir)", "Ja (kein DNR); keine Patientenverfuegung", "kein Kontakt hinterlegt; gesetzl. Betreuung offen"
    ];

    private static readonly string[] _entzugEnglish =
    [
        "Mr Brandt, 41", "Inpatient detox, physically restless and sweating, demands his medication right now and threatens to leave the ward on his own.", "irritable, demanding, distrustful, driven and restless, swinging between pressure and despair", "Set clear boundaries while staying empathetic, de-escalate calmly, ensure safety, do not moralise and do not let yourself be pressured.", "Now you listen to me: I need something right now, I feel like hell. And if nobody moves, I'm out of here.", "05/06/1984", "male", "Ward; inpatient detoxification", "Alcohol dependence; withdrawal syndrome", "none known", "Withdrawal medication per protocol; thiamine (vitamin B)", "none", "Risk of absconding; aggression; withdrawal complications (seizure/delirium)", "Yes (no DNR); no advance directive", "no contact on file; legal guardianship pending"
    ];

    private static readonly string[] _vogelGerman =
    [
        "Frau Vogel, 68", "Akute Verwirrtheit nach Narkose, will Zugaenge ziehen und aus dem Bett aufstehen.", "desorientiert, aengstlich, abwehrend", "Orientierung geben, beruhigen, Sicherheit ohne Zwang, ruhig deeskalieren.", "Wo bin ich hier? Lassen Sie mich los, ich muss nach Hause!", "03.02.1958", "weiblich", "Station Chirurgie; postoperativ nach Hueft-OP", "Postoperatives Delir; Z.n. Hueft-TEP", "keine bekannt", "Analgesie nach Schema; Bedarfsmedikation", "2", "Sturzgefahr; Selbstgefaehrdung; Entfernen von Zugaengen", "Ja (kein DNR); keine Patientenverfuegung", "Sohn hinterlegt"
    ];

    private static readonly string[] _vogelEnglish =
    [
        "Mrs Vogel, 68", "Acute confusion after anaesthesia, wants to pull out her IV lines and get out of bed.", "disoriented, anxious, defensive", "Provide orientation, calm her, ensure safety without coercion, de-escalate calmly.", "Where am I? Let go of me, I have to go home!", "03/02/1958", "female", "Surgical ward; post-operative after hip surgery", "Post-operative delirium; status post total hip replacement", "none known", "Analgesia per protocol; medication as needed", "2", "Fall risk; self-endangerment; removal of IV lines", "Yes (no DNR); no advance directive", "Son on file"
    ];

    private static NightShiftTemplateEntity CreateTemplate(
        string key,
        short tensionStart,
        int sortOrder,
        string[] german,
        string[] english)
    {
        var template = NightShiftTemplateEntity.CreateDraft(null, key, tensionStart, sortOrder);
        template.AddOrReplaceText(CreateText(template.Guid, Language.German, german));
        template.AddOrReplaceText(CreateText(template.Guid, Language.English, english));
        template.Activate(null);
        return template;
    }

    private static NightShiftTemplateTextEntity CreateText(
        Guid templateGuid,
        Language language,
        string[] fields)
    {
        return NightShiftTemplateTextEntity.Create(
            null,
            templateGuid,
            language,
            fields[0],
            fields[1],
            fields[2],
            fields[3],
            fields[4],
            fields[5],
            fields[6],
            fields[7],
            fields[8],
            fields[9],
            fields[10],
            fields[11],
            fields[12],
            fields[13],
            fields[14]);
    }
}
